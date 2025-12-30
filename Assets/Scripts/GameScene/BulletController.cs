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