namespace Sol.Battle.State;

public enum BattleState
{
	Start,
	PlayerTurn,
	EnemyTurn,
	ExecuteAction,
	TurnEnd,
	BattleEnd
}

public enum BattleOutcome
{
	None,
	PlayerWin,
	PlayerLose,
	PlayerFled
}
