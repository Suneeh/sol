using System.Text.Json;
using Godot;
using Sol.Data;

namespace Sol.Autoloads;

/// <summary>
/// Handles save/load with atomic writes and auto-save on app background.
/// Uses System.Text.Json source generators for NativeAOT/iOS safety.
/// </summary>
public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; } = null!;

	private const string SavePath = "user://save.json";
	private const string TempPath = "user://save_tmp.json";
	private const int MaxSlots = 3;

	public SaveData? CurrentSave { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		GD.Print("SaveManager: Ready");
	}

	public override void _Notification(int what)
	{
		// Auto-save when the app goes to background (mobile) or closes
		if (what == (int)NotificationApplicationPaused || what == (int)NotificationWMCloseRequest)
		{
			if (CurrentSave is not null)
				Save();
		}
	}

	/// <summary>
	/// Create a fresh save with the given player name.
	/// </summary>
	public void NewGame(string playerName)
	{
		CurrentSave = new SaveData { PlayerName = playerName };
		Save();
		GD.Print($"SaveManager: New game created for '{playerName}'");
	}

	/// <summary>
	/// Save current game state to disk. Uses atomic write (temp + rename).
	/// </summary>
	public void Save()
	{
		if (CurrentSave is null) return;

		var json = JsonSerializer.Serialize(CurrentSave, SaveJsonContext.Default.SaveData);

		// Write to temp file first
		using (var file = FileAccess.Open(TempPath, FileAccess.ModeFlags.Write))
		{
			if (file is null)
			{
				GD.PrintErr($"SaveManager: Cannot write to {TempPath}: {FileAccess.GetOpenError()}");
				return;
			}
			file.StoreString(json);
		}

		// Atomic rename
		var tempGlobal = ProjectSettings.GlobalizePath(TempPath);
		var saveGlobal = ProjectSettings.GlobalizePath(SavePath);
		var err = DirAccess.RenameAbsolute(tempGlobal, saveGlobal);
		if (err != Error.Ok)
			GD.PrintErr($"SaveManager: Rename failed: {err}");
		else
			EventBus.Instance?.EmitGameSaved();
	}

	/// <summary>
	/// Load save data from disk. Returns true if a save was found.
	/// </summary>
	public bool Load()
	{
		if (!FileAccess.FileExists(SavePath))
			return false;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		if (file is null)
		{
			GD.PrintErr($"SaveManager: Cannot read {SavePath}: {FileAccess.GetOpenError()}");
			return false;
		}

		var json = file.GetAsText();
		CurrentSave = JsonSerializer.Deserialize(json, SaveJsonContext.Default.SaveData);

		if (CurrentSave is null)
		{
			GD.PrintErr("SaveManager: Deserialization returned null");
			return false;
		}

		EventBus.Instance?.EmitGameLoaded();
		GD.Print($"SaveManager: Loaded save for '{CurrentSave.PlayerName}'");
		return true;
	}

	/// <summary>
	/// Check whether a save file exists.
	/// </summary>
	public static bool SaveExists() => FileAccess.FileExists(SavePath);
}
