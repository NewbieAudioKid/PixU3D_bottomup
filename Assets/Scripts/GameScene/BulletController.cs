// ================================================================================
// TL;DR:
// 子弹飞行控制器，采用 Ground Truth 位置缓存 + sqrMagnitude 高性能碰撞判定。
// 子弹发射时立即缓存目标位置，确保即使目标被提前销毁也能飞到正确位置。
//
// 目标：
// - 实现子弹从发射点到目标的平滑飞行
// - 使用 Ground Truth 缓存，避免目标销毁导致的空指针
// - 使用 sqrMagnitude 替代 Distance，提升性能 (避免开方运算)
// - 自动朝向目标（LookAt），提升视觉真实感
// - 击中后调用 target.OnHit() 并自毁
//
// 非目标：
// - 不处理弹道物理（如重力、空气阻力）
// - 不支持穿透或范围伤害
// - 不处理子弹特效（粒子、音效等应由其他组件负责）
// ================================================================================
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // ==================== 内部状态 ====================
    private CellController targetCell;  // 目标方块引用 (用于命中回调)
    private Vector3 targetPos;          // 目标位置缓存 (Ground Truth，防止空指针)
    private float speed = 25f;          // 飞行速度 (单位/秒)
    private bool isFired = false;       // 是否已发射

    // ==================== 发射接口 ====================
    /// <summary>
    /// 发射子弹到指定目标
    /// </summary>
    /// <param name="target">目标方块控制器</param>
    /// <param name="startPos">发射起点 (Ground Truth 位置，由 PigController 提供)</param>
    public void Fire(CellController target, Vector3 startPos)
    {
        transform.position = startPos;
        targetCell = target;
        
        // 【关键】立即缓存目标位置 (Ground Truth)
        // 即使目标下一帧被其他子弹销毁，本子弹也能飞到正确位置
        targetPos = target.transform.position; 
        isFired = true;
        
        // 朝向目标 (视觉效果)
        transform.LookAt(targetPos);
    }

    // ==================== 飞行逻辑 ====================
    void Update()
    {
        if (!isFired) return;

        // 计算本帧飞行距离
        float step = speed * Time.deltaTime;

        // MoveTowards 内置边界处理，不会越过目标
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // 【性能优化】使用 sqrMagnitude 替代 Distance (避免开方)
        // 0.001f 是浮点容差，表示距离 < 0.03 单位视为到达
        if ((transform.position - targetPos).sqrMagnitude < 0.001f)
        {
            Hit();
        }
    }

    // ==================== 命中处理 ====================
    /// <summary>
    /// 到达目标位置后的处理：通知目标销毁 + 自毁
    /// </summary>
    void Hit()
    {
        // 再次检查目标是否还存在 (可能被前面的子弹打爆了)
        if (targetCell != null)
        {
            targetCell.OnHit();
        }
        
        // 无论目标还在不在，子弹到达目的地后必须销毁
        Destroy(gameObject);
    }
}