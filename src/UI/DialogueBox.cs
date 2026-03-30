using Godot;
using Sol.Autoloads;
using Sol.Core.Types;

namespace Sol.UI;

/// <summary>
/// Dialogue box UI. Shows text with a typewriter effect at the bottom of the screen.
/// Blocks player input while active. Advance with interact key.
/// </summary>
public partial class DialogueBox : CanvasLayer
{
	public static DialogueBox? Instance { get; private set; }

	private PanelContainer _panel = null!;
	private Label _nameLabel = null!;
	private Label _textLabel = null!;
	private Label _continueHint = null!;

	private string[] _lines = [];
	private int _currentLine;
	private int _visibleChars;
	private bool _lineComplete;
	private bool _active;

	private const float CharsPerSecond = 30f;
	private float _charTimer;

	public override void _Ready()
	{
		Instance = this;
		Layer = 90; // Above game, below transition overlay

		BuildUI();
		TogglePanel(false);
	}

	public override void _Process(double delta)
	{
		if (!_active) return;

		// Typewriter effect
		if (!_lineComplete)
		{
			_charTimer += (float)delta * CharsPerSecond;
			while (_charTimer >= 1f && _visibleChars < _textLabel.Text.Length)
			{
				_visibleChars++;
				_charTimer -= 1f;
			}
			_textLabel.VisibleCharacters = _visibleChars;

			if (_visibleChars >= _textLabel.Text.Length)
			{
				_lineComplete = true;
				_continueHint.Visible = true;
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_active) return;

		if (@event.IsActionPressed("interact"))
		{
			GetViewport().SetInputAsHandled();

			if (!_lineComplete)
			{
				// Skip typewriter — show full line
				_visibleChars = _textLabel.Text.Length;
				_textLabel.VisibleCharacters = _visibleChars;
				_lineComplete = true;
				_continueHint.Visible = true;
			}
			else
			{
				// Advance to next line
				_currentLine++;
				if (_currentLine < _lines.Length)
					ShowCurrentLine();
				else
					Close();
			}
		}
	}

	/// <summary>
	/// Open the dialogue box with the given lines.
	/// </summary>
	public void Open(string npcName, string[] lines)
	{
		if (lines.Length == 0) return;

		_lines = lines;
		_currentLine = 0;
		_active = true;

		_nameLabel.Text = npcName;
		ShowCurrentLine();
		TogglePanel(true);

		GameManager.Instance?.SetMode(GameMode.Dialogue);
	}

	private void ShowCurrentLine()
	{
		_textLabel.Text = _lines[_currentLine];
		_textLabel.VisibleCharacters = 0;
		_visibleChars = 0;
		_charTimer = 0;
		_lineComplete = false;
		_continueHint.Visible = false;
	}

	private void Close()
	{
		_active = false;
		TogglePanel(false);
		GameManager.Instance?.SetMode(GameMode.Overworld);
	}

	private void TogglePanel(bool visible)
	{
		_panel.Visible = visible;
	}

	private void BuildUI()
	{
		// Background panel at the bottom of the viewport
		_panel = new PanelContainer();
		_panel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
		_panel.OffsetTop = -40;
		_panel.OffsetBottom = 0;
		_panel.OffsetLeft = 4;
		_panel.OffsetRight = -4;

		// Style the panel
		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.05f, 0.05f, 0.1f, 0.92f),
			BorderWidthBottom = 1,
			BorderWidthTop = 1,
			BorderWidthLeft = 1,
			BorderWidthRight = 1,
			BorderColor = new Color(0.6f, 0.6f, 0.7f),
			ContentMarginLeft = 4,
			ContentMarginRight = 4,
			ContentMarginTop = 2,
			ContentMarginBottom = 2
		};
		_panel.AddThemeStyleboxOverride("panel", style);

		var vbox = new VBoxContainer();
		_panel.AddChild(vbox);

		// NPC name
		_nameLabel = new Label
		{
			Text = "",
			HorizontalAlignment = HorizontalAlignment.Left,
		};
		_nameLabel.AddThemeFontSizeOverride("font_size", 7);
		_nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f));
		vbox.AddChild(_nameLabel);

		// Dialogue text
		_textLabel = new Label
		{
			Text = "",
			AutowrapMode = TextServer.AutowrapMode.Word,
			VisibleCharacters = 0
		};
		_textLabel.AddThemeFontSizeOverride("font_size", 7);
		vbox.AddChild(_textLabel);

		// Continue hint
		_continueHint = new Label
		{
			Text = ">>",
			HorizontalAlignment = HorizontalAlignment.Right,
			Visible = false
		};
		_continueHint.AddThemeFontSizeOverride("font_size", 6);
		_continueHint.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
		vbox.AddChild(_continueHint);

		AddChild(_panel);
	}
}
