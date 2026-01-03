// ================================================================================
// TL;DR:
// 射手备战台管理器，负责从 JSON 生成多列射手库存并处理玩家选择交互。
// 采用列表嵌套结构（List<List>）实现"下层自动顶上"的堆栈效果。
//
// 目标：
// - 从 JSON 加载并生成5列×6行的射手网格
// - 响应玩家点击，将最上层射手移动到准备队列
// - 实现列内自动补位（下层射手上移）
// - 检测备战台是否清空（用于绝地反击判定）
//
// 非目标：
// - 不处理射手上传送带后的行为（由 PigController 负责）
// - 不管理准备队列（由 ReadyQueueManager 负责）
// - 不处理射击和移动逻辑（由 PigController 和 BeltWalker 负责）
// ================================================================================
using UnityEngine;
using System.Collections.Generic;

public class ShooterTableManager : MonoBehaviour
{
    public static ShooterTableManager Instance;

    [Header("配置")]
    public GameObject pigPrefab;
    public int columns = 5; // 几列
    public int rows = 6;    // 几行
    public float spacingX = 1.5f; // 间距要和上面的 Slot 对应
    public float spacingY = 1.6f;

    // 存储库存数据：List的List，外层是列，内层是该列的小猪
    // 这样方便处理“最下面顶上来”的逻辑
    public List<List<PigController>> tableColumns = new List<List<PigController>>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateTable();
    }

void GenerateTable()
    {
        // 1. 获取数据
        ShooterTableData data = null;
        if (GameManager.Instance != null)
        {
            data = GameManager.Instance.LoadTableData();
        }
        
        if (data == null) return;

        // 计算起始 X
        float startX = -(columns - 1) * spacingX / 2.0f;

        // 遍历 JSON 里的列 (注意：JSON列数可能和游戏设置不一致，要注意防呆)
        for (int col = 0; col < columns; col++)
        {
            List<PigController> currentColumnList = new List<PigController>();

            // 如果 JSON 里这一列有数据
            if (col < data.columns.Count)
            {
                List<ShooterData> shootersInCol = data.columns[col].shooters;

                // 从下往上生成还是从上往下？通常 JSON 数组第0个是最上面的
                for (int row = 0; row < shootersInCol.Count; row++)
                {
                    // 限制行数，防止超出屏幕
                    if (row >= rows) break; 

                    ShooterData shooterInfo = shootersInCol[row];

                    Vector3 pos = new Vector3(
                        startX + col * spacingX, 
                        -row * spacingY, 
                        0
                    ) + transform.position;

                    GameObject pigObj = Instantiate(pigPrefab, pos, Quaternion.identity, transform);
                    PigController pig = pigObj.GetComponent<PigController>();
                    
                    // 使用 JSON 里的数据初始化
                    pig.InitData(shooterInfo.color, shooterInfo.ammo);
                    pig.SetState(PigState.InTable);

                    currentColumnList.Add(pig);
                }
            }
            
            tableColumns.Add(currentColumnList);
        }
    }

    // 当玩家点击某一列的最上面那只猪时
    public void OnPigClicked(PigController pig)
    {
        // 1. 找到这只猪在哪一列
        int colIndex = -1;
        for(int i=0; i<columns; i++)
        {
            if (tableColumns[i].Count > 0 && tableColumns[i][0] == pig)
            {
                colIndex = i;
                break;
            }
        }

        // 如果不是最上面那只，或者没找到，忽略
        if (colIndex == -1) return;

        // 2. 尝试移动到 ReadyQueue
        int targetSlot = ReadyQueueManager.Instance.GetFirstEmptyIndex();
        if (targetSlot != -1) // 有空位
        {
            // 从库存列表移除
            tableColumns[colIndex].RemoveAt(0);
            
            // 执行移动逻辑
            Vector3 targetPos = ReadyQueueManager.Instance.GetSlotPosition(targetSlot);
            pig.MoveToQueue(targetSlot, targetPos);
            
            // 3. 让这一列剩下的猪向上补位
            UpdateColumnPositions(colIndex);
        }
        else
        {
            Debug.Log("备战区满啦！");
            // 这里可以播个拒绝动画
        }
    }

    void UpdateColumnPositions(int colIndex)
    {
        List<PigController> colList = tableColumns[colIndex];
        float startX = -(columns - 1) * spacingX / 2.0f;
        
        for (int row = 0; row < colList.Count; row++)
        {
             Vector3 newPos = new Vector3(
                startX + colIndex * spacingX, 
                -row * spacingY, 
                0
            ) + transform.position;
            
            // 让猪猪平滑移动过去（这里为了简单先直接瞬移，或者你可以加个MoveTowards）
            // 建议在 PigController 里加一个 MoveTo 方法
            colList[row].SmoothMoveTo(newPos);
        }
    }

    // ================= 新增：查询库存是否为空 =================
    public bool IsTableEmpty()
    {
        // 遍历每一列
        foreach (var column in tableColumns)
        {
            // 只要有一列里还有猪，就不算空
            if (column.Count > 0) return false;
        }
        return true; // 所有列都空了
    }
}