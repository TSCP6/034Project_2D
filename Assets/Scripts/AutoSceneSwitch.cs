using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class AutoSceneSwitch : MonoBehaviour
{
    [Header("Core Settings")]
    public float switchDelay = 30f; // Countdown for auto switch (seconds), default 30s
    public string nextSceneName; // Name of the next scene (must match Build Settings)
    public int nextSceneIndex = -1; // Index of the next scene (priority over name)

    [Header("Countdown UI (Optional)")]
    public Text countdownText; // Text to display countdown (e.g. UI Text/TMP_Text)
    public bool showCountdown = true; // Show countdown

    [Header("Advanced Settings")]
    public bool startTimerOnAwake = true; // Start timer on game awake
    public bool pauseOnTimeScaleZero = true; // Pause timer when time scale is 0
    private Coroutine timerCoroutine;
    private float remainingTime;

    void Awake()
    {
        // Initialize remaining time
        remainingTime = switchDelay;

        // Start timer (if enabled)
        if (startTimerOnAwake)
        {
            StartTimer();
        }

        // Initialize UI
        UpdateCountdownUI();
    }

    /// <summary>
    /// Start countdown (can be called manually, e.g. after game start)
    /// </summary>
    public void StartTimer()
    {
        StartTimer(switchDelay);
    }

    /// <summary>
    /// Start countdown with a custom delay.
    /// </summary>
    public void StartTimer(float delay)
    {
        remainingTime = Mathf.Max(0, delay);
        UpdateCountdownUI();

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        if (remainingTime <= 0)
        {
            SwitchToNextScene();
            return;
        }

        timerCoroutine = StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// Core: countdown coroutine
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        while (remainingTime > 0)
        {
            // Stop timer when time is paused
            if (pauseOnTimeScaleZero && Time.timeScale == 0)
            {
                yield return null;
                continue;
            }

            // Decrement time (use unscaledDeltaTime, not affected by time scale)
            remainingTime -= Time.unscaledDeltaTime;
            // Prevent negative time
            remainingTime = Mathf.Max(0, remainingTime);

            // Update countdown UI
            if (showCountdown)
            {
                UpdateCountdownUI();
            }

            yield return null;
        }

        // Countdown finished, switch scene
        SwitchToNextScene();
    }

    /// <summary>
    /// Update countdown UI display
    /// </summary>
    private void UpdateCountdownUI()
    {
        if (countdownText == null) return;

        // Display format: X seconds remaining (integer only)
        countdownText.text = $"Auto scene switch in: {Mathf.FloorToInt(remainingTime)}s";
    }

    /// <summary>
    /// Switch to the next scene
    /// </summary>
    private void SwitchToNextScene()
    {
        // Priority: scene index > scene name
        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            Debug.Log($"Countdown finished, switching to scene index: {nextSceneIndex}");
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Check if scene exists using Unity recommended method
            bool sceneExists = false;
            // Iterate all scenes in Build Settings (not currently loaded)
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
                Debug.Log($"Countdown finished, switching to scene: {nextSceneName}");
            }
            else
            {
                Debug.LogError($"Scene {nextSceneName} does not exist! Please check if it is added in Build Settings.");
            }
        }
        else
        {
            Debug.LogError("Next scene name/index not set!");
        }
    }

    /// <summary>
    /// Manually pause timer (optional)
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
    /// Manually reset timer (optional)
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = switchDelay;
        UpdateCountdownUI();
        StartTimer();
    }

    /// <summary>
    /// Manually trigger scene switch (optional)
    /// </summary>
    public void ManualSwitchScene()
    {
        remainingTime = 0;
        SwitchToNextScene();
    }
}