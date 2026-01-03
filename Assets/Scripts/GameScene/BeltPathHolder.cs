// ================================================================================
// TL;DR:
// 传送带路径数据容器，采用单例模式全局共享路点列表。
// 用于在场景编辑器中可视化配置传送带路径，避免硬编码。
//
// 目标：
// - 提供全局访问的传送带路径点列表
// - 支持在 Unity Inspector 中拖拽配置路点
// - 采用单例模式简化跨系统访问
//
// 非目标：
// - 不处理物体移动逻辑（由 BeltWalker 负责）
// - 不动态生成路径（路径由关卡设计师手动配置）
// - 不验证路径有效性（假设设计师正确配置了4个角点）
// ================================================================================
using UnityEngine;
using System.Collections.Generic;

public class BeltPathHolder : MonoBehaviour
{
    // 1. 定义一个静态的“自己”，让全世界都能直接访问
    public static BeltPathHolder Instance;

    public List<Transform> waypoints;

    void Awake()
    {
        // 2. 初始化单例
        Instance = this;
    }
}