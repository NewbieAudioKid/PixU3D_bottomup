using UnityEngine;
using System.Collections; // 必须引用这个，才能用协程

public class UI_StartButton : MonoBehaviour
{
    [Header("跳转设置")]
    public string targetLevelName = "level_1";
    
    [Header("延迟设置")]
    [Tooltip("等待动画播放的时间，通常0.2-0.4秒")]
    public float delayBeforeLoad = 0.3f; 

    // 这个是被按钮 OnClick 调用的方法
    public void ClickStartGame()
    {
        // 开启协程：启动一个并行的计时器
        StartCoroutine(WaitAndLoadRoutine());
    }

    // 协程的具体逻辑
    IEnumerator WaitAndLoadRoutine()
    {
        // 1. 这里什么都不做，只是等待
        // 此时 JellyButton 脚本正在播放它的 Q 弹动画
        yield return new WaitForSeconds(delayBeforeLoad);

        // 2. 时间到了，检查并跳转
        Debug.Log("动画播完了，现在开始加载场景...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel(targetLevelName);
        }
        else
        {
            Debug.LogError("找不到 GameManager！");
        }
    }
}