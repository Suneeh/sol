using Godot;

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

	private void MoveToTile(Vector2 direction)
	{
		_moving = true;

		var target = Position + direction * TileSize;
		var tween = CreateTween();
		tween.TweenProperty(this, "position", target, 1.0 / WalkSpeed)
			.SetTrans(Tween.TransitionType.Linear);
		tween.Finished += () => _moving = false;
	}

	private void UpdateRayDirection()
	{
		_ray.TargetPosition = _facingDirection * TileSize;
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
