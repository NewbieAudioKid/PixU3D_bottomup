// ================================================================================
// TL;DR:
// 背景滚动效果组件，通过修改 RawImage UV 坐标实现无限循环滚动。
// 适用于主菜单或游戏背景的动态氛围营造，性能开销极低。
//
// 目标：
// - 实现背景图片的无限循环滚动（X/Y 方向可独立配置）
// - 使用 UV 偏移替代 Transform 移动，避免重复实例化
// - 支持可调速度，使用 Time.deltaTime 确保不同设备速度一致
// - 挂载即用，无需额外配置（仅需调整 Inspector 参数）
//
// 非目标：
// - 不支持 Image 组件（仅支持 RawImage，因为需要 UV 坐标）
// - 不处理视差滚动（若需多层视差，应使用多个 BackgroundScroller 实例）
// - 不处理加速/减速动画（速度恒定，若需动态变化可扩展）
// ================================================================================
using UnityEngine;
using UnityEngine.UI; // 必须引用 UI

public class BackgroundScroller : MonoBehaviour
{
    [Header("流动设置")]
    public float scrollSpeedX = 0f; // 横向速度
    public float scrollSpeedY = 0.1f; // 纵向速度（向下）

    private RawImage _rawImage;
    private Rect _uvRect;

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        // 每一帧更新 UV 坐标
        // Time.deltaTime 保证在不同性能的手机上速度一致
        _uvRect = _rawImage.uvRect;
        _uvRect.x += scrollSpeedX * Time.deltaTime;
        _uvRect.y += scrollSpeedY * Time.deltaTime; // 改变 Y 轴实现上下流动

        // 重新赋值回去
        _rawImage.uvRect = _uvRect;
    }
}