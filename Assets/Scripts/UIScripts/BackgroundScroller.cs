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