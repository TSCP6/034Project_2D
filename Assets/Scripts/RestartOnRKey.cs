using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RestartOnRKey : MonoBehaviour
{


    [Header("Restart Settings")]
    public bool enableRestart = true; // Enable R key restart
    public KeyCode restartKey = KeyCode.R; // Restart key (default R)
    public float restartDelay = 0.1f; // Restart delay (to prevent accidental trigger, default 0.1s)
    public bool showDebugLog = true; // Show restart debug log (for debugging)

    [Header("Optional: Extra actions on restart")]
    public bool resetTimeScale = true; // Reset time scale on restart (prevent pause state residue)

    private bool isRestarting = false; // Prevent repeated restart triggers
    void Update()
    {
        // Do not respond to key if not enabled or already restarting
        if (!enableRestart || isRestarting) return;

        // Detect R key press (with delay debounce)
        if (Input.GetKeyDown(restartKey))
        {
            StartCoroutine(RestartSceneCoroutine());
        }
    }

    private IEnumerator RestartSceneCoroutine()
    {
        isRestarting = true;

        // Optional: reset time scale 
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        if (showDebugLog)
        {
            Debug.Log($"press {restartKey}£¬reload after {restartDelay} seconds...");
        }

        // Delay briefly to prevent accidental key press
        yield return new WaitForSecondsRealtime(restartDelay);

        // Get current active scene name and restart it
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        if (showDebugLog)
        {
            Debug.Log($"scene [{currentSceneName}] reload successfully£¡");
        }

        isRestarting = false;
    }

    public void ManualRestart()
    {
        if (!isRestarting)
        {
            StartCoroutine(RestartSceneCoroutine());
        }
    }
}