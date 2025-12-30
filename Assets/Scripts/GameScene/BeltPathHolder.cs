using UnityEngine;
using System.Collections.Generic;

public class BeltPathHolder : MonoBehaviour
{
    // 1. 定义一个静态的“自己”，让全世界都能直接访问
    public static BeltPathHolder Instance;

    public List<Transform> waypoints;

    void Awake()
    {
        // 2. 初始化单例
        Instance = this;
    }
}