using UnityEngine;

public class LevelSelectorUI : MonoBehaviour
{
    public void OnClickLevel1()
    {
        // 告诉 GameManager 去加载 level_1
        // 假设你的 json 文件名是 level_1_grid.json 和 level_1_table.json
        GameManager.Instance.StartLevel("level_1");
    }

    // 你可以复制这个方法给 Level 2, Level 3...
}