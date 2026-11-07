# Phiale.Design

A shared visual foundation for WPF, Avalonia, and WinUI 3: color roles,
metrics, typography, and explicitly selected styles for basic controls.
The libraries are independent of the applications and products that use them.

| Package | Version | Platform |
| --- | --- | --- |
| Phiale.Design.Wpf | 0.1.0-preview.9 | net48, net8.0-windows10.0.19041.0 |
| Phiale.Design.Avalonia | 0.1.0-preview.10 | net8.0, Avalonia 11.3.7 |
| Phiale.Design.WinUI | 0.1.0-preview.11 | net8.0-windows10.0.19041.0, Windows App SDK 1.8 |

Preview status: the public API may change. Building local packages does not
publish them to a public feed.

## Building and testing

The full solution requires Windows, .NET SDK 10, the Windows SDK, and
WPF/WinUI 3 build tools. The Avalonia library and tests also support Linux.

```powershell
dotnet build Phiale.Design.sln -c Release -p:Platform=x64
pwsh -File packaging/Pack.ps1
pwsh -File packaging/Verify.ps1
pwsh -File packaging/Verify.ps1 -UsePackages
```

Packages are written to `artifacts/packages`. Verification runs the Avalonia
tests and native WPF (net48 and net8), Avalonia, and WinUI hosts.
`-UsePackages` verifies local packages instead of using project references.
Native host tests require a Windows desktop session. On Linux:

```sh
dotnet test tests/Avalonia/Phiale.Design.Avalonia.Tests.csproj -c Release
```

## WPF

Import the resource dictionaries in this order:

```xml
<ResourceDictionary Source="pack://application:,,,/Phiale.Design.Wpf;component/Themes/Palette.xaml" />
<ResourceDictionary Source="pack://application:,,,/Phiale.Design.Wpf;component/Themes/Controls.xaml" />
```

```xml
<StackPanel xmlns:design="clr-namespace:Phiale.Design.Wpf;assembly=Phiale.Design.Wpf"
            design:ThemeScope.Mode="Dark">
    <Button Style="{StaticResource Phiale.Design.Style.Button.Primary}" Content="Save" />
</StackPanel>
```

`ThemeScope.Mode` is an inherited choice of Light, Dark, or HighContrast.
The host is responsible for tracking system settings. Optionally import
`Themes/HighContrastBlack.xaml` after the palette to use black, white, and
yellow high contrast colors.

## Avalonia

The host loads `FluentTheme`. Add these entries to
`ResourceDictionary.MergedDictionaries`:

```xml
<ResourceInclude Source="avares://Phiale.Design.Avalonia/Themes/Palette.axaml" />
<ResourceInclude Source="avares://Phiale.Design.Avalonia/Themes/Controls.axaml" />
```

```xml
<ThemeVariantScope RequestedThemeVariant="Dark">
    <Button Theme="{StaticResource Phiale.Design.Style.Button.Primary}" Content="Save" />
</ThemeVariantScope>
```

Select high contrast using
`Phiale.Design.Avalonia.DesignThemeVariants.HighContrast`.
Avalonia provides explicit CheckBox and Calendar themes. `LocalizedCalendar`
exposes `LanguageCode` to control labels and the first day of the week without
changing the process culture. The host selects the calendar style through `Theme`.

## WinUI 3

Import these dictionaries after `XamlControlsResources`:

```xml
<ResourceDictionary Source="ms-appx:///Phiale.Design.WinUI/Themes/Palette.xaml" />
<ResourceDictionary Source="ms-appx:///Phiale.Design.WinUI/Themes/Controls.xaml" />
```

Use `Style="{StaticResource Phiale.Design.Style.Button.Primary}"`.
Set `RequestedTheme="Light"` or `Dark` on the desired scope; the system
selects high contrast. The optional `Controls.HighContrastBlack.xaml`
dictionary provides styles with the `.HighContrastBlack` suffix. The package
includes the DLL, PRI, XAML, and rules for copying resources to the application.

## Resources and customization

[The token catalog](Tokens/README.md) describes roles, variants, and contrast
limitations. Styles use the `Phiale.Design.Style.` prefix and cover text,
surfaces, badges, buttons, ToggleButton, TextBox, ComboBox, and ScrollBar.
A badge requires matching container and text styles, such as `Badge.Accent`
and `Badge.Accent.Text`.

Styles are selected explicitly; ordinary host controls retain their own
appearance. Extend styles through `BasedOn` and override roles within the
scope chosen by the application. Do not mutate shared brush instances.
Native templates and states differ between frameworks; the library does not
guarantee identical pixel rendering or full WCAG compliance.

## License

[MIT](LICENSE.md). Copyright (c) 2026 Roland Rusiecki.

## Local Avalonia typography correction — 2026-11-07

The author requested correction of the Linux/WSLg typography and temporary use
of the local feed from Phiale.Shell. The agent selected packaged Inter as the
Avalonia implementation: `Avalonia.Fonts.Inter` 11.3.7 is a normal transitive
NuGet dependency, not copied source. No system font installation or application
style override is required. Font assets and their notices remain in that package.
WPF and WinUI retain Segoe UI. The catalog records a platform-specific font
value; generated palettes must not be edited independently.

Local Avalonia version: `0.1.0-preview.10`, not published.
Build it with:

```powershell
dotnet pack Platforms/Avalonia/Phiale.Design.Avalonia/Phiale.Design.Avalonia.csproj -c Release -o artifacts/packages
```

The font regression checks actual glyph-family resolution, Normal/SemiBold/Bold
weights and Polish characters, so a successful system fallback cannot mask a
missing font asset. The calendar input helper now updates layout before computing
click coordinates, since font changes affect button geometry.

Verification of this correction: Avalonia tests 13/13 on Windows and Ubuntu-22.04
WSL2, including real Inter glyph resolution. The consuming Shell passed 26/26
Avalonia tests on both systems and native WSLg Workspace/legacy scenarios with
the local package. Screenshots and performance limitations are recorded in the
sibling Phiale.Shell repository at `docs/Done/Linux-Typografia.md`.

## Version naming correction — 2026-11-08

The author required ordinary preview versions instead of the agent's local suffix.
Avalonia is now 0.1.0-preview.10; locality is represented by the feed directory.
Existing history is preserved. This change does not itself publish the package.
