using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 使用 LateUpdate 确保在小猪动完之后，我们再修正文字角度
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 方案 A：直接复制摄像机的旋转角度（最稳，摄像机怎么转它就怎么转）
            transform.rotation = mainCamera.transform.rotation;
            
            // 方案 B（备选）：如果你希望它永远完全竖直，不管摄像机有没有歪
            // transform.rotation = Quaternion.identity; 
        }
    }
}