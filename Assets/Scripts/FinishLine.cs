using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine2D : MonoBehaviour
{
    [Header("Level Completion Settings")]
    public float requiredTime = 3.0f; // Required continuous stay time (seconds)
    public string playerTag = "Player"; // Player object tag

    private float timer = 0f;
    private bool isTouching = false;

    void Update()
    {
        if (isTouching)
        {
            timer += Time.deltaTime;

            // Load next level after required stay time is reached
            if (timer >= requiredTime)
            {
                LoadNextLevel();
            }
        }
    }

    // Triggered while inside a 2D trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("Player entered finish zone. Timer started...");
        }
    }

    // Triggered when exiting a 2D trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = false;
            timer = 0f; // Reset timer after leaving
            Debug.Log("Player left finish zone. Timer reset.");
        }
    }

    void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("This is already the last level. Cannot load next level.");
        }
    }
}