using Godot;

namespace Sol.Autoloads;

/// <summary>
/// Manages BGM playback with crossfade and SFX playback.
/// Music is loaded on demand (streamed from disk).
/// </summary>
public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; } = null!;

	private AudioStreamPlayer _bgmPlayerA = null!;
	private AudioStreamPlayer _bgmPlayerB = null!;
	private AudioStreamPlayer _sfxPlayer = null!;

	/// <summary>Which BGM player is currently active (for crossfade).</summary>
	private bool _usePlayerA = true;

	private const float CrossfadeDuration = 0.5f;

	public override void _Ready()
	{
		Instance = this;
		GD.Print("AudioManager: Ready");

		_bgmPlayerA = new AudioStreamPlayer { Bus = "Music" };
		_bgmPlayerB = new AudioStreamPlayer { Bus = "Music" };
		_sfxPlayer = new AudioStreamPlayer { Bus = "SFX" };

		AddChild(_bgmPlayerA);
		AddChild(_bgmPlayerB);
		AddChild(_sfxPlayer);
	}

	/// <summary>
	/// Play a BGM track with crossfade. Pass null to stop music.
	/// </summary>
	/// <param name="path">Resource path, e.g. "res://assets/audio/music/battle.ogg"</param>
	public void PlayBGM(string? path)
	{
		var activePlayer = _usePlayerA ? _bgmPlayerA : _bgmPlayerB;
		var inactivePlayer = _usePlayerA ? _bgmPlayerB : _bgmPlayerA;

		// Fade out current
		if (activePlayer.Playing)
		{
			var fadeOut = CreateTween();
			fadeOut.TweenProperty(activePlayer, "volume_db", -40f, CrossfadeDuration);
			fadeOut.TweenCallback(Callable.From(() => activePlayer.Stop()));
		}

		if (path is null) return;

		// Load and play on the inactive player
		var stream = GD.Load<AudioStream>(path);
		if (stream is null)
		{
			GD.PrintErr($"AudioManager: Cannot load BGM: {path}");
			return;
		}

		inactivePlayer.Stream = stream;
		inactivePlayer.VolumeDb = -40f;
		inactivePlayer.Play();

		var fadeIn = CreateTween();
		fadeIn.TweenProperty(inactivePlayer, "volume_db", 0f, CrossfadeDuration);

		_usePlayerA = !_usePlayerA;
	}

	/// <summary>
	/// Play a one-shot sound effect.
	/// </summary>
	/// <param name="path">Resource path, e.g. "res://assets/audio/sfx/hit.ogg"</param>
	public void PlaySFX(string path)
	{
		var stream = GD.Load<AudioStream>(path);
		if (stream is null)
		{
			GD.PrintErr($"AudioManager: Cannot load SFX: {path}");
			return;
		}

		_sfxPlayer.Stream = stream;
		_sfxPlayer.Play();
	}

	/// <summary>
	/// Stop all music immediately.
	/// </summary>
	public void StopBGM()
	{
		_bgmPlayerA.Stop();
		_bgmPlayerB.Stop();
	}
}
