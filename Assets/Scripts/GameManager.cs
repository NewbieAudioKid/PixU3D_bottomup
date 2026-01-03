// ================================================================================
// TL;DR:
// 游戏总管理器，负责全局状态管理和场景间数据传递。
// 采用单例模式 + DontDestroyOnLoad 实现跨场景持久化。
//
// 目标：
// - 统一管理当前关卡选择和场景切换流程
// - 提供 JSON 关卡数据加载接口（Grid 和 Shooter Table）
// - 处理游戏胜利/失败的全局逻辑
//
// 非目标：
// - 不处理具体游戏玩法逻辑（由 GridManager、PigController 等负责）
// - 不处理 UI 渲染细节（由各 UI Controller 负责）
// ================================================================================
using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用，用于切换场景
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 当前选择的关卡名字 (默认 level_1)
    public string currentLevelName = "level_1";

    void Awake()
    {
        // 单例模式 + 切换场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 关键！切换场景时我会活下来
        }
        else
        {
            Destroy(gameObject); // 如果已经有一个管家了，我这个新的就自杀
        }
    }

    // 供 UI 调用的方法：开始关卡
    public void StartLevel(string levelName)
    {
        currentLevelName = levelName;
        // 假设你的游戏场景叫 "GameScene"，请确保 Scene Build Settings 里加了这个场景
        SceneManager.LoadScene("GameScene");
    }
// ================== 新增：游戏结束逻辑 ==================
    public void GameOver(bool isWin)
    {
        if (isWin)
        {
            Debug.Log("🎉 VICTORY! 游戏胜利！所有方块已消除！");
            
            // 这里可以写弹出胜利 UI 的逻辑
            // 比如: WinUIPanel.SetActive(true);
            // 暂时先简单地重加载当前关卡，或者暂停游戏
            // Time.timeScale = 0; // 暂停游戏
        }
        else
        {
            Debug.Log("💀 DEFEAT! 游戏失败！");
        }
    }
    // ================== JSON 数据读取辅助类 ==================

    // 读取 Grid JSON
    public LevelGridData LoadGridData()
    {
        // 从 Resources/Levels/ 文件夹加载文本
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/{currentLevelName}_grid");
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<LevelGridData>(jsonFile.text);
        }
        Debug.LogError("找不到 Grid JSON 文件: " + currentLevelName);
        return null;
    }

    // 读取 Table JSON
    public ShooterTableData LoadTableData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/{currentLevelName}_table");
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<ShooterTableData>(jsonFile.text);
        }
        Debug.LogError("找不到 Table JSON 文件: " + currentLevelName);
        return null;
    }
}

// ================== JSON 数据结构定义 (放在类外面) ==================

[System.Serializable]
public class LevelGridData
{
    public List<CellData> cells;
}

[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public string color;
}

[System.Serializable]
public class ShooterTableData
{
    public List<ShooterColumn> columns;
}

[System.Serializable]
public class ShooterColumn
{
    public List<ShooterData> shooters;
}

[System.Serializable]
public class ShooterData
{
    public string color;
    public int ammo;
}