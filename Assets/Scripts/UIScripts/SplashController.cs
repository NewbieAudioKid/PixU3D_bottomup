// ================================================================================
// TL;DR:
// 启动闪屏控制器，负责游戏启动时的品牌展示和平滑场景过渡。
// 采用协程实现"停留 → 淡出 → 跳转"的三段式流程。
//
// 目标：
// - 展示游戏 Logo 或品牌图片（可配置停留时间）
// - 实现黑幕淡入效果（CanvasGroup Alpha 插值）
// - 在完全黑屏后无缝切换到主菜单场景
// - 提供可调参数（停留时间、淡出时长）
//
// 非目标：
// - 不处理游戏主逻辑（由 GameManager 负责）
// - 不处理用户交互（闪屏通常不可跳过，若需要可扩展）
// - 不加载资源或关卡数据（仅负责场景过渡）
// ================================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 必须引用 UI
using System.Collections;

public class SplashController : MonoBehaviour
{
    [Header("设置")]
    public string nextSceneName = "MenuScene"; 
    
    [Tooltip("展示图片的停留时间")]
    public float waitTime = 1.5f; 

    [Tooltip("淡出变黑需要的时间")]
    public float fadeDuration = 2.0f;

    [Header("引用")]
    public CanvasGroup blackCurtain; // 把刚才那个 BlackCurtain 拖进来

    void Start()
    {
        // 确保黑幕一开始是透明的
        if (blackCurtain != null) blackCurtain.alpha = 0f;
        
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        // 1. 停留：让玩家欣赏一会儿海报
        yield return new WaitForSeconds(waitTime);

        // 2. 渐变：让黑幕慢慢变成不透明 (Alpha 0 -> 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 计算当前进度 0~1
            float progress = timer / fadeDuration;
            
            if (blackCurtain != null)
            {
                blackCurtain.alpha = progress; 
            }
            
            yield return null; // 等待下一帧
        }

        // 确保完全变黑
        if (blackCurtain != null) blackCurtain.alpha = 1f;

        // 3. 趁着全黑的时候，偷偷加载场景
        SceneManager.LoadScene(nextSceneName);
    }
}