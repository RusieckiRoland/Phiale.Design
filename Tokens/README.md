# Visual roles

[design.candidate.json](design.candidate.json) is the source of the palette
and metrics. It contains 69 primitive colors, 58 shared roles, and 10 private
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
