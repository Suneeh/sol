using System;
using Sol.Core.Types;

namespace Sol.Creatures;

/// <summary>
/// A specific creature the player owns. Mutable runtime state.
/// References a CreatureData for base stats.
/// </summary>
public class CreatureInstance
{
	public CreatureData BaseData { get; }
	public string Nickname { get; set; }
	public int Level { get; set; }
	public int CurrentHp { get; set; }
	public int Xp { get; set; }
	public MoveInstance[] Moves { get; set; }

	public int MaxHp => CalcStat(BaseData.BaseHp, isHp: true);
	public int Attack => CalcStat(BaseData.BaseAttack);
	public int Defense => CalcStat(BaseData.BaseDefense);
	public int Speed => CalcStat(BaseData.BaseSpeed);

	public bool IsFainted => CurrentHp <= 0;
	public CreatureType Type => BaseData.PrimaryType;

	public CreatureInstance(CreatureData baseData, int level)
	{
		BaseData = baseData;
		Nickname = baseData.CreatureName;
		Level = level;
		Moves = [];
		CurrentHp = MaxHp;

		// Learn all moves up to this level
		LearnMovesForLevel();
	}

	/// <summary>
	/// Simplified stat formula: stat = (base * level / 50) + 5.
	/// HP gets +level bonus instead.
	/// </summary>
	private int CalcStat(int baseStat, bool isHp = false)
	{
		var stat = baseStat * Level / 50 + 5;
		if (isHp) stat += Level;
		return Math.Max(stat, 1);
	}

	/// <summary>
	/// Calculate damage dealt to a target.
	/// Formula: (ATK / DEF) * Power * typeMultiplier
	/// Returns negative if the attack heals the target.
	/// </summary>
	public static DamageResult CalculateDamage(CreatureInstance attacker, CreatureInstance defender, MoveInstance move)
	{
		var multiplier = TypeChart.GetMultiplier(move.Data.MoveType, defender.Type);

		if (TypeChart.IsImmune(multiplier))
			return new DamageResult(0, multiplier, false);

		var atk = (float)attacker.Attack;
		var def = (float)defender.Defense;
		var power = (float)move.Data.Power;

		var rawDamage = (int)(atk / def * power * Math.Abs(multiplier));
		rawDamage = Math.Max(rawDamage, 1);

		var isHealing = TypeChart.IsHealing(multiplier);
		return new DamageResult(rawDamage, multiplier, isHealing);
	}

	/// <summary>
	/// Apply damage (or healing) to this creature. Returns actual HP change.
	/// </summary>
	public int ApplyDamage(DamageResult result)
	{
		if (result.IsHealing)
		{
			var healed = Math.Min(result.Amount, MaxHp - CurrentHp);
			CurrentHp += healed;
			return -healed; // negative = healed
		}
		else
		{
			var dealt = Math.Min(result.Amount, CurrentHp);
			CurrentHp -= dealt;
			return dealt;
		}
	}

	/// <summary>
	/// Add XP and check for level up. Returns true if leveled up.
	/// </summary>
	public bool AddXp(int amount)
	{
		Xp += amount;
		var threshold = XpForNextLevel();
		if (Xp < threshold) return false;

		Xp -= threshold;
		Level++;
		var oldMaxHp = MaxHp;
		CurrentHp += MaxHp - oldMaxHp; // heal by the HP gain
		LearnMovesForLevel();
		return true;
	}

	/// <summary>
	/// XP needed for the next level. Simple quadratic curve.
	/// </summary>
	public int XpForNextLevel() => Level * Level * 3;

	/// <summary>
	/// Heal to full HP.
	/// </summary>
	public void FullHeal()
	{
		CurrentHp = MaxHp;
		foreach (var move in Moves)
			move.CurrentPp = move.Data.MaxPp;
	}

	private void LearnMovesForLevel()
	{
		if (BaseData.LearnableMoves.Length == 0) return;

		// Collect all moves this creature should know at current level (max 4)
		var learnedCount = 0;
		var learned = new MoveInstance[Math.Min(4, BaseData.LearnableMoves.Length)];

		for (var i = 0; i < BaseData.LearnableMoves.Length && learnedCount < 4; i++)
		{
			var requiredLevel = i < BaseData.LearnLevels.Length ? BaseData.LearnLevels[i] : 1;
			if (requiredLevel <= Level)
			{
				learned[learnedCount++] = new MoveInstance(BaseData.LearnableMoves[i]);
			}
		}

		// Only update if we learned something new
		if (learnedCount > Moves.Length)
			Moves = learned[..learnedCount];
	}
}

/// <summary>
/// Result of a damage calculation.
/// </summary>
public record DamageResult(int Amount, float TypeMultiplier, bool IsHealing);

/// <summary>
/// A move known by a specific creature instance. Tracks current PP.
/// </summary>
public class MoveInstance
{
	public MoveData Data { get; }
	public int CurrentPp { get; set; }

	public MoveInstance(MoveData data)
	{
		Data = data;
		CurrentPp = data.MaxPp;
	}

	public bool HasPp => CurrentPp > 0;

	public void UsePp()
	{
		if (CurrentPp > 0) CurrentPp--;
	}
}
