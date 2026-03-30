namespace Sol.Core;

/// <summary>
/// Central game constants. Change these to adjust resolution and scale globally.
/// </summary>
public static class GameConstants
{
	// ── Resolution ──
	public const int ViewportWidth = 480;
	public const int ViewportHeight = 320;
	public const int WindowScale = 2; // Desktop window = viewport * this

	// ── Tiles ──
	public const int TileSize = 32;

	// ── UI ──
	public const int FontSizeLarge = 16;
	public const int FontSizeNormal = 14;
	public const int FontSizeSmall = 12;
	public const int UiMargin = 8;

	// ── Battle ──
	public const int CreatureSpriteSize = 64;
	public const float EncounterChance = 0.15f;

	// ── Derived ──
	public const int TilesVisibleX = ViewportWidth / TileSize;   // 15
	public const int TilesVisibleY = ViewportHeight / TileSize;  // 10
}
