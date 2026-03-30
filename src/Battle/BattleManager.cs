using System;
using Godot;
using Sol.Battle.State;
using Sol.Core.Types;
using Sol.Creatures;

namespace Sol.Battle;

/// <summary>
/// Pure logic for a turn-based battle. No UI references.
/// Emits events that the BattleScene UI subscribes to.
/// </summary>
public class BattleManager
{
	public CreatureInstance PlayerCreature { get; }
	public CreatureInstance EnemyCreature { get; }
	public BattleState CurrentState { get; private set; }
	public BattleOutcome Outcome { get; private set; }
	public int TurnNumber { get; private set; }

	// Events — UI subscribes to these
	public event Action? OnBattleStarted;
	public event Action<string>? OnMessage;
	public event Action<CreatureInstance, DamageResult, int>? OnDamageDealt; // target, result, actualHpChange
	public event Action<CreatureInstance, DamageResult, int>? OnHealing; // target, result, healAmount
	public event Action? OnPlayerTurnStart;
	public event Action<BattleOutcome>? OnBattleEnd;
	public event Action? OnStateChanged;

	private MoveInstance? _playerMove;
	private MoveInstance? _enemyMove;

	public BattleManager(CreatureInstance player, CreatureInstance enemy)
	{
		PlayerCreature = player;
		EnemyCreature = enemy;
		CurrentState = BattleState.Start;
	}

	public void Start()
	{
		TurnNumber = 0;
		Outcome = BattleOutcome.None;
		CurrentState = BattleState.Start;
		OnBattleStarted?.Invoke();
		OnMessage?.Invoke($"A wild {EnemyCreature.Nickname} appeared!");
		BeginPlayerTurn();
	}

	public void SelectMove(int moveIndex)
	{
		if (CurrentState != BattleState.PlayerTurn) return;
		if (moveIndex < 0 || moveIndex >= PlayerCreature.Moves.Length) return;

		var move = PlayerCreature.Moves[moveIndex];
		if (!move.HasPp)
		{
			OnMessage?.Invoke("No PP left for that move!");
			return;
		}

		_playerMove = move;
		PickEnemyMove();
		ExecuteTurn();
	}

	public void SelectFlee()
	{
		if (CurrentState != BattleState.PlayerTurn) return;

		// Simple flee: 75% chance
		if (GD.Randf() < 0.75f)
		{
			OnMessage?.Invoke("Got away safely!");
			Outcome = BattleOutcome.PlayerFled;
			EndBattle();
		}
		else
		{
			OnMessage?.Invoke("Can't escape!");
			PickEnemyMove();
			// Enemy still attacks
			ExecuteAction(EnemyCreature, PlayerCreature, _enemyMove!);
			CheckBattleEnd();
		}
	}

	private void BeginPlayerTurn()
	{
		TurnNumber++;
		CurrentState = BattleState.PlayerTurn;
		OnPlayerTurnStart?.Invoke();
		OnStateChanged?.Invoke();
	}

	private void PickEnemyMove()
	{
		// Simple AI: pick a random move with PP
		var available = 0;
		foreach (var m in EnemyCreature.Moves)
			if (m.HasPp) available++;

		if (available == 0)
		{
			// Struggle equivalent — just tackle with Neutral type
			_enemyMove = new MoveInstance(StarterCreatures.Tackle);
			return;
		}

		while (true)
		{
			var idx = GD.RandRange(0, EnemyCreature.Moves.Length - 1);
			if (EnemyCreature.Moves[idx].HasPp)
			{
				_enemyMove = EnemyCreature.Moves[idx];
				return;
			}
		}
	}

	private void ExecuteTurn()
	{
		CurrentState = BattleState.ExecuteAction;
		OnStateChanged?.Invoke();

		// Speed determines who goes first
		CreatureInstance first, second;
		MoveInstance firstMove, secondMove;

		if (PlayerCreature.Speed >= EnemyCreature.Speed)
		{
			first = PlayerCreature; firstMove = _playerMove!;
			second = EnemyCreature; secondMove = _enemyMove!;
		}
		else
		{
			first = EnemyCreature; firstMove = _enemyMove!;
			second = PlayerCreature; secondMove = _playerMove!;
		}

		// First attack
		ExecuteAction(first, second, firstMove);
		if (CheckBattleEnd()) return;

		// Second attack
		ExecuteAction(second, first, secondMove);
		if (CheckBattleEnd()) return;

		// Turn end
		CurrentState = BattleState.TurnEnd;
		OnStateChanged?.Invoke();
		BeginPlayerTurn();
	}

	private void ExecuteAction(CreatureInstance attacker, CreatureInstance defender, MoveInstance move)
	{
		move.UsePp();
		var result = CreatureInstance.CalculateDamage(attacker, defender, move);

		OnMessage?.Invoke($"{attacker.Nickname} used {move.Data.MoveName}!");

		if (TypeChart.IsImmune(result.TypeMultiplier))
		{
			OnMessage?.Invoke("It had no effect!");
			return;
		}

		var hpChange = defender.ApplyDamage(result);

		if (result.IsHealing)
		{
			OnMessage?.Invoke($"It healed {defender.Nickname} for {-hpChange} HP!");
			OnHealing?.Invoke(defender, result, -hpChange);
		}
		else
		{
			var effectiveness = result.TypeMultiplier switch
			{
				>= 2.0f => " It's super effective!",
				>= 1.25f => " It's strong!",
				<= 0.5f => " It's not very effective...",
				_ => ""
			};
			OnMessage?.Invoke($"Dealt {hpChange} damage!{effectiveness}");
			OnDamageDealt?.Invoke(defender, result, hpChange);
		}
	}

	private bool CheckBattleEnd()
	{
		if (EnemyCreature.IsFainted)
		{
			OnMessage?.Invoke($"{EnemyCreature.Nickname} fainted!");
			Outcome = BattleOutcome.PlayerWin;

			// XP reward
			var xp = EnemyCreature.Level * 10;
			var leveledUp = PlayerCreature.AddXp(xp);
			OnMessage?.Invoke($"Gained {xp} XP!");
			if (leveledUp)
				OnMessage?.Invoke($"{PlayerCreature.Nickname} grew to level {PlayerCreature.Level}!");

			EndBattle();
			return true;
		}

		if (PlayerCreature.IsFainted)
		{
			OnMessage?.Invoke($"{PlayerCreature.Nickname} fainted!");
			Outcome = BattleOutcome.PlayerLose;
			EndBattle();
			return true;
		}

		return false;
	}

	private void EndBattle()
	{
		CurrentState = BattleState.BattleEnd;
		OnBattleEnd?.Invoke(Outcome);
		OnStateChanged?.Invoke();
	}
}
