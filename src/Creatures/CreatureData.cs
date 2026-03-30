using Godot;
using Sol.Core.Types;

namespace Sol.Creatures;

/// <summary>
/// Static definition of a creature species. Create as .tres files in resources/creatures/.
/// This is the "Pokedex entry" — immutable base data.
/// </summary>
[GlobalClass]
public partial class CreatureData : Resource
{
	[Export] public int Id { get; set; }
	[Export] public string CreatureName { get; set; } = "";
	[Export] public CreatureType PrimaryType { get; set; } = CreatureType.Neutral;
	[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

	// Base stats
	[ExportGroup("Base Stats")]
	[Export] public int BaseHp { get; set; } = 40;
	[Export] public int BaseAttack { get; set; } = 40;
	[Export] public int BaseDefense { get; set; } = 40;
	[Export] public int BaseSpeed { get; set; } = 40;

	// Moves this creature can learn (ordered by level)
	[ExportGroup("Moves")]
	[Export] public MoveData[] LearnableMoves { get; set; } = [];
	[Export] public int[] LearnLevels { get; set; } = [];

	// Sprites
	[ExportGroup("Sprites")]
	[Export] public Texture2D? FrontSprite { get; set; }
	[Export] public Texture2D? BackSprite { get; set; }
	[Export] public Texture2D? OverworldSprite { get; set; }
}
