using Sol.Core.Types;

namespace Sol.Creatures;

/// <summary>
/// Hardcoded starter creature and move definitions for development.
/// Move these to .tres resource files via the Godot editor once you have sprites.
/// </summary>
public static class StarterCreatures
{
	// ── Moves ──────────────────────────────────────────────

	public static MoveData Tackle => new()
	{
		MoveName = "Tackle", MoveType = CreatureType.Neutral,
		Category = MoveCategory.Physical, Power = 35, Accuracy = 100, MaxPp = 30,
		Description = "A basic physical attack."
	};

	public static MoveData Ember => new()
	{
		MoveName = "Ember", MoveType = CreatureType.Fire,
		Category = MoveCategory.Special, Power = 40, Accuracy = 100, MaxPp = 25,
		Description = "A small flame attack."
	};

	public static MoveData VineWhip => new()
	{
		MoveName = "Vine Whip", MoveType = CreatureType.Nature,
		Category = MoveCategory.Physical, Power = 40, Accuracy = 100, MaxPp = 25,
		Description = "Strikes with thin vines."
	};

	public static MoveData WaterGun => new()
	{
		MoveName = "Water Gun", MoveType = CreatureType.Water,
		Category = MoveCategory.Special, Power = 40, Accuracy = 100, MaxPp = 25,
		Description = "A blast of water."
	};

	public static MoveData SandThrow => new()
	{
		MoveName = "Sand Throw", MoveType = CreatureType.Sand,
		Category = MoveCategory.Physical, Power = 40, Accuracy = 95, MaxPp = 25,
		Description = "Throws sand at the target."
	};

	public static MoveData Spark => new()
	{
		MoveName = "Spark", MoveType = CreatureType.Electricity,
		Category = MoveCategory.Special, Power = 40, Accuracy = 100, MaxPp = 25,
		Description = "An electric spark."
	};

	public static MoveData Gust => new()
	{
		MoveName = "Gust", MoveType = CreatureType.Wind,
		Category = MoveCategory.Special, Power = 40, Accuracy = 100, MaxPp = 25,
		Description = "A gust of wind."
	};

	// ── Starters (one per type triangle) ──────────────────

	public static CreatureData Flamix => new()
	{
		Id = 1, CreatureName = "Flamix", PrimaryType = CreatureType.Fire,
		Description = "A small flame creature with a fiery tail.",
		BaseHp = 42, BaseAttack = 50, BaseDefense = 38, BaseSpeed = 48,
		LearnableMoves = [Tackle, Ember],
		LearnLevels = [1, 1],
	};

	public static CreatureData Leafyn => new()
	{
		Id = 2, CreatureName = "Leafyn", PrimaryType = CreatureType.Nature,
		Description = "A gentle plant creature covered in leaves.",
		BaseHp = 48, BaseAttack = 42, BaseDefense = 48, BaseSpeed = 38,
		LearnableMoves = [Tackle, VineWhip],
		LearnLevels = [1, 1],
	};

	public static CreatureData Aquara => new()
	{
		Id = 3, CreatureName = "Aquara", PrimaryType = CreatureType.Water,
		Description = "A playful water creature that loves rain.",
		BaseHp = 46, BaseAttack = 44, BaseDefense = 44, BaseSpeed = 44,
		LearnableMoves = [Tackle, WaterGun],
		LearnLevels = [1, 1],
	};

	// ── Wild creatures ────────────────────────────────────

	public static CreatureData Dustling => new()
	{
		Id = 4, CreatureName = "Dustling", PrimaryType = CreatureType.Sand,
		Description = "A tiny sand dweller found near deserts.",
		BaseHp = 38, BaseAttack = 45, BaseDefense = 50, BaseSpeed = 35,
		LearnableMoves = [Tackle, SandThrow],
		LearnLevels = [1, 3],
	};

	public static CreatureData Zapplet => new()
	{
		Id = 5, CreatureName = "Zapplet", PrimaryType = CreatureType.Electricity,
		Description = "A buzzing creature that sparks when excited.",
		BaseHp = 36, BaseAttack = 40, BaseDefense = 35, BaseSpeed = 55,
		LearnableMoves = [Tackle, Spark],
		LearnLevels = [1, 3],
	};

	public static CreatureData Breezle => new()
	{
		Id = 6, CreatureName = "Breezle", PrimaryType = CreatureType.Wind,
		Description = "A swift creature that rides the wind.",
		BaseHp = 38, BaseAttack = 38, BaseDefense = 36, BaseSpeed = 52,
		LearnableMoves = [Tackle, Gust],
		LearnLevels = [1, 3],
	};

	/// <summary>
	/// All defined creatures for quick lookup.
	/// </summary>
	public static CreatureData[] All => [Flamix, Leafyn, Aquara, Dustling, Zapplet, Breezle];

	/// <summary>
	/// The 3 starter choices.
	/// </summary>
	public static CreatureData[] Starters => [Flamix, Leafyn, Aquara];
}
