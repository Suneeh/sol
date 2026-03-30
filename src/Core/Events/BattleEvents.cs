namespace Sol.Core.Events;

public record BattleStartedEvent(string MapId, int EncounterId);
public record BattleEndedEvent(bool PlayerWon);
public record TurnCompletedEvent(int TurnNumber);
