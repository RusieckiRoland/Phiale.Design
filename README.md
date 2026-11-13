# Phiale.Design

A shared visual foundation for WPF, Avalonia, and WinUI 3: color roles,
metrics, typography, and explicitly selected styles for basic controls.
The libraries are independent of the applications and products that use them.

| Package | Version | Platform |
| --- | --- | --- |
| Phiale.Design.Wpf | 0.1.0-preview.47 | net48, net8.0-windows10.0.19041.0 |
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


## WPF ComboBox and profile disclosure glyphs

On 2026-11-10 the author requested matching vector chevrons for WPF ComboBoxes
and the Shell user panel, with Down for a closed list, Up for an open list
and Right for collapsed hierarchical navigation. The AI agent added
Themes/DisclosureGlyphs.xaml, imported by Controls.xaml. Shared Geometry.Chevron
resources and Style.ChevronGlyph render a centered 14 DIP glyph with the existing
1.5 DIP ChevronStroke token and a consumer-supplied semantic foreground.
The ComboBox reserves a 24 DIP arrow column and 6 DIP text gap, including its
editable variant. Existing theme colors, focus border and list styles remain.
This WPF package change is versioned as 0.1.0-preview.20 for local consumption.
Verification for this change used the local preview.20 NuGet package. The native
WPF foundation hosts passed 426 checks on net8 and net48. Evidence is under
artifacts/combobox-chevrons-*. The build reported two existing nullable warnings
in tests/Foundation/Wpf/Checks.cs at line 54. Shell consumer tests passed 20 cases,
including initial popup themes, shared glyphs, editing, focus and 96/144 DPI renders.
These render scales do not establish a physical mixed-monitor DPI transfer result.

## Rounded WPF panels and edge splitters

On 2026-11-10 the author supplied an editor screenshot and requested rounded
Shell panels and matching splitters in Phiale.Design. The AI agent chose a
5 DIP radius, 4 DIP panel gap, existing 1 DIP outline and an 8 DIP transparent
resize target with a 2 DIP accent indicator on hover, drag or keyboard focus.
These values are an independent interpretation of the image. No editor source
code was used. Import Themes/Panels.xaml and select Panel.Surface, Panel.Header
and Panel.Splitter under the Phiale.Design.Style prefix. SplitterVisual.Orientation
selects the vertical or horizontal form. High Contrast keeps a visible idle line.
The consumer owns sizing, bounds, panel placement, commands and content.
The WPF package version is 0.1.0-preview.21 for local consumption.
The local preview.21 package passed 426 foundation checks on each WPF target,
net8 and net48. Shell passed 72 selected WPF tests, including rounded-corner
pixels at 96/144 DPI, transparent hit targets, indicator states and opaque
resize command forwarding. Native Shell smoke tests passed four theme variants
on both runtimes. Evidence is in artifacts/rounded-panels-* in Design and
artifacts-temp/rounded-panels-* in Shell. The foundation host build retains
two existing nullable warnings in Checks.cs at line 54.

On 2026-11-10 the author clarified that the supplied splitter reference included
a visible three-dot resize affordance. Version 0.1.0-preview.23 adds three
2 DIP vector dots centered on the shared Panel.Splitter. The grip rotates
for horizontal separators and stays visible at rest. A compact Workspace-colored
backplate separates the dots from the highlighted resize line. The outer hit
target remains transparent, with its existing bounds and native drag behavior.

## WPF ComboBox popup consistency

On 2026-11-10 the author required the existing Design rules to govern both
the closed ComboBox and its popup, without new agent-selected visual values.
The review found that the popup omitted Radius.Medium, its ScrollViewer did
not apply the Design scrollbar style, and four spacing/glyph dimensions had
no named tokens. The author explicitly approved naming the existing 24 DIP
arrow column, 6 DIP text gap, 14 DIP glyph and 10,7 item padding unchanged.
They are now Size.ComboBox.DisclosureWidth, Thickness.ComboBox.Content.Margin,
Size.DisclosureGlyph and Thickness.ComboBox.Item.Padding under Phiale.Design.

The popup uses the existing radius, border thickness, brushes and typography.
Only its content is clipped inside the rounded outline by a geometry converter.
The ScrollViewer retains its WPF scrolling behavior and explicitly consumes
Phiale.Design.Style.ScrollBar with the existing scrollbar-thickness token.
Both templates and their visual states remain in XAML. There is no separate
approved whole-ComboBox hover rule in Design. That gap remains for an author
decision. Existing focus and disabled-state rules remain authoritative.
This change is packaged locally as Phiale.Design.Wpf 0.1.0-preview.29.
Verification of preview.27 passed 426 checks on each native WPF foundation host,
net8 and net48, and 26 targeted Shell WPF tests. Tests include actual Thumb
scrolling, popup token changes, 14 DIP scrollbar width, initial/live themes,
focus, selection pairs and 96/144 DPI corner renders. Standard Shell demo smoke
passed all four theme variants on both runtimes. The popup ScrollViewer now
has an explicit Design template because the system template/minimum imposed
17 DIP despite the Design width token. Evidence is under artifacts/combobox-popup-*
and Shell artifacts-temp/combobox-design-review and combobox-popup-native-*.
Existing nullable warnings remain in the foundation test host Checks.cs line 54.
On 2026-11-10 the author approved the proposed whole-ComboBox hover rule and
required its implementation in Phiale.Design. For enabled WPF ComboBoxes,
hover uses the current theme's Focus.Ring brush for the outline and chevron.
Surface.Base remains the background, Text.Primary remains the text color,
and the existing keyboard focus outline remains. This resolves the earlier
whole-control hover-rule gap. The implementation is in the shared XAML style.
The approved hover change passed 15 targeted WPF tests covering Light, Dark,
HighContrast, pointer entry/exit, keyboard focus, disabled controls and popup
regressions. The standard Shell demo was rebuilt and native profile/theme
smoke tests passed on net8 and net48. Evidence is in Shell artifacts-temp/
combobox-hover and combobox-hover-native-*.
The author subsequently reported that hover was not visually apparent. A real
pointer inspection showed that the profile menu initially focuses its ComboBox,
so its outline already has Focus.Ring before pointer entry. The earlier forced
IsMouseOver test did not cover this visual overlap. The author approved adding
Hover.Fill for the whole control, explicitly including HighContrast after being
informed that ControlLight/WindowText is not a guaranteed contrast pair.
Version 0.1.0-preview.29 keeps Text.Primary and uses Focus.Ring for the outline
and chevron. The popup stays on Surface.Base while its owner is hovered.
Fifteen targeted WPF tests passed, including hover with pre-existing keyboard
focus, pointer exit, disabled controls and independent popup background.
Evidence is in Shell artifacts-temp/combobox-visible-hover/visible-hover-final.trx.

## WPF form composition — 2026-11-12

Task 020 uses `Themes/Forms.xaml` for generic form sections, labels, choices and a fixed action bar. Spacing is named in the token catalogue and follows the inspected references. Colors, fonts, radius and borders reuse existing roles; no palette changes. Local WPF package: `0.1.0-preview.32`; not published by this task.

Task 020 final local package: `0.1.0-preview.33`; the remaining inline form spacing is named in Design tokens without changing its values. Not published.


## WPF localized forms and semantic status roles — 2026-11-12

The author's current request covers application button and FormNavigator colors
with comparison to the author's Figma (54:2). The AI agent added paired
Action.Primary, Status.Complete, Status.Error and Status.Current roles to the
existing token catalogue and WPF palettes. Light fills use #2866A7, #2E9468,
#C74747 and #2C6DB1 respectively, read from that reference. Dark uses existing
primitives; Windows High Contrast uses dynamic system pairs. Existing sidebar
palette, 5 DIP radius and 1 DIP border remain unchanged.

Controls.xaml consumes Action.Primary for primary actions. Forms.xaml owns
Form.ValidationSummary, including wrapping for longer translations. Consumer
XAML references these styles; there is no palette or style creation in C#.
Final local WPF package is 0.1.0-preview.36, built for net48/net8, not published.
Verification and real WPF screenshots are documented in Phiale.Shell
docs/Opinon/Lokalizacja-i-kolory-WPF.md. Actual OS High Contrast and 150%
native screenshot acceptance remain outside the observed 96 DPI session.


### WPF review corrections — 2026-11-12

The author reported unreadable form selection and a square validation outline
over rounded fields. Design WPF preview.38 keeps Form.Choice checked state
on the existing Action.Primary paired colors. Hover does not erase selection.
The shared TextBox style suppresses the default rectangular WPF validation
adorner; the existing Danger.Border trigger colors its own rounded border.
Validation/bindings remain active. Consumer WPF tests passed 35 cases, including
all three choices and validation geometry across Light/Dark/HighContrast.
No palette values, field geometry, other platforms or behavior were changed.
Package is local only. FormNavigator consumes the same Design version.


### WPF review — shared scrolling and disabled actions, 2026-11-12

The author requested rounded, styleable scrolling and clearer disabled buttons.
Design WPF preview.40 provides Style.ScrollViewer with both native scroll axes,
using the shared ScrollBar. Track/thumb use Radius.Medium; the existing 2 DIP
thumb inset is now Internal.Thickness.ScrollBar.Thumb.Padding. The existing
14 DIP thickness and theme brush roles remain dynamic. The application and
Grid consume this shared XAML template. No C# appearance assignments were added.
Disabled.Fill/Text/Border use existing neutral primitives: Light #E9EEF5 /
#4B5563 / #BCC8D9 and Dark #1F2937 / #D3D9E1 / #343C4B. Windows High Contrast
retains Window/GrayText. No new hex colors were introduced into the palette.
Consumer tests verify live theme/resource changes, scrolling commands, radius,
width, inset, disabled hover stability and >=4.5:1 Light/Dark text contrast.
Final local WPF package is 0.1.0-preview.40, not published.

WPF local preview.41 adds explicit PasswordBox and Form.PasswordBox styles using existing input, radius and theme tokens. Author authorized the shared masked input for CEIDG settings on 2026-11-12; AI generated the style from this repository's existing text-input rules. No public package publication.

Local WPF preview.42 adds Button.DangerOnHover, preserving neutral rest and the keyboard focus ring, with existing Danger hover tokens. Requested for the CEIDG delete-key action on 2026-11-12. No palette changes or public publication.

Local WPF preview.43 adds Success.Fill / Success.Border and Notification.Success. Light values follow Figma 81:156; the author approved Dark #17382C with the existing green accent and dynamic Windows High Contrast on 2026-11-12. The AI implemented the roles and XAML style. No public publication.

Local WPF preview.44 also adds Notification.Warning from Figma 81:314, with the author-approved dark variant and system High Contrast. Existing generic Warning.Border remains unchanged. No public publication.


## WPF segmented choices — preview.45

Author direction, 2026-11-12/13: joined choices for client types and reusable
pane tabs. The agent implemented `Themes/SegmentedChoices.xaml`, imported by
`Forms.xaml`. Use `Phiale.Design.Style.SegmentedChoice` on ToggleButton and
`SegmentVisual.Position` (First/Middle/Last or Single). Selection is supplied
by IsChecked; existing commands retain behavior. The shared ButtonBase template
is `Phiale.Design.Template.SegmentedChoice` and accepts SegmentVisual.IsSelected.

Visual appearance remains entirely in XAML. SegmentVisual only declares state
properties. Outer corners follow the approved WPF 5 DIP geometry; joined borders
avoid double separators. Colors reuse existing theme roles, including dynamic
Windows High Contrast resources. No Avalonia or WinUI implementation was changed.
Local package only, not published. WPF net48/net8 pack succeeded. The consuming
Shell's 48 selected WPF form tests passed; native captures of Light, Dark and
High Contrast were inspected (Shell artifacts-temp/segmented-choice and
artifacts-temp/localization/captures/20261113T060947218Z-320a6fb4).


## WPF options icon — 2026-11-13

The author requested replacing the Grid hamburger with horizontal ellipsis and
providing a general options icon in Design. The agent added
`Phiale.Design.Geometry.Options` (three vector circles) and
`Phiale.Design.Style.OptionsGlyph`, imported by Controls.xaml.
The glyph uses the existing disclosure size and theme text roles; no font glyph,
product command, or interaction behavior is embedded in it.
WPF package preview.46 is built locally; no publication is authorized.

## WPF checkbox availability — 2026-11-13

The author requested clearer enabled/disabled options and a shared checkbox pattern.
WPF preview.47 uses Disabled.Fill/Border/Text at full opacity, including checked
disabled controls. Enabled controls retain Border.Strong and Text.Primary; hover,
keyboard focus and selection retain their existing roles. No palette was changed.
The agent implemented the XAML correction and native state checks. Grid consumers
should derive checkbox styles from Phiale.Design.Style.CheckBox, without copying
the template. Avalonia and WinUI styles remain unchanged.


## Pre-push verification — 2026-11-14

The author requested committing and pushing pending changes and approved the
message `feat: unify WPF controls, segmented choices and panel styles`.
The full WPF .NET 8 Foundation runner currently fails at
`Checks.cs` / `Local resource override missing` (OverrideButton background).
This is an open verification failure, not a passing full foundation review.
No additional control behavior was changed during the publication request.
The publication covers repository sources; it does not publish NuGet packages.
