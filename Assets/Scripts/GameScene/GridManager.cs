// ================================================================================
// TL;DR:
// 网格系统核心管理器，负责 20x20 方块网格的生成、智能目标查找和胜利条件检测。
// 采用 O(1) 二维数组存储 + 智能分区穿透查找算法，支持颜色阻挡判定。
//
// 目标：
// - 从 JSON 动态生成关卡网格，支持多颜色方块 (red/blue/green/yellow)
// - 提供 O(1) 的二维数组访问性能
// - 实现智能穿透查找（GetTargetCellSmart），支持颜色阻挡 + isPendingDeath 穿透
// - 支持传送带预计算（GetSimulatedPosition），返回传送带任意步数的理论坐标
// - 检测方块全部消除时触发胜利条件
//
// 非目标：
// - 不处理射手逻辑（由 PigController 负责）
// - 不处理子弹飞行（由 BulletController 负责）
// - 不处理 UI 显示（由各 UI 组件负责）
// ================================================================================
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    // ==================== 单例模式 ====================
    public static GridManager Instance;

    // ==================== 网格配置 ====================
    [Header("设置")]
    public int gridSize = 20;           // 网格大小 (20x20)
    public float cellSize = 1.0f;       // 单格世界坐标尺寸
    public int activeCellCount = 0;     // 当前存活方块数，归零触发胜利
    private Vector2 gridOrigin;         // 网格左下角 (0,0) 的世界坐标

    // ==================== 资源引用 ====================
    [Header("资源")]
    public GameObject cellPrefab;                                               // 方块预制体
    public Material[] colorMaterials;                                           // 颜色材质数组 (按 colorNames 顺序)
    public string[] colorNames = new string[] { "red", "blue", "green", "yellow" }; // 颜色ID映射

    // ==================== 核心数据结构 ====================
    private CellController[,] logicGrid;  // 二维数组存储，O(1) 随机访问

    // ==================== 生命周期 ====================
    void Awake() { Instance = this; }
    void Start() { GenerateGrid(); }

    // ==================== 网格生成 ====================
    /// <summary>
    /// 从 JSON 数据生成网格，初始化 logicGrid 二维数组
    /// </summary>
    void GenerateGrid()
    {
        activeCellCount = 0;
        logicGrid = new CellController[gridSize, gridSize];
        
        // 计算居中偏移，使网格中心在 transform.position
        float totalWidth = gridSize * cellSize;
        float startOffset = totalWidth / 2.0f - (cellSize / 2.0f);
        gridOrigin = new Vector2(transform.position.x - startOffset, transform.position.y - startOffset);

        // 从 GameManager 加载关卡 JSON 数据
        LevelGridData data = null;
        if (GameManager.Instance != null) data = GameManager.Instance.LoadGridData();

        if (data == null || data.cells == null)
        {
            Debug.LogWarning("GridManager: Empty Grid Generated.");
            return;
        }

        // 遍历 JSON 数据，实例化方块
        foreach (CellData cellData in data.cells)
        {
            if (cellData.x >= gridSize || cellData.y >= gridSize) continue;

            float posX = (cellData.x * cellSize) - startOffset;
            float posY = (cellData.y * cellSize) - startOffset;
            Vector3 spawnPos = new Vector3(posX, posY, 0) + transform.position;

            GameObject newObj = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);
            CellController cell = newObj.GetComponent<CellController>();
            
            Material mat = GetMaterialByColorID(cellData.color);
            if (mat == null && colorMaterials.Length > 0) mat = colorMaterials[0];

            cell.Init(cellData.color, mat);
            logicGrid[cellData.x, cellData.y] = cell;
            activeCellCount++;
        }
    }

    // ==========================================
    // 【核心算法】智能穿透查找 + 颜色阻挡判定
    // ==========================================
    // 功能：根据射手位置和颜色，智能查找可命中的目标方块
    //
    // 查找规则：
    //   1. 根据射手位置判断所在区域 (底部/右侧/顶部/左侧)
    //   2. 向网格内部扫描，遇到的第一个方块：
    //      - 如果颜色匹配 -> 返回目标
    //      - 如果颜色不匹配 -> 被阻挡，返回 null
    //      - 如果 isPendingDeath=true -> 穿透，继续查找
    //      - 如果 isDestroyed=true -> 穿透，继续查找
    //
    // 参数：
    //   shooterPos - 射手世界坐标
    //   shooterColor - 射手颜色ID，用于阻挡判定
    //
    // 返回：
    //   匹配的目标方块，或 null（无目标/被阻挡）
    // ==========================================
    public CellController GetTargetCellSmart(Vector3 shooterPos, string shooterColor)
    {
        // 1. 坐标计算
        float rawX = (shooterPos.x - gridOrigin.x) / cellSize;
        float rawY = (shooterPos.y - gridOrigin.y) / cellSize;
        int xInt = Mathf.RoundToInt(rawX);
        int yInt = Mathf.RoundToInt(rawY);

        // 定义一个局部委托，用于统一处理查找逻辑 (减少重复代码)
        // 返回值：0=继续找, 1=找到目标, 2=被挡住停止
        System.Func<int, int, int> CheckCell = (x, y) => 
        {
            CellController c = logicGrid[x, y];
            
            // 1. 如果是空的或已物理销毁 -> 视为空气，继续找
            if (c == null || c.isDestroyed) return 0;

            // 2. 【预计算穿透】如果它还没死，但在未来会被前面的猪打死 (PendingDeath) 
            // -> 视为空气，继续找 (这就是我们要的"流水线"效果)
            if (c.isPendingDeath) return 0;

            // 3. 【阻挡判定 - 修复 Issue 1】
            // 如果方块活着，且没被预定，但颜色不对 -> 它是墙！停止查找！
            if (c.colorID != shooterColor) return 2; // Stop

            // 4. 颜色匹配，且活着 -> 它是目标！
            return 1; // Found
        };

        // --- 区域 A：底部 (Bottom) ---
        if (rawY < -0.4f) 
        {
            int xClamped = Mathf.Clamp(xInt, 0, gridSize - 1);
            for (int y = 0; y < gridSize; y++) // 从下往上
            {
                int result = CheckCell(xClamped, y);
                if (result == 1) return logicGrid[xClamped, y];
                if (result == 2) return null; // 被异色方块挡住，直接返回 null
            }
        }
        // --- 区域 B：右侧 (Right) ---
        else if (rawX > (gridSize - 1) + 0.4f)
        {
            int yClamped = Mathf.Clamp(yInt, 0, gridSize - 1);
            for (int x = gridSize - 1; x >= 0; x--) // 从右往左
            {
                int result = CheckCell(x, yClamped);
                if (result == 1) return logicGrid[x, yClamped];
                if (result == 2) return null;
            }
        }
        // --- 区域 C：顶部 (Top) ---
        else if (rawY > (gridSize - 1) + 0.4f)
        {
            int xClamped = Mathf.Clamp(xInt, 0, gridSize - 1);
            for (int y = gridSize - 1; y >= 0; y--) // 从上往下
            {
                int result = CheckCell(xClamped, y);
                if (result == 1) return logicGrid[xClamped, y];
                if (result == 2) return null;
            }
        }
        // --- 区域 D：左侧 (Left) ---
        else if (rawX < -0.4f)
        {
            int yClamped = Mathf.Clamp(yInt, 0, gridSize - 1);
            for (int x = 0; x < gridSize; x++) // 从左往右
            {
                int result = CheckCell(x, yClamped);
                if (result == 1) return logicGrid[x, yClamped];
                if (result == 2) return null;
            }
        }

        return null; // 没找到任何东西
    }

    // ==========================================
    // 【预计算支持】传送带步数 -> 世界坐标
    // ==========================================
    // 将传送带逻辑步数索引 (0-79) 转换为世界坐标
    // 用于 PigController.PreCalculatePath() 预计算射击路径
    //
    // 传送带布局 (逆时针，共80步):
    //   0~19:  Bottom (从左到右，y=-1)
    //   20~39: Right  (从下到上，x=20)
    //   40~59: Top    (从右到左，y=20)
    //   60~79: Left   (从上到下，x=-1)
    // ==========================================
    public Vector3 GetSimulatedPosition(int beltIndex)
    {
        int sideLength = gridSize;
        int x = 0; int y = 0;

        if (beltIndex < sideLength) 
        { 
            x = beltIndex; y = -1;  // Bottom: 从左到右
        }
        else if (beltIndex < sideLength * 2) 
        { 
            x = sideLength; y = beltIndex - sideLength;  // Right: 从下到上
        }
        else if (beltIndex < sideLength * 3) 
        { 
            int localIndex = beltIndex - sideLength * 2; 
            x = (sideLength - 1) - localIndex; y = sideLength;  // Top: 从右到左
        }
        else 
        { 
            int localIndex = beltIndex - sideLength * 3; 
            x = -1; y = (sideLength - 1) - localIndex;  // Left: 从上到下
        }

        // 逻辑坐标 -> 世界坐标
        float worldX = gridOrigin.x + (x * cellSize);
        float worldY = gridOrigin.y + (y * cellSize);
        return new Vector3(worldX, worldY, transform.position.z);
    }

    // ==================== 工具方法 ====================
    /// <summary>
    /// 根据颜色ID获取对应材质
    /// </summary>
    public Material GetMaterialByColorID(string colorID)
    {
        for (int i = 0; i < colorNames.Length; i++)
        {
            if (colorNames[i] == colorID && i < colorMaterials.Length) return colorMaterials[i];
        }
        return colorMaterials.Length > 0 ? colorMaterials[0] : null;
    }

    // ==================== 胜利条件检测 ====================
    /// <summary>
    /// 方块销毁回调，由 CellController.OnHit() 调用
    /// 当 activeCellCount 归零时触发胜利
    /// </summary>
    public void OnCellDestroyed()
    {
        activeCellCount--;
        if (activeCellCount <= 0)
        {
            activeCellCount = 0;
            if (GameManager.Instance != null) GameManager.Instance.GameOver(true);
        }
    }
}