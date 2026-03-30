using Godot;
using Sol.Core.Types;

namespace Sol.Autoloads;

/// <summary>
/// Tracks the current game mode. Thin on purpose — does NOT own
/// party, map, or progression state (those belong in dedicated services).
/// </summary>
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; } = null!;

	public GameMode CurrentMode { get; private set; } = GameMode.TitleScreen;

	/// <summary>
	/// Whether gameplay input should be processed (false during menus, dialogue, transitions).
	/// </summary>
	public bool InputEnabled => CurrentMode is GameMode.Overworld;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("GameManager: Ready");
	}

	public void SetMode(GameMode mode)
	{
		var previous = CurrentMode;
		CurrentMode = mode;
		GD.Print($"GameManager: {previous} -> {mode}");
	}
}
