// ================================================================================
// TL;DR:
// 全局游戏管理器，负责关卡状态管理、场景切换和关卡数据加载。
// 使用 DontDestroyOnLoad 单例模式，在整个游戏生命周期内持久存在。
//
// 目标：
// - 管理当前关卡名称（currentLevelName）和关卡进度
// - 提供关卡切换接口（StartLevel、LoadNextLevel）
// - 加载关卡 JSON 数据（网格数据 + 射手表数据）
// - 触发游戏结束弹窗（胜利/失败）
// - 提供关卡进度更新接口（AdvanceLevelProgress）供菜单使用
//
// 非目标：
// - 不处理具体的游戏玩法逻辑（由 GridManager、PigController 等负责）
// - 不渲染 UI（由 GameResultPopup、MenuLevelDisplay 等负责）
// - 不管理射手生成（由 ShooterTableManager 负责）
// - 不处理传送带移动逻辑（由 BeltWalker 负责）
// ================================================================================
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 当前关卡名称，格式为 "Level_X"（X 为关卡数字）
    public string currentLevelName = "Level_1";

    // ==================== 生命周期 ====================

    /// <summary>
    /// 单例初始化，确保全局只有一个 GameManager 实例。
    /// 使用 DontDestroyOnLoad 使其在场景切换时不被销毁。
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    // ==================== 关卡信息查询 ====================

    /// <summary>
    /// 获取当前关卡的数字部分。
    /// 例如 "Level_5" 返回 5，格式不标准时返回 1。
    /// </summary>
    public int GetCurrentLevelNum()
    {
        if (string.IsNullOrEmpty(currentLevelName)) return 1;

        string[] parts = currentLevelName.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
            return num;
        
        Debug.LogWarning($"关卡名格式不标准 ({currentLevelName})，默认返回 1");
        return 1;
    }

    // ==================== 场景切换 ====================

    /// <summary>
    /// 开始指定关卡，更新 currentLevelName 并加载 GameScene。
    /// </summary>
    /// <param name="levelName">关卡名称，格式为 "Level_X"</param>
    public void StartLevel(string levelName)
    {
        currentLevelName = levelName;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 加载下一关：先更新关卡进度，然后重新加载 GameScene。
    /// 如果没有下一关数据，则返回菜单。
    /// </summary>
    public void LoadNextLevel()
    {
        bool hasNext = AdvanceLevelProgress();

        if (hasNext)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// 只更新关卡进度（currentLevelName +1），不切换场景。
    /// 用于胜利后返回菜单时更新进度。
    /// </summary>
    /// <returns>true 表示成功找到下一关，false 表示已通关</returns>
    public bool AdvanceLevelProgress()
    {
        string[] parts = currentLevelName.Split('_'); 

        if (parts.Length == 2 && int.TryParse(parts[1], out int currentNum))
        {
            int nextNum = currentNum + 1;
            string nextLevelName = "Level_" + nextNum;

            // 检查下一关的数据文件是否存在
            TextAsset testFile = Resources.Load<TextAsset>($"Levels/{nextLevelName}_grid");
            if (testFile != null)
            {
                currentLevelName = nextLevelName;
                return true;
            }
        }
        
        return false;
    }

    // ==================== 游戏结束处理 ====================

    /// <summary>
    /// 游戏结束入口，根据胜负状态触发对应弹窗。
    /// </summary>
    /// <param name="isWin">true 表示胜利，false 表示失败</param>
    public void GameOver(bool isWin)
    {
        if (isWin)
            TriggerVictory(); 
        else
            TriggerGameOver();
    }

    /// <summary>
    /// 触发胜利弹窗。
    /// </summary>
    public void TriggerVictory()
    {
        if (GameResultPopup.Instance != null)
            GameResultPopup.Instance.ShowVictory();
        else
            Debug.LogError("❌ 场景里找不到 GameResultPopup！");
    }

    /// <summary>
    /// 触发失败弹窗（延迟显示，避免协程冲突）。
    /// </summary>
    public void TriggerGameOver()
    {
        if (GameResultPopup.Instance != null)
            GameResultPopup.Instance.ShowGameOverDelayed();
        else
            Debug.LogError("❌ 场景里找不到 GameResultPopup！");
    }

    // ==================== JSON 数据加载 ====================

    /// <summary>
    /// 加载当前关卡的网格数据（方块位置和颜色）。
    /// </summary>
    /// <returns>LevelGridData 对象，加载失败返回 null</returns>
    public LevelGridData LoadGridData()
    {
        string path = $"Levels/{currentLevelName}_grid";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        
        if (jsonFile != null)
            return JsonUtility.FromJson<LevelGridData>(jsonFile.text);
        
        Debug.LogError($"❌ 找不到 Grid JSON 文件: {path}");
        return null;
    }

    /// <summary>
    /// 加载当前关卡的射手表数据（射手颜色和弹药数量）。
    /// </summary>
    /// <returns>ShooterTableData 对象，加载失败返回 null</returns>
    public ShooterTableData LoadTableData()
    {
        string path = $"Levels/{currentLevelName}_table";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        
        if (jsonFile != null)
            return JsonUtility.FromJson<ShooterTableData>(jsonFile.text);
        
        Debug.LogError($"❌ 找不到 Table JSON 文件: {path}");
        return null;
    }
}

// ================================================================================
// JSON 数据结构定义
// 用于反序列化 Resources/Levels/ 目录下的关卡 JSON 文件
// ================================================================================

/// <summary>
/// 关卡网格数据，包含所有方块的位置和颜色信息。
/// 对应文件：Level_X_grid.json
/// </summary>
[System.Serializable]
public class LevelGridData
{
    public List<CellData> cells;
}

/// <summary>
/// 单个方块数据：网格坐标 (x, y) 和颜色ID。
/// </summary>
[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public string color;
}

/// <summary>
/// 射手表数据，包含 5 列射手的配置信息。
/// 对应文件：Level_X_table.json
/// </summary>
[System.Serializable]
public class ShooterTableData
{
    public List<ShooterColumn> columns;
}

/// <summary>
/// 单列射手数据，包含该列所有射手的配置。
/// </summary>
[System.Serializable]
public class ShooterColumn
{
    public List<ShooterData> shooters;
}

/// <summary>
/// 单个射手数据：颜色ID 和弹药数量。
/// </summary>
[System.Serializable]
public class ShooterData
{
    public string color;
    public int ammo;
}