using UnityEngine;

public class SceneFader : MonoBehaviour
{
    public float fadeSpeed = 1.5f;
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        
        // 确保一开始是全黑
        cg.alpha = 1f;
    }

    void Update()
    {
        // 只要还不是透明的，就每帧减小 Alpha
        if (cg.alpha > 0)
        {
            cg.alpha -= Time.deltaTime * fadeSpeed;
        }
        else
        {
            // 变透明后，销毁这个黑布，节省资源
            Destroy(gameObject); 
        }
    }
}