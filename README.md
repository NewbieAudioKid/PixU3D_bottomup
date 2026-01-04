# PixelFlow

A 2D puzzle shooter game built with Unity where colored shooters patrol a conveyor belt around a grid, automatically firing at matching colored cells.

## Gameplay

1. **Select Shooters** - Click shooters from the table to move them to the ready queue (5 slots max)
2. **Deploy to Belt** - Click a queued shooter to start conveyor belt patrol
3. **Auto-Fire** - Shooters automatically fire at cells matching their color
4. **Win Condition** - Clear all cells from the grid
5. **Lose Condition** - Ready queue is full when a shooter returns from patrol

### Last Stand Mechanic

When the shooter table and ready queue are both empty, the current shooter triggers "Last Stand" mode:
- 2x movement speed
- Automatically re-enters the belt for another patrol
- Continues until ammo depletes or win/lose condition met

## Project Structure

```
Assets/
├── Resources/
│   ├── Levels/           # JSON level data
│   │   ├── Level_1_grid.json
│   │   ├── Level_1_table.json
│   │   └── ...
│   └── Scenes/
│       ├── SplashScene.unity
│       ├── MenuScene.unity
│       └── GameScene.unity
└── Scripts/
    ├── GameManager.cs              # Global state, scene management
    ├── GameScene/
    │   ├── GridManager.cs          # 20x20 cell grid
    │   ├── PigController.cs        # Shooter logic, pre-calculated shots
    │   ├── ReadyQueueManager.cs    # 5-slot queue management
    │   ├── ShooterTableManager.cs  # 5x6 shooter inventory
    │   ├── BeltWalker.cs           # Conveyor movement
    │   ├── BeltPathHolder.cs       # Belt waypoints
    │   ├── CellController.cs       # Grid cell behavior
    │   ├── BulletController.cs     # Projectile logic
    │   └── UIBillboard.cs          # Camera-facing UI
    ├── UIScripts/
    │   ├── GameResultPopup.cs      # Victory/GameOver modal
    │   ├── SplashController.cs     # Splash screen
    │   ├── MenuLevelDisplay.cs     # Menu level numbers
    │   ├── GameLevelDisplay.cs     # In-game level display
    │   ├── UI_StartButton.cs       # Play button handler
    │   ├── BackgroundScroller.cs   # Scrolling background
    │   └── Animation/
    │       ├── ElasticButton.cs    # Q-bounce button effect
    │       └── SceneFader.cs       # Fade transition
    └── Level/
        ├── LevelDataGenerator.cs   # Editor tool for JSON generation
        └── LevelSelectorUI.cs      # Level selection handler
```

## Requirements

- Unity 2021.3.x or later
- TextMeshPro (included in Unity)

## Getting Started

1. Clone the repository
2. Open in Unity Hub
3. Open `SplashScene` and hit Play

### Generate Level Data

1. Add `LevelDataGenerator` component to any GameObject
2. Right-click the component in Inspector
3. Select "Generate All 10 Levels"
4. JSON files are created in `Resources/Levels/`

## Architecture

### Core Systems

| System | Responsibility |
|--------|---------------|
| `GameManager` | Singleton. Level state, scene transitions, JSON loading |
| `GridManager` | 20x20 cell grid, smart target lookup, win detection |
| `PigController` | Shooter state machine, pre-calculated shot scheduling |
| `ReadyQueueManager` | 5-slot queue with auto shift-left |
| `ShooterTableManager` | 5-column shooter inventory with stack behavior |

### Pre-Calculated Shot System

Instead of real-time target detection, shooters pre-calculate their entire shot schedule before entering the belt:

```
PreCalculatePath():
    for step in 0..79:
        pos = GetSimulatedPosition(step)
        target = GetTargetCellSmart(pos)
        if target.color == shooter.color && !target.isPendingDeath:
            shotSchedule.Enqueue({step, target})
            target.isPendingDeath = true
```

This approach:
- Eliminates frame-rate dependent targeting issues
- Allows cells to be "reserved" preventing duplicate targeting
- Enables predictable shot timing

### State Flow

```
Shooter Lifecycle:
InTable -> InQueue -> OnBelt -> Returning -> InQueue
                        |
                        v (if table & queue empty)
                   LastStand (2x speed, re-enter belt)
```

## Level Data Format

### Grid JSON (Level_X_grid.json)

```json
{
  "cells": [
    { "x": 0, "y": 0, "color": "red" },
    { "x": 0, "y": 1, "color": "blue" }
  ]
}
```

### Shooter Table JSON (Level_X_table.json)

```json
{
  "columns": [
    {
      "shooters": [
        { "color": "red", "ammo": 25 },
        { "color": "blue", "ammo": 30 }
      ]
    }
  ]
}
```

**Balance Rule:** Total ammo per color must equal total cells of that color.

## Documentation

Full technical documentation available at `docs/index.html` or via GitHub Pages.

## License

MIT
