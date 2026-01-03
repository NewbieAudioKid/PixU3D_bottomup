// ================================================================================
// TL;DR:
// 准备队列管理器，负责管理5个射手准备位（槽位）的占用状态和自动补位。
// 采用 Shift Left 算法实现队列前移，避免空位分散。
//
// 目标：
// - 管理5个固定槽位的射手注册/注销
// - 实现射手离开后的自动左移补位（类似数组删除元素）
// - 提供槽位空满状态查询接口
// - 检测队列满时触发游戏失败条件
//
// 非目标：
// - 不处理射手生成（由 ShooterTableManager 负责）
// - 不处理射手上传送带的逻辑（由 PigController 负责）
// - 不渲染 UI 提示（由 UI 组件负责）
// ================================================================================
using UnityEngine;

public class ReadyQueueManager : MonoBehaviour
{
    public static ReadyQueueManager Instance;

    [Header("设置")]
    // 这里拖拽场景里那 5 个 Slot_Prefab (底座)
    public Transform[] slots; 
    
    // 记录每个坑位当前放了哪只猪 (null 代表空)
    private PigController[] currentPigs;

    void Awake()
    {
        Instance = this;
        currentPigs = new PigController[slots.Length];
    }

    // 查找第一个空位的索引 (0-4)，如果没有空位返回 -1
    public int GetFirstEmptyIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (currentPigs[i] == null)
            {
                return i;
            }
        }
        return -1; // 满了
    }

    // 获取某个坑位的世界坐标
    public Vector3 GetSlotPosition(int index)
    {
        if (index >= 0 && index < slots.Length)
        {
            return slots[index].position;
        }
        return Vector3.zero;
    }

    // 注册小猪
    public void RegisterPig(int index, PigController pig)
    {
        if (index >= 0 && index < currentPigs.Length)
        {
            currentPigs[index] = pig;
            pig.transform.SetParent(slots[index]); 
        }
    }

    // ==========================================
    // 【核心修复】注销并自动补位 (Shift Left)
    // ==========================================
    public void UnregisterPig(PigController pigLeaving)
    {
        int removedIndex = -1;

        // 1. 找到这只离开的猪刚才坐哪 (比如 index 0)
        for (int i = 0; i < currentPigs.Length; i++)
        {
            if (currentPigs[i] == pigLeaving)
            {
                currentPigs[i] = null; // 先挖坑
                removedIndex = i;
                break;
            }
        }

        // 如果没找到这只猪，或者它是最后一只，就不需要挪动别人
        if (removedIndex == -1 || removedIndex == currentPigs.Length - 1) return;

        // 2. 循环移位：从被挖坑的位置开始，把后面的猪统统往左挪一位
        for (int i = removedIndex; i < currentPigs.Length - 1; i++)
        {
            // 获取后一位的猪
            PigController nextPig = currentPigs[i + 1];

            if (nextPig != null)
            {
                // A. 数据搬家：把后面的猪移到当前位置 (i)
                currentPigs[i] = nextPig;
                currentPigs[i + 1] = null; // 把原来的位置 (i+1) 设为空

                // B. 视觉搬家：让猪飞到新的底座上
                // 我们直接复用 PigController 里的 MoveToQueue 方法
                // 这非常方便，因为它会自动更新猪内部的 index，并且播放平滑移动动画
                nextPig.MoveToQueue(i, GetSlotPosition(i));
            }
        }
    }
    // ================= 新增：查询备战区是否为空 =================
    public bool IsQueueEmpty()
    {
        // 遍历所有坑位
        for (int i = 0; i < currentPigs.Length; i++)
        {
            // 只要有一个坑里有猪，就不算空
            if (currentPigs[i] != null) return false;
        }
        return true; // 全是空的
    }
    // 检查是否已满
    public bool IsFull()
    {
        return GetFirstEmptyIndex() == -1;
    }
}