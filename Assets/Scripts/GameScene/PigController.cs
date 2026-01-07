// ================================================================================
// TL;DR:
// 射手猪猪核心控制器，整合状态管理、预计算射击、传送带巡逻和绝地反击机制。
// 采用协程驱动的事件流架构，支持复杂时序控制和动画编排。
//
// 目标：
// - 实现完整的射手生命周期：备战台 → 队列 → 传送带 → 返回/绝地反击
// - 采用预计算算法（PreCalculatePath）提前规划射击路径，避免实时计算
// - 支持绝地反击模式（场上最后一个射手时2倍速度再次上传送带）
// - 弹药耗尽时播放死亡动画（旋转+缩放）后销毁
// - 提供平滑移动动画（基于 AnimationCurve 曲线插值）
//
// 非目标：
// - 不生成网格或方块（由 GridManager 负责）
// - 不管理队列和备战台状态（由 ReadyQueueManager 和 ShooterTableManager 负责）
// - 不处理子弹飞行细节（由 BulletController 负责）
// ================================================================================
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// ========================================
// 猪猪射手状态枚举
// ========================================
// InTable:       在备战台上（初始状态，等待玩家点击选中）
// InQueue:       在准备队列中（被选中后排队，等待玩家再次点击上传送带）
// OnBelt:        在传送带上巡逻射击中
// Returning:     巡逻完毕，正在返回队列
// Transitioning: 过渡状态（从一个位置移动到另一个位置的中间状态）
public enum PigState { InTable, InQueue, OnBelt, Returning, Transitioning }

// ========================================
// 射击排期表结构体
// ========================================
// 用于预计算传送带路径上的射击计划
// 存储"在传送带第几步"应该向"哪个目标"开火
struct ShotScheduleItem
{
    public int beltStepIndex;    // 在传送带走的第几步开火（0-based索引）
    public CellController target; // 目标方块控制器引用
}

// ========================================
// 猪猪射手控制器 (PigController)
// ========================================
// 这是游戏的核心角色控制器，负责管理射手猪猪的完整生命周期：
// 1. 状态管理：从备战台 -> 队列 -> 传送带 -> 返回队列
// 2. 射击逻辑：预计算射击路径，在传送带上自动瞄准开火
// 3. 移动系统：平滑移动动画，支持曲线插值
// 4. 绝地反击：当场上无其他射手时，自动加速二次上传送带
// 5. 死亡动画：弹药耗尽时播放旋转缩放动画后销毁
// ========================================
public class PigController : MonoBehaviour
{
    // ========================================
    // 基础属性配置
    // ========================================
    [Header("=== 基础属性 ===")]
    public string colorID = "red";          // 射手的颜色标识，只能打相同颜色的方块
    public int ammo = 20;                   // 当前弹药数（每发射一次减1，归零时触发死亡）
    public GameObject bulletPrefab;         // 子弹预制体引用

    // ========================================
    // UI 与 视觉组件
    // ========================================
    [Header("=== UI 与 视觉引用 ===")]
    public TextMeshPro ammoTextUI;          // 用于显示剩余弹药数的文本组件
    public Renderer bodyRenderer;           // 射手身体的渲染器（用于设置材质颜色）

    // ========================================
    // 移动手感参数调节
    // ========================================
    [Header("=== 手感调节 ===")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 移动曲线（缓入缓出效果）
    public float moveDuration = 0.4f;       // 单次移动的持续时间（秒）

    // ========================================
    // 内部运行时状态
    // ========================================
    [Header("=== 内部状态 ===")]
    public PigState currentState = PigState.InTable;  // 当前状态（默认在备战台）
    private int currentQueueIndex = -1;               // 当前在队列中的位置索引（-1表示不在队列）
    
    // ========================================
    // 绝地反击加速机制
    // ========================================
    // 当场上没有其他射手（备战台和队列都为空）时触发
    // 此时射手会以2倍速度再次冲上传送带进行最后一搏
    private bool isBoosted = false;                   // 是否处于加速（绝地反击）状态

    // ========================================
    // 内部组件引用
    // ========================================
    private BeltWalker beltWalker;                    // 传送带行走组件（控制在传送带上的移动速度）
    
    // ========================================
    // 射击排期表（预计算系统）
    // ========================================
    // 在上传送带前，会预先计算整个传送带路径上所有可以命中的目标
    // 将每个射击点记录为 (步数索引, 目标方块) 的配对，存入队列
    // 在传送带移动过程中，按照排期表依次开火
    private Queue<ShotScheduleItem> shotSchedule = new Queue<ShotScheduleItem>();

    // ========================================
    // Unity 生命周期：Awake
    // ========================================
    // 在脚本实例被加载时调用（比 Start 更早）
    // 用于获取组件引用
    void Awake()
    {
        beltWalker = GetComponent<BeltWalker>();  // 获取同GameObject上的BeltWalker组件
    }

    // ========================================
    // 初始化数据（外部调用）
    // ========================================
    // 在实例化射手后，由外部管理器调用此方法设置初始参数
    // 参数：
    //   color - 颜色标识（如 "red", "blue" 等）
    //   bulletCount - 初始弹药数
    public void InitData(string color, int bulletCount)
    {
        this.colorID = color;
        this.ammo = bulletCount;
        
        // 根据颜色ID设置身体材质
        if (GridManager.Instance != null && bodyRenderer != null)
        {
            Material mat = GridManager.Instance.GetMaterialByColorID(this.colorID);
            if (mat != null) bodyRenderer.material = mat;
        }
        
        UpdateAmmoUI();  // 更新UI显示
    }
    
    // ========================================
    // 状态设置器（简单封装）
    // ========================================
    public void SetState(PigState state) { currentState = state; }

    // ========================================
    // Unity 生命周期：Update
    // ========================================
    // 注意：Update 方法本体留空，是因为所有主逻辑（如移动、射击等）都已被实现为协程（Coroutine）。
    // 这样做的好处是可以更容易地控制时序和动画，比如等待动画播放完后再处理下一步，或者分步执行复杂流程。
    // 如果以后需要响应实时输入或实现定时器，也可以在此加入代码。
    // 当前无需每帧主动处理，所以这里是空的。
    void Update()
    {
    }

    // ========================================
    // UI 更新：弹药显示
    // ========================================
    void UpdateAmmoUI()
    {
        if (ammoTextUI != null) ammoTextUI.text = ammo.ToString();
    }

    // ========================================
    // 玩家交互：鼠标点击事件
    // ========================================
    // Unity 的内置事件，当玩家点击此对象时触发
    // 根据当前状态执行不同的逻辑：
    //   - InTable 状态：通知 ShooterTableManager 处理选中逻辑（通常是移到队列）
    //   - InQueue 状态：直接调用 GoToBelt() 上传送带
    void OnMouseDown()
    {
        if (currentState == PigState.InTable)
        {
            // 在备战台上被点击，通知管理器
            if (ShooterTableManager.Instance != null) 
                ShooterTableManager.Instance.OnPigClicked(this);
        }
        else if (currentState == PigState.InQueue)
        {
            // 在队列中被点击，直接上传送带
            GoToBelt();
        }
    }

    // ========================================
    // 动作：移动到队列
    // ========================================
    // 由外部管理器调用，将射手从备战台移动到指定的队列位置
    // 参数：
    //   slotIndex - 队列槽位索引
    //   pos - 目标位置的世界坐标
    public void MoveToQueue(int slotIndex, Vector3 pos)
    {
        currentState = PigState.InQueue;            // 更新状态
        currentQueueIndex = slotIndex;               // 记录队列位置
        
        // 在队列管理器中注册自己
        if (ReadyQueueManager.Instance != null) 
            ReadyQueueManager.Instance.RegisterPig(slotIndex, this);
        
        SmoothMoveTo(pos);                          // 平滑移动到目标位置
    }

    // ========================================
    // 动作：上传送带（核心流程入口）
    // ========================================
    // 射手从队列进入传送带的完整流程：
    // 1. 从队列中注销
    // 2. 预计算整条传送带路径上的射击计划
    // 3. 启动传送带巡逻协程
    void GoToBelt()
    {
        if (beltWalker == null) return;
        
        currentState = PigState.Transitioning;      // 设置为过渡状态
        
        // 从队列管理器中注销
        if (ReadyQueueManager.Instance != null) 
            ReadyQueueManager.Instance.UnregisterPig(this);

        // 获取传送带路径
        if (BeltPathHolder.Instance != null && BeltPathHolder.Instance.waypoints.Count > 0)
        {
            // 步骤1：预计算路径（核心算法，提前规划所有射击点）
            PreCalculatePath();
            
            // 步骤2：开始传送带巡逻序列（执行预计算的计划）
            StartCoroutine(RunBeltSequence(BeltPathHolder.Instance.waypoints));
        }
        else
        {
            Debug.LogError("错误：场景里找不到 BeltPathHolder！");
        }
    }

    // =========================================================
    // 【核心算法】预计算射击路径
    // =========================================================
    // 在射手上传送带前，模拟整个传送带路径(80步)，预计算所有射击点
    // 生成"射击排期表"(shotSchedule Queue)，存储每个射击的 (步数, 目标) 配对
    //
    // 工作原理：
    // 1. 遍历传送带 80 步 (gridSize * 4)
    // 2. 调用 GridManager.GetTargetCellSmart(位置, 颜色) 查找可命中目标
    //    - 支持颜色阻挡：异色方块会阻挡射线
    //    - 支持 isPendingDeath 穿透：已被前面射手"预定"的方块视为透明
    // 3. 找到目标后：加入排期表 + 标记 isPendingDeath + 模拟弹药 -1
    // 4. 模拟弹药归零或路径走完时停止
    //
    // 优势：
    // - 避免实时计算开销，提高性能
    // - 保证射击时机准确（不会因为帧率波动漏射）
    // - 支持多射手"流水线"：后面的射手能看到前面射手的射击计划
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
            
            // 【修改点】传入 this.colorID，让 GridManager 帮我们判断阻挡
            CellController target = GridManager.Instance.GetTargetCellSmart(simPos, this.colorID);

            // 这里的条件可以简化了，因为 GridManager 已经做了大部分判断
            if (target != null)
            {
                ShotScheduleItem item = new ShotScheduleItem();
                item.beltStepIndex = i;
                item.target = target;
                shotSchedule.Enqueue(item);

                // 标记为"待死亡"，这样下一只猪计算时就会透视它
                target.isPendingDeath = true;
                simulatedAmmo--;
            }
        }
    }
    // =========================================================
    // 【核心协程】执行传送带巡逻与射击序列
    // =========================================================
    // 这是游戏的主要玩法循环，射手会：
    // 1. 飞向传送带起点
    // 2. 沿着传送带的4条边依次巡逻
    // 3. 按照预计算的排期表，在特定位置自动开火
    // 4. 实时检测弹药，归零时触发死亡动画
    // 5. 巡逻完成后决定下一步行动（返回队列 or 绝地反击）
    //
    // 参数：
    //   path - 传送带路径的路点列表（通常是4个角点）
    //
    // 支持加速：
    //   当 isBoosted = true 时，移动速度翻倍（绝地反击模式）
    // =========================================================
    IEnumerator RunBeltSequence(List<Transform> path)
    {
        // ========================================
        // 阶段1：飞向起点
        // ========================================
        currentState = PigState.Transitioning;
        yield return StartCoroutine(MoveRoutine(path[0].position));

        // ========================================
        // 阶段2：落地，开始传送带巡逻
        // ========================================
        currentState = PigState.OnBelt;
        
        int gridSize = GridManager.Instance.gridSize;

        // ========================================
        // 速度控制（支持 Boost 加速）
        // ========================================
        float baseSpeed = (beltWalker != null && beltWalker.speed > 0) ? beltWalker.speed : 5f;
        float currentRunSpeed = isBoosted ? (baseSpeed * 2f) : baseSpeed;  // 绝地反击时2倍速
        // ========================================

        // 将 Transform 路点转换为 Vector3 列表（方便索引）
        List<Vector3> waypoints = new List<Vector3>();
        foreach(var t in path) waypoints.Add(t.position);
        
        // ========================================
        // 阶段3：巡逻4条边
        // ========================================
        // 传送带是方形路径，有4个角点，射手会沿着这4条边移动
       // 阶段3：巡逻4条边
        for (int segmentIndex = 0; segmentIndex < 4; segmentIndex++)
        {
            Vector3 start = waypoints[segmentIndex];
            Vector3 end = waypoints[(segmentIndex + 1) % waypoints.Count];
            
            int minStepIndex = segmentIndex * gridSize;
            int maxStepIndex = (segmentIndex + 1) * gridSize - 1;

            float segmentDist = Vector3.Distance(start, end);
            float travelTime = segmentDist / currentRunSpeed;
            float timer = 0f;

            while (timer < travelTime)
            {
                timer += Time.deltaTime;
                float fraction = timer / travelTime;
                
                // 视觉位置：依然受帧率影响，但这只是为了让玩家看到猪在动
                transform.position = Vector3.Lerp(start, end, fraction);
                
                // 逻辑进度：计算当前理论上走到了第几步
                int currentStep = minStepIndex + Mathf.FloorToInt(fraction * gridSize);

                // ========================================
                // 【核心】基于 Ground Truth 的射击执行
                // ========================================
                // 遍历排期表，执行所有"应该在当前步数之前发射"的子弹
                // 使用 while 而非 if，确保即使一帧跨越多步也不会漏射
                while (shotSchedule.Count > 0 && shotSchedule.Peek().beltStepIndex <= currentStep)
                {
                    ShotScheduleItem nextShot = shotSchedule.Dequeue();
                    
                    // 确保不跨边射击 (防止本边的子弹在下一边发射)
                    if (nextShot.beltStepIndex <= maxStepIndex)
                    {
                        // 1. 获取预计算时的理论发射位置 (Ground Truth)
                        //    这个位置是 PreCalculatePath 时计算好的，与帧率无关
                        Vector3 idealFirePos = GridManager.Instance.GetSimulatedPosition(nextShot.beltStepIndex);
                        
                        // 2. 修正 Z 坐标 (保持和射手同一平面)
                        idealFirePos.z = transform.position.z; 

                        // 3. 从理论位置开火，确保子弹轨迹正确
                        //    即使射手当前位置因帧率波动已经偏离
                        PerformVisualFire(nextShot.target, idealFirePos);
                    }
                }
                // ========================================
                // 【关键检测】弹药耗尽处理
                // ========================================
                // 如果弹药归零，立即停止移动并播放死亡动画
                if (ammo <= 0)
                {
                    Debug.Log("弹药耗尽，播放死亡动画...");

                    // 1. 立即停止移动（不再继续 yield return null 循环）
                    
                    // 2. 播放死亡动画，并等待它播完
                    yield return StartCoroutine(PerformDeathAnimation());

                    // 3. 彻底销毁游戏对象
                    Destroy(gameObject);
                    
                    // 4. 退出整个 RunBeltSequence 协程
                    yield break; 
                }
                
                yield return null;  // 等待下一帧
            }
            
            // 确保精确到达终点（避免浮点误差）
            transform.position = end;
        }

        // ========================================
        // 阶段4：巡逻完成，决定下一步
        // ========================================
        CheckEndGameAndReturn();
    }

    // ========================================
    // 执行视觉射击效果 (Ground Truth 版)
    // ========================================
    // 当射手到达射击点时，调用此方法执行实际的射击动作
    //
    // 【关键设计】Ground Truth 发射位置
    // 由于帧率波动，射手的实际位置可能与预计算时的理论位置有偏差
    // 为保证子弹轨迹正确，使用预计算时的理论位置 (fireOrigin) 作为发射点
    // 而非射手当前的 transform.position
    //
    // 流程：
    // 1. 消耗1发弹药 (ammo--)
    // 2. 更新UI显示 (UpdateAmmoUI)
    // 3. 实例化子弹，从 fireOrigin 发射向 target
    //
    // 参数：
    //   target - 目标方块
    //   fireOrigin - Ground Truth 发射位置 (来自 GetSimulatedPosition)
    // ========================================
    void PerformVisualFire(CellController target, Vector3 fireOrigin)
    {
        ammo--; 
        UpdateAmmoUI(); 

        if (bulletPrefab != null) 
        {
            GameObject b = Instantiate(bulletPrefab);
            // 【关键】使用传入的 fireOrigin，而不是 transform.position
            b.GetComponent<BulletController>().Fire(target, fireOrigin);
        }
    }

    // ========================================
    // 巡逻结束决策：返回队列 or 绝地反击
    // ========================================
    // 射手完成一圈传送带巡逻后，会检查场上状态决定下一步行动：
    //
    // 【绝地反击模式】条件：
    //   - 备战台完全空了（没有新射手可以选）
    //   - 队列也完全空了（没有其他射手在排队）
    //   => 说明这是场上最后一个射手，触发"绝地反击"
    //   => 以2倍速度直接再上一次传送带，最后一搏！
    //
    // 【正常返回模式】条件：
    //   - 备战台或队列中还有其他射手
    //   => 正常返回队列，排队等待下次出击
    // ========================================
    void CheckEndGameAndReturn()
    {
        bool isTableEmpty = false;
        bool isQueueEmpty = false;
        
        // 检查备战台是否为空
        if (ShooterTableManager.Instance != null) 
            isTableEmpty = ShooterTableManager.Instance.IsTableEmpty();
        
        // 检查队列是否为空
        if (ReadyQueueManager.Instance != null) 
            isQueueEmpty = ReadyQueueManager.Instance.IsQueueEmpty();

        // 绝地反击条件：两处全空
        if (isTableEmpty && isQueueEmpty)
        {
            StartCoroutine(AutoRejoinBelt());  // 触发绝地反击
        }
        else
        {
            ReturnToQueueNormal();             // 正常返回队列
        }
    }

    // ========================================
    // 【绝地反击模式】自动再次上传送带
    // ========================================
    // 这是游戏的高潮时刻！当场上只剩这一个射手时，它不会返回队列，
    // 而是立即加速冲回传送带，以2倍速度再打一圈！
    //
    // 流程：
    // 1. 先"回弹"到队列位置（视觉效果，加快节奏）
    // 2. 开启加速状态（isBoosted = true，移动速度×2）
    // 3. 重新预计算射击路径（因为上一圈可能打掉了一些方块，格局变了）
    // 4. 再次启动传送带巡逻序列
    //
    // 注意：此协程递归调用 RunBeltSequence，理论上可以无限循环
    //       直到弹药耗尽或游戏结束条件满足
    // ========================================
    IEnumerator AutoRejoinBelt()
    {
        currentState = PigState.Returning;
        
        // 找一个队列位置作为"回弹"目标（纯视觉效果）
        Vector3 bounceTarget = Vector3.zero;
        if (ReadyQueueManager.Instance != null)
        {
            int slotIndex = ReadyQueueManager.Instance.GetFirstEmptyIndex();
            if (slotIndex == -1) slotIndex = 0;  // 如果队列满了，就用0号位置
            bounceTarget = ReadyQueueManager.Instance.GetSlotPosition(slotIndex);
        }

        // 视觉回弹效果（加快移动速度，营造紧张感）
        float originalDuration = moveDuration;
        moveDuration = originalDuration * 0.5f;      // 移动时间减半
        yield return StartCoroutine(MoveRoutine(bounceTarget));

        // ========================================
        // 开启加速状态
        // ========================================
        isBoosted = true; 
        Debug.Log(">>> 开启 2 倍速狂暴模式！");
        // ========================================

        moveDuration = originalDuration;             // 恢复原始移动时间
        
        // ========================================
        // 重新预计算路径（关键！）
        // ========================================
        // 因为上一圈可能打掉了一些方块，格局变了，必须重算
        PreCalculatePath(); 
        // ========================================

        // 再次上传送带（递归调用）
        if (BeltPathHolder.Instance != null)
        {
            // 注意：这里调用的是 RunBeltSequence (预计算版)
            yield return StartCoroutine(RunBeltSequence(BeltPathHolder.Instance.waypoints));
        }
    }

    // ========================================
    // 【正常返回模式】返回队列
    // ========================================
    // 射手完成传送带巡逻后的正常流程：
    // 1. 检查队列是否已满（失败条件）
    // 2. 关闭加速状态
    // 3. 找到队列中第一个空位
    // 4. 在队列管理器中注册自己
    // 5. 平滑移动到队列位置
    //
    // 失败条件：
    //   如果队列已满（所有槽位都被占用），说明射手无处可去
    //   => 游戏失败！触发 GameOver
    // ========================================
    void ReturnToQueueNormal()
    {
        if (ReadyQueueManager.Instance == null) return;

        // ========================================
        // 检查失败条件：队列已满
        // ========================================
        if (ReadyQueueManager.Instance.IsFull())
        {
            StopAllCoroutines();
            
            if (GameManager.Instance != null) 
                GameManager.Instance.GameOver(false);
            
            // 延迟销毁，给弹窗足够时间弹出
            Destroy(gameObject, 0.5f);
            return;
        }

        // ========================================
        // 关闭加速状态（如果之前开启过）
        // ========================================
        isBoosted = false;
        // ========================================

        // 找到队列中第一个空位
        int targetSlot = ReadyQueueManager.Instance.GetFirstEmptyIndex();
        Vector3 pos = ReadyQueueManager.Instance.GetSlotPosition(targetSlot);
        
        // 更新状态和位置
        currentState = PigState.InQueue;
        currentQueueIndex = targetSlot;
        
        // 在队列管理器中注册
        ReadyQueueManager.Instance.RegisterPig(targetSlot, this);
        
        // 平滑移动到目标位置
        SmoothMoveTo(pos);
        
        // 重置旋转（确保站立姿态正确）
        transform.rotation = Quaternion.identity;
    }

    // ========================================
    // 平滑移动（公共接口）
    // ========================================
    // 停止所有正在进行的移动，启动新的移动协程
    // 用于任何需要移动到指定位置的场景
    //
    // 参数：
    //   targetPos - 目标位置的世界坐标
    public void SmoothMoveTo(Vector3 targetPos)
    {
        StopAllCoroutines();                    // 停止所有协程（避免冲突）
        StartCoroutine(MoveRoutine(targetPos)); // 启动新的移动协程
    }

    // ========================================
    // 移动协程（核心移动算法）
    // ========================================
    // 使用动画曲线实现平滑的缓入缓出效果
    // 比简单的 Lerp 更有手感，更符合真实物理运动
    //
    // 参数：
    //   target - 目标位置
    //
    // 工作原理：
    // 1. 记录起始位置
    // 2. 在 moveDuration 时间内，每帧更新位置
    // 3. 使用 moveCurve 曲线计算插值百分比（而非线性）
    // 4. 用 LerpUnclamped 允许曲线超出 [0,1] 范围（支持弹性效果）
    // ========================================
    IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;
        
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / moveDuration;  // 0 ~ 1 的进度
            
            // 使用曲线计算实际插值百分比（支持缓入缓出等效果）
            transform.position = Vector3.LerpUnclamped(
                startPos, 
                target, 
                moveCurve.Evaluate(percent)
            );
            
            yield return null;  // 等待下一帧
        }
        
        // 确保精确到达目标（避免浮点误差）
        transform.position = target;
    }

    // ==========================================
    // 【死亡动画】弹药耗尽时的视觉效果
    // ==========================================
    // 当射手弹药归零时，会播放这个0.3秒的死亡动画，然后销毁
    //
    // 动画分为两个阶段：
    // 【第一阶段】0 ~ 0.15秒：
    //   - 旋转：顺时针旋转 360 度（绕 Y 轴）
    //   - 缩放：从原始大小放大到 1.2 倍
    //
    // 【第二阶段】0.15 ~ 0.3秒：
    //   - 旋转：逆时针旋转回去（360度 -> 0度）
    //   - 缩放：从 1.2 倍缩小到 0（完全消失）
    //
    // 视觉效果：
    //   像一个气球先膨胀旋转，然后放气缩小消失
    //   营造出"能量耗尽"的感觉
    // ==========================================
    IEnumerator PerformDeathAnimation()
    {
        float totalDuration = 0.3f;                  // 总动画时长
        float halfDuration = totalDuration / 2f;     // 单阶段时长
        
        Vector3 originalScale = transform.localScale; // 记住初始大小
        Quaternion originalRot = transform.rotation;  // 记住初始朝向

        // ========================================
        // 第一阶段：膨胀 + 顺时针旋转 (0 ~ 0.15秒)
        // ========================================
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;  // 0 ~ 1 的进度

            // 缩放：从原始大小放大到 1.2 倍
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
            
            // 旋转：顺时针转 360 度（绕 Y 轴）
            // t = 0 时角度 = 0，t = 1 时角度 = 360
            transform.rotation = originalRot * Quaternion.Euler(0, 360f * t, 0);

            yield return null;
        }

        // ========================================
        // 第二阶段：收缩 + 逆时针旋转 (0.15 ~ 0.3秒)
        // ========================================
        timer = 0f;
        Vector3 bigScale = originalScale * 1.2f;  // 记住放大后的大小
        
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;  // 0 ~ 1 的进度

            // 缩放：从 1.2 倍缩小到 0（完全消失）
            transform.localScale = Vector3.Lerp(bigScale, Vector3.zero, t);
            
            // 旋转：逆时针转回去（360度 -> 0度）
            // t = 0 时角度 = 360，t = 1 时角度 = 0
            float angle = Mathf.Lerp(360f, 0f, t);
            transform.rotation = originalRot * Quaternion.Euler(0, angle, 0);

            yield return null;
        }

        // ========================================
        // 彻底隐藏（防止 Destroy 延迟导致的闪烁）
        // ========================================
        transform.localScale = Vector3.zero;
    }


}