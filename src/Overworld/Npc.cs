using Godot;

namespace Sol.Overworld;

/// <summary>
/// Base NPC that the player can interact with.
/// Place as a child of a map scene. Set DialogueLines in the inspector.
/// </summary>
public partial class Npc : Area2D
{
	[Export] public string NpcName { get; set; } = "NPC";
	[Export(PropertyHint.MultilineText)] public string[] DialogueLines { get; set; } = ["Hello!"];
	[Export] public Color SpriteColor { get; set; } = new(0.9f, 0.5f, 0.2f);

	private Sprite2D _sprite = null!;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite");

		// Placeholder: tint the sprite if no texture
		if (_sprite.Texture is null)
		{
			var img = Image.CreateEmpty(14, 14, false, Image.Format.Rgba8);
			img.Fill(SpriteColor);
			_sprite.Texture = ImageTexture.CreateFromImage(img);
		}

		CollisionLayer = 0;
		CollisionMask = 0;
	}

	/// <summary>
	/// Called by the player when they press interact while facing this NPC.
	/// </summary>
	public string[] GetDialogue() => DialogueLines;
}
