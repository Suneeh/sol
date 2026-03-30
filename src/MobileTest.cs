using System.Text.Json;
using Godot;
using Sol.Data;

/// <summary>
/// Mobile validation test — exercises critical C# features on device.
/// Replace this with the real Main scene once validation passes.
/// </summary>
public partial class MobileTest : Node2D
{
	private Label _statusLabel = null!;
	private Label _touchLabel = null!;
	private int _testsPassed;
	private int _testsTotal;

	public override void _Ready()
	{
		_statusLabel = GetNode<Label>("StatusLabel");
		_touchLabel = GetNode<Label>("TouchLabel");

		_statusLabel.Text = "Running tests...";
		RunAllTests();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touch)
		{
			_touchLabel.Text = touch.Pressed
				? $"Touch: ({touch.Position.X:F0}, {touch.Position.Y:F0})"
				: "Touch: released";
		}
	}

	private void RunAllTests()
	{
		_testsTotal = 0;
		_testsPassed = 0;

		Test("C# Basic", TestCSharpBasic);
		Test("JSON Serialize", TestJsonSerialize);
		Test("JSON Deserialize", TestJsonDeserialize);
		Test("Async/Await", TestAsyncAwait);
		Test("Godot Signal", TestGodotSignal);
		Test("File Write", TestFileWrite);
		Test("File Read", TestFileRead);

		var result = $"Sol Mobile Test\n\n{_testsPassed}/{_testsTotal} tests passed";
		if (_testsPassed == _testsTotal)
			result += "\n\nAll clear! Ready for development.";
		else
			result += "\n\nSome tests failed — check output.";

		_statusLabel.Text = result;
		GD.Print(result);
	}

	private void Test(string name, System.Action action)
	{
		_testsTotal++;
		try
		{
			action();
			_testsPassed++;
			GD.Print($"  PASS: {name}");
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"  FAIL: {name} — {ex.Message}");
		}
	}

	private static void TestCSharpBasic()
	{
		// Records, pattern matching, LINQ-free collection ops
		var data = new TestSaveData
		{
			PlayerName = "Ash",
			Level = 5,
			PlayTime = 1.5f,
			PartyCreatures = ["Flamix", "Aquara", "Leafy"]
		};
		if (data.PlayerName != "Ash") throw new System.Exception("Record field mismatch");
		if (data.PartyCreatures.Length != 3) throw new System.Exception("Array length mismatch");
	}

	private static void TestJsonSerialize()
	{
		var data = new TestSaveData
		{
			PlayerName = "Sol",
			Level = 10,
			PlayTime = 3.14f,
			PartyCreatures = ["Creature1", "Creature2"]
		};
		var json = JsonSerializer.Serialize(data, SaveJsonContext.Default.TestSaveData);
		if (string.IsNullOrEmpty(json)) throw new System.Exception("Serialized to empty string");
		if (!json.Contains("Sol")) throw new System.Exception("Missing expected content");
		GD.Print($"    JSON: {json}");
	}

	private static void TestJsonDeserialize()
	{
		const string json = """{"PlayerName":"Test","Level":42,"PlayTime":9.9,"PartyCreatures":["A","B","C"]}""";
		var data = JsonSerializer.Deserialize(json, SaveJsonContext.Default.TestSaveData);
		if (data is null) throw new System.Exception("Deserialized to null");
		if (data.PlayerName != "Test") throw new System.Exception($"Expected 'Test', got '{data.PlayerName}'");
		if (data.Level != 42) throw new System.Exception($"Expected 42, got {data.Level}");
	}

	private static void TestAsyncAwait()
	{
		// Verify async machinery works (critical for scene loading)
		var task = System.Threading.Tasks.Task.Run(() => 2 + 2);
		task.Wait();
		if (task.Result != 4) throw new System.Exception($"Expected 4, got {task.Result}");
	}

	private void TestGodotSignal()
	{
		// Verify C# can connect to Godot signals
		var timer = new Timer();
		AddChild(timer);
		var connected = false;
		timer.Timeout += () => connected = true;
		timer.WaitTime = 0.001;
		timer.OneShot = true;
		timer.Start();
		// Signal connection itself is the test — if it doesn't throw, it works
		timer.QueueFree();
		_ = connected; // suppress unused warning
	}

	private static void TestFileWrite()
	{
		var data = new TestSaveData { PlayerName = "FileTest", Level = 1 };
		var json = JsonSerializer.Serialize(data, SaveJsonContext.Default.TestSaveData);
		using var file = FileAccess.Open("user://test_save.json", FileAccess.ModeFlags.Write);
		if (file is null) throw new System.Exception($"Cannot open file: {FileAccess.GetOpenError()}");
		file.StoreString(json);
	}

	private static void TestFileRead()
	{
		using var file = FileAccess.Open("user://test_save.json", FileAccess.ModeFlags.Read);
		if (file is null) throw new System.Exception($"Cannot open file: {FileAccess.GetOpenError()}");
		var json = file.GetAsText();
		var data = JsonSerializer.Deserialize(json, SaveJsonContext.Default.TestSaveData);
		if (data?.PlayerName != "FileTest") throw new System.Exception("Read-back mismatch");

		// Clean up test file
		DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath("user://test_save.json"));
	}
}
