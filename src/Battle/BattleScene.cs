using System.Collections.Generic;
using Godot;
using Sol.Autoloads;
using Sol.Battle.State;
using Sol.Core.Types;
using Sol.Creatures;

namespace Sol.Battle;

/// <summary>
/// Battle scene UI. Subscribes to BattleManager events and renders the battle.
/// Added as a CanvasLayer overlay on top of the overworld (which is paused).
/// </summary>
public partial class BattleScene : CanvasLayer
{
	private BattleManager _battle = null!;
	private Control _root = null!;

	// UI elements
	private Label _playerName = null!;
	private Label _playerHp = null!;
	private ProgressBar _playerHpBar = null!;
	private Label _playerLevel = null!;

	private Label _enemyName = null!;
	private Label _enemyHp = null!;
	private ProgressBar _enemyHpBar = null!;
	private Label _enemyLevel = null!;

	private Label _messageLabel = null!;
	private GridContainer _moveGrid = null!;
	private Button _fleeButton = null!;
	private VBoxContainer _actionPanel = null!;

	private readonly Queue<string> _messageQueue = new();
	private bool _waitingForInput;
	private bool _battleEnded;

	public void Initialize(CreatureInstance player, CreatureInstance enemy)
	{
		_battle = new BattleManager(player, enemy);
		_battle.OnMessage += QueueMessage;
		_battle.OnPlayerTurnStart += ShowActions;
		_battle.OnBattleEnd += OnBattleEnd;
		_battle.OnDamageDealt += (_, _, _) => UpdateHpBars();
		_battle.OnHealing += (_, _, _) => UpdateHpBars();
	}

	public override void _Ready()
	{
		Layer = 50;
		BuildUI();

		if (_battle is not null)
		{
			UpdateHpBars();
			_battle.Start();
			ShowNextMessage();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("interact") && _waitingForInput)
		{
			GetViewport().SetInputAsHandled();
			_waitingForInput = false;
			ShowNextMessage();
		}
	}

	private void QueueMessage(string msg)
	{
		_messageQueue.Enqueue(msg);
	}

	private void ShowNextMessage()
	{
		if (_messageQueue.Count > 0)
		{
			_messageLabel.Text = _messageQueue.Dequeue();
			_actionPanel.Visible = false;
			_waitingForInput = true;
		}
		else if (_battleEnded)
		{
			CloseBattle();
		}
		else
		{
			// No more messages — if it's player turn, show actions
			if (_battle.CurrentState == BattleState.PlayerTurn)
				ShowActions();
		}
	}

	private void ShowActions()
	{
		if (_messageQueue.Count > 0)
		{
			ShowNextMessage();
			return;
		}

		_messageLabel.Text = "What will you do?";
		_actionPanel.Visible = true;
		_waitingForInput = false;

		// Update move buttons
		for (var i = 0; i < 4; i++)
		{
			var btn = _moveGrid.GetChild<Button>(i);
			if (i < _battle.PlayerCreature.Moves.Length)
			{
				var move = _battle.PlayerCreature.Moves[i];
				btn.Text = $"{move.Data.MoveName}\n{move.CurrentPp}/{move.Data.MaxPp}";
				btn.Disabled = !move.HasPp;
				btn.Visible = true;
			}
			else
			{
				btn.Visible = false;
			}
		}
	}

	private void OnMoveSelected(int index)
	{
		_actionPanel.Visible = false;
		_battle.SelectMove(index);
		ShowNextMessage();
	}

	private void OnFleePressed()
	{
		_actionPanel.Visible = false;
		_battle.SelectFlee();
		ShowNextMessage();
	}

	private void OnBattleEnd(BattleOutcome outcome)
	{
		_battleEnded = true;
		EventBus.Instance?.EmitBattleEnded(outcome == BattleOutcome.PlayerWin);
	}

	private void CloseBattle()
	{
		GameManager.Instance?.SetMode(GameMode.Overworld);
		QueueFree();
	}

	private void UpdateHpBars()
	{
		if (_battle is null) return;

		var pc = _battle.PlayerCreature;
		var ec = _battle.EnemyCreature;

		_playerHp.Text = $"{pc.CurrentHp}/{pc.MaxHp}";
		_playerHpBar.MaxValue = pc.MaxHp;
		_playerHpBar.Value = pc.CurrentHp;
		_playerLevel.Text = $"Lv{pc.Level}";

		_enemyHp.Text = $"{ec.CurrentHp}/{ec.MaxHp}";
		_enemyHpBar.MaxValue = ec.MaxHp;
		_enemyHpBar.Value = ec.CurrentHp;
		_enemyLevel.Text = $"Lv{ec.Level}";
	}

	// ── UI Construction ──────────────────────────────────

	private static readonly Color BgColor = new(0.08f, 0.08f, 0.12f);
	private static readonly Color PanelColor = new(0.12f, 0.12f, 0.18f, 0.95f);

	private void BuildUI()
	{
		_root = new Control();
		_root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(_root);

		// Background
		var bg = new ColorRect { Color = BgColor };
		bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_root.AddChild(bg);

		// Enemy info (top-left)
		BuildCreaturePanel(true, out _enemyName, out _enemyHp, out _enemyHpBar, out _enemyLevel,
			new Vector2(8, 4), "Enemy");

		// Player info (bottom-right area)
		BuildCreaturePanel(false, out _playerName, out _playerHp, out _playerHpBar, out _playerLevel,
			new Vector2(128, 56), "Player");

		// Creature "sprites" — colored rectangles for now
		var enemySprite = new ColorRect
		{
			Color = GetTypeColor(_battle?.EnemyCreature.Type ?? CreatureType.Neutral),
			Position = new Vector2(160, 8),
			Size = new Vector2(32, 32)
		};
		_root.AddChild(enemySprite);

		var playerSprite = new ColorRect
		{
			Color = GetTypeColor(_battle?.PlayerCreature.Type ?? CreatureType.Neutral),
			Position = new Vector2(32, 48),
			Size = new Vector2(32, 32)
		};
		_root.AddChild(playerSprite);

		// Message box (bottom)
		var msgPanel = new PanelContainer();
		msgPanel.Position = new Vector2(4, 108);
		msgPanel.Size = new Vector2(232, 48);
		var msgStyle = new StyleBoxFlat
		{
			BgColor = PanelColor,
			BorderWidthBottom = 1, BorderWidthTop = 1,
			BorderWidthLeft = 1, BorderWidthRight = 1,
			BorderColor = new Color(0.5f, 0.5f, 0.6f),
			ContentMarginLeft = 4, ContentMarginRight = 4,
			ContentMarginTop = 2, ContentMarginBottom = 2
		};
		msgPanel.AddThemeStyleboxOverride("panel", msgStyle);
		_root.AddChild(msgPanel);

		var msgVbox = new VBoxContainer();
		msgPanel.AddChild(msgVbox);

		_messageLabel = new Label
		{
			Text = "",
			AutowrapMode = TextServer.AutowrapMode.Word
		};
		_messageLabel.AddThemeFontSizeOverride("font_size", 7);
		msgVbox.AddChild(_messageLabel);

		// Action panel (overlays message area when it's player's turn)
		_actionPanel = new VBoxContainer();
		_actionPanel.Position = new Vector2(4, 108);
		_actionPanel.Size = new Vector2(232, 48);
		_actionPanel.Visible = false;
		_root.AddChild(_actionPanel);

		_moveGrid = new GridContainer { Columns = 2 };
		_actionPanel.AddChild(_moveGrid);

		for (var i = 0; i < 4; i++)
		{
			var idx = i;
			var btn = new Button { Text = $"Move {i + 1}" };
			btn.AddThemeFontSizeOverride("font_size", 6);
			btn.CustomMinimumSize = new Vector2(110, 18);
			btn.Pressed += () => OnMoveSelected(idx);
			_moveGrid.AddChild(btn);
		}

		_fleeButton = new Button { Text = "Run" };
		_fleeButton.AddThemeFontSizeOverride("font_size", 6);
		_fleeButton.Pressed += OnFleePressed;
		_actionPanel.AddChild(_fleeButton);
	}

	private void BuildCreaturePanel(bool isEnemy, out Label nameLabel, out Label hpLabel,
		out ProgressBar hpBar, out Label levelLabel, Vector2 pos, string defaultName)
	{
		var panel = new VBoxContainer();
		panel.Position = pos;
		panel.Size = new Vector2(100, 40);
		_root.AddChild(panel);

		var nameRow = new HBoxContainer();
		panel.AddChild(nameRow);

		nameLabel = new Label { Text = isEnemy ? _battle?.EnemyCreature.Nickname ?? defaultName : _battle?.PlayerCreature.Nickname ?? defaultName };
		nameLabel.AddThemeFontSizeOverride("font_size", 7);
		nameRow.AddChild(nameLabel);

		levelLabel = new Label { Text = "Lv5" };
		levelLabel.AddThemeFontSizeOverride("font_size", 6);
		levelLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
		nameRow.AddChild(levelLabel);

		hpBar = new ProgressBar
		{
			CustomMinimumSize = new Vector2(96, 6),
			MaxValue = 100,
			Value = 100,
			ShowPercentage = false
		};
		panel.AddChild(hpBar);

		hpLabel = new Label { Text = "??/??" };
		hpLabel.AddThemeFontSizeOverride("font_size", 6);
		hpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.7f));
		panel.AddChild(hpLabel);
	}

	private static Color GetTypeColor(CreatureType type) => type switch
	{
		CreatureType.Fire => new Color(0.9f, 0.3f, 0.2f),
		CreatureType.Nature => new Color(0.3f, 0.8f, 0.3f),
		CreatureType.Water => new Color(0.3f, 0.4f, 0.9f),
		CreatureType.Sand => new Color(0.8f, 0.7f, 0.4f),
		CreatureType.Electricity => new Color(0.9f, 0.8f, 0.2f),
		CreatureType.Wind => new Color(0.7f, 0.85f, 0.9f),
		_ => new Color(0.6f, 0.6f, 0.6f),
	};
}
