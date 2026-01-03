// ================================================================================
// TL;DR:
// 关卡选择器 UI 控制器，响应玩家点击关卡按钮并触发关卡加载。
// 采用极简设计，每个关卡一个方法，易于扩展和维护。
//
// 目标：
// - 提供关卡选择界面的点击响应接口
// - 通知 GameManager 加载指定关卡
// - 支持多关卡扩展（复制方法即可）
//
// 非目标：
// - 不处理关卡解锁逻辑（若需要可扩展存档系统）
// - 不处理关卡预览或难度显示（由 UI 设计师配置静态元素）
// - 不处理场景切换动画（由 SceneFader 或 SplashController 负责）
// ================================================================================
using UnityEngine;

public class LevelSelectorUI : MonoBehaviour
{
    public void OnClickLevel1()
    {
        // 告诉 GameManager 去加载 level_1
        // 假设你的 json 文件名是 level_1_grid.json 和 level_1_table.json
        GameManager.Instance.StartLevel("level_1");
    }

    // 你可以复制这个方法给 Level 2, Level 3...
}