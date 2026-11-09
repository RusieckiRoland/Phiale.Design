# Phiale.Design

A shared visual foundation for WPF, Avalonia, and WinUI 3: color roles,
metrics, typography, and explicitly selected styles for basic controls.
The libraries are independent of the applications and products that use them.

| Package | Version | Platform |
| --- | --- | --- |
| Phiale.Design.Wpf | 0.1.0-preview.19 | net48, net8.0-windows10.0.19041.0 |
| Phiale.Design.Avalonia | 0.1.0-preview.10 | net8.0, Avalonia 11.3.7 |
| Phiale.Design.WinUI | 0.1.0-preview.12 | net8.0-windows10.0.19041.0, Windows App SDK 1.8 |

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

## WPF tool cards

Version `0.1.0-preview.13` adds explicitly selected tool-card styles.
Import `Themes/Palette.xaml` and `Themes/ToolCards.xaml`. ToolCards also imports
Controls.xaml. The styles do not depend on Grid, Shell, application commands,
localization, docking state, or application content.

All keys start with `Phiale.Design.Style.ToolCard.`

| Suffix | Target |
| --- | --- |
| Surface | Border |
| Header | Border |
| HeaderLayout | DockPanel |
| Title | TextBlock |
| DragGrip | TextBlock |
| ActionButton | Button |
| CloseButton | Button |
| ToggleButton | Button |
| Splitter | Thumb |

```xml
<Border Style="{StaticResource Phiale.Design.Style.ToolCard.Surface}">
    <StackPanel>
        <Border Style="{StaticResource Phiale.Design.Style.ToolCard.Header}">
            <DockPanel Style="{StaticResource Phiale.Design.Style.ToolCard.HeaderLayout}" LastChildFill="False">
                <Button DockPanel.Dock="Right" Style="{StaticResource Phiale.Design.Style.ToolCard.CloseButton}"
                        Command="{Binding ClosePanelCommand}" />
                <TextBlock Style="{StaticResource Phiale.Design.Style.ToolCard.Title}" Text="{Binding PanelTitle}" />
            </DockPanel>
        </Border>
        <ContentPresenter Content="{Binding PanelContent}" />
    </StackPanel>
</Border>
```

The appearance preserves the Grid reference supplied by the author. Tool-card
surface and header radii are 12 and 8 DIP, header minimum height is 34 DIP,
and the circular toggle visual is 22 DIP. Named ToolCard metrics live in the
source token catalog. These are separate semantic roles from Radius.Medium.
Colors use the existing Surface, Text, Border and interaction roles, including
dynamic Windows High Contrast colors. The application owns all actions.

On 2026-11-09 the author requested moving the existing appearance from the
project's Grid into Phiale.Design, with English names. The agent extracted
and generalized those local styles, added token and style contracts, and
connected the Grid consumer to the local WPF package. Native package consumers
passed 224 checks on each of net8 and net48, including Light, Dark, HighContrast,
interaction, content isolation and dynamic radius overrides. Evidence is under
`artifacts/tool-card-wpf-net8-final` and `artifacts/tool-card-wpf-net48-final`.

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

## 2026-11-08 — WinUI ComboBox contrast

The author requested a ComboBox HighContrastBlack variant and approved normal
version 0.1.0-preview.12 for the local feed, without publication. The AI agent
implemented the additive style based on the existing Design ComboBox and
semantic black/white/yellow roles, plus native resource, color, focus and
selection checks. Shell consumes this ready style; it does not duplicate it.

Local package verification: `packaging/Verify.ps1 -UsePackages` passed with a
fresh package cache. Native results: WPF net48/net8 178 checks each, Avalonia
107, WinUI 62, including opening and selecting the contrast ComboBox. Evidence:
`artifacts/verification/d1fecb8ffc254bc3be356e21fd0492c2`. The consuming Shell
also passed its history smoke test and captured Light/Dark/HighContrast
(`artifacts-temp/route-R3/history-WinUI-design12` in the sibling repository).
This verifies the explicit demonstration variant, not exhaustive system HC.


## Light palette approved on 2026-11-08

The author supplied five exact colors during the Phiale.Shell visual review.
The AI agent mapped them to existing semantic roles in Tokens/design.candidate.json.

| Role | Color |
| --- | --- |
| Surface.Secondary | #F5F6F8 |
| Selection.Active.Fill | #E5EFFB |
| Accent, Accent.Strong, Border.Accent, Focus.Ring | #1E5799 |
| Text.Primary, Selection.Active.Text | #243247 |
| Border.Subtle | #DDE3EA |

The shared catalog is the source for these Light values. The current delivery
builds the WPF preview.10 package into artifacts/packages for local Shell use.
Validation for this change is limited to WPF at the author's request.

The Shell WPF consumer verified all five exact resource values and passed six
focused WPF tests, including native theme and compact-layout rendering.
Evidence is in Phiale.Shell/artifacts-temp/wpf-design-palette.


The author's subsequent 2026-11-08 reference separates navigation and content.
Surface.Secondary.Light is #ECEFF3 for the sidebar. Surface.Base.Light is
#FFFFFF for the workspace. WPF preview.11 carries this clarification.

## WPF geometry approved on 2026-11-09

After comparing the running Phiale.Shell demonstrator at 96 DPI, the author
selected a 1 DIP border and a 5 DIP corner radius. The AI agent implemented
this decision as a WPF-specific value for Phiale.Design.Radius.Medium in
Tokens/design.candidate.json. The existing platform-value mechanism now also
accepts corner-radius roles. The shared fallback remains 6 DIP for Avalonia
and WinUI. Phiale.Design.Thickness.Border.Default remains 1 DIP.

Phiale.Design.Wpf 0.1.0-preview.12 carries the accepted WPF geometry. It was
built locally in artifacts/packages. Package-based WPF foundation hosts passed
180 checks each on .NET 8 and .NET Framework 4.8. Generator output was checked
in artifacts/wpf-radius-5/generator for WPF 5/1 and Avalonia/WinUI 6/1.
The Shell consumer switched to preview.12 and passed ten focused WPF cases,
including the default radius without a preview switch and comparison modes.

## Shared WPF ToolCard splitter

On 2026-11-09 the author requested one shared splitter appearance for Shell
and Grid, based on the local Grid reference. The AI agent implemented
Phiale.Design.Style.ToolCard.Splitter for Thumb. GridSplitter can derive
from it. SplitterVisual.Orientation selects Vertical or Horizontal without
product layout logic in Design. Consumers own resizing input and bounds.

The grip and track use existing dynamic theme roles. ToolCard.MinWidth is
200 DIP and ToolCard.Splitter.Width is 12 DIP, chosen by the agent. WPF
0.1.0-preview.14 is built in artifacts/packages for local consumption.
Package-based foundation hosts passed 243 checks each on net8 and net48,
including both splitter orientations and Light, Dark and HighContrast.
Evidence is in artifacts/splitter-wpf-net8 and artifacts/splitter-wpf-net48.

## Shared WPF disclosure chevrons

On 2026-11-09 the author requested a common chevron pattern in Phiale.Design
and its adoption by the WPF Grid in E:/GitRepos/PhialeGis.Library. The AI agent
selected a 28 DIP hit target, a centered 12 DIP vector glyph and a 1.5 DIP
rounded stroke. These dimensions are named ChevronButton, ChevronIcon and
ChevronStroke tokens. Existing theme roles provide the colors.

Import Themes/Palette.xaml and Themes/Chevrons.xaml. ToolCards.xaml also
imports this dictionary. Select Phiale.Design.Style.ChevronButton explicitly.
ChevronVisual.Direction accepts Left, Right, Up and Down. The consumer owns
the state, direction, command and accessible name. The template renders a
vector glyph independently of Button.Content and the current font.

```xml
<Button Style="{StaticResource Phiale.Design.Style.ChevronButton}"
        design:ChevronVisual.Direction="Left"
        AutomationProperties.Name="Collapse panel"
        Command="{Binding CollapsePanelCommand}" />
```

Idle background and border are transparent. Hover uses the theme hover fill.
Keyboard focus adds the theme focus ring. HighContrast hover and focus use
the system Highlight/HighlightText pair through existing selection roles.
Disabled glyphs use Disabled.Text. Native Button keyboard and command behavior
remain available.

WPF package 0.1.0-preview.15 is available in the local artifacts/packages feed.
Package-based WPF foundation hosts passed 380 checks each on net8 and net48.
The checks cover four directions, idle, hover, focus, disabled, click delivery
and rendered images at 96, 144 and 192 DPI. These images verify rasterization
at the specified scales. Monitor transitions were not exercised in this task.
Evidence is in artifacts/chevrons-net8.0-windows10.0.19041.0 and
artifacts/chevrons-net48.

## WPF checkbox rows

On 2026-11-09 the author requested a checkbox list for showing and hiding Shell
Workspace panels. The AI agent added the reusable Phiale.Design.Style.CheckBox
in Themes/CheckBoxes.xaml, with a 16 DIP box, a 3 DIP box radius, a vector tick,
transparent idle row, hover fill and keyboard focus ring. Colors use existing
theme roles. HighContrast uses the selection fill/text pair. The consumer
supplies content, checked state and actions. Import Palette.xaml and
CheckBoxes.xaml and select this style explicitly.

The local WPF preview.16 package includes this style. Package-based foundation
hosts passed 414 checks each on net8 and net48, including checkbox states in
Light, Dark and HighContrast. Evidence is in artifacts/checkbox-net8.0-windows10.0.19041.0
and artifacts/checkbox-net48. This task verified WPF.

## Compact Workspace metrics

On 2026-11-09 the author requested compact WPF Workspace composition with
4–6 DIP gaps, uniform drawer headers and 5 DIP corners. Local WPF preview.17
adds Size.Workspace.Gap = 6 and Thickness.Workspace.Content.Padding = 8.
Shell Styles consumes these tokens and Radius.Medium for its Workspace
composition. Existing ToolCard surface/header radii retain their values for
other consumers. This update adds metrics without changing the palette.

## Adjoining Workspace metrics

On 2026-11-09 the author approved adjoining Push panels with independent layout
and hit-region metrics. Local WPF preview.18 adds Size.Workspace.DockedGap (0),
SplitterHitWidth (8), DrawerInset (6), TabGap (4) and HeaderGap (0).
SplitterVisual.IsIntegrated selects a continuous boundary track and a 6 DIP
handle. The original ToolCard splitter remains the default. The consumer owns
hit rectangles, resize behavior and layout. Package-based WPF foundation hosts
passed 423 checks each on net8 and net48. Evidence is in artifacts/adjoining-net8.0-windows10.0.19041.0
and artifacts/adjoining-net48. Other platforms were not tested in this task.


## Workspace surface

On 2026-11-09 the author requested visible separation between the empty Shell
and an active Workspace, with its header belonging to the same visual unit.
The AI agent added `Phiale.Design.Brush.Surface.Workspace` in local WPF
`0.1.0-preview.19`, reusing existing palette primitives: Light `#EAF0F7`,
Dark `#1E232D`. Empty hosts continue to use `Surface.Base`.

The Workspace role covers a continuous work area, its header and docked panes;
it does not replace the fill of input controls. WPF HighContrast maps it to
the dynamic Windows Window color. HighContrastBlack provides the separate
black fallback as before. Shell-specific grouping and elevation remain in
Shell Styles. No Avalonia or WinUI implementation was changed or built here.

Package-based native WPF Foundation verification passed 426 checks on net8
and net48, including the distinct work-area fill and system HighContrast
mapping. Evidence: `artifacts/workspace-surface-net8` and
`artifacts/workspace-surface-net48`. No package was published.
