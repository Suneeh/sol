using Godot;
using Sol.Core;

namespace Sol.Overworld;

/// <summary>
/// Generates a simple test map programmatically using colored rectangles.
/// Replace with a proper TileMapLayer once you have real tilesets in the editor.
/// </summary>
public partial class TestMap : Node2D
{
	public static int TileSize => GameConstants.TileSize;
	public const int MapWidth = 20;
	public const int MapHeight = 15;

	public const int TileGrass = 0;
	public const int TilePath = 1;
	public const int TileWater = 2;
	public const int TileTree = 3;
	public const int TileBuilding = 4;

	// Map layout: 0=grass, 1=path, 2=water, 3=tree(blocked), 4=building(blocked)
	public static readonly int[,] Layout = {
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

		// Spawn test NPCs
		SpawnNpc("Robin", new Vector2(9, 3), new Color(0.9f, 0.4f, 0.3f),
			"Hey! Welcome to Nufarm!",
			"This is just a small village...",
			"But there are creatures hidden\nall over the world!");

		SpawnNpc("Ferdi", new Vector2(14, 8), new Color(0.3f, 0.5f, 0.9f),
			"I'm Ferdi, the scientist.",
			"I've been studying the\ncreatures in this region.",
			"There are 7 elemental types.\nDid you know that?");

		SpawnHealerNpc("Marion", new Vector2(12, 5), new Color(0.95f, 0.6f, 0.8f));

		// Hidden creatures — tucked away in corners
		SpawnCreatureNpc(creatureId: 2, level: 5, tilePos: new Vector2(1, 13));   // Leafyn — bottom-left corner
		SpawnCreatureNpc(creatureId: 5, level: 4, tilePos: new Vector2(18, 1));   // Zapplet — top-right corner
		SpawnCreatureNpc(creatureId: 4, level: 6, tilePos: new Vector2(3, 9));    // Dustling — near the trees
	}

	private void SpawnNpc(string npcName, Vector2 tilePos, Color color, params string[] lines)
	{
		var npcScene = GD.Load<PackedScene>("res://scenes/overworld/Npc.tscn");
		var npc = npcScene.Instantiate<Npc>();
		npc.NpcName = npcName;
		npc.DialogueLines = lines;
		npc.SpriteColor = color;
		npc.Position = new Vector2(tilePos.X * TileSize + TileSize / 2, tilePos.Y * TileSize + TileSize / 2);
		AddChild(npc);
	}

	private void SpawnHealerNpc(string npcName, Vector2 tilePos, Color color)
	{
		var scene = GD.Load<PackedScene>("res://scenes/overworld/HealerNpc.tscn");
		var npc = scene.Instantiate<HealerNpc>();
		npc.NpcName = npcName;
		npc.SpriteColor = color;
		npc.Position = new Vector2(tilePos.X * TileSize + TileSize / 2, tilePos.Y * TileSize + TileSize / 2);
		AddChild(npc);
	}

	private void SpawnCreatureNpc(int creatureId, int level, Vector2 tilePos)
	{
		var scene = GD.Load<PackedScene>("res://scenes/overworld/CreatureNpc.tscn");
		var npc = scene.Instantiate<CreatureNpc>();
		npc.CreatureId = creatureId;
		npc.CreatureLevel = level;
		npc.Position = new Vector2(tilePos.X * TileSize + TileSize / 2, tilePos.Y * TileSize + TileSize / 2);
		AddChild(npc);
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
