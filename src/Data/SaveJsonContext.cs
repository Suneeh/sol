using System.Text.Json.Serialization;

namespace Sol.Data;

/// <summary>
/// Source generator context for AOT-safe JSON serialization.
/// Add all types that need to be serialized/deserialized here.
/// </summary>
[JsonSerializable(typeof(SaveData))]
[JsonSerializable(typeof(TestSaveData))]
public partial class SaveJsonContext : JsonSerializerContext { }

/// <summary>
/// Test data structure to validate JSON serialization on mobile.
/// </summary>
public record TestSaveData
{
	public string PlayerName { get; init; } = "";
	public int Level { get; init; }
	public float PlayTime { get; init; }
	public string[] PartyCreatures { get; init; } = [];
}
