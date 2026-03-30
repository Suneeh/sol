using Godot;
using Sol.Autoloads;
using Sol.Core;
using Sol.Creatures;
using Sol.UI;

namespace Sol.Overworld;

/// <summary>
/// A hidden creature NPC. When the player interacts, it shows dialogue
/// then asks to join. On accept, adds to party and disappears from the map.
/// </summary>
public partial class CreatureNpc : Npc
{
	[Export] public int CreatureId { get; set; }
	[Export] public int CreatureLevel { get; set; } = 5;

	private CreatureData? _creatureData;
	private bool _recruited;

	public override void _Ready()
	{
		base._Ready();

		// Find creature data from starters
		foreach (var c in StarterCreatures.All)
		{
			if (c.Id == CreatureId)
			{
				_creatureData = c;
				break;
			}
		}

		if (_creatureData is not null)
		{
			NpcName = _creatureData.CreatureName;
			// Set sprite color based on type
			SpriteColor = TypeToColor(_creatureData.PrimaryType);
			// Regenerate sprite with correct color
			var sprite = GetNode<Sprite2D>("Sprite");
			var img = Image.CreateEmpty(28, 28, false, Image.Format.Rgba8);
			img.Fill(SpriteColor);
			sprite.Texture = ImageTexture.CreateFromImage(img);
		}
	}

	/// <summary>
	/// Override dialogue to show recruitment flow.
	/// Called by Player.TryInteract() which opens the DialogueBox.
	/// We intercept by providing special dialogue and listening for completion.
	/// </summary>
	public new string[] GetDialogue()
	{
		if (_recruited || _creatureData is null)
			return ["..."];

		return [
			$"*You found a wild {_creatureData.CreatureName}!*",
			_creatureData.Description,
			$"{_creatureData.CreatureName} wants to join you!"
		];
	}

	/// <summary>
	/// Called after dialogue finishes. Recruits the creature.
	/// </summary>
	public void Recruit()
	{
		if (_recruited || _creatureData is null) return;
		_recruited = true;

		var instance = new CreatureInstance(_creatureData, CreatureLevel);
		var inParty = PartyManager.Instance.AddCreature(instance);

		EventBus.Instance?.EmitCreatureRecruited(_creatureData.Id);

		// Fade out and remove from map
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	private static Color TypeToColor(Core.Types.CreatureType type) => type switch
	{
		Core.Types.CreatureType.Fire => new Color(0.9f, 0.35f, 0.2f),
		Core.Types.CreatureType.Nature => new Color(0.2f, 0.75f, 0.3f),
		Core.Types.CreatureType.Water => new Color(0.2f, 0.4f, 0.9f),
		Core.Types.CreatureType.Sand => new Color(0.85f, 0.7f, 0.35f),
		Core.Types.CreatureType.Electricity => new Color(0.95f, 0.85f, 0.2f),
		Core.Types.CreatureType.Wind => new Color(0.65f, 0.85f, 0.95f),
		_ => new Color(0.6f, 0.6f, 0.6f),
	};
}
