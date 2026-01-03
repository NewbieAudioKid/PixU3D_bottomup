// ================================================================================
// TL;DR:
// 传送带行走组件，负责物体沿预定义路径点的自动移动和转向。
// 支持速度动态调整（用于绝地反击加速），基于 UnityEvent 的事件通知机制。
//
// 目标：
// - 实现沿多个路点的循环巡逻移动
// - 自动朝向下一个路点（2D 平面旋转）
// - 支持运行时速度倍增（绝地反击模式）
// - 路径完成后通过 UnityEvent 通知上层逻辑
//
// 非目标：
// - 不处理射击逻辑（由 PigController 负责）
// - 不生成或管理路径点（由 BeltPathHolder 提供）
// - 不处理碰撞检测（纯粹基于路径移动）
// ================================================================================
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events; // 必须引用这个才能用 UnityEvent

public class BeltWalker : MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 5.0f;
    private float baseSpeed; // 记住初始速度
    // 内部状态
    private bool isMoving = false;
    private List<Transform> currentPath;
    private int targetIndex = 0;

    // 【核心修复】直接在这里初始化 new UnityEvent()
    // 这样就不怕 PigController 在 Awake 里访问它时是空的了
    public UnityEvent OnPathComplete = new UnityEvent();
void Start()
    {
        baseSpeed = speed; // 游戏开始时记下：哦，原来我是跑 5 码的
    }
    void Update()
    {
        // 安全检查：如果没开始动，或者路径为空，就什么都不做
        if (!isMoving || currentPath == null || currentPath.Count == 0) return;

        // 获取当前要去的目标点
        Transform target = currentPath[targetIndex];
        
        // 1. 移动逻辑 (匀速移动)
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 2. 检查是否到达目标点 (距离小于 0.05 就算到了)
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            // 到了，索引加1，准备去下一个点
            targetIndex++;

            // 检查是不是跑完了一整圈 (索引超出了列表长度)
            if (targetIndex >= currentPath.Count)
            {
                // 跑完了，停止移动
                isMoving = false; 
                
                // 触发事件，通知 PigController "我回来了"
                // 这里的 Invoke() 就是打电话告诉订阅者
                OnPathComplete?.Invoke();
            }
            else
            {
                // 没跑完，调整朝向看下一个点
                LookAtTarget(currentPath[targetIndex]);
            }
        }
    }

    // 转向逻辑：让小猪的头（Y轴）指向目标
    void LookAtTarget(Transform target)
    {
         Vector3 direction = target.position - transform.position;
         
         // 计算角度 (Atan2 返回的是弧度，要转成度数)
         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
         
         // 2D平面旋转 Z 轴
         // 因为 Unity 0度通常指右边，如果你的模型头朝上，通常要减90度
         transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    // 供外部 (PigController) 调用：开始跑
    public void BeginJourney(List<Transform> path)
    {
        // 安全检查
        if (path == null || path.Count == 0)
        {
            Debug.LogError("BeltWalker: 接收到的路径是空的！无法开始移动。");
            return;
        }

        currentPath = path;
        targetIndex = 0; // 从第0个点开始
        isMoving = true;
        
        // 瞬间把猪传送到起跑点，防止位置瞬移穿帮
        transform.position = path[0].position;
        
        // 既然已经在了起点，立刻让猪面向第二个点（防止刚开始的一帧朝向不对）
        if (path.Count > 1)
        {
            LookAtTarget(path[1]);
        }
    }
    // 提供一个重置方法，或者在 PigController 里直接改 speed 也可以
    // 但为了安全，建议用代码控制
    public void SetDoubleSpeed()
    {
        speed = baseSpeed * 2f;
    }
    
    public void ResetSpeed()
    {
        speed = baseSpeed;
    }
}