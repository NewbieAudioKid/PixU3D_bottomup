// ================================================================================
// TL;DR:
// 网格系统核心管理器，负责 20x20 方块网格的生成、查询和生命周期管理。
// 采用智能分区查找算法解决传送带射击的拐角漏射和边缘盲区问题。
//
// 目标：
// - 从 JSON 动态生成关卡网格，支持多颜色方块
// - 提供 O(1) 的二维数组访问性能
// - 实现智能目标查找（GetTargetCellSmart），根据射手位置自动推断扫描方向
// - 支持传送带预计算（GetSimulatedPosition），用于射击路径规划
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
    // 单例模式
    public static GridManager Instance;

    [Header("设置")]
    public int gridSize = 20;
    public float cellSize = 1.0f;
    public int activeCellCount = 0;
    // 核心坐标基准：记录网格左下角 (0,0) 格子的中心点在世界空间的坐标
    private Vector2 gridOrigin;

    [Header("资源")]
    public GameObject cellPrefab;
    public Material[] colorMaterials;
    public string[] colorNames = new string[] { "red", "blue", "green", "yellow" };

    // 核心数据结构：二维数组，O(1) 访问
    private CellController[,] logicGrid;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {// 1. 重置计数器 (防止上一关的数据残留)
        activeCellCount = 0;
        logicGrid = new CellController[gridSize, gridSize];
        
        // 1. 计算偏移量，让 Grid 居中生成
        float totalWidth = gridSize * cellSize;
        float startOffset = totalWidth / 2.0f - (cellSize / 2.0f);
        
        // 【核心修复 1】正确保存 gridOrigin 到成员变量
        // 这样 GetTargetCellSmart 才能算出正确的相对坐标
        gridOrigin = new Vector2(transform.position.x - startOffset, transform.position.y - startOffset);

        // 2. 获取数据 (JSON)
        LevelGridData data = null;
        if (GameManager.Instance != null)
        {
            data = GameManager.Instance.LoadGridData();
        }

        if (data == null || data.cells == null)
        {
            Debug.LogWarning("GridManager: 未加载到关卡数据，生成空网格。请从 MenuScene 进入游戏。");
            return;
        }

        // 3. 根据 JSON 生成方块
        foreach (CellData cellData in data.cells)
        {
            // 安全检查
            if (cellData.x >= gridSize || cellData.y >= gridSize) continue;

            // 计算生成位置
            float posX = (cellData.x * cellSize) - startOffset;
            float posY = (cellData.y * cellSize) - startOffset;
            Vector3 spawnPos = new Vector3(posX, posY, 0) + transform.position;

            // 实例化
            GameObject newObj = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);
            CellController cell = newObj.GetComponent<CellController>();
            
            // 材质查找
            Material mat = GetMaterialByColorID(cellData.color);
            if (mat == null && colorMaterials.Length > 0) mat = colorMaterials[0]; // 默认材质防隐形

            cell.Init(cellData.color, mat);

            // 存入逻辑数组
            logicGrid[cellData.x, cellData.y] = cell;
            // 【新增】生成一个，计数加 1
            activeCellCount++;
        }
    }

    // ==========================================
    //  【核心修复 2】智能分区查找算法 (Smart Lookup)
    //  仿 React 逻辑：根据坐标所在的绝对区域，强制钳制 (Clamp) 查询范围
    //  解决拐角漏射、边缘盲区问题
    // ==========================================
    public CellController GetTargetCellSmart(Vector3 shooterPos)
    {
        // 1. 算出相对 Grid 原点的“浮点数”坐标
        float rawX = (shooterPos.x - gridOrigin.x) / cellSize;
        float rawY = (shooterPos.y - gridOrigin.y) / cellSize;

        // 2. 转成整数，用于后续计算
        int xInt = Mathf.RoundToInt(rawX);
        int yInt = Mathf.RoundToInt(rawY);

        // 3. 区域判定逻辑
        // 容差值 0.4f 用于处理浮点数边界，确保刚过线就能被识别

        // --- 区域 A：底部 (Bottom) ---
        // 特征：Y 坐标明显小于 0
        if (rawY < -0.4f) 
        {
            // 强制 X 在 0-19 之间 (即使物理坐标偏出去了，也算作边缘列)
            int xClamped = Mathf.Clamp(xInt, 0, gridSize - 1);
            
            // 从下往上找 (Up)
            for (int y = 0; y < gridSize; y++)
            {
                CellController c = logicGrid[xClamped, y];
                if (IsValidTarget(c)) return c;
            }
        }
        // --- 区域 B：右侧 (Right) ---
        // 特征：X 坐标明显大于 19
        else if (rawX > (gridSize - 1) + 0.4f)
        {
            // 强制 Y 在 0-19 之间
            int yClamped = Mathf.Clamp(yInt, 0, gridSize - 1);

            // 从右往左找 (Left)
            for (int x = gridSize - 1; x >= 0; x--)
            {
                CellController c = logicGrid[x, yClamped];
                if (IsValidTarget(c)) return c;
            }
        }
        // --- 区域 C：顶部 (Top) ---
        // 特征：Y 坐标明显大于 19
        else if (rawY > (gridSize - 1) + 0.4f)
        {
            int xClamped = Mathf.Clamp(xInt, 0, gridSize - 1);

            // 从上往下找 (Down)
            for (int y = gridSize - 1; y >= 0; y--)
            {
                CellController c = logicGrid[xClamped, y];
                if (IsValidTarget(c)) return c;
            }
        }
        // --- 区域 D：左侧 (Left) ---
        // 特征：X 坐标明显小于 0
        else if (rawX < -0.4f)
        {
            int yClamped = Mathf.Clamp(yInt, 0, gridSize - 1);

            // 从左往右找 (Right)
            for (int x = 0; x < gridSize; x++)
            {
                CellController c = logicGrid[x, yClamped];
                if (IsValidTarget(c)) return c;
            }
        }

        return null;
    }
// ==========================================
    // 【核心新增】根据传送带的逻辑索引，计算出物理坐标
    // 用于小猪进场时的预计算模拟
    // beltIndex: 0 到 (gridSize * 4 - 1)
    // ==========================================
    public Vector3 GetSimulatedPosition(int beltIndex)
    {
        // 传送带围绕 Grid 一圈，总长度是 4 条边
        // 0 ~ 19: Bottom (从左到右)
        // 20 ~ 39: Right (从下到上)
        // 40 ~ 59: Top (从右到左)
        // 60 ~ 79: Left (从上到下)

        int sideLength = gridSize;
        int x = 0;
        int y = 0;

        if (beltIndex < sideLength) // Bottom
        {
            x = beltIndex;
            y = -1; // 强制在下方
        }
        else if (beltIndex < sideLength * 2) // Right
        {
            x = sideLength; // 强制在右侧
            y = beltIndex - sideLength;
        }
        else if (beltIndex < sideLength * 3) // Top
        {
            // 注意：Top 是逆时针，所以 X 是从大到小
            // 比如 index 40 对应 x=19, index 59 对应 x=0
            int localIndex = beltIndex - sideLength * 2;
            x = (sideLength - 1) - localIndex;
            y = sideLength; // 强制在上方
        }
        else // Left
        {
            // Left 是逆时针，所以 Y 是从大到小
            int localIndex = beltIndex - sideLength * 3;
            x = -1; // 强制在左侧
            y = (sideLength - 1) - localIndex;
        }

        // 把逻辑坐标 (x, y) 转回世界坐标
        // 公式逆推：World = Origin + (Index * CellSize)
        float worldX = gridOrigin.x + (x * cellSize);
        float worldY = gridOrigin.y + (y * cellSize);

        // Z 轴保持不变，假设 GridManager 的 Z 就是基准
        return new Vector3(worldX, worldY, transform.position.z);
    }
    // ==========================================
    //  【核心修复 3】物理阻挡判定
    // ==========================================
    bool IsValidTarget(CellController c)
    {
        // 只要方块还没彻底销毁 (isDestroyed == false)，它就是障碍物。
        // 即使它 isPendingDeath (快死了)，也必须返回它！
        // 这样 PigController 看到它快死了，就会停火等待，而不是穿透它打后面的。
        return c != null && !c.isDestroyed;
    }

    // 工具方法：根据颜色名获取材质
    public Material GetMaterialByColorID(string colorID)
    {
        for (int i = 0; i < colorNames.Length; i++)
        {
            if (colorNames[i] == colorID)
            {
                if (i < colorMaterials.Length) return colorMaterials[i];
            }
        }
        // 如果找不到，尝试返回第一个材质，避免粉色丢失
        if (colorMaterials.Length > 0) return colorMaterials[0];
        return null;
    }


    // ==========================================
    // 【新增】方块销毁回调
    // ==========================================
    public void OnCellDestroyed()
    {
        activeCellCount--;

        // 检查是否胜利
        if (activeCellCount <= 0)
        {
            // 防止减到负数
            activeCellCount = 0;
            
            // 通知 GameManager 胜利
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver(true); // true 代表胜利
            }
        }
    }
}