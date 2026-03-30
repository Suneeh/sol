using Godot;

namespace Sol.Overworld;

/// <summary>
/// Generates a simple test map programmatically using colored rectangles.
/// Replace with a proper TileMapLayer once you have real tilesets in the editor.
/// </summary>
public partial class TestMap : Node2D
{
	private const int TileSize = 16;
	private const int MapWidth = 20;
	private const int MapHeight = 15;

	// Map layout: 0=grass, 1=path, 2=water, 3=tree(blocked), 4=building(blocked)
	private static readonly int[,] Layout = {
		{ 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },
		{ 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 3, 0, 3 },
		{ 3, 0, 0, 3, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 3, 3, 0, 3 },
		{ 3, 0, 0, 3, 3, 0, 0, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 0, 0, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 3, 3, 0, 0, 3 },
		{ 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
		{ 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },
	};

	private static readonly Color GrassColor = new(0.27f, 0.63f, 0.27f);
	private static readonly Color PathColor = new(0.71f, 0.63f, 0.47f);
	private static readonly Color WaterColor = new(0.2f, 0.31f, 0.71f);
	private static readonly Color TreeColor = new(0.12f, 0.47f, 0.16f);
	private static readonly Color TrunkColor = new(0.39f, 0.24f, 0.12f);
	private static readonly Color BuildingColor = new(0.63f, 0.39f, 0.31f);
	private static readonly Color RoofColor = new(0.5f, 0.2f, 0.2f);

	public override void _Ready()
	{
		// Generate collision bodies for blocked tiles
		for (var y = 0; y < MapHeight; y++)
		{
			for (var x = 0; x < MapWidth; x++)
			{
				var tile = Layout[y, x];
				if (tile is 2 or 3 or 4) // water, tree, building = blocked
				{
					var body = new StaticBody2D();
					body.Position = new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
					body.CollisionLayer = 1;

					var shape = new CollisionShape2D();
					shape.Shape = new RectangleShape2D { Size = new Vector2(TileSize, TileSize) };
					body.AddChild(shape);
					AddChild(body);
				}
			}
		}
	}

	public override void _Draw()
	{
		for (var y = 0; y < MapHeight; y++)
		{
			for (var x = 0; x < MapWidth; x++)
			{
				var rect = new Rect2(x * TileSize, y * TileSize, TileSize, TileSize);
				var tile = Layout[y, x];

				// Draw grass base everywhere
				DrawRect(rect, GrassColor);

				switch (tile)
				{
					case 1: // Path
						DrawRect(rect, PathColor);
						break;
					case 2: // Water
						DrawRect(rect, WaterColor);
						// Simple wave highlight
						var waveY = y * TileSize + 4 + (x % 3);
						DrawLine(
							new Vector2(x * TileSize + 2, waveY),
							new Vector2(x * TileSize + 14, waveY),
							WaterColor.Lightened(0.3f), 1);
						break;
					case 3: // Tree
						// Trunk
						DrawRect(new Rect2(x * TileSize + 5, y * TileSize + 8, 6, 8), TrunkColor);
						// Canopy
						DrawRect(new Rect2(x * TileSize + 2, y * TileSize + 1, 12, 9), TreeColor);
						break;
					case 4: // Building
						DrawRect(rect, BuildingColor);
						// Roof line
						DrawRect(new Rect2(x * TileSize, y * TileSize, TileSize, 4), RoofColor);
						break;
				}
			}
		}
	}
}
