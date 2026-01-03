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
    // 在 Unity 编辑器里右键点击这个组件 -> "Generate Level 1 JSON" 即可触发
    [ContextMenu("Generate Level 1 JSON")]
    public void GenerateJSON()
    {
        LevelGridData data = new LevelGridData();
        data.cells = new List<CellData>();

        int size = 20;

        // 遍历 20x20 网格
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                string colorID = "";

                // === 图案逻辑 ===
                
                // 1. 蓝色边框 (Border)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    colorID = "blue";
                }
                // 2. 红色对角线 (X Shape) - 避开边框
                else if (x == y || x == (size - 1 - y))
                {
                    colorID = "red";
                }
                // 3. 绿色上下区域 (Top/Bottom Triangles)
                // 这里利用坐标判断：如果是下半部分(y < size/2)且夹角内，或者上半部分
                // 简单几何判断：如果 |y - center| > |x - center| 则是上下
                else if (Mathf.Abs(y - 9.5f) > Mathf.Abs(x - 9.5f))
                {
                    colorID = "green";
                }
                // 4. 黄色左右区域 (Left/Right Triangles)
                else
                {
                    colorID = "yellow";
                }

                // 添加数据
                CellData cell = new CellData();
                cell.x = x;
                cell.y = y;
                cell.color = colorID;
                data.cells.Add(cell);
            }
        }

        // === 保存文件 ===
        string json = JsonUtility.ToJson(data, true); // true 表示格式化美观
        
        // 确保路径存在
        string dirPath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string filePath = dirPath + "/level_1_grid.json";
        File.WriteAllText(filePath, json);

        Debug.Log("✅ 成功生成文件: " + filePath + "\n总格子数: " + data.cells.Count);
        
        // 刷新资源让 Unity 看见新文件
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}