namespace Sol.Data;

/// <summary>
/// Root save data structure. Contains all player-specific state.
/// Static game data (creature base stats, moves, type chart) is NOT saved here.
/// </summary>
public class SaveData
{
	public int Version { get; set; } = 1;
	public string PlayerName { get; set; } = "";
	public string CurrentMapId { get; set; } = "";
	public float PlayerX { get; set; }
	public float PlayerY { get; set; }
	public float PlayTimeSeconds { get; set; }
	// Party and inventory will be added in later phases
}
