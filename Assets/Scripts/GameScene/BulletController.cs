// ================================================================================
// TL;DR:
// 子弹飞行控制器，采用基于距离检测的高性能碰撞判定。
// 无需物理引擎，避免复杂碰撞体配置。
//
// 目标：
// - 实现子弹从射手到目标的平滑飞行
// - 使用距离检测替代物理碰撞，提升性能
// - 自动朝向目标（LookAt）提升视觉真实感
// - 击中后自动销毁，避免内存泄漏
//
// 非目标：
// - 不处理弹道物理（如重力、空气阻力）
// - 不支持穿透或范围伤害
// - 不处理子弹特效（粒子、音效等应由其他组件负责）
// ================================================================================
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private CellController targetCell;
    private float speed = 20f;
    private bool isFired = false;

    public void Fire(CellController target, Vector3 startPos)
    {
        transform.position = startPos;
        targetCell = target;
        isFired = true;
        
        // 朝着目标看
        transform.LookAt(target.transform);
    }

    void Update()
    {
        if (!isFired || targetCell == null) 
        {
            Destroy(gameObject); // 目标没了，子弹自毁
            return;
        }

        // 飞向目标
        transform.position = Vector3.MoveTowards(transform.position, targetCell.transform.position, speed * Time.deltaTime);

        // 击中判定 (不用碰撞体，用距离判断，极快)
        if (Vector3.Distance(transform.position, targetCell.transform.position) < 0.1f)
        {
            targetCell.OnHit(); // 告诉方块它碎了
            Destroy(gameObject); // 子弹自毁
        }
    }
}