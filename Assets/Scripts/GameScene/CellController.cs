// ================================================================================
// TL;DR:
// 单个方块的生命周期控制器，管理颜色、销毁状态和击中响应。
// 采用"待死亡"标记机制避免射击路径预计算时的重复瞄准。
//
// 目标：
// - 管理单个方块的颜色ID和视觉材质
// - 提供击中销毁接口（OnHit）
// - 支持 isPendingDeath 占位标记（用于射击预计算）
// - 销毁时通知 GridManager 更新计数（用于胜利判定）
//
// 非目标：
// - 不处理方块生成位置计算（由 GridManager 负责）
// - 不处理击中特效和音效（应由子弹或特效系统负责）
// - 不参与射击逻辑（由 PigController 和 BulletController 负责）
// ================================================================================
using UnityEngine;

public class CellController : MonoBehaviour
{
    public string colorID;
    public bool isDestroyed = false;
    
    // 【新增】是否即将死亡（已有子弹飞向我）
    public bool isPendingDeath = false;

    public void Init(string color, Material mat)
    {
        this.colorID = color;
        GetComponent<Renderer>().material = mat;
        this.isDestroyed = false;
        this.isPendingDeath = false; // 重置状态
        gameObject.SetActive(true);
    }

    public void OnHit()
    {
// 防止已经被销毁的方块重复触发
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // 【新增】通知 GridManager 减少计数
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnCellDestroyed();
        }

        // 视觉上消失
        gameObject.SetActive(false);
    }
}