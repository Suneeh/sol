using System.Collections.Generic;

namespace Sol.Core.Types;

/// <summary>
/// Type effectiveness lookup. Returns a float multiplier:
///   2.0  = super effective
///   1.25 = strong
///   1.0  = neutral
///   0.75 = reduced (Neutral deals/receives this)
///   0.0  = immune
///  -0.5  = heals the target (same-type attacks)
/// </summary>
public static class TypeChart
{
	// Chart[attacker][defender] = multiplier
	private static readonly Dictionary<CreatureType, Dictionary<CreatureType, float>> Chart = new()
	{
		[CreatureType.Neutral] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 0.75f },
			{ CreatureType.Nature, 0.75f },
			{ CreatureType.Water, 0.75f },
			{ CreatureType.Sand, 0.75f },
			{ CreatureType.Electricity, 0.75f },
			{ CreatureType.Wind, 0.75f },
		},
		[CreatureType.Fire] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, -0.5f },
			{ CreatureType.Nature, 2.0f },
			{ CreatureType.Water, 0.0f },
			{ CreatureType.Sand, 0.75f },
			{ CreatureType.Electricity, 1.0f },
			{ CreatureType.Wind, 1.25f },
		},
		[CreatureType.Nature] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 0.0f },
			{ CreatureType.Nature, -0.5f },
			{ CreatureType.Water, 2.0f },
			{ CreatureType.Sand, 1.0f },
			{ CreatureType.Electricity, 1.25f },
			{ CreatureType.Wind, 0.75f },
		},
		[CreatureType.Water] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 2.0f },
			{ CreatureType.Nature, 0.0f },
			{ CreatureType.Water, -0.5f },
			{ CreatureType.Sand, 1.25f },
			{ CreatureType.Electricity, 0.75f },
			{ CreatureType.Wind, 1.0f },
		},
		[CreatureType.Sand] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 1.25f },
			{ CreatureType.Nature, 1.0f },
			{ CreatureType.Water, 0.75f },
			{ CreatureType.Sand, -0.5f },
			{ CreatureType.Electricity, 2.0f },
			{ CreatureType.Wind, 0.0f },
		},
		[CreatureType.Electricity] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 1.0f },
			{ CreatureType.Nature, 0.75f },
			{ CreatureType.Water, 1.25f },
			{ CreatureType.Sand, 0.0f },
			{ CreatureType.Electricity, -0.5f },
			{ CreatureType.Wind, 2.0f },
		},
		[CreatureType.Wind] = new()
		{
			{ CreatureType.Neutral, 0.75f },
			{ CreatureType.Fire, 0.75f },
			{ CreatureType.Nature, 1.25f },
			{ CreatureType.Water, 1.0f },
			{ CreatureType.Sand, 2.0f },
			{ CreatureType.Electricity, 0.0f },
			{ CreatureType.Wind, -0.5f },
		},
	};

	/// <summary>
	/// Get the type effectiveness multiplier.
	/// Negative values mean the attack heals the target.
	/// </summary>
	public static float GetMultiplier(CreatureType attackType, CreatureType defenderType)
	{
		if (Chart.TryGetValue(attackType, out var row) &&
			row.TryGetValue(defenderType, out var mult))
			return mult;
		return 1.0f;
	}

	/// <summary>
	/// Whether this multiplier means the attack heals instead of damages.
	/// </summary>
	public static bool IsHealing(float multiplier) => multiplier < 0f;

	/// <summary>
	/// Whether this multiplier means the target is immune.
	/// </summary>
	public static bool IsImmune(float multiplier) => multiplier == 0f;
}
