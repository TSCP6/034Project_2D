using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 新增：添加协程所需的命名空间

/// <summary>
/// 按R键重启当前场景的通用脚本
/// 可挂载到任意空物体（比如GameManager）
/// </summary>
public class RestartOnRKey : MonoBehaviour
{
    [Header("重启设置")]
    public bool enableRestart = true; // 是否启用R键重启功能
    public KeyCode restartKey = KeyCode.R; // 重启按键（默认R）
    public float restartDelay = 0.1f; // 重启延迟（避免误触，默认0.1秒）
    public bool showDebugLog = true; // 是否打印重启日志（调试用）

    [Header("可选：重启时的额外操作")]
    public bool resetTimeScale = true; // 重启时重置时间缩放（防止暂停状态残留）

    private bool isRestarting = false; // 防止重复触发重启

    void Update()
    {
        // 未启用/正在重启时，不响应按键
        if (!enableRestart || isRestarting) return;

        // 检测R键按下（带延迟防抖）
        if (Input.GetKeyDown(restartKey))
        {
            StartCoroutine(RestartSceneCoroutine());
        }
    }

    /// <summary>
    /// 协程重启场景（带延迟，避免卡顿）
    /// </summary>
    private IEnumerator RestartSceneCoroutine()
    {
        isRestarting = true;

        // 可选：重置时间缩放（比如游戏暂停后重启，恢复正常时间）
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        if (showDebugLog)
        {
            Debug.Log($"按下{restartKey}键，{restartDelay}秒后重启当前场景...");
        }

        // 延迟一小段时间，避免按键误触
        yield return new WaitForSecondsRealtime(restartDelay);

        // 获取当前激活的场景名称，重启该场景
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        if (showDebugLog)
        {
            Debug.Log($"场景[{currentSceneName}]重启完成！");
        }

        isRestarting = false;
    }

    /// <summary>
    /// 手动调用重启（比如给UI按钮绑定）
    /// </summary>
    public void ManualRestart()
    {
        if (!isRestarting)
        {
            StartCoroutine(RestartSceneCoroutine());
        }
    }
}