using System.Collections.Generic;
using Godot;
using Sol.Autoloads;

namespace Sol.Creatures;

/// <summary>
/// Manages the player's active party (max 3) and storage box.
/// </summary>
public partial class PartyManager : Node
{
	public static PartyManager Instance { get; private set; } = null!;

	public const int MaxPartySize = 3;

	private readonly List<CreatureInstance> _party = [];
	private readonly List<CreatureInstance> _storage = [];

	public IReadOnlyList<CreatureInstance> Party => _party;
	public IReadOnlyList<CreatureInstance> Storage => _storage;
	public int PartyCount => _party.Count;
	public bool PartyFull => _party.Count >= MaxPartySize;

	/// <summary>First non-fainted creature in the party, or null.</summary>
	public CreatureInstance? ActiveCreature
	{
		get
		{
			foreach (var c in _party)
				if (!c.IsFainted) return c;
			return _party.Count > 0 ? _party[0] : null;
		}
	}

	public override void _Ready()
	{
		Instance = this;
		GD.Print("PartyManager: Ready");
	}

	/// <summary>
	/// Add a creature to the party. Goes to storage if party is full.
	/// Returns true if added to party, false if sent to storage.
	/// </summary>
	public bool AddCreature(CreatureInstance creature)
	{
		if (!PartyFull)
		{
			_party.Add(creature);
			GD.Print($"PartyManager: {creature.Nickname} joined the party! ({_party.Count}/{MaxPartySize})");
			EventBus.Instance?.EmitPartyChanged();
			return true;
		}

		_storage.Add(creature);
		GD.Print($"PartyManager: Party full — {creature.Nickname} sent to storage.");
		EventBus.Instance?.EmitPartyChanged();
		return false;
	}

	/// <summary>
	/// Swap a party creature with one in storage.
	/// </summary>
	public void SwapWithStorage(int partyIndex, int storageIndex)
	{
		if (partyIndex < 0 || partyIndex >= _party.Count) return;
		if (storageIndex < 0 || storageIndex >= _storage.Count) return;

		(_party[partyIndex], _storage[storageIndex]) = (_storage[storageIndex], _party[partyIndex]);
		GD.Print($"PartyManager: Swapped {_party[partyIndex].Nickname} into party");
		EventBus.Instance?.EmitPartyChanged();
	}

	/// <summary>
	/// Heal all creatures in the party.
	/// </summary>
	public void HealAll()
	{
		foreach (var c in _party)
			c.FullHeal();
		GD.Print("PartyManager: All creatures healed");
	}

	/// <summary>
	/// Total number of creatures recruited (party + storage).
	/// </summary>
	public int TotalRecruited => _party.Count + _storage.Count;
}
