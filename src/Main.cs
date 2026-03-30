using Godot;
using Sol.Autoloads;
using Sol.Core.Types;
using Sol.Creatures;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		GD.Print("Sol - Engine initialized");

		// Give the player a starter creature for testing
		var player = GetNode<Sol.Player.Player>("Player");
		player.ActiveCreature = new CreatureInstance(StarterCreatures.Flamix, 5);
		GD.Print($"Player has {player.ActiveCreature.Nickname} (Lv{player.ActiveCreature.Level})");

		GameManager.Instance?.SetMode(GameMode.Overworld);
	}
}
