using UnityEngine;
using TMPro;

public class GameLevelDisplay : MonoBehaviour
{
    [Header("引用顶部 Level Name 文本")]
    public TextMeshProUGUI levelNameText;

    void Start()
    {
        if (GameManager.Instance != null && levelNameText != null)
        {
            int current = GameManager.Instance.GetCurrentLevelNum();
            // 拼接字符串，例如 "Level 5"
            levelNameText.text = "Level " + current;
        }
    }
}