using UnityEngine;
using UnityEngine.Events;
using System.Collections; // 引入协程，用于平滑移动
using System.Collections.Generic;
using TMPro; // 引入 TextMeshPro，用于头顶文字

// 定义小猪的状态枚举 (放在类外面，方便全局访问)
public enum PigState { InTable, InQueue, OnBelt, Returning }

public class PigController : MonoBehaviour
{
    [Header("=== 基础属性 ===")]
    public string colorID = "red"; // 颜色 ID，需与 GridManager 里的匹配
    public int ammo = 20;          // 初始弹药量
    public GameObject bulletPrefab; // 子弹预制体

    [Header("=== UI 与 视觉引用 (请在 Prefab 里拖拽) ===")]
    public TextMeshPro ammoTextUI; // 头顶的数字显示
    public Renderer bodyRenderer;  // 身体渲染器 (用于改色)

    [Header("=== 手感调节 (Juice) ===")]
    // 动画曲线：建议设置为 (0,0) -> (1,1)，中间稍微拱起一点实现回弹效果
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // 移动耗时：越小飞得越快 (秒)
    public float moveDuration = 0.4f; 

    [Header("=== 内部状态 (Debug用) ===")]
    public PigState currentState = PigState.InTable;
    private int currentQueueIndex = -1; // 在 ReadyQueue 里的座位号 (0-4)

    // === 内部引用 ===
    private BeltWalker beltWalker;         // 负责跑路的组件
    private CellController lastEngagedTarget = null; // 上一次锁定的目标 (防止重复判定)

    // =========================================================
    // 1. 初始化与生命周期
    // =========================================================

    void Awake()
    {
        // 获取自身的跑路组件
        beltWalker = GetComponent<BeltWalker>();
        
        // 监听跑路完成事件 (当 beltWalker 跑完一圈时调用 OnRunComplete)
        if (beltWalker != null)
        {
            beltWalker.OnPathComplete.AddListener(OnRunComplete);
        }
    }

    // 初始化数据 (通常由 ShooterTableManager 生成时调用)
    public void InitData(string color, int bulletCount)
    {
        this.colorID = color;
        this.ammo = bulletCount;

        // A. 更新身体颜色 (去 GridManager 领颜料)
        if (GridManager.Instance != null && bodyRenderer != null)
        {
            Material mat = GridManager.Instance.GetMaterialByColorID(this.colorID);
            if (mat != null)
            {
                bodyRenderer.material = mat;
            }
        }

        // B. 更新头顶文字
        UpdateAmmoUI();
    }
    
    // 强制设置状态 (外部调用)
    public void SetState(PigState state)
    {
        currentState = state;
    }

    void Update()
    {
        // 只有在传送带上跑的时候，才进行射击检测
        // 移动逻辑全部移交给了协程 (Coroutine)，Update 里不再处理移动
        if (currentState == PigState.OnBelt && ammo > 0)
        {
            CheckAndFire();
        }
    }

    // 更新 UI 显示
    void UpdateAmmoUI()
    {
        if (ammoTextUI != null)
        {
            ammoTextUI.text = ammo.ToString();
        }
    }

    // =========================================================
    // 2. 交互逻辑 (鼠标点击)
    // =========================================================

    void OnMouseDown()
    {
        // 情况 A: 在库存里被点 -> 请求去备战区
        if (currentState == PigState.InTable)
        {
            if (ShooterTableManager.Instance != null)
            {
                ShooterTableManager.Instance.OnPigClicked(this);
            }
        }
        // 情况 B: 在备战区被点 -> 请求上跑道
        else if (currentState == PigState.InQueue)
        {
            GoToBelt();
        }
    }

    // =========================================================
    // 3. 动作逻辑 (移动与流转)
    // =========================================================

    // 【动作 1】从库存飞到备战区 (或者在备战区内补位移动)
    public void MoveToQueue(int slotIndex, Vector3 pos)
    {
        currentState = PigState.InQueue;
        currentQueueIndex = slotIndex;
        
        // 告诉管理器：我占了这个坑
        if (ReadyQueueManager.Instance != null)
        {
            ReadyQueueManager.Instance.RegisterPig(slotIndex, this);
        }
        
        // 启动平滑弹射移动
        SmoothMoveTo(pos);
    }

    // 【动作 2】从备战区飞向跑道起点
    void GoToBelt()
    {
        // 安全检查
        if (beltWalker == null) return;
        
        // 从备战区注销 (把坑腾出来)
        if (ReadyQueueManager.Instance != null)
        {
            ReadyQueueManager.Instance.UnregisterPig(this);
        }

        // 获取全局路点
        if (BeltPathHolder.Instance != null && BeltPathHolder.Instance.waypoints.Count > 0)
        {
            // 开启组合拳协程：先飞过去 -> 再跑圈
            StartCoroutine(EnterBeltSequence(BeltPathHolder.Instance.waypoints));
        }
        else
        {
            Debug.LogError("错误：场景里找不到 BeltPathHolder 或者没有设置路点！");
        }
    }

    // 协程：进入跑道序列
    IEnumerator EnterBeltSequence(List<Transform> path)
    {
        currentState = PigState.OnBelt; // 标记状态

        // A. 飞向起点 (利用 moveCurve 曲线)
        // path[0] 是跑道的起点 (右下角)
        yield return StartCoroutine(MoveRoutine(path[0].position));

        // B. 飞到了，把控制权交给 BeltWalker，开始跑圈
        beltWalker.BeginJourney(path);
    }

    // 【动作 3】跑完一圈后的逻辑抉择
    void OnRunComplete()
    {
        CheckEndGameAndReturn();
    }

    // 核心决策：是回营休息，还是绝地反击？
    void CheckEndGameAndReturn()
    {
        bool isTableEmpty = false;
        bool isQueueEmpty = false;

        // 查询两大管理器状态
        if (ShooterTableManager.Instance != null) isTableEmpty = ShooterTableManager.Instance.IsTableEmpty();
        if (ReadyQueueManager.Instance != null) isQueueEmpty = ReadyQueueManager.Instance.IsQueueEmpty();

        // 判定：如果库存空了 && 备战区也空了
        // 说明我是最后的希望 (或者场上仅存的几只都在跑道上)
        if (isTableEmpty && isQueueEmpty)
        {
            Debug.Log("🔥 进入绝地反击模式！加速循环！");
            StartCoroutine(AutoRejoinBelt());
        }
        else
        {
            // 正常情况：回备战区待命
            ReturnToQueueNormal();
        }
    }

    // 逻辑分支 A: 自动加速循环 (Climax Mode)
    IEnumerator AutoRejoinBelt()
    {
        currentState = PigState.Returning;

        // 1. 视觉欺骗：假装要飞回备战区，制造“回弹”的视觉张力
        Vector3 bounceTarget = Vector3.zero;
        if (ReadyQueueManager.Instance != null)
        {
            // 找个位置假装落脚
            int slotIndex = ReadyQueueManager.Instance.GetFirstEmptyIndex();
            if (slotIndex == -1) slotIndex = 0;
            bounceTarget = ReadyQueueManager.Instance.GetSlotPosition(slotIndex);
        }

        // 2. 快速飞向备战区 (时间减半，制造紧迫感)
        float originalDuration = moveDuration;
        moveDuration = originalDuration * 0.5f; 
        yield return StartCoroutine(MoveRoutine(bounceTarget));

        // 3. 碰到备战区瞬间，反弹回跑道！
        // 开启 2 倍速 BUFF
        if (beltWalker != null)
        {
            beltWalker.SetDoubleSpeed(); // 需确保 BeltWalker 里有这个方法，或者直接 beltWalker.speed *= 2;
        }

        // 恢复飞行时间参数
        moveDuration = originalDuration;

        // 4. 再次上跑道
        if (BeltPathHolder.Instance != null)
        {
            yield return StartCoroutine(EnterBeltSequence(BeltPathHolder.Instance.waypoints));
        }
    }

    // 逻辑分支 B: 正常回营
    void ReturnToQueueNormal()
    {
        // 如果队列满了，这就尴尬了 (游戏失败逻辑通常在外部处理，这里防止报错)
        if (ReadyQueueManager.Instance == null || ReadyQueueManager.Instance.IsFull()) return;

        // 如果之前被加速过，记得恢复正常速度
        if (beltWalker != null)
        {
            beltWalker.ResetSpeed(); // 需确保 BeltWalker 里有这个方法
        }

        // 找空位
        int targetSlot = ReadyQueueManager.Instance.GetFirstEmptyIndex();
        Vector3 pos = ReadyQueueManager.Instance.GetSlotPosition(targetSlot);
        
        // 设置状态
        currentState = PigState.InQueue;
        currentQueueIndex = targetSlot;
        ReadyQueueManager.Instance.RegisterPig(targetSlot, this);
        
        // 飞回去
        SmoothMoveTo(pos);
        
        // 摆正身体 (防止跑圈时转歪了)
        transform.rotation = Quaternion.identity;
    }

    // =========================================================
    // 4. 移动核心算法 (AnimationCurve)
    // =========================================================

    public void SmoothMoveTo(Vector3 targetPos)
    {
        StopAllCoroutines(); // 打断之前的移动，防止冲突
        StartCoroutine(MoveRoutine(targetPos));
    }

    // 通用的非线性移动协程
    IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / moveDuration;

            // 【关键】使用曲线 Evaluation 计算进度
            // 如果曲线中间拱起超过 1.0，就会产生“超过目标再弹回来”的效果
            float curvedPercent = moveCurve.Evaluate(percent);

            // LerpUnclamped 允许插值超过 0-1 的范围
            transform.position = Vector3.LerpUnclamped(startPos, target, curvedPercent);

            yield return null; // 等下一帧
        }

        // 确保最后精准停在目标点
        transform.position = target;
    }

    // =========================================================
    // 5. 射击核心逻辑 (Smart Fire)
    // =========================================================

    void CheckAndFire()
    {
        if (GridManager.Instance == null) return;

        // 1. 智能查找目标 (不依赖朝向，依赖绝对坐标分区)
        CellController currentTarget = GridManager.Instance.GetTargetCellSmart(transform.position);

        // 如果没找到，或者目标没了，重置锁定状态
        if (currentTarget == null) 
        { 
            lastEngagedTarget = null; 
            return; 
        }

        // 2. 停火等待逻辑
        // 如果目标被标记为“即将死亡”，说明别人打过了，我不能穿透它，必须等待
        if (currentTarget.isPendingDeath) return;

        // 3. 防止对同一个健康目标重复开火
        if (currentTarget == lastEngagedTarget) return;

        // 4. 颜色匹配判断
        if (currentTarget.colorID == this.colorID) 
        {
            FireBullet(currentTarget);
            lastEngagedTarget = currentTarget; // 锁定它，防止一帧内多次开火
        } 
        else 
        {
            // 颜色不对，但也算看过了，避免每帧重复 query 浪费性能
            lastEngagedTarget = currentTarget;
        }
    }
    
    void FireBullet(CellController target)
    {
        // 1. 立即标记目标为“将死”，防止后面的猪穿透射击
        target.isPendingDeath = true; 
        
        // 2. 扣除弹药并更新 UI
        ammo--;
        UpdateAmmoUI();

        // 3. 生成并其发射子弹
        if (bulletPrefab != null) 
        {
            GameObject b = Instantiate(bulletPrefab);
            // 子弹脚本负责飞过去并销毁方块
            b.GetComponent<BulletController>().Fire(target, transform.position);
        }

        // 4. 弹药耗尽逻辑
        if (ammo <= 0) 
        {
            Debug.Log("弹药耗尽，小猪退场！");
            Destroy(gameObject); // 销毁自身，自动触发 BeltWalker 失效，不会再回营
        }
    }
}