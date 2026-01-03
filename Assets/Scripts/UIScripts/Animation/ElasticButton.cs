// ================================================================================
// TL;DR:
// Q弹按钮动效组件，实现"按下缩小 → 松开回弹"的弹性交互效果。
// 基于 AnimationCurve 和协程实现，支持音效、自定义曲线，零侵入式设计。
//
// 目标：
// - 响应 OnPointerDown/Up 事件，播放弹性缩放动画
// - 使用 AnimationCurve 实现超过目标值的"回弹"效果（如 1.0 → 1.15 → 0.95 → 1.0）
// - 支持可选音效播放（AudioClip + AudioSource）
// - 使用 unscaledDeltaTime 确保即使游戏暂停（TimeScale=0）UI 动画仍正常播放
// - 可配置弹性曲线、按压比例、动画时长
//
// 非目标：
// - 不处理按钮业务逻辑（由具体 Button 的 OnClick 事件或专用脚本如 UI_StartButton 负责）
// - 不处理按钮禁用状态的视觉变化（依赖 Unity Button 组件的 interactable）
// - 不支持长按或双击（仅响应单次点击）
// ================================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 必须引用：处理触摸事件
using System.Collections;

[RequireComponent(typeof(RectTransform))] // 强制要求必须有 RectTransform
public class ElasticButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Q弹参数 (Visuals)")]
    [Tooltip("按下去缩小的比例 (0.8 = 缩小到80%)")]
    public float pressedScale = 0.85f;
    
    [Tooltip("回弹动画总时长")]
    public float duration = 0.5f;

    [Header("弹性曲线")]
    [Tooltip("核心魔法：X轴是时间(0-1)，Y轴是比例插值")]
    // 我们在代码里直接“画”一个默认的波浪线，防止你忘了设置
    public AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),        // 起点
        new Keyframe(0.2f, 1.15f),   // 第一次反弹：冲过头 (1.15倍)
        new Keyframe(0.5f, 0.95f),   // 第二次反弹：缩回来一点 (0.95倍)
        new Keyframe(0.8f, 1.02f),   // 微调
        new Keyframe(1f, 1f)         // 终点：归位
    );

    [Header("可选：音效")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    // 内部变量
    private Vector3 originalScale;
    private Vector3 scaleAtRelease;
    private Coroutine currentCoroutine;
    private Button btn;

    void Awake()
    {
        originalScale = transform.localScale;
        btn = GetComponent<Button>();
        
        // 尝试自动获取或添加 AudioSource
        if (clickSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnEnable()
    {
        // 每次物体由于被隐藏又重新显示时，重置大小，防止卡在缩小状态
        transform.localScale = originalScale;
    }

    // --- 接口实现 ---

    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        // 打断之前的回弹，瞬间变小，手感更干脆
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        transform.localScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        // 播放音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 记录此时此刻的大小（作为动画起点）
        scaleAtRelease = transform.localScale;
        
        // 启动回弹
        currentCoroutine = StartCoroutine(ElasticRoutine());
    }

    // --- 动画协程 ---

    IEnumerator ElasticRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 即使游戏暂停(TimeScale=0)也能播放UI动画
            
            float progress = timer / duration;
            float curveValue = bounceCurve.Evaluate(progress);

            // LerpUnclamped 允许数值超出 0-1 的范围，实现变大效果
            transform.localScale = Vector3.LerpUnclamped(scaleAtRelease, originalScale, curveValue);

            yield return null;
        }

        // 强迫症兜底：确保严丝合缝回到原始大小
        transform.localScale = originalScale;
    }
}