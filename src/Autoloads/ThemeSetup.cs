using Godot;

namespace Sol.Autoloads;

/// <summary>
/// Sets up a project-wide theme with crisp pixel-art-friendly font rendering.
/// Registered as an Autoload to run before any UI is created.
/// </summary>
public partial class ThemeSetup : Node
{
	public override void _Ready()
	{
		var font = new SystemFont();
		font.FontNames = ["monospace"];
		font.Antialiasing = TextServer.FontAntialiasing.None;
		font.Hinting = TextServer.Hinting.None;
		font.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;

		var theme = new Theme();
		theme.DefaultFont = font;
		theme.DefaultFontSize = 14;

		// Apply to the entire scene tree
		GetTree().Root.Theme = theme;

		GD.Print("ThemeSetup: Pixel font applied");
	}
}
