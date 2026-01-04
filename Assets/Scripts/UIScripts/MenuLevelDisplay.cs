using UnityEngine;
using TMPro; // 必须引用

public class MenuLevelDisplay : MonoBehaviour
{
    [Header("请按顺序拖入 Bottom_1 到 Bottom_4 的数字文本")]
    public TextMeshProUGUI[] levelTexts; // 数组存4个文本组件

    void Start()
    {
        UpdateLevelNumbers();
    }

    // 每次回到主菜单时（OnEnable）也刷新一下，防止数据过时
    void OnEnable()
    {
        UpdateLevelNumbers();
    }

    void UpdateLevelNumbers()
    {
        if (GameManager.Instance == null) return;

        // 1. 获取当前关卡数字 (例如 5)
        int currentLevel = GameManager.Instance.GetCurrentLevelNum();

        // 2. 循环更新 4 个图标
        // levelTexts[0] 是 Bottom_1 -> 显示 5
        // levelTexts[1] 是 Bottom_2 -> 显示 6 ...
        for (int i = 0; i < levelTexts.Length; i++)
        {
            if (levelTexts[i] != null)
            {
                // i=0 显示 current+0, i=1 显示 current+1...
                levelTexts[i].text = (currentLevel + i).ToString();
            }
        }
    }
}