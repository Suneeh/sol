using Godot;
using Sol.Creatures;

namespace Sol.Overworld;

/// <summary>
/// An NPC that heals all creatures in the player's party when spoken to.
/// </summary>
public partial class HealerNpc : Npc
{
	public override void _Ready()
	{
		base._Ready();
	}

	public new string[] GetDialogue()
	{
		return [
			"Oh dear, your creatures\nlook tired!",
			"Let me heal them up\nfor you...",
			"There you go!\nAll better now!"
		];
	}

	public void Heal()
	{
		PartyManager.Instance?.HealAll();
		GD.Print("HealerNpc: Healed all creatures");
	}
}
