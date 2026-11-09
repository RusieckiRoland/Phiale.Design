# Visual roles

[design.candidate.json](design.candidate.json) is the source of the palette
and metrics. It contains primitive colors, shared roles, and private
metrics for the base control styles. [controls.contract.json](controls.contract.json)
defines the explicit style keys. The generator in `tools/Phiale.Design.Generator`
produces WPF, Avalonia, and WinUI resources when the libraries are built.

Shared brushes use the `Phiale.Design.Brush.` prefix. Roles describe purpose:
`Surface.*` for surfaces, `Text.*` for text, `Border.*` for borders,
`Accent.*` for actions, `Summary.*` for summaries, `Disabled.*` for disabled
states, and `Scroll.*` for scrolling. `Shadow` defines the shadow color;
renderers must account for both color alpha and brush opacity. Shadows are
invisible in HighContrast.

Each brush has Light, Dark, and HighContrast variants. WPF and WinUI use
system colors in high contrast; Avalonia uses a black background, white text,
and yellow accents. WPF and WinUI also provide an explicit HighContrastBlack
variant. Primitives and `Phiale.Design.Internal.*` keys are implementation
details. The preview public API may change.

The palette does not imply WCAG compliance. Known contrast limitations:
white text on `Accent.Light`: 3.87:1; `Border.Default.Light` on white: 1.42:1;
`Accent` on `Surface.Active`: 3.55:1 in Light and 3.20:1 in Dark.
Custom role mappings require contrast evaluation in their intended context.

`fontFamily` roles may specify `platformValues` keyed by Wpf/Avalonia/WinUI.
The shared `value` remains the default. Body font uses packaged Inter for
Avalonia (`avares://Avalonia.Fonts.Inter/Assets#Inter`), Segoe UI for WPF/WinUI.
The generator validates platform names and nonempty font values. This avoids
relying on the system's substitute for a Windows-only font on Linux.

## Tool-card roles

The author requested promotion of the existing Grid tool-card appearance on
2026-11-09. Public ToolCard metrics in design.candidate.json define the surface,
header and toggle radii, header/action sizes, typography and spacing. WPF styles
are listed in platformStyles.Wpf in controls.contract.json and loaded from
Themes/ToolCards.xaml. Other platform style implementations have their own scope.

`Surface.Workspace` identifies the continuous active work area, including its
header and docked panes. It differs from the empty host's `Surface.Base` in
Light and Dark. HighContrast follows Window, so clients distinguish the group
with a system-colored outline rather than an invented background color.
