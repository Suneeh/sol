using Godot;
using Sol.Autoloads;
using Sol.Battle;
using Sol.Core.Types;
using Sol.Creatures;
using Sol.Overworld;
using Sol.UI;

namespace Sol.Player;

/// <summary>
/// Grid-based player controller. Uses Area2D + RayCast2D for collision,
/// Tween for smooth tile-to-tile movement.
/// </summary>
public partial class Player : Area2D
{
	public const int TileSize = 16;

	[Export] public float WalkSpeed { get; set; } = 4f;

	private RayCast2D _ray = null!;
	private Sprite2D _sprite = null!;
	private bool _moving;
	private Vector2 _facingDirection = Vector2.Down;

	/// <summary>The player's active creature. Set this when the game starts.</summary>
	public CreatureInstance? ActiveCreature { get; set; }

	private const float EncounterChance = 0.15f;

	public override void _Ready()
	{
		_ray = GetNode<RayCast2D>("RayCast2D");
		_sprite = GetNode<Sprite2D>("Sprite");

		// Load placeholder texture if no texture is assigned
		if (_sprite.Texture is null)
		{
			var path = "res://assets/sprites/characters/player_placeholder.png";
			_sprite.Texture = GD.Load<Texture2D>(path);
		}

		// Snap to grid on spawn
		Position = Position.Snapped(Vector2.One * TileSize);
		Position += Vector2.One * (TileSize / 2);
	}

	public override void _Process(double delta)
	{
		if (_moving) return;
		if (GameManager.Instance?.InputEnabled != true) return;

		var direction = GetInputDirection();
		if (direction == Vector2.Zero) return;

		// Update facing direction
		_facingDirection = direction;
		UpdateRayDirection();

		// Check collision via raycast
		_ray.ForceRaycastUpdate();
		if (_ray.IsColliding()) return;

		// Move to the next tile
		MoveToTile(direction);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_moving) return;
		if (GameManager.Instance?.InputEnabled != true) return;

		if (@event.IsActionPressed("interact"))
		{
			TryInteract();
			GetViewport().SetInputAsHandled();
		}
	}

	private void TryInteract()
	{
		// Cast a ray in the facing direction to find an NPC
		_ray.TargetPosition = _facingDirection * TileSize;
		_ray.ForceRaycastUpdate();

		if (!_ray.IsColliding()) return;

		// Walk up the collider's parent tree to find an Npc node
		var collider = _ray.GetCollider();
		if (collider is Node node)
		{
			var npc = FindNpcParent(node);
			if (npc is not null)
			{
				var lines = npc.GetDialogue();
				DialogueBox.Instance?.Open(npc.NpcName, lines);
			}
		}
	}

	private static Npc? FindNpcParent(Node node)
	{
		var current = node;
		while (current is not null)
		{
			if (current is Npc npc) return npc;
			current = current.GetParent();
		}
		return null;
	}

	private void MoveToTile(Vector2 direction)
	{
		_moving = true;

		var target = Position + direction * TileSize;
		var tween = CreateTween();
		tween.TweenProperty(this, "position", target, 1.0 / WalkSpeed)
			.SetTrans(Tween.TransitionType.Linear);
		tween.Finished += () =>
		{
			_moving = false;
			CheckForEncounter();
		};
	}

	private void UpdateRayDirection()
	{
		_ray.TargetPosition = _facingDirection * TileSize;
	}

	private void CheckForEncounter()
	{
		if (ActiveCreature is null || ActiveCreature.IsFainted) return;

		// Only trigger encounters on grass tiles
		var tileX = (int)(Position.X / TileSize);
		var tileY = (int)(Position.Y / TileSize);

		if (tileX < 0 || tileX >= TestMap.MapWidth || tileY < 0 || tileY >= TestMap.MapHeight)
			return;

		if (TestMap.Layout[tileY, tileX] != TestMap.TileGrass) return;

		if (GD.Randf() > EncounterChance) return;

		StartBattle();
	}

	private void StartBattle()
	{
		if (ActiveCreature is null) return;

		// Pick a random wild creature at a similar level
		var wildData = StarterCreatures.All[GD.RandRange(0, StarterCreatures.All.Length - 1)];
		var wildLevel = Mathf.Max(1, ActiveCreature.Level + GD.RandRange(-2, 1));
		var wildCreature = new CreatureInstance(wildData, wildLevel);

		GameManager.Instance?.SetMode(GameMode.Battle);
		EventBus.Instance?.EmitBattleStarted("test_map", 0);

		// Create battle scene as overlay
		var battleScene = new BattleScene();
		battleScene.Initialize(ActiveCreature, wildCreature);
		GetTree().Root.AddChild(battleScene);
	}

	private static Vector2 GetInputDirection()
	{
		// Priority order — no diagonals allowed
		if (Input.IsActionPressed("move_up")) return Vector2.Up;
		if (Input.IsActionPressed("move_down")) return Vector2.Down;
		if (Input.IsActionPressed("move_left")) return Vector2.Left;
		if (Input.IsActionPressed("move_right")) return Vector2.Right;
		return Vector2.Zero;
	}
}
