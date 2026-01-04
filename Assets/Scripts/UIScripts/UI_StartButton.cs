// ================================================================================
// TL;DR:
// 开始按钮业务逻辑控制器，处理点击事件并触发场景跳转。
// 配合 ElasticButton 实现"先播动画，再跳转"的优雅过渡。
//
// 目标：
// - 响应开始按钮点击事件
// - 延迟跳转以配合按钮弹性动画（避免动画被打断）
// - 通知 GameManager 加载指定关卡
//
// 非目标：
// - 不处理按钮视觉动画（由 ElasticButton 负责）
// - 不处理场景加载细节（由 GameManager 负责）
// - 不处理淡入淡出效果（由 SceneFader 负责）
// ================================================================================
using UnityEngine;
using System.Collections; // 必须引用这个，才能用协程
// UI_StartButton.cs (只负责业务逻辑)
public class UI_StartButton : MonoBehaviour 
{
    public float delayBeforeLoad = 0.4f; // 这里的时间最好和 ElasticButton 的 duration 差不多

    public void ClickStartGame() 
    {
        StartCoroutine(WaitAndGo());
    }
    
    IEnumerator WaitAndGo()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        
        if (GameManager.Instance != null)
        {
            // 使用 GameManager 记录的当前关卡，而不是硬编码 Level_1
            GameManager.Instance.StartLevel(GameManager.Instance.currentLevelName);
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance 是 null！请检查 MenuScene 中是否有 GameManager 对象。");
        }
    }
}