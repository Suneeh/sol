using Godot;

namespace Sol.Creatures;

/// <summary>
/// Holds all creature species data. Load a single .tres instance at startup.
/// </summary>
[GlobalClass]
public partial class CreatureDatabase : Resource
{
	[Export] public CreatureData[] Creatures { get; set; } = [];

	public CreatureData? GetById(int id)
	{
		foreach (var c in Creatures)
			if (c.Id == id) return c;
		return null;
	}

	public CreatureData? GetByName(string name)
	{
		foreach (var c in Creatures)
			if (c.CreatureName == name) return c;
		return null;
	}
}
