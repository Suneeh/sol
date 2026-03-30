namespace Sol.Core.Events;

public record MapChangedEvent(string NewMapId, string? FromMapId);
public record EncounterTriggeredEvent(string MapId, int EncounterId);
