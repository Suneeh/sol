using System;
using Godot;
using Sol.Core.Events;

namespace Sol.Autoloads;

public partial class EventBus : Node
{
	public static EventBus Instance { get; private set; } = null!;

	// Battle
	public event Action<BattleStartedEvent>? BattleStarted;
	public event Action<BattleEndedEvent>? BattleEnded;
	public event Action<TurnCompletedEvent>? TurnCompleted;

	// Creatures
	public event Action<CreatureRecruitedEvent>? CreatureRecruited;
	public event Action<PartyChangedEvent>? PartyChanged;
	public event Action<CreatureFaintedEvent>? CreatureFainted;

	// Overworld
	public event Action<MapChangedEvent>? MapChanged;
	public event Action<EncounterTriggeredEvent>? EncounterTriggered;

	// Game
	public event Action<GameSavedEvent>? GameSaved;
	public event Action<GameLoadedEvent>? GameLoaded;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("EventBus: Ready");
	}

	public void EmitBattleStarted(string mapId, int encounterId)
		=> BattleStarted?.Invoke(new BattleStartedEvent(mapId, encounterId));

	public void EmitBattleEnded(bool playerWon)
		=> BattleEnded?.Invoke(new BattleEndedEvent(playerWon));

	public void EmitTurnCompleted(int turnNumber)
		=> TurnCompleted?.Invoke(new TurnCompletedEvent(turnNumber));

	public void EmitCreatureRecruited(int creatureId)
		=> CreatureRecruited?.Invoke(new CreatureRecruitedEvent(creatureId));

	public void EmitPartyChanged()
		=> PartyChanged?.Invoke(new PartyChangedEvent());

	public void EmitCreatureFainted(int creatureId)
		=> CreatureFainted?.Invoke(new CreatureFaintedEvent(creatureId));

	public void EmitMapChanged(string newMapId, string? fromMapId = null)
		=> MapChanged?.Invoke(new MapChangedEvent(newMapId, fromMapId));

	public void EmitEncounterTriggered(string mapId, int encounterId)
		=> EncounterTriggered?.Invoke(new EncounterTriggeredEvent(mapId, encounterId));

	public void EmitGameSaved()
		=> GameSaved?.Invoke(new GameSavedEvent());

	public void EmitGameLoaded()
		=> GameLoaded?.Invoke(new GameLoadedEvent());
}
