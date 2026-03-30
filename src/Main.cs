using Godot;
using Sol.Autoloads;
using Sol.Core.Types;
using Sol.Creatures;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		GD.Print("Sol - Engine initialized");

		// Give the player a starter creature
		var starter = new CreatureInstance(StarterCreatures.Flamix, 5);
		PartyManager.Instance.AddCreature(starter);

		// Link player to party
		var player = GetNode<Sol.Player.Player>("Player");
		player.ActiveCreature = PartyManager.Instance.ActiveCreature;

		GameManager.Instance?.SetMode(GameMode.Overworld);
	}
}
