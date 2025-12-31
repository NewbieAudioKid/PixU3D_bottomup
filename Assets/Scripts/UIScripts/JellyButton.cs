using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class JellyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Q弹设置")]
    [Tooltip("按下去缩小的比例，建议 0.8")]
    public float pressedScale = 0.8f; 
    
    [Tooltip("回弹动画的总时间")]
    public float bounceDuration = 0.4f; // 稍微长一点，让弹性充分展示

    [Header("弹性曲线 (核心)")]
    [Tooltip("请在 Inspector 里把这条线画成波浪形")]
    public AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0, 0),        // 开始：在压缩状态
        new Keyframe(0.3f, 1.2f),  // 冲刺：弹出去，超过原大小 (1.2倍)
        new Keyframe(0.6f, 0.95f), // 回收：稍微缩回来一点 (0.95倍)
        new Keyframe(1, 1)         // 结束：稳稳停在原大小 (1.0倍)
    );

    private Vector3 originalScale;
    private Vector3 scaleOnRelease; // 手指抬起那一瞬间的大小
    private Coroutine bounceCoroutine;
    private Button btn;

    void Awake()
    {
        originalScale = transform.localScale;
        btn = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        
        // 按下时简单变小 (也可以用 Tween，但瞬间变小手感更干脆)
        transform.localScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        // 记录现在的状态作为起点
        scaleOnRelease = transform.localScale;
        
        // 开始基于曲线的回弹
        bounceCoroutine = StartCoroutine(ElasticBounce());
    }

    IEnumerator ElasticBounce()
    {
        float timer = 0;

        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;
            
            // 计算当前进度 (0 到 1)
            float progress = timer / bounceDuration;
            
            // 【关键点】从曲线中采样 "弹性值"
            // 如果你在 Inspector 画了波浪线，curveValue 就会在 0 -> 1.2 -> 0.9 -> 1 之间变化
            float curveValue = bounceCurve.Evaluate(progress);

            // 使用 LerpUnclamped，因为 curveValue 可能会超过 1
            // 它是从 "刚才按下的状态" 过渡到 "原始状态"
            transform.localScale = Vector3.LerpUnclamped(scaleOnRelease, originalScale, curveValue);

            yield return null;
        }

        // 确保最后严丝合缝
        transform.localScale = originalScale;
    }
}