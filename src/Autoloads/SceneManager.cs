using System.Threading.Tasks;
using Godot;

namespace Sol.Autoloads;

/// <summary>
/// Manages scene transitions with fade-to-black overlay.
/// Uses async ResourceLoader to prevent frame hitches during map loads.
/// </summary>
public partial class SceneManager : Node
{
	public static SceneManager Instance { get; private set; } = null!;

	private ColorRect _overlay = null!;
	private CanvasLayer _overlayLayer = null!;
	private bool _transitioning;

	/// <summary>Duration of fade-in/fade-out in seconds.</summary>
	private const float FadeDuration = 0.3f;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("SceneManager: Ready");

		// Create a persistent overlay for fade transitions
		_overlayLayer = new CanvasLayer { Layer = 100 };
		AddChild(_overlayLayer);

		_overlay = new ColorRect
		{
			Color = new Color(0, 0, 0, 0),
			AnchorsPreset = (int)Control.LayoutPreset.FullRect,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		_overlayLayer.AddChild(_overlay);
	}

	/// <summary>
	/// Transition to a new scene with a fade effect.
	/// </summary>
	/// <param name="scenePath">Resource path, e.g. "res://scenes/maps/Town01.tscn"</param>
	/// <param name="spawnPosition">Optional position to place the player after loading.</param>
	public async void TransitionTo(string scenePath, Vector2? spawnPosition = null)
	{
		if (_transitioning) return;
		_transitioning = true;

		// Fade out (to black)
		await Fade(0f, 1f);

		// Start async load
		var error = ResourceLoader.LoadThreadedRequest(scenePath);
		if (error != Error.Ok)
		{
			GD.PrintErr($"SceneManager: Failed to start loading {scenePath}: {error}");
			await Fade(1f, 0f);
			_transitioning = false;
			return;
		}

		// Wait for load to complete
		while (true)
		{
			var status = ResourceLoader.LoadThreadedGetStatus(scenePath);
			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
				break;
			if (status == ResourceLoader.ThreadLoadStatus.Failed)
			{
				GD.PrintErr($"SceneManager: Failed to load {scenePath}");
				await Fade(1f, 0f);
				_transitioning = false;
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		// Instantiate new scene
		var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(scenePath);
		var newScene = packedScene.Instantiate();

		// Swap scenes
		var currentScene = GetTree().CurrentScene;
		currentScene?.QueueFree();
		GetTree().Root.AddChild(newScene);
		GetTree().CurrentScene = newScene;

		// Fade in (from black)
		await Fade(1f, 0f);
		_transitioning = false;

		EventBus.Instance?.EmitMapChanged(scenePath);
	}

	private async Task Fade(float from, float to)
	{
		var tween = CreateTween();
		tween.TweenProperty(_overlay, "color:a", to, FadeDuration)
			.From(from);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
