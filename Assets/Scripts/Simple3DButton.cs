using UnityEngine;

public class Simple3DButton : MonoBehaviour
{
    [Header("跳转设置")]
    public string targetLevelName = "level_1"; // 要去哪一关

    [Header("视觉反馈")]
    public Color hoverColor = Color.gray; // 鼠标悬停变色
    private Color originalColor;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    // 鼠标移上去变色
    void OnMouseEnter()
    {
        if (rend != null) rend.material.color = hoverColor;
    }

    // 鼠标移开恢复
    void OnMouseExit()
    {
        if (rend != null) rend.material.color = originalColor;
    }

    // 鼠标点击触发
    void OnMouseDown()
    {
        Debug.Log("点击了 3D 按钮！");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel(targetLevelName);
        }
        else
        {
            Debug.LogError("找不到 GameManager，无法跳转！");
        }
    }
}