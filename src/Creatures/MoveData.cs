using Godot;
using Sol.Core.Types;

namespace Sol.Creatures;

/// <summary>
/// Static definition of a move/attack. Create as .tres files in resources/moves/.
/// </summary>
[GlobalClass]
public partial class MoveData : Resource
{
	[Export] public string MoveName { get; set; } = "";
	[Export] public CreatureType MoveType { get; set; } = CreatureType.Neutral;
	[Export] public MoveCategory Category { get; set; } = MoveCategory.Physical;
	[Export] public int Power { get; set; } = 40;
	[Export] public int Accuracy { get; set; } = 100;
	[Export] public int MaxPp { get; set; } = 20;
	[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
}
