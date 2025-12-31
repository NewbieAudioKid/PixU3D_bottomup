using UnityEngine;
using System.Collections; // 必须引用这个，才能用协程
// UI_StartButton.cs (只负责业务逻辑)
public class UI_StartButton : MonoBehaviour 
{
    public float delayBeforeLoad = 0.4f; // 这里的时间最好和 ElasticButton 的 duration 差不多

    public void ClickStartGame() 
    {
        StartCoroutine(WaitAndGo());
    }
    
    IEnumerator WaitAndGo()
    {
        // 这里纯粹就是为了等 ElasticButton 的视觉动画播一会儿
        yield return new WaitForSeconds(delayBeforeLoad);
        
        GameManager.Instance.StartLevel("Level_1");
    }
}