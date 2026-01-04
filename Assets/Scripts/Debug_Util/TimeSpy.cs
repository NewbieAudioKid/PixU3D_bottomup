using UnityEngine;

public class TimeSpy : MonoBehaviour
{
    private float lastTimeScale = 1f;
    
    void Update()
    {
        // 每一帧都监控时间流速
        if (Time.timeScale != lastTimeScale)
        {
            Debug.LogError($"🚨🚨🚨 Time.timeScale 改变了！从 {lastTimeScale} 变成 {Time.timeScale}");
            Debug.LogError($"🚨 调用堆栈：\n{System.Environment.StackTrace}");
            lastTimeScale = Time.timeScale;
        }
        
        if (Time.timeScale == 0)
        {
            Debug.LogError("🚨 游戏被暂停了！Time.timeScale = 0");
        }
    }
}