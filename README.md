# Sól

A turn-based creature battling RPG, inspired by Pokemon FireRed/LeafGreen.

## About

Sol is an exploration-focused RPG where ~25 creatures are hidden throughout the world as NPCs. Find them through exploration and they'll join your party. Battles exist for XP and trainer fights, but the core loop is discovering creatures, not catching them.

## Tech Stack

- **Engine:** Godot 4.6 (.NET edition)
- **Language:** C#
- **Art style:** GBA-era 2D pixel art (240x160 base resolution)
- **Target platform:** Mobile (iOS + Android), desktop

## Project Structure

```
sol/
├── project.godot          Godot project config
├── Sol.csproj / Sol.sln   C# project
├── src/                   All C# source code
│   ├── Autoloads/         Singletons (EventBus, SceneManager, GameManager, SaveManager, AudioManager)
│   ├── Core/              Shared types, events, interfaces
│   ├── Player/            Player controller and movement
│   ├── Battle/            Battle system (commands, state machine)
│   ├── Creatures/         Creature data and party management
│   ├── Overworld/         Maps, NPCs, encounters
│   ├── Inventory/         Items
│   ├── Skills/            Skill progression
│   ├── UI/                HUD, menus, dialogue
│   └── Data/              Save/load, JSON serialization
├── scenes/                Godot scene files (.tscn)
├── resources/             Godot resource files (.tres)
├── assets/                Sprites, audio, fonts
├── wiki/                  Game design documentation
└── tests/                 Unit tests
```

## Game Design

Design docs live in `wiki/`:

- **7 elemental types:** Neutral, Fire, Nature, Water, Sand, Electricity, Wind
- **Type triangles:** Fire > Nature > Water > Fire, Sand > Electricity > Wind > Sand
- **Same-type attacks heal the target** (-50% = heals by 50% of would-be damage)
- **Party size:** 3 creatures
- **Damage formula:** `(ATK / DEF) * Power * typeMultiplier`
- **4 cities:** Nufarm (start), Onhill (main hub), Raincastle, Kareskalm

## Getting Started

See `docs/godot-setup-guide.md` for installation instructions.

1. Install .NET SDK 8.0+ and Godot 4.6 (.NET edition)
2. Open `project.godot` in Godot
3. Press F5 to run

## Controls

- **WASD / Arrow keys** — Move
- **Space / Enter** — Interact
