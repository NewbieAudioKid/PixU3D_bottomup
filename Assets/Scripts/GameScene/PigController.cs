using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// 状态枚举
public enum PigState { InTable, InQueue, OnBelt, Returning, Transitioning }

// 射击排期表结构体
struct ShotScheduleItem
{
    public int beltStepIndex;    // 在传送带走的第几步开火
    public CellController target; // 目标是谁
}

public class PigController : MonoBehaviour
{
    [Header("=== 基础属性 ===")]
    public string colorID = "red";
    public int ammo = 20;
    public GameObject bulletPrefab;

    [Header("=== UI 与 视觉引用 ===")]
    public TextMeshPro ammoTextUI;
    public Renderer bodyRenderer;

    [Header("=== 手感调节 ===")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float moveDuration = 0.4f;

    [Header("=== 内部状态 ===")]
    public PigState currentState = PigState.InTable;
    private int currentQueueIndex = -1;
    
    // 【新增】是否处于加速（绝地反击）状态
    private bool isBoosted = false; 

    // === 内部引用 ===
    private BeltWalker beltWalker;
    
    // 射击排期表
    private Queue<ShotScheduleItem> shotSchedule = new Queue<ShotScheduleItem>();

    void Awake()
    {
        beltWalker = GetComponent<BeltWalker>();
    }

    public void InitData(string color, int bulletCount)
    {
        this.colorID = color;
        this.ammo = bulletCount;
        if (GridManager.Instance != null && bodyRenderer != null)
        {
            Material mat = GridManager.Instance.GetMaterialByColorID(this.colorID);
            if (mat != null) bodyRenderer.material = mat;
        }
        UpdateAmmoUI();
    }
    
    public void SetState(PigState state) { currentState = state; }

    void Update()
    {
        // Update 置空，逻辑全在协程里
    }

    void UpdateAmmoUI()
    {
        if (ammoTextUI != null) ammoTextUI.text = ammo.ToString();
    }

    // ================= 交互逻辑 =================
    void OnMouseDown()
    {
        if (currentState == PigState.InTable)
        {
            if (ShooterTableManager.Instance != null) ShooterTableManager.Instance.OnPigClicked(this);
        }
        else if (currentState == PigState.InQueue)
        {
            GoToBelt();
        }
    }

    // ================= 动作逻辑 =================
    public void MoveToQueue(int slotIndex, Vector3 pos)
    {
        currentState = PigState.InQueue;
        currentQueueIndex = slotIndex;
        if (ReadyQueueManager.Instance != null) ReadyQueueManager.Instance.RegisterPig(slotIndex, this);
        SmoothMoveTo(pos);
    }

    void GoToBelt()
    {
        if (beltWalker == null) return;
        
        currentState = PigState.Transitioning;
        
        if (ReadyQueueManager.Instance != null) ReadyQueueManager.Instance.UnregisterPig(this);

        if (BeltPathHolder.Instance != null && BeltPathHolder.Instance.waypoints.Count > 0)
        {
            // 1. 预计算路径
            PreCalculatePath();
            // 2. 开始跑路 (RunBeltSequence 是预计算版本的跑路逻辑)
            StartCoroutine(RunBeltSequence(BeltPathHolder.Instance.waypoints));
        }
        else
        {
            Debug.LogError("错误：场景里找不到 BeltPathHolder！");
        }
    }

    // =========================================================
    // 【核心逻辑】预计算射击路径
    // =========================================================
    void PreCalculatePath()
    {
        if (GridManager.Instance == null) return;

        shotSchedule.Clear();
        int simulatedAmmo = ammo; 
        int gridSize = GridManager.Instance.gridSize;
        int totalSteps = gridSize * 4; 

        for (int i = 0; i < totalSteps; i++)
        {
            if (simulatedAmmo <= 0) break; 

            Vector3 simPos = GridManager.Instance.GetSimulatedPosition(i);
            CellController target = GridManager.Instance.GetTargetCellSmart(simPos);

            if (target != null 
                && !target.isDestroyed 
                && !target.isPendingDeath 
                && target.colorID == this.colorID)
            {
                ShotScheduleItem item = new ShotScheduleItem();
                item.beltStepIndex = i;
                item.target = target;
                shotSchedule.Enqueue(item);

                target.isPendingDeath = true; // 占位
                simulatedAmmo--;
            }
        }
    }

    // =========================================================
    // 【核心逻辑】执行跑路与射击 (含加速逻辑)
    // =========================================================
    IEnumerator RunBeltSequence(List<Transform> path)
    {
        // 1. 飞向起点
        currentState = PigState.Transitioning;
        yield return StartCoroutine(MoveRoutine(path[0].position));

        // 2. 落地，开始跑圈
        currentState = PigState.OnBelt;
        
        int gridSize = GridManager.Instance.gridSize;

        // === 速度控制 (含 Boost) ===
        float baseSpeed = (beltWalker != null && beltWalker.speed > 0) ? beltWalker.speed : 5f;
        float currentRunSpeed = isBoosted ? (baseSpeed * 2f) : baseSpeed;
        // =========================

        List<Vector3> waypoints = new List<Vector3>();
        foreach(var t in path) waypoints.Add(t.position);
        
        for (int segmentIndex = 0; segmentIndex < 4; segmentIndex++)
        {
            Vector3 start = waypoints[segmentIndex];
            Vector3 end = waypoints[(segmentIndex + 1) % waypoints.Count];
            
            int minStepIndex = segmentIndex * gridSize;
            int maxStepIndex = (segmentIndex + 1) * gridSize - 1;

            float segmentDist = Vector3.Distance(start, end);
            float travelTime = segmentDist / currentRunSpeed; // 应用加速后的速度
            float timer = 0f;

            while (timer < travelTime)
            {
                timer += Time.deltaTime;
                float fraction = timer / travelTime;
                transform.position = Vector3.Lerp(start, end, fraction);
                
                int currentStep = minStepIndex + Mathf.FloorToInt(fraction * gridSize);

                if (shotSchedule.Count > 0)
                {
                    ShotScheduleItem nextShot = shotSchedule.Peek();
                    if (nextShot.beltStepIndex > maxStepIndex) { }
                    else if (currentStep >= nextShot.beltStepIndex)
                    {
                        PerformVisualFire(nextShot.target);
                        shotSchedule.Dequeue(); 
                    }
                }

// ================= 【核心修改】弹药耗尽处理 =================
                if (ammo <= 0)
                {
                    Debug.Log("弹药耗尽，播放死亡动画...");

                    // 1. 立即停止移动 (不再执行 yield return null 继续跑了)
                    
                    // 2. 播放死亡动画，并等待它播完
                    yield return StartCoroutine(PerformDeathAnimation());

                    // 3. 彻底销毁
                    Destroy(gameObject);
                    
                    // 4. 退出整个 RunBeltSequence 协程
                    yield break; 
                }
                yield return null;
            }
            transform.position = end;
        }

        CheckEndGameAndReturn();
    }

    void PerformVisualFire(CellController target)
    {
        ammo--; 
        UpdateAmmoUI();

        if (bulletPrefab != null) 
        {
            GameObject b = Instantiate(bulletPrefab);
            b.GetComponent<BulletController>().Fire(target, transform.position);
        }
    }

    // ================= 回营决策逻辑 =================
    void CheckEndGameAndReturn()
    {
        bool isTableEmpty = false;
        bool isQueueEmpty = false;
        if (ShooterTableManager.Instance != null) isTableEmpty = ShooterTableManager.Instance.IsTableEmpty();
        if (ReadyQueueManager.Instance != null) isQueueEmpty = ReadyQueueManager.Instance.IsQueueEmpty();

        // 绝地反击条件：两处全空
        if (isTableEmpty && isQueueEmpty)
        {
            StartCoroutine(AutoRejoinBelt());
        }
        else
        {
            ReturnToQueueNormal();
        }
    }

    // 绝地反击模式 (修复了之前的报错)
    IEnumerator AutoRejoinBelt()
    {
        currentState = PigState.Returning;
        Vector3 bounceTarget = Vector3.zero;
        if (ReadyQueueManager.Instance != null)
        {
            int slotIndex = ReadyQueueManager.Instance.GetFirstEmptyIndex();
            if (slotIndex == -1) slotIndex = 0;
            bounceTarget = ReadyQueueManager.Instance.GetSlotPosition(slotIndex);
        }

        // 视觉回弹效果
        float originalDuration = moveDuration;
        moveDuration = originalDuration * 0.5f; 
        yield return StartCoroutine(MoveRoutine(bounceTarget));

        // === 开启加速 ===
        isBoosted = true; 
        Debug.Log(">>> 开启 2 倍速狂暴模式！");

        moveDuration = originalDuration;
        
        // 再次上跑道前，重新进行预计算！
        // 因为上一圈可能打掉了一些方块，格局变了，必须重算
        PreCalculatePath(); 

        if (BeltPathHolder.Instance != null)
        {
            // 注意：这里调用的是 RunBeltSequence (预计算版)，不是 EnterBeltSequence
            yield return StartCoroutine(RunBeltSequence(BeltPathHolder.Instance.waypoints));
        }
    }

    // 正常回营
    void ReturnToQueueNormal()
    {
        if (ReadyQueueManager.Instance == null) return;

        // 检查失败条件
        if (ReadyQueueManager.Instance.IsFull())
        {
            Debug.LogError("💀 GAME OVER: 队列已满！");
            if (GameManager.Instance != null) GameManager.Instance.GameOver(false);
            Destroy(gameObject);
            return;
        }

        // === 关闭加速 ===
        isBoosted = false;

        int targetSlot = ReadyQueueManager.Instance.GetFirstEmptyIndex();
        Vector3 pos = ReadyQueueManager.Instance.GetSlotPosition(targetSlot);
        
        currentState = PigState.InQueue;
        currentQueueIndex = targetSlot;
        ReadyQueueManager.Instance.RegisterPig(targetSlot, this);
        SmoothMoveTo(pos);
        transform.rotation = Quaternion.identity;
    }

    // ================= 移动核心算法 =================
    public void SmoothMoveTo(Vector3 targetPos)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPos));
    }

    IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / moveDuration;
            transform.position = Vector3.LerpUnclamped(startPos, target, moveCurve.Evaluate(percent));
            yield return null;
        }
        transform.position = target;
    }

// ==========================================
    // 【新增】死亡动画协程 (0.3秒)
    // 逻辑：变大+顺时针转 -> 变小+逆时针转
    // ==========================================
    IEnumerator PerformDeathAnimation()
    {
        float totalDuration = 0.3f;
        float halfDuration = totalDuration / 2f;
        
        Vector3 originalScale = transform.localScale; // 记住初始大小
        Quaternion originalRot = transform.rotation;  // 记住初始朝向

        // --- 第一阶段：0 ~ 0.15秒 ---
        // 动作：顺时针旋转 180度 (或者360度)，同时放大到 1.2倍
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration; // 0 ~ 1

            // 变大：使用 Lerp 插值
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
            
            // 旋转：顺时针转 (绕 Y 轴)
            // 这里我们用 RotateAround 或者简单的欧拉角插值
            // 为了简单，直接在原角度基础上加角度
            transform.rotation = originalRot * Quaternion.Euler(0, 360f * t, 0);

            yield return null;
        }

        // --- 第二阶段：0.15 ~ 0.3秒 ---
        // 动作：逆时针旋转回去，同时缩小到 0
        timer = 0f;
        // 此时已经是 1.2倍大，且转了一圈
        Vector3 bigScale = originalScale * 1.2f;
        
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration; // 0 ~ 1

            // 变小：从 1.2 变到 0
            transform.localScale = Vector3.Lerp(bigScale, Vector3.zero, t);
            
            // 逆时针转：从 360度 转回 0度 (或者继续转，看你喜好，这里按要求逆时针回去)
            // 这里的 t 是 0->1，我们让角度从 360 -> 0
            float angle = Mathf.Lerp(360f, 0f, t);
            transform.rotation = originalRot * Quaternion.Euler(0, angle, 0);

            yield return null;
        }

        // 彻底隐藏 (防止 Destroy 延迟的那一瞬间闪烁)
        transform.localScale = Vector3.zero;
    }


}