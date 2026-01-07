<div align="center">

# PixelFlow

**一款 2D 益智射击游戏**

A 2D Puzzle Shooter Game | 2D パズルシューティングゲーム

---

[![Unity](https://img.shields.io/badge/Unity-2021.3+-000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](LICENSE)

### [技术文档 / Documentation / ドキュメント](https://newbieaudiokid.github.io/PixU3D_bottomup/)

[中文](#中文) | [English](#english) | [日本語](#日本語)

</div>

---

<a name="中文"></a>

## 概述

PixelFlow 是一款策略益智游戏。彩色射手在传送带上巡逻，自动射击匹配颜色的方块。玩家需要策略性地从备战台部署射手到准备队列，然后送上传送带，清除网格中的所有方块。

### 核心玩法

- **选择射手** - 点击备战台(ShooterTable)上的射手，移动到准备队列(ReadyQueue)，最多 5 个槽位
- **部署传送带** - 点击队列中的射手，开始传送带巡逻
- **自动射击** - 射手沿传送带移动，自动射击颜色匹配的方块
- **颜色阻挡** - 异色方块会阻挡射线，不会穿透击中后面的同色方块
- **胜利条件** - 清除网格中所有方块
- **失败条件** - 射手返回时准备队列已满

### 绝地反击机制

当备战台和准备队列都为空时，当前射手触发"绝地反击"模式，获得 **2 倍速度加成**，自动重新进入传送带直到弹药耗尽。这是翻盘的最后机会！

### 核心技术亮点

- **PreCalculatePath 预计算算法** - 射手上传送带前模拟 80 步路径，预计算所有射击点生成"射击排期表"
- **GetTargetCellSmart 智能穿透查找** - 根据射手位置判断区域，支持**颜色阻挡判定**（异色方块阻挡射线）+ **isPendingDeath 穿透**（已被预定的方块视为透明）
- **Ground Truth 射击系统** - 子弹从预计算的理论位置发射，确保帧率波动不影响射击精度
- **sqrMagnitude 高性能碰撞** - 子弹碰撞检测使用平方距离替代开方运算，提升性能
- **协程驱动事件流** - 使用 IEnumerator + yield 实现复杂时序控制和动画编排

## 项目结构

```
Assets/
├── Scripts/
│   ├── GameManager.cs                 # 全局状态、场景切换、JSON加载
│   ├── GameScene/
│   │   ├── GridManager.cs             # 20x20网格、智能目标查找、胜利检测
│   │   ├── PigController.cs           # 射手状态机、预计算射击、绝地反击
│   │   ├── CellController.cs          # 方块生命周期、isPendingDeath占位
│   │   ├── BeltWalker.cs              # 传送带移动、2倍速支持
│   │   ├── ReadyQueueManager.cs       # 5槽位队列、Shift Left补位
│   │   ├── ShooterTableManager.cs     # 5x6备战台、堆栈式顶上
│   │   ├── BulletController.cs        # 子弹飞行、距离检测碰撞
│   │   └── BeltPathHolder.cs          # 路径点容器
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs         # 胜利/失败弹窗、AnimationCurve动画
│   │   ├── SceneFader.cs              # 场景淡入淡出
│   │   └── SplashController.cs        # 启动画面
│   └── Level/
│       └── LevelDataGenerator.cs      # [ContextMenu]编辑器关卡生成工具
├── Resources/
│   └── Levels/                        # Level_X_grid.json + Level_X_table.json
└── Scenes/
    ├── SplashScene.unity              # 启动画面
    ├── MenuScene.unity                # 主菜单
    └── GameScene.unity                # 游戏场景
```

## 系统要求

- Unity 2021.3 LTS 或更高版本
- TextMeshPro 包

## 快速开始

1. 克隆仓库
2. 在 Unity 中打开项目
3. 打开 `Assets/Scenes/SplashScene.unity`
4. 点击播放

---

<a name="english"></a>

## Overview

PixelFlow is a strategic puzzle game where colored shooters patrol a conveyor belt, automatically firing at matching cells. Players must strategically deploy shooters from the table to the ready queue, then onto the belt to clear all cells from the grid.

### Core Mechanics

- **Select Shooters** - Click shooters from the table to move them to the ready queue (5 slots max)
- **Deploy to Belt** - Click a queued shooter to start conveyor belt patrol
- **Auto-Fire** - Shooters automatically fire at cells matching their color
- **Color Blocking** - Different-color cells block shots, no penetration to same-color cells behind
- **Win Condition** - Clear all cells from the grid
- **Lose Condition** - Ready queue is full when a shooter returns

### Last Stand Mechanic

When both the shooter table and ready queue are empty, the current shooter triggers "Last Stand" mode with **2x speed bonus**, automatically re-entering the conveyor belt until ammunition depletes.

### Technical Highlights

- **PreCalculatePath Algorithm** - Simulates 80-step belt path before entry, pre-calculates all shot points into a "shot schedule"
- **GetTargetCellSmart Zone Lookup** - Zone-based scan with **color blocking** (different-color cells block shots) + **isPendingDeath penetration** (pre-targeted cells are transparent)
- **Ground Truth Firing System** - Bullets fire from pre-calculated theoretical positions, ensuring frame-rate independent accuracy
- **sqrMagnitude High-Performance Collision** - Uses squared distance instead of sqrt for bullet collision detection
- **Coroutine-Driven Event Flow** - Uses IEnumerator + yield for complex timing control and animation orchestration

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameManager.cs                 # Global state, scene transitions, JSON loading
│   ├── GameScene/
│   │   ├── GridManager.cs             # 20x20 grid, smart target lookup, win detection
│   │   ├── PigController.cs           # Shooter state machine, pre-calculated shots, last stand
│   │   ├── CellController.cs          # Cell lifecycle, isPendingDeath flag
│   │   ├── BeltWalker.cs              # Belt movement, 2x speed support
│   │   ├── ReadyQueueManager.cs       # 5-slot queue, Shift Left algorithm
│   │   ├── ShooterTableManager.cs     # 5x6 table, stack-like auto-rise
│   │   ├── BulletController.cs        # Bullet flight, distance-based collision
│   │   └── BeltPathHolder.cs          # Waypoint container
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs         # Victory/GameOver popup, AnimationCurve
│   │   ├── SceneFader.cs              # Scene fade transitions
│   │   └── SplashController.cs        # Splash screen
│   └── Level/
│       └── LevelDataGenerator.cs      # [ContextMenu] Editor level generation tool
├── Resources/
│   └── Levels/                        # Level_X_grid.json + Level_X_table.json
└── Scenes/
    ├── SplashScene.unity
    ├── MenuScene.unity
    └── GameScene.unity
```

## Requirements

- Unity 2021.3 LTS or higher
- TextMeshPro package

## Getting Started

1. Clone the repository
2. Open project in Unity
3. Open `Assets/Scenes/SplashScene.unity`
4. Press Play

---

<a name="日本語"></a>

## 概要

PixelFlow は戦略パズルゲームです。色付きシューターがコンベアベルトを巡回し、一致するセルに自動的に発射します。プレイヤーはテーブルからシューターを待機キューに戦略的に配置し、ベルトに送ってグリッドからすべてのセルをクリアする必要があります。

### ゲームプレイ

- **シューター選択** - テーブルからシューターをクリックして待機キューに移動（最大 5 スロット）
- **ベルトへ配置** - キュー内のシューターをクリックしてベルトパトロールを開始
- **自動発射** - シューターは同じ色のセルに自動的に発射
- **色ブロッキング** - 異色セルはショットをブロック、後ろの同色セルに貫通しない
- **勝利条件** - グリッドからすべてのセルをクリア
- **敗北条件** - シューターが戻ったとき待機キューが満杯

### ラストスタンドメカニクス

シューターテーブルと待機キューの両方が空のとき、現在のシューターは「ラストスタンド」モードを発動し、**2 倍のスピードボーナス**を得て、弾薬が尽きるまで自動的にコンベアベルトに再突入します。

### 技術的ハイライト

- **PreCalculatePath アルゴリズム** - ベルト進入前に 80 ステップのパスをシミュレート、すべてのショットポイントを事前計算
- **GetTargetCellSmart ゾーン検索** - **色ブロッキング**（異色セルはショットをブロック）+ **isPendingDeath 穿透**（予約済みセルは透明）
- **Ground Truth 射撃システム** - 事前計算された理論位置から発射、フレームレート変動に依存しない精度
- **sqrMagnitude 高性能衝突** - 弾丸衝突検出に平方距離を使用（sqrt 回避）
- **コルーチン駆動イベントフロー** - IEnumerator + yield で複雑なタイミング制御

## プロジェクト構造

```
Assets/
├── Scripts/
│   ├── GameManager.cs                 # グローバル状態、シーン遷移、JSONロード
│   ├── GameScene/
│   │   ├── GridManager.cs             # 20x20グリッド、スマートターゲット検索
│   │   ├── PigController.cs           # シューター状態マシン、事前計算ショット
│   │   ├── CellController.cs          # セルライフサイクル
│   │   ├── BeltWalker.cs              # ベルト移動、2倍速サポート
│   │   ├── ReadyQueueManager.cs       # 5スロットキュー
│   │   ├── ShooterTableManager.cs     # 5x6テーブル
│   │   ├── BulletController.cs        # 弾丸飛行
│   │   └── BeltPathHolder.cs          # ウェイポイントコンテナ
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs         # 勝利/ゲームオーバーポップアップ
│   │   ├── SceneFader.cs              # シーンフェード
│   │   └── SplashController.cs        # スプラッシュスクリーン
│   └── Level/
│       └── LevelDataGenerator.cs      # エディタレベル生成ツール
├── Resources/
│   └── Levels/                        # JSONレベルデータ
└── Scenes/
    ├── SplashScene.unity
    ├── MenuScene.unity
    └── GameScene.unity
```

## 動作環境

- Unity 2021.3 LTS 以降
- TextMeshPro パッケージ

## はじめに

1. リポジトリをクローン
2. Unity でプロジェクトを開く
3. `Assets/Scenes/SplashScene.unity`を開く
4. プレイを押す

---

<div align="center">

**Built with Unity**

</div>
