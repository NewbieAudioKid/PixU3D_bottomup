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