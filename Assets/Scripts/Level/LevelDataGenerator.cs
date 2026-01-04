// ================================================================================
// TL;DR:
// 关卡数据生成器工具类，用于在 Unity 编辑器中快速生成关卡 JSON 配置文件。
// 采用 [ContextMenu] 实现右键菜单触发，避免运行时开销。
//
// 目标：
// - 提供可视化的关卡数据生成工具（右键组件 → 生成 JSON）
// - 通过代码逻辑生成复杂图案（边框、对角线、分区等）
// - 自动保存到 Resources/Levels/ 文件夹，供 GameManager 加载
// - 生成后自动刷新 AssetDatabase，确保 Unity 立即识别新文件
// - 确保每关的射手子弹数量和方块数量完全匹配
//
// 非目标：
// - 不在运行时使用（仅编辑器工具，不参与游戏逻辑）
// - 不提供可视化编辑器 UI（若需要可使用 EditorWindow 扩展）
// - 不验证关卡可玩性（生成的图案需要手动调整或测试）
// ================================================================================
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelDataGenerator : MonoBehaviour
{
    private int gridSize = 20;
    private string[] colors = new string[] { "red", "blue", "green", "yellow" };

    // 在 Unity 编辑器里右键点击这个组件 -> "Generate All 10 Levels" 即可触发
    [ContextMenu("Generate All 10 Levels")]
    public void GenerateAllLevels()
    {
        for (int level = 1; level <= 10; level++)
        {
            GenerateLevel(level);
        }
        Debug.Log("✅ 成功生成全部 10 关！");
    }

    // 生成单个关卡（Grid + Table）
    void GenerateLevel(int levelNumber)
    {
        // 1. 生成 Grid 数据和颜色统计
        LevelGridData gridData = GenerateGridPattern(levelNumber, out Dictionary<string, int> colorCounts);
        
        // 2. 根据颜色统计生成 Shooter Table
        ShooterTableData tableData = GenerateShooterTable(colorCounts);

        // 3. 保存文件
        SaveGridJSON(levelNumber, gridData);
        SaveTableJSON(levelNumber, tableData);

        // 4. 验证数量匹配
        VerifyBalance(levelNumber, colorCounts, tableData);
    }

    // ========================================
    // 生成 Grid 图案
    // ========================================
    LevelGridData GenerateGridPattern(int levelNumber, out Dictionary<string, int> colorCounts)
    {
        LevelGridData data = new LevelGridData();
        data.cells = new List<CellData>();
        colorCounts = new Dictionary<string, int>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                string colorID = "";

                // 根据关卡号选择不同图案
                switch (levelNumber)
                {
                    case 1: colorID = Pattern_BorderAndX(x, y); break;
                    case 2: colorID = Pattern_ConcentricSquares(x, y); break;
                    case 3: colorID = Pattern_Checkerboard(x, y); break;
                    case 4: colorID = Pattern_Stripes(x, y); break;
                    case 5: colorID = Pattern_Spiral(x, y); break;
                    case 6: colorID = Pattern_Circle(x, y); break;
                    case 7: colorID = Pattern_DiagonalStripes(x, y); break;
                    case 8: colorID = Pattern_Quadrants(x, y); break;
                    case 9: colorID = Pattern_Radial(x, y); break;
                    case 10: colorID = Pattern_Maze(x, y); break;
                    default: colorID = "red"; break;
                }

                // 添加 Cell
                CellData cell = new CellData();
                cell.x = x;
                cell.y = y;
                cell.color = colorID;
                data.cells.Add(cell);

                // 统计颜色数量
                if (!colorCounts.ContainsKey(colorID)) colorCounts[colorID] = 0;
                colorCounts[colorID]++;
            }
        }

        return data;
    }

    // ========================================
    // 图案 1: 边框 + X 对角线 + 四分区
    // ========================================
    string Pattern_BorderAndX(int x, int y)
    {
        // 蓝色边框
        if (x == 0 || x == gridSize - 1 || y == 0 || y == gridSize - 1)
            return "blue";
        // 红色对角线
        else if (x == y || x == (gridSize - 1 - y))
            return "red";
        // 上下区域绿色
        else if (Mathf.Abs(y - 9.5f) > Mathf.Abs(x - 9.5f))
            return "green";
        // 左右区域黄色
        else
            return "yellow";
    }

    // ========================================
    // 图案 2: 同心方框（从外到内：蓝→绿→黄→红）
    // ========================================
    string Pattern_ConcentricSquares(int x, int y)
    {
        int distFromEdge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(gridSize - 1 - x, gridSize - 1 - y));
        
        if (distFromEdge <= 2) return "blue";
        else if (distFromEdge <= 5) return "green";
        else if (distFromEdge <= 8) return "yellow";
        else return "red";
    }

    // ========================================
    // 图案 3: 棋盘格（4色交替）
    // ========================================
    string Pattern_Checkerboard(int x, int y)
    {
        int index = ((x / 5) + (y / 5)) % 4;
        return colors[index];
    }

    // ========================================
    // 图案 4: 横竖条纹
    // ========================================
    string Pattern_Stripes(int x, int y)
    {
        if (y % 10 < 5)
        {
            // 横条纹
            if (x % 10 < 5) return "red";
            else return "blue";
        }
        else
        {
            // 竖条纹
            if (x % 10 < 5) return "green";
            else return "yellow";
        }
    }

    // ========================================
    // 图案 5: 螺旋（简化版，用角度判断）
    // ========================================
    string Pattern_Spiral(int x, int y)
    {
        float centerX = 9.5f;
        float centerY = 9.5f;
        float dx = x - centerX;
        float dy = y - centerY;
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int colorIndex = Mathf.FloorToInt(angle / 90f) % 4;
        return colors[colorIndex];
    }

    // ========================================
    // 图案 6: 圆形（从中心到外：红→黄→绿→蓝）
    // ========================================
    string Pattern_Circle(int x, int y)
    {
        float centerX = 9.5f;
        float centerY = 9.5f;
        float dist = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

        if (dist < 4) return "red";
        else if (dist < 7) return "yellow";
        else if (dist < 10) return "green";
        else return "blue";
    }

    // ========================================
    // 图案 7: 对角条纹
    // ========================================
    string Pattern_DiagonalStripes(int x, int y)
    {
        int sum = (x + y) / 5;
        return colors[sum % 4];
    }

    // ========================================
    // 图案 8: 四象限（左上红、右上蓝、左下绿、右下黄）
    // ========================================
    string Pattern_Quadrants(int x, int y)
    {
        bool isLeft = x < 10;
        bool isBottom = y < 10;

        if (isLeft && isBottom) return "green";
        else if (isLeft && !isBottom) return "red";
        else if (!isLeft && isBottom) return "yellow";
        else return "blue";
    }

    // ========================================
    // 图案 9: 放射状（8个扇形区域）
    // ========================================
    string Pattern_Radial(int x, int y)
    {
        float centerX = 9.5f;
        float centerY = 9.5f;
        float dx = x - centerX;
        float dy = y - centerY;
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int sector = Mathf.FloorToInt(angle / 45f);
        if (sector % 2 == 0) return colors[sector / 2 % 4];
        else return colors[(sector + 1) / 2 % 4];
    }

    // ========================================
    // 图案 10: 迷宫风格（复杂混合）
    // ========================================
    string Pattern_Maze(int x, int y)
    {
        // 边框蓝色
        if (x == 0 || x == gridSize - 1 || y == 0 || y == gridSize - 1)
            return "blue";
        
        // 十字红色
        if (x == 10 || y == 10)
            return "red";
        
        // 对角线绿色
        if (Mathf.Abs(x - y) < 2 || Mathf.Abs(x + y - 19) < 2)
            return "green";
        
        // 其他黄色
        return "yellow";
    }

    // ========================================
    // 生成 Shooter Table（根据颜色统计分配射手）
    // ========================================
    ShooterTableData GenerateShooterTable(Dictionary<string, int> colorCounts)
    {
        ShooterTableData tableData = new ShooterTableData();
        tableData.columns = new List<ShooterColumn>();

        // 创建 5 列
        for (int col = 0; col < 5; col++)
        {
            ShooterColumn column = new ShooterColumn();
            column.shooters = new List<ShooterData>();
            tableData.columns.Add(column);
        }

        // 为每种颜色分配射手
        foreach (var kvp in colorCounts)
        {
            string color = kvp.Key;
            int totalAmmo = kvp.Value;

            // 将总弹药分配到多个射手中
            // 策略：分成 2-4 个射手
            int shooterCount = Random.Range(2, 5);
            int baseAmmo = totalAmmo / shooterCount;
            int remainder = totalAmmo % shooterCount;

            for (int i = 0; i < shooterCount; i++)
            {
                int ammo = baseAmmo;
                if (i < remainder) ammo++; // 余数分配给前几个射手

                // 随机选择一列
                int randomCol = Random.Range(0, 5);
                
                ShooterData shooter = new ShooterData();
                shooter.color = color;
                shooter.ammo = ammo;
                tableData.columns[randomCol].shooters.Add(shooter);
            }
        }

        // 打乱每列的射手顺序
        foreach (var column in tableData.columns)
        {
            ShuffleList(column.shooters);
        }

        return tableData;
    }

    // 洗牌算法
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // ========================================
    // 保存 Grid JSON
    // ========================================
    void SaveGridJSON(int levelNumber, LevelGridData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string dirPath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string fileName = $"Level_{levelNumber}_grid.json";
        string filePath = Path.Combine(dirPath, fileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"✅ 生成 {fileName}, 方块数: {data.cells.Count}");
    }

    // ========================================
    // 保存 Table JSON
    // ========================================
    void SaveTableJSON(int levelNumber, ShooterTableData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string dirPath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string fileName = $"Level_{levelNumber}_table.json";
        string filePath = Path.Combine(dirPath, fileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"✅ 生成 {fileName}");
    }

    // ========================================
    // 验证数量平衡
    // ========================================
    void VerifyBalance(int levelNumber, Dictionary<string, int> colorCounts, ShooterTableData tableData)
    {
        Dictionary<string, int> shooterCounts = new Dictionary<string, int>();

        foreach (var column in tableData.columns)
        {
            foreach (var shooter in column.shooters)
            {
                if (!shooterCounts.ContainsKey(shooter.color))
                    shooterCounts[shooter.color] = 0;
                shooterCounts[shooter.color] += shooter.ammo;
            }
        }

        bool isBalanced = true;
        foreach (var kvp in colorCounts)
        {
            string color = kvp.Key;
            int gridCount = kvp.Value;
            int shooterCount = shooterCounts.ContainsKey(color) ? shooterCounts[color] : 0;

            if (gridCount != shooterCount)
            {
                Debug.LogError($"❌ Level {levelNumber} 不平衡！{color}: Grid={gridCount}, Shooter={shooterCount}");
                isBalanced = false;
            }
        }

        if (isBalanced)
        {
            Debug.Log($"✅ Level {levelNumber} 数量完全匹配！");
        }
    }

    // ========================================
    // 刷新 Unity 资源数据库
    // ========================================
    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
