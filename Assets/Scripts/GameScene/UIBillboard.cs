// ================================================================================
// TL;DR:
// UI 看板效果（Billboard），让 3D 世界中的 UI 元素始终面向摄像机。
// 常用于角色头顶的血条、名字、弹药数等文本，避免视角旋转时文字倾斜。
//
// 目标：
// - 使 UI 元素的旋转始终与主摄像机一致
// - 使用 LateUpdate 确保在物体移动后更新（避免抖动）
// - 极简实现，零配置开箱即用
//
// 非目标：
// - 不处理 UI 内容更新（由具体 UI 组件如 TextMeshPro 负责）
// - 不处理 UI 缩放或淡入淡出
// - 不支持自定义朝向偏移（固定完全对齐摄像机）
// ================================================================================
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 使用 LateUpdate 确保在小猪动完之后，我们再修正文字角度
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 方案 A：直接复制摄像机的旋转角度（最稳，摄像机怎么转它就怎么转）
            transform.rotation = mainCamera.transform.rotation;
            
            // 方案 B（备选）：如果你希望它永远完全竖直，不管摄像机有没有歪
            // transform.rotation = Quaternion.identity; 
        }
    }
}