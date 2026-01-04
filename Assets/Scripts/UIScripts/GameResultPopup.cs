using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameResultPopup : MonoBehaviour
{
    [Header("UI 组件引用")]
    public Transform windowContainer; 
    public TextMeshProUGUI titleText;
    
    [Header("按钮引用")]
    public GameObject btnRetry;
    public GameObject btnMenu;
    public GameObject btnNext;

    [Header("动画设置")]
    public float popDuration = 0.5f;
    public AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0, 0), 
        new Keyframe(0.5f, 1.1f), 
        new Keyframe(1, 1)        
    );

    [Header("场景设置")]
    public string menuSceneName = "MenuScene"; 

    public static GameResultPopup Instance;

    void Awake()
    {
        Instance = this;
        // 注意：不要在这里 SetActive(false)，否则可能导致后续逻辑问题
        // 我们改在 Start 里隐藏
    }

    void Start()
    {
        // 游戏刚开始时，自动隐藏弹窗
        // 这样可以保证你在编辑器里勾选它是 Active 的（确保 Awake 运行），但进游戏它是隐藏的
        
        // 【重要】先重置 windowContainer 的缩放为 0，避免闪烁
        if (windowContainer != null)
        {
            windowContainer.localScale = Vector3.zero;
        }
        
        gameObject.SetActive(false);
    }

    // --- 公共接口 ---

    public void ShowVictory()
    {
        SetupWindow("Victory", true);
    }

    public void ShowGameOver()
    {
        SetupWindow("Game Over", false);
    }

    // 延迟显示 GameOver（在自己身上运行协程，避免 DontDestroyOnLoad 对象的协程被中断）
    public void ShowGameOverDelayed()
    {
        gameObject.SetActive(true);
        StartCoroutine(DelayedGameOverCoroutine());
    }

    private IEnumerator DelayedGameOverCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        SetupWindow("Game Over", false);
    }

    // --- 内部逻辑 ---

    private void SetupWindow(string title, bool isVictory)
    {
        // 确保父对象也是激活的
        if (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
            transform.parent.gameObject.SetActive(true);
        
        gameObject.SetActive(true);
        
        // 激活窗口容器并重置缩放
        if (windowContainer != null)
        {
            windowContainer.gameObject.SetActive(true);
            windowContainer.localScale = Vector3.zero;
        }
        else
        {
            Debug.LogError("❌ Inspector 里没绑定 Window Container！");
            return;
        }
        
        // 设置标题
        if (titleText != null)
            titleText.text = title;

        // 控制按钮显隐
        if (isVictory)
        {
            if(btnNext) btnNext.SetActive(true);
            if(btnMenu) btnMenu.SetActive(true);
            if(btnRetry) btnRetry.SetActive(false);
        }
        else
        {
            if(btnNext) btnNext.SetActive(false);
            if(btnRetry) btnRetry.SetActive(true);
            if(btnMenu) btnMenu.SetActive(true);
        }

        // 播放弹出动画
        StartCoroutine(PopAnimation());
    }

    IEnumerator PopAnimation()
    {
        if (windowContainer == null) yield break;
        
        float timer = 0f;
        windowContainer.localScale = Vector3.zero; 

        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float progress = timer / popDuration;
            float scale = popCurve.Evaluate(progress);
            windowContainer.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
        
        windowContainer.localScale = Vector3.one;
    }

    // --- 按钮点击事件 ---

    public void OnClickRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void OnClickReturnToMenu()
    {
        // 如果是胜利状态（btnNext 显示），回到菜单时更新关卡进度
        if (btnNext != null && btnNext.activeSelf)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AdvanceLevelProgress();
        }

        SceneManager.LoadScene(menuSceneName);
    }

    public void OnClickNext()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel(); 
    }
}