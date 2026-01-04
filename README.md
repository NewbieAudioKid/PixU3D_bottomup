<div align="center">

# PixelFlow

**A 2D Puzzle Shooter Game**

一款2D益智射击游戏 | 2Dパズルシューティングゲーム

---

[![Unity](https://img.shields.io/badge/Unity-2021.3+-000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](LICENSE)

[English](#english) | [中文](#中文) | [日本語](#日本語)

</div>

---

<a name="english"></a>

## Overview

PixelFlow is a strategic puzzle game where colored shooters patrol a conveyor belt, automatically firing at matching cells. Players must strategically deploy shooters from their inventory to clear all cells from the grid.

### Gameplay

- **Select**: Click shooters from the table to queue (5 slots max)
- **Deploy**: Click queued shooter to start belt patrol
- **Auto-Fire**: Shooters automatically target matching cells
- **Win**: Clear all cells from the grid
- **Lose**: Ready queue is full when a shooter returns

### Last Stand Mechanic

When both the shooter table and ready queue are empty, the current shooter triggers "Last Stand" mode with 2x speed bonus, automatically re-entering the conveyor belt until ammunition depletes.

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameManager.cs              # Global state, scene transitions
│   ├── GameScene/
│   │   ├── GridManager.cs          # 20x20 cell grid
│   │   ├── PigController.cs        # Shooter state machine
│   │   ├── CellController.cs       # Grid cell logic
│   │   ├── BeltWalker.cs           # Belt movement
│   │   ├── ReadyQueueManager.cs    # 5-slot queue
│   │   ├── ShooterTableManager.cs  # Shooter inventory
│   │   └── BeltPathHolder.cs       # Waypoints
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs      # Victory/GameOver modal
│   │   ├── ElasticButton.cs        # Animated buttons
│   │   └── SceneFader.cs           # Scene transitions
│   └── Level/
│       └── LevelDataGenerator.cs   # Editor tool
├── Resources/
│   └── Levels/                     # JSON level data
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

## Documentation

Full technical documentation: [PixelFlow Docs](https://newbieaudiokid.github.io/PixU3D_bottomup/)

---

<a name="中文"></a>

## 概述

PixelFlow 是一款策略益智游戏，彩色射手在传送带上巡逻，自动射击匹配颜色的方块。玩家需要策略性地从库存中部署射手，清除网格中的所有方块。

### 玩法

- **选择**: 点击备战台射手加入队列（最多5个）
- **部署**: 点击队列中的射手开始传送带巡逻
- **自动射击**: 射手自动瞄准匹配颜色的方块
- **胜利**: 清除网格中所有方块
- **失败**: 射手返回时队列已满

### 绝地反击机制

当备战台和准备队列都为空时，当前射手触发"绝地反击"模式，获得2倍速度加成，自动重新进入传送带直到弹药耗尽。

## 项目结构

```
Assets/
├── Scripts/
│   ├── GameManager.cs              # 全局状态、场景切换
│   ├── GameScene/
│   │   ├── GridManager.cs          # 20x20方块网格
│   │   ├── PigController.cs        # 射手状态机
│   │   ├── CellController.cs       # 网格方块逻辑
│   │   ├── BeltWalker.cs           # 传送带移动
│   │   ├── ReadyQueueManager.cs    # 5槽位队列
│   │   ├── ShooterTableManager.cs  # 射手库存
│   │   └── BeltPathHolder.cs       # 路径点
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs      # 胜利/失败弹窗
│   │   ├── ElasticButton.cs        # 动画按钮
│   │   └── SceneFader.cs           # 场景过渡
│   └── Level/
│       └── LevelDataGenerator.cs   # 编辑器工具
├── Resources/
│   └── Levels/                     # JSON关卡数据
└── Scenes/
    ├── SplashScene.unity
    ├── MenuScene.unity
    └── GameScene.unity
```

## 系统要求

- Unity 2021.3 LTS 或更高版本
- TextMeshPro 包

## 快速开始

1. 克隆仓库
2. 在 Unity 中打开项目
3. 打开 `Assets/Scenes/SplashScene.unity`
4. 点击播放

## 文档

完整技术文档: [PixelFlow 文档](https://newbieaudiokid.github.io/PixU3D_bottomup/)

---

<a name="日本語"></a>

## 概要

PixelFlow は戦略パズルゲームです。色付きシューターがコンベアベルトを巡回し、一致するセルに自動的に発射します。プレイヤーはインベントリからシューターを戦略的に配置し、グリッドからすべてのセルをクリアする必要があります。

### ゲームプレイ

- **選択**: テーブルからシューターをクリックしてキューに追加（最大5スロット）
- **配置**: キュー内のシューターをクリックしてベルトパトロールを開始
- **自動発射**: シューターは一致するセルを自動的にターゲット
- **勝利**: グリッドからすべてのセルをクリア
- **敗北**: シューターが戻ったときキューが満杯

### ラストスタンドメカニクス

シューターテーブルと待機キューの両方が空のとき、現在のシューターは「ラストスタンド」モードを発動し、2倍のスピードボーナスを得て、弾薬が尽きるまで自動的にコンベアベルトに再突入します。

## プロジェクト構造

```
Assets/
├── Scripts/
│   ├── GameManager.cs              # グローバル状態、シーン遷移
│   ├── GameScene/
│   │   ├── GridManager.cs          # 20x20セルグリッド
│   │   ├── PigController.cs        # シューター状態マシン
│   │   ├── CellController.cs       # グリッドセルロジック
│   │   ├── BeltWalker.cs           # ベルト移動
│   │   ├── ReadyQueueManager.cs    # 5スロットキュー
│   │   ├── ShooterTableManager.cs  # シューターインベントリ
│   │   └── BeltPathHolder.cs       # ウェイポイント
│   ├── UIScripts/
│   │   ├── GameResultPopup.cs      # 勝利/ゲームオーバーモーダル
│   │   ├── ElasticButton.cs        # アニメーションボタン
│   │   └── SceneFader.cs           # シーントランジション
│   └── Level/
│       └── LevelDataGenerator.cs   # エディタツール
├── Resources/
│   └── Levels/                     # JSONレベルデータ
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
2. Unityでプロジェクトを開く
3. `Assets/Scenes/SplashScene.unity`を開く
4. プレイを押す

## ドキュメント

完全な技術ドキュメント: [PixelFlow ドキュメント](https://newbieaudiokid.github.io/PixU3D_bottomup/)

---

<div align="center">

**Built with Unity**

</div>
