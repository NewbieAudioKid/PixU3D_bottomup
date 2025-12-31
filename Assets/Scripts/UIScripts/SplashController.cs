using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 必须引用 UI
using System.Collections;

public class SplashController : MonoBehaviour
{
    [Header("设置")]
    public string nextSceneName = "MenuScene"; 
    
    [Tooltip("展示图片的停留时间")]
    public float waitTime = 1.5f; 

    [Tooltip("淡出变黑需要的时间")]
    public float fadeDuration = 2.0f;

    [Header("引用")]
    public CanvasGroup blackCurtain; // 把刚才那个 BlackCurtain 拖进来

    void Start()
    {
        // 确保黑幕一开始是透明的
        if (blackCurtain != null) blackCurtain.alpha = 0f;
        
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        // 1. 停留：让玩家欣赏一会儿海报
        yield return new WaitForSeconds(waitTime);

        // 2. 渐变：让黑幕慢慢变成不透明 (Alpha 0 -> 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 计算当前进度 0~1
            float progress = timer / fadeDuration;
            
            if (blackCurtain != null)
            {
                blackCurtain.alpha = progress; 
            }
            
            yield return null; // 等待下一帧
        }

        // 确保完全变黑
        if (blackCurtain != null) blackCurtain.alpha = 1f;

        // 3. 趁着全黑的时候，偷偷加载场景
        SceneManager.LoadScene(nextSceneName);
    }
}