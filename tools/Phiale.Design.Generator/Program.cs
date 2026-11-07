using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

if (args.Length != 3 || !new[] { "Wpf", "Avalonia", "WinUI" }.Contains(args[0]))
    throw new ArgumentException("Usage: Phiale.Design.Generator <Wpf|Avalonia|WinUI> <catalog.json> <output.xaml>");

var platform = args[0];
var source = Path.GetFullPath(args[1]);
var destination = Path.GetFullPath(args[2]);
using var document = JsonDocument.Parse(File.ReadAllText(source));
var root = document.RootElement;
var schemaVersion = root.GetProperty("schemaVersion").GetInt32();
if (schemaVersion is not (2 or 3))
    throw new InvalidDataException("Unsupported catalog schema.");

var primitives = new Dictionary<string, string>(StringComparer.Ordinal);
if (schemaVersion == 3)
{
    var distinctColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var item in root.GetProperty("primitives").EnumerateObject())
    {
        if (!Regex.IsMatch(item.Name, @"^Palette\.[A-Za-z][A-Za-z0-9.]*$") ||
            item.Value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException("Invalid primitive: " + item.Name);
        var literal = item.Value.GetString() ?? "";
        if (!IsHex(literal)) throw new InvalidDataException("Invalid primitive color: " + item.Name);
        var canonicalColor = literal.Length == 9 && literal.Substring(1, 2).Equals("FF", StringComparison.OrdinalIgnoreCase)
            ? "#" + literal[3..] : literal;
        if (!distinctColors.Add(canonicalColor))
            throw new InvalidDataException("Duplicate primitive color: " + item.Name);
        primitives.Add(item.Name, literal);
    }
}

var variants = new[] { "Light", "Dark", "HighContrast" };
var roles = new List<Role>();
var keys = new HashSet<string>(StringComparer.Ordinal);
var roleDefinitions = root.GetProperty("roles").EnumerateArray().ToList();
if (root.TryGetProperty("internalRoles", out var internalRoles))
{
    foreach (var item in internalRoles.EnumerateArray())
    {
        if (!(item.GetProperty("key").GetString() ?? "").StartsWith("Phiale.Design.Internal.", StringComparison.Ordinal))
            throw new InvalidDataException("Internal resources must use the Internal namespace.");
        roleDefinitions.Add(item);
    }
}
foreach (var item in roleDefinitions)
{
    var key = item.GetProperty("key").GetString() ?? "";
    var kind = item.GetProperty("kind").GetString() ?? "";
    if (!Regex.IsMatch(key, @"^Phiale\.Design\.[A-Za-z][A-Za-z0-9.]*$") || !keys.Add(key))
        throw new InvalidDataException("Invalid or duplicate role: " + key);
    if (kind == "brush")
    {
        var values = item.GetProperty("variants");
        if (values.EnumerateObject().Count() != variants.Length)
            throw new InvalidDataException("Expected exactly three variants: " + key);
        var colors = variants.ToDictionary(v => v, v => ParseColor(values.GetProperty(v), v, key,
            primitives, schemaVersion == 3, platform));
        roles.Add(new Role(key, kind, null, colors));
    }
    else if (kind is "double" or "fontFamily" or "fontWeight" or "thickness" or "cornerRadius")
    {
        var value = item.GetProperty("value").GetString() ?? "";
        if (kind == "double" && (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ||
                                  !double.IsFinite(number) || number <= 0))
            throw new InvalidDataException("Invalid size: " + key);
        if (kind == "fontFamily" && string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException("Empty font family: " + key);
        if (kind == "fontWeight" && !new HashSet<string>(StringComparer.Ordinal)
            { "Thin", "ExtraLight", "Light", "Normal", "Medium", "SemiBold", "Bold", "ExtraBold", "Black" }.Contains(value))
            throw new InvalidDataException("Invalid font weight: " + key);
        if (kind is "thickness" or "cornerRadius")
        {
            var components = value.Split(',');
            if (components.Length is not (1 or 2 or 4) || components.Any(component =>
                    !double.TryParse(component.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var measure) ||
                    !double.IsFinite(measure) || measure < 0))
                throw new InvalidDataException("Invalid metric: " + key);
        }
        roles.Add(new Role(key, kind, value, null));
    }
    else throw new InvalidDataException("Unsupported role kind: " + kind);
}

var output = new StringBuilder();
var ns = platform == "Avalonia" ? "https://github.com/avaloniaui" : "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
output.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
output.Append("<ResourceDictionary xmlns=\"").Append(ns)
    .Append("\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
if (platform == "Wpf") output.Append(" xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"");
if (platform == "Avalonia") output.Append(" xmlns:design=\"clr-namespace:Phiale.Design.Avalonia;assembly=Phiale.Design.Avalonia\"");
output.AppendLine(">");

if (platform == "Wpf")
{
    foreach (var variant in variants)
        foreach (var role in roles.Where(r => r.Kind == "brush"))
            AddBrush(output, platform, role.Key + "." + variant, role.Variants![variant], "    ");
}
else
{
    output.AppendLine("    <ResourceDictionary.ThemeDictionaries>");
    foreach (var variant in variants)
    {
        var variantKey = platform == "Avalonia" && variant == "HighContrast"
            ? "{x:Static design:DesignThemeVariants.HighContrast}" : variant;
        output.Append("        <ResourceDictionary x:Key=\"").Append(variantKey).AppendLine("\">");
        foreach (var role in roles.Where(r => r.Kind == "brush"))
            AddBrush(output, platform, role.Key, role.Variants![variant], "            ");
        output.AppendLine("        </ResourceDictionary>");
    }
    output.AppendLine("    </ResourceDictionary.ThemeDictionaries>");
}

foreach (var primitive in primitives.OrderBy(item => item.Key, StringComparer.Ordinal))
    output.Append("    <Color x:Key=\"Phiale.Design.").Append(primitive.Key).Append("\">")
        .Append(primitive.Value).AppendLine("</Color>");

foreach (var role in roles.Where(r => r.Kind != "brush"))
{
    var tag = role.Kind switch
    {
        "double" => platform == "Wpf" ? "sys:Double" : "x:Double",
        "fontFamily" => "FontFamily",
        "fontWeight" => "FontWeight",
        "thickness" => "Thickness",
        "cornerRadius" => "CornerRadius",
        _ => throw new InvalidDataException("Unsupported resource kind: " + role.Kind)
    };
    output.Append("    <").Append(tag).Append(" x:Key=\"").Append(role.Key).Append("\">")
        .Append(XmlEscape(role.Value!)).Append("</").Append(tag).AppendLine(">");
}
output.AppendLine("</ResourceDictionary>");
Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
var result = output.ToString();
if (!File.Exists(destination) || File.ReadAllText(destination) != result)
    File.WriteAllText(destination, result, new UTF8Encoding(false));

// Optional WPF palette uses the catalog's predefined contrast pair, just as Avalonia.
// Keep the default WPF palette bound to system colors.
if (platform is "Wpf" or "WinUI")
{
    var contrast = new StringBuilder();
    contrast.AppendLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
    foreach (var role in roles.Where(role => role.Kind == "brush"))
        AddBrush(contrast, "Avalonia", role.Key + (platform == "Wpf" ? ".HighContrast" : ".HighContrastBlack"), role.Variants!["HighContrast"], "    ");
    contrast.AppendLine("</ResourceDictionary>");
    var contrastPath = Path.Combine(Path.GetDirectoryName(destination)!, "HighContrastBlack.xaml");
    if (!File.Exists(contrastPath) || File.ReadAllText(contrastPath) != contrast.ToString())
        File.WriteAllText(contrastPath, contrast.ToString(), new UTF8Encoding(false));
}

static ColorValue ParseColor(JsonElement element, string variant, string key,
    IReadOnlyDictionary<string, string> primitives, bool requirePrimitive, string platform)
{
    if (element.ValueKind == JsonValueKind.String && variant != "HighContrast")
    {
        if (requirePrimitive)
            throw new InvalidDataException("Layered catalog requires a primitive reference: " + key);
        var literal = element.GetString() ?? "";
        if (!IsHex(literal)) throw new InvalidDataException("Invalid color: " + key);
        return new ColorValue(literal, null);
    }
    if (element.ValueKind == JsonValueKind.Object && variant != "HighContrast" &&
        element.EnumerateObject().Count() == 1 && element.TryGetProperty("ref", out var reference))
    {
        var primitiveKey = reference.GetString() ?? "";
        if (!primitives.TryGetValue(primitiveKey, out var literal))
            throw new InvalidDataException("Unknown primitive " + primitiveKey + " in " + key);
        return new ColorValue(literal, null);
    }
    if (element.ValueKind != JsonValueKind.Object || variant != "HighContrast" ||
        element.EnumerateObject().Count() != (element.TryGetProperty("opacity", out _) ? 3 : 2))
        throw new InvalidDataException("Invalid High Contrast role: " + key);
    var opacity = element.TryGetProperty("opacity", out var opacityValue) ? opacityValue.GetDouble() : 1;
    if (!double.IsFinite(opacity) || opacity < 0 || opacity > 1)
        throw new InvalidDataException("Invalid High Contrast opacity: " + key);
    var systemValue = element.GetProperty("system");
    var wpfSystems = new HashSet<string>(StringComparer.Ordinal)
    {
        "Window", "WindowText", "Control", "ControlDark", "ControlDarkDark",
        "ControlLight", "ControlText", "GrayText", "Highlight", "HighlightText", "HotTrack"
    };
    var winuiSystems = new HashSet<string>(StringComparer.Ordinal)
    {
        "Window", "WindowText", "ButtonFace", "ButtonText", "GrayText",
        "Highlight", "HighlightText", "Hotlight"
    };
    string wpfSystem;
    string winuiSystem;
    if (systemValue.ValueKind == JsonValueKind.String)
    {
        wpfSystem = winuiSystem = systemValue.GetString() ?? "";
    }
    else if (systemValue.ValueKind == JsonValueKind.Object &&
             systemValue.EnumerateObject().Count() == 2)
    {
        wpfSystem = systemValue.GetProperty("Wpf").GetString() ?? "";
        winuiSystem = systemValue.GetProperty("WinUI").GetString() ?? "";
    }
    else throw new InvalidDataException("Invalid High Contrast system mapping: " + key);
    var fallback = element.GetProperty("fallback").GetString() ?? "";
    if (!wpfSystems.Contains(wpfSystem) || !winuiSystems.Contains(winuiSystem) || !IsHex(fallback))
        throw new InvalidDataException("Invalid High Contrast value: " + key);
    var system = platform == "WinUI" ? winuiSystem : wpfSystem;
    return new ColorValue("system:" + system, fallback, opacity);
}

static bool IsHex(string color) => Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$");

static void AddBrush(StringBuilder output, string platform, string key, ColorValue value, string indent)
{
    var color = value.Main;
    if (value.Main.StartsWith("system:", StringComparison.Ordinal))
    {
        var name = value.Main[7..];
        color = platform switch
        {
            "Wpf" => "{DynamicResource {x:Static SystemColors." + name + "ColorKey}}",
            "WinUI" => "{ThemeResource SystemColor" + name + "Color}",
            _ => value.Fallback ?? throw new InvalidDataException("Missing predefined HC color: " + key)
        };
    }
    output.Append(indent).Append("<SolidColorBrush x:Key=\"").Append(key)
        .Append("\" Color=\"").Append(XmlEscape(color)).Append('"');
    if (value.Opacity != 1)
        output.Append(" Opacity=\"").Append(value.Opacity.ToString(CultureInfo.InvariantCulture)).Append('"');
    output.AppendLine(" />");
}

static string XmlEscape(string value) => System.Security.SecurityElement.Escape(value) ?? "";

internal sealed record ColorValue(string Main, string? Fallback, double Opacity = 1);
internal sealed record Role(string Key, string Kind, string? Value, Dictionary<string, ColorValue>? Variants);
