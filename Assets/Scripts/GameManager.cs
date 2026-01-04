using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 【修改点】默认改成大写 Level_1，与生成器保持一致
    public string currentLevelName = "Level_1";

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
// ================== 新增：获取当前关卡数字 ==================
    public int GetCurrentLevelNum()
    {
        // 把 "Level_5" 拆开，取出 "5"
        if (string.IsNullOrEmpty(currentLevelName)) return 1;

        string[] parts = currentLevelName.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
        {
            return num;
        }
        
        Debug.LogWarning($"关卡名格式不标准 ({currentLevelName})，默认返回 1");
        return 1;
    }
    public void StartLevel(string levelName)
    {
        currentLevelName = levelName;
        SceneManager.LoadScene("GameScene");
    }

    public void GameOver(bool isWin)
    {
        if (isWin)
            TriggerVictory(); 
        else
            TriggerGameOver();
    }

    // 下一关逻辑
    public void LoadNextLevel()
    {
        // 先只更新数据
        bool hasNext = AdvanceLevelProgress();

        if (hasNext)
        {
            Debug.Log($"✅ 找到下一关数据，即将进入: {currentLevelName}");
            // 重新加载 GameScene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning("没有下一关数据，返回菜单");
            SceneManager.LoadScene("MenuScene");
        }
    }

    // 只负责把 currentLevelName +1，不负责跳转场景
    public bool AdvanceLevelProgress()
    {
        Debug.Log($"当前关卡: {currentLevelName}, 正在计算下一关...");

        string[] parts = currentLevelName.Split('_'); 

        if (parts.Length == 2 && int.TryParse(parts[1], out int currentNum))
        {
            int nextNum = currentNum + 1;
            string nextLevelName = "Level_" + nextNum;

            // 检查文件是否存在
            TextAsset testFile = Resources.Load<TextAsset>($"Levels/{nextLevelName}_grid");
            if (testFile != null)
            {
                // 更新 currentLevelName
                currentLevelName = nextLevelName;
                return true; // 成功进阶
            }
        }
        
        return false; // 没找到下一关（可能通关了）
    }

    public void TriggerVictory()
    {
        if (GameResultPopup.Instance != null)
            GameResultPopup.Instance.ShowVictory();
        else
            Debug.LogError("❌ 场景里找不到 GameResultPopup！请检查 Prefab 是否在场景中且 Active。");
    }

    public void TriggerGameOver()
    {
        if (GameResultPopup.Instance != null)
            GameResultPopup.Instance.ShowGameOverDelayed();
        else
            Debug.LogError("❌ 场景里找不到 GameResultPopup！");
    }

    // ================== JSON 数据读取 ==================

    public LevelGridData LoadGridData()
    {
        // 这里的路径必须和 Generator 生成的路径一致
        string path = $"Levels/{currentLevelName}_grid";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<LevelGridData>(jsonFile.text);
        }
        Debug.LogError($"❌ 找不到 Grid JSON 文件: {path}");
        return null;
    }

    public ShooterTableData LoadTableData()
    {
        string path = $"Levels/{currentLevelName}_table";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<ShooterTableData>(jsonFile.text);
        }
        Debug.LogError($"❌ 找不到 Table JSON 文件: {path}");
        return null;
    }
}
// JSON 类保持不变...
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