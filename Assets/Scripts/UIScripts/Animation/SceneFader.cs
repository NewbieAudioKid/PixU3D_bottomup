// ================================================================================
// TL;DR:
// 场景淡入特效组件，用于游戏场景加载后的黑屏淡入效果。
// 自动从黑屏渐变到透明，完成后自动销毁以释放资源。
//
// 目标：
// - 新场景加载后自动播放淡入动画（黑 → 透明）
// - 使用 CanvasGroup Alpha 实现平滑过渡
// - 动画完成后自动销毁自身，避免常驻内存
// - 极简实现，挂载即用，无需手动调用
//
// 非目标：
// - 不处理场景切换逻辑（由 GameManager 或 SceneManager 负责）
// - 不支持淡出效果（仅淡入，淡出应由 SplashController 等负责）
// - 不支持自定义颜色（固定黑色，若需其他颜色可修改 Canvas Image）
// ================================================================================
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    public float fadeSpeed = 1.5f;
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        
        // 确保一开始是全黑
        cg.alpha = 1f;
    }

    void Update()
    {
        // 只要还不是透明的，就每帧减小 Alpha
        if (cg.alpha > 0)
        {
            cg.alpha -= Time.deltaTime * fadeSpeed;
        }
        else
        {
            // 变透明后，销毁这个黑布，节省资源
            Destroy(gameObject); 
        }
    }
}