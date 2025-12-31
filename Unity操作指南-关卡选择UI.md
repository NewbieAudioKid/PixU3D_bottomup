# Unity 操作指南 - 关卡选择 UI 复刻

## 📋 准备工作

### 1. 打开场景

1. 在 Unity 中，找到 **Project 窗口**
2. 导航到 `Assets/Scenes/`
3. 双击 `LevelSelectUI.unity` 打开场景

### 2. 资源路径说明

本教程使用的所有图片资源的绝对路径如下：

**Picto Icons (128x128)**

- 设置图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Gear.Png`
- 加号图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Plus.Png`
- 金币图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Coin_Star.Png`
- 生命值图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Life.Png`
- 禁止图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Nostop.Png`
- 商店图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Shop_0.Png`
- 锁图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Lock.Png`

**Item Icons (128x128)**

- 心形图标: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/128/Icon_Heart.png`
- 金色钥匙: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/128/Icon_Key_Gold.png`

**Item Icons (256x256) - 高清版本**

- 蓝色六边形: `/Users/benz/Desktop/Stanford/FA25/Guru/PixelFlow_prototype2/My project (2)/Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/256/Icon_Gem02_Hexagon_Blue.png`

**在 Unity 中使用时的简短路径（搜索时使用）**

- 只需在 Unity 的搜索框中输入文件名（如 `Pictoicon_Gear`），Unity 会自动找到资源
- 或者在 Project 窗口中导航到 `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/` 目录

---

## 🎨 第一步：创建蓝色背景

### 操作步骤：

1. 在 **Hierarchy** 窗口中，右键点击 `Canvas`
2. 选择 `UI → Image`，命名为 `Background`

3. 在 **Inspector** 窗口中设置：

   - **RectTransform**:

     - 点击 Anchor Presets（左上角的小方框）
     - 按住 **Shift + Alt**，点击右下角的 **拉伸全屏** 图标
     - 所有边距设为 `0`

   - **Image** 组件:
     - **Color**: RGB(49, 103, 192) - 深蓝色
       - R: 0.19
       - G: 0.40
       - B: 0.75
       - A: 1

4. 在 Hierarchy 中，拖动 `Background` 到最上面（第一个子对象），这样它在最底层

---

## 🔝 第二步：创建顶部栏 (TopBar)

### 1. 创建容器

1. 右键 `Canvas` → `Create Empty`，命名为 `TopBar`
2. 在 Inspector 中，**Add Component** → **Rect Transform**（如果还没有）
3. 设置 **RectTransform**:
   - Anchor: **顶部拉伸** (点击 Anchor Presets，按住 **Alt**，点击顶部中间的拉伸图标)
   - **Height**: 150
   - **Top**: 0
   - **Left**: 0
   - **Right**: 0

### 2. 添加设置按钮（左上角齿轮）

1. 右键 `TopBar` → `UI → Button`，命名为 `SettingsButton`
2. 设置 **RectTransform**:

   - **Anchor**: 左上角 (0, 1), (0, 1)
   - **Position**: X: 80, Y: -80
   - **Width**: 100, **Height**: 100

3. 删除按钮的 `Text` 子对象（不需要）

4. 选中 `SettingsButton`，添加图标：
   - 右键 → `UI → Image`，命名为 `Icon`
   - 在 Inspector 中：
     - 点击 **Image** 组件的 **Source Image** 右边的圆圈
     - 在弹出窗口中搜索 `Pictoicon_Gear`
     - 选择 `Pictoicon_Gear.Png`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Gear.Png`
   - **RectTransform**: 拉伸填充父对象（Anchor Presets → 右下角拉伸全屏，边距都为 10）

### 3. 添加生命值显示

1. 右键 `TopBar` → `UI → Image`，命名为 `HealthDisplay`
2. 设置位置：

   - **Anchor**: 左上角
   - **Position**: X: 240, Y: -80
   - **Width**: 150, **Height**: 80

3. 设置图标：

   - **Source Image**: 搜索并选择 `Icon_Heart` 或 `Pictoicon_Life`
     - 推荐路径: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/128/Icon_Heart.png`
     - 或: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Life.Png`
   - **Color**: 红色 (R:1, G:0.3, B:0.3, A:1)
   - **Preserve Aspect**: 勾选

4. 添加数字文本：
   - 右键 `HealthDisplay` → `UI → Text - TextMeshPro`
   - 如果提示导入 TMP Essentials，点击 **Import TMP Essentials**
   - 命名为 `HealthText`
   - 设置：
     - **Text**: "4"
     - **Font Size**: 48
     - **Alignment**: 居中对齐
     - **Color**: 白色
     - **Font Style**: Bold（粗体）

### 4. 添加计时器文本

1. 右键 `TopBar` → `UI → Text - TextMeshPro`，命名为 `TimerText`
2. 设置：
   - **Position**: X: 360, Y: -80
   - **Width**: 150, **Height**: 60
   - **Text**: "29:58"
   - **Font Size**: 40
   - **Alignment**: 居中对齐
   - **Color**: 白色

### 5. 添加加号按钮（生命值旁边）

1. 右键 `TopBar` → `UI → Button`，命名为 `AddHealthButton`
2. 设置：

   - **Anchor**: 左上角
   - **Position**: X: 440, Y: -80
   - **Width**: 80, **Height**: 80

3. 设置按钮背景色：

   - 选中 `AddHealthButton`
   - **Image** 组件 → **Color**: RGB(1, 0.4, 0.6) - 粉红色

4. 添加加号图标：
   - 右键 `AddHealthButton` → `UI → Image`，命名为 `Icon`
   - **Source Image**: `Pictoicon_Plus`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Plus.Png`
   - 拉伸填充，边距 15

### 6. 添加金币显示（右上角）

1. 右键 `TopBar` → `UI → Image`，命名为 `CoinDisplay`
2. 设置：

   - **Anchor**: 右上角 (1, 1), (1, 1)
   - **Position**: X: -280, Y: -80
   - **Width**: 200, **Height**: 80

3. 设置金币图标：

   - **Source Image**: `Pictoicon_Coin_Star`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Coin_Star.Png`
   - **Color**: 金黄色
   - **Preserve Aspect**: 勾选

4. 添加金币数量文本：
   - 右键 `CoinDisplay` → `UI → Text - TextMeshPro`
   - 命名为 `CoinText`
   - 设置：
     - **Text**: "1370"
     - **Font Size**: 44
     - **Alignment**: 居中
     - **Color**: 白色
     - **Font Style**: Bold

### 7. 添加金币加号按钮

1. 右键 `TopBar` → `UI → Button`，命名为 `AddCoinButton`
2. 设置：

   - **Anchor**: 右上角
   - **Position**: X: -80, Y: -80
   - **Width**: 80, **Height**: 80
   - **Color**: RGB(1, 0.7, 0.2) - 橙黄色

3. 添加加号图标（同上）

---

## 🔑 第三步：创建左侧钥匙区域

### 1. 创建钥匙显示

1. 右键 `Canvas` → `Create Empty`，命名为 `LeftSide`

2. 右键 `LeftSide` → `UI → Image`，命名为 `KeyDisplay`
3. 设置：

   - **Anchor**: 左上角
   - **Position**: X: 150, Y: -350
   - **Width**: 180, **Height**: 180

4. 添加背景圆形：

   - 右键 `KeyDisplay` → `UI → Image`，命名为 `Background`
   - **Color**: 半透明白色 RGB(1, 1, 1, 0.3)
   - 拉伸填充

5. 添加钥匙图标：

   - 右键 `KeyDisplay` → `UI → Image`，命名为 `KeyIcon`
   - **Source Image**: `Icon_Key_Gold`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/128/Icon_Key_Gold.png`
   - **Color**: 金黄色
   - **Width**: 120, **Height**: 120

6. 添加进度文本：

   - 右键 `KeyDisplay` → `UI → Text - TextMeshPro`，命名为 `ProgressText`
   - 设置：
     - **Position**: Y: -80
     - **Text**: "1/10"
     - **Font Size**: 36
     - **Alignment**: 居中
     - **Color**: 白色

7. 添加徽章数字（右上角的绿色数字 4）：
   - 右键 `KeyDisplay` → `UI → Image`，命名为 `Badge`
   - 设置：
     - **Anchor**: 右上角
     - **Position**: X: 30, Y: 30
     - **Width**: 50, **Height**: 50
     - **Color**: 绿色 RGB(0.2, 0.8, 0.3)
   - 添加子对象 Text，显示"4"

---

## 🚫 第四步：创建右上角 NO ADS 区域

1. 右键 `Canvas` → `Create Empty`，命名为 `RightSide`

2. 右键 `RightSide` → `UI → Button`，命名为 `NoAdsButton`
3. 设置：

   - **Anchor**: 右上角
   - **Position**: X: -150, Y: -350
   - **Width**: 180, **Height**: 200

4. 设置按钮：

   - **Color**: RGB(0.9, 0.2, 0.4) - 红粉色

5. 添加禁止图标：

   - 右键 `NoAdsButton` → `UI → Image`，命名为 `ProhibitIcon`
   - **Source Image**: `Pictoicon_Nostop`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Nostop.Png`
     - 如果找不到，可以手动创建圆圈+斜线的禁止符号
   - **Position**: Y: 30
   - **Width**: 100, **Height**: 100

6. 添加 "NO ADS" 文本：
   - 右键 `NoAdsButton` → `UI → Text - TextMeshPro`
   - 设置：
     - **Text**: "NO ADS"
     - **Position**: Y: -50
     - **Font Size**: 32
     - **Font Style**: Bold
     - **Color**: 白色
     - **Alignment**: 居中

---

## 🎯 第五步：创建中央关卡链

### 1. 创建容器

1. 右键 `Canvas` → `Create Empty`，命名为 `LevelChain`
2. 设置：
   - **Anchor**: 中心
   - **Position**: X: 0, Y: 0

### 2. 创建关卡 56 （顶部）

1. 右键 `LevelChain` → `UI → Image`，命名为 `Level56`
2. 设置：

   - **Position**: X: 0, Y: 300
   - **Width**: 180, **Height**: 180
   - **Source Image**: 搜索 `Icon_Gem02_Hexagon_Blue`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_ItemIcons/256/Icon_Gem02_Hexagon_Blue.png`
     - 也可搜索其他 `Hexagon` 相关的图标
   - **Color**: RGB(0.4, 0.6, 0.9) - 蓝色

3. 添加数字：
   - 右键 `Level56` → `UI → Text - TextMeshPro`，命名为 `LevelNumber`
   - 设置：
     - **Text**: "56"
     - **Font Size**: 72
     - **Font Style**: Bold
     - **Color**: 白色
     - **Alignment**: 居中
     - 拉伸填充父对象

### 3. 创建连接线

1. 右键 `LevelChain` → `UI → Image`，命名为 `Connection1`
2. 设置：
   - **Position**: X: 0, Y: 200
   - **Width**: 20, **Height**: 100
   - **Color**: RGB(0.9, 0.7, 0.2) - 金黄色

### 4. 创建关卡 55

1. 复制 `Level56`（Ctrl+D 或 Cmd+D）
2. 重命名为 `Level55`
3. 修改：
   - **Position**: Y: 100
   - 修改子对象 `LevelNumber` 的 Text 为 "55"

### 5. 创建第二条连接线

1. 复制 `Connection1`，命名为 `Connection2`
2. **Position**: Y: 0

### 6. 创建关卡 54（当前关卡 - 重点！）

1. 复制 `Level55`，命名为 `Level54`
2. 设置：

   - **Position**: Y: -200
   - **Width**: 220, **Height**: 220（比其他关卡大）
   - **Color**: RGB(0.9, 0.3, 0.3) - 红色

3. 修改数字为 "54"

4. 添加 "Very Hard" 标签：

   - 右键 `Level54` → `UI → Image`，命名为 `DifficultyLabel`
   - 设置：

     - **Position**: Y: -130
     - **Width**: 240, **Height**: 60
     - **Color**: RGB(0.8, 0.2, 0.2) - 深红色

   - 添加文本子对象：
     - **Text**: "Very Hard"
     - **Font Size**: 32
     - **Font Style**: Bold
     - **Color**: 白色

5. 添加发光效果（可选）：
   - 选中 `Level54`
   - **Add Component** → **Shadow**
   - 设置：
     - **Effect Distance**: X: 0, Y: 0
     - **Color**: RGB(1, 0.5, 0, 0.5) - 橙色半透明
     - **Effect Size**: X: 10, Y: 10

---

## 🎮 第六步：创建 Play 按钮

### 1. 创建按钮

1. 右键 `Canvas` → `UI → Button`，命名为 `PlayButton`
2. 设置：

   - **Anchor**: 底部中心（按住 Alt，点击底部中间）
   - **Position**: X: 0, Y: 400
   - **Width**: 500, **Height**: 150

3. 设置按钮样式：

   - **Color**: RGB(1, 0.8, 0.2) - 金黄色

4. 修改文本：

   - 展开 `PlayButton`，选中 `Text`
   - 在 Inspector 中删除 Text 组件
   - **Add Component** → `TextMeshProUGUI`
   - 设置：
     - **Text**: "Play"
     - **Font Size**: 80
     - **Font Style**: Bold
     - **Color**: 白色
     - **Alignment**: 居中

5. 添加阴影：
   - 选中 `PlayButton`
   - **Add Component** → **Shadow**
   - 设置：
     - **Effect Distance**: X: 0, Y: -8
     - **Color**: RGB(0.6, 0.4, 0, 1) - 深黄色

---

## 📱 第七步：创建底部导航栏

### 1. 创建导航栏容器

1. 右键 `Canvas` → `Create Empty`，命名为 `BottomNav`
2. 设置：

   - **Anchor**: 底部拉伸
   - **Position**: Y: 100
   - **Height**: 180

3. 添加背景：
   - 右键 `BottomNav` → `UI → Image`，命名为 `NavBackground`
   - 拉伸填充
   - **Color**: RGB(0.15, 0.15, 0.25) - 深色背景

### 2. 创建商店按钮（左侧）

1. 右键 `BottomNav` → `UI → Button`，命名为 `ShopButton`
2. 设置：

   - **Anchor**: 左下角
   - **Position**: X: 180, Y: 90
   - **Width**: 140, **Height**: 140

3. 添加图标：
   - 删除 Text 子对象
   - 右键 → `UI → Image`，命名为 `Icon`
   - **Source Image**: 搜索 `Pictoicon_Shop_0`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Shop_0.Png`
   - **Color**: 粉红色 RGB(1, 0.6, 0.7)
   - 拉伸填充，边距 15

### 3. 创建 Start 按钮（中间 - 高亮）

1. 右键 `BottomNav` → `UI → Button`，命名为 `StartButton`
2. 设置：

   - **Anchor**: 底部中心
   - **Position**: X: 0, Y: 110
   - **Width**: 160, **Height**: 160

3. 设置高亮样式：

   - **Source Image**: 搜索 `BorderFrame_Circle` 或类似圆形边框
     - 推荐路径: `Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/` 目录下的任意圆形边框
     - 或直接拖拽 Prefab: `BorderFrame_Circle81_White.prefab`
   - **Color**: RGB(0.3, 0.5, 0.9) - 蓝色高亮
   - **Image Type**: Sliced（如果有）

4. 添加角色图标：

   - 右键 → `UI → Image`，命名为 `CharacterIcon`
   - **Position**: Y: 20
   - **Width**: 100, **Height**: 100
   - 使用小猪或其他角色图标

5. 添加 "Start" 文本：
   - 右键 → `UI → Text - TextMeshPro`
   - **Text**: "Start"
   - **Position**: Y: -50
   - **Font Size**: 36
   - **Font Style**: Bold
   - **Color**: 白色

### 4. 创建锁按钮（右侧）

1. 右键 `BottomNav` → `UI → Button`，命名为 `LockedButton`
2. 设置：

   - **Anchor**: 右下角
   - **Position**: X: -180, Y: 90
   - **Width**: 140, **Height**: 140

3. 添加锁图标：
   - 删除 Text 子对象
   - 右键 → `UI → Image`，命名为 `Icon`
   - **Source Image**: `Pictoicon_Lock`
     - **完整路径**: `Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/Icon_PictoIcons/128/Pictoicon_Lock.Png`
   - **Color**: RGB(0.8, 0.8, 0.8) - 灰白色
   - 拉伸填充，边距 15

---

## ✨ 第八步：添加装饰和特效（可选）

### 1. 背景图案

可以在背景上添加一些装饰性图案（星星、几何图形等）：

1. 右键 `Background` → `UI → Image`，命名为 `Pattern`
2. 使用透明度很低的图案
3. 多个 Pattern 分散放置

### 2. 关卡 54 发光动画

1. 选中 `Level54`
2. **Add Component** → **Animator**
3. 创建动画：
   - **Window** → **Animation**
   - 点击 **Create**，保存为 `Level54Glow.anim`
   - 添加 Scale 或 Color 的关键帧动画

### 3. Play 按钮脉冲效果

同样方式为 PlayButton 添加轻微的缩放动画

---

## 🎨 颜色参考表

| 元素       | RGB 值          | 说明   |
| ---------- | --------------- | ------ |
| 背景       | (49, 103, 192)  | 深蓝色 |
| Play 按钮  | (255, 204, 51)  | 金黄色 |
| 关卡 54    | (230, 77, 77)   | 红色   |
| 关卡 55/56 | (102, 153, 230) | 蓝色   |
| 连接线     | (230, 180, 50)  | 金色   |
| 生命值+    | (255, 102, 153) | 粉红色 |
| 金币+      | (255, 178, 102) | 橙色   |

---

## 🔧 常用快捷键

- **复制对象**: Ctrl+D (Windows) / Cmd+D (Mac)
- **对齐父对象**: 选中后在 RectTransform 中点击 Anchor Presets
- **快速搜索资源**: 在 Project 窗口按 Ctrl+F
- **重命名**: F2 或双击名称
- **删除**: Delete
- **保存场景**: Ctrl+S / Cmd+S

---

## ✅ 检查清单

完成后，检查以下项目：

- [ ] 背景是蓝色且填充整个屏幕
- [ ] 顶部栏包含所有元素（设置、生命值、计时器、金币）
- [ ] 左侧钥匙显示正确
- [ ] 右上角 NO ADS 显示正确
- [ ] 三个关卡正确显示，54 是红色且最大
- [ ] Play 按钮醒目且居中
- [ ] 底部导航栏有三个按钮
- [ ] 所有文字清晰可读
- [ ] Canvas Scaler 设置正确（1080x1920）

---

## 🎯 完成！

恭喜！你已经完成了关卡选择 UI 的搭建。

### 下一步：

1. **测试不同分辨率**: 在 Game 窗口切换不同的分辨率测试
2. **添加交互**: 为按钮添加点击事件
3. **添加动画**: 使用 Animator 添加动画效果
4. **优化**: 调整间距和大小以获得最佳视觉效果

如果遇到问题，可以：

- 检查 Canvas Scaler 设置
- 确保所有图标都正确导入
- 检查 Anchor 和 Pivot 设置
- 查看 Inspector 中的警告信息

祝你成功！🎉
