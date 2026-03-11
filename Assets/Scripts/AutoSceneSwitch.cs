using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 30秒自动切换场景管理器
/// 挂载到任意空物体（比如SceneTimer）即可
/// </summary>
public class AutoSceneSwitch : MonoBehaviour
{
    [Header("核心设置")]
    public float switchDelay = 30f; // 自动切换的倒计时（秒），默认30秒
    public string nextSceneName; // 下一个场景的名称（必须和Build Settings里一致）
    public int nextSceneIndex = -1; // 下一个场景的索引（优先级高于名称）

    [Header("倒计时UI（可选）")]
    public Text countdownText; // 显示倒计时的文本（比如UI Text/TMP_Text）
    public bool showCountdown = true; // 是否显示倒计时

    [Header("进阶设置")]
    public bool startTimerOnAwake = true; // 是否启动游戏就开始计时
    public bool pauseOnTimeScaleZero = true; // 时间缩放为0时（暂停），是否暂停计时

    private Coroutine timerCoroutine;
    private float remainingTime; // 剩余倒计时

    void Awake()
    {
        // 初始化剩余时间
        remainingTime = switchDelay;

        // 启动计时（如果开启）
        if (startTimerOnAwake)
        {
            StartTimer();
        }

        // 初始化UI
        UpdateCountdownUI();
    }

    /// <summary>
    /// 启动倒计时（可手动调用，比如游戏开始后再启动）
    /// </summary>
    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// 核心：倒计时协程
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        while (remainingTime > 0)
        {
            // 时间暂停时，停止计时
            if (pauseOnTimeScaleZero && Time.timeScale == 0)
            {
                yield return null;
                continue;
            }

            // 扣减时间（用unscaledDeltaTime，不受时间缩放影响）
            remainingTime -= Time.unscaledDeltaTime;
            // 防止时间为负数
            remainingTime = Mathf.Max(0, remainingTime);

            // 更新倒计时UI
            if (showCountdown)
            {
                UpdateCountdownUI();
            }

            yield return null;
        }

        // 计时结束，切换场景
        SwitchToNextScene();
    }

    /// <summary>
    /// 更新倒计时UI显示
    /// </summary>
    private void UpdateCountdownUI()
    {
        if (countdownText == null) return;

        // 显示格式：剩余X秒（保留整数）
        countdownText.text = $"自动切换场景：{Mathf.FloorToInt(remainingTime)}秒";
    }

    /// <summary>
    /// 切换到下一个场景
    /// </summary>
    private void SwitchToNextScene()
    {
        // 优先级：场景索引 > 场景名称
        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            Debug.Log($"计时结束，切换到场景索引：{nextSceneIndex}");
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // 修复警告：改用官方推荐的方式检查场景是否存在
            bool sceneExists = false;
            // 遍历Build Settings中的所有场景（不是当前加载的）
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == nextSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }

            if (sceneExists)
            {
                SceneManager.LoadScene(nextSceneName);
                Debug.Log($"计时结束，切换到场景：{nextSceneName}");
            }
            else
            {
                Debug.LogError($"场景{nextSceneName}不存在！请检查Build Settings是否添加该场景。");
            }
        }
        else
        {
            Debug.LogError("未设置下一个场景的名称/索引！");
        }
    }

    /// <summary>
    /// 手动暂停计时（可选）
    /// </summary>
    public void PauseTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    /// <summary>
    /// 手动重置计时（可选）
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = switchDelay;
        UpdateCountdownUI();
        StartTimer();
    }

    /// <summary>
    /// 手动触发场景切换（可选）
    /// </summary>
    public void ManualSwitchScene()
    {
        remainingTime = 0;
        SwitchToNextScene();
    }
}