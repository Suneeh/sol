using Godot;
using Sol.Autoloads;
using Sol.Core.Types;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		GD.Print("Sol - Engine initialized");
		GameManager.Instance?.SetMode(GameMode.Overworld);
	}
}
