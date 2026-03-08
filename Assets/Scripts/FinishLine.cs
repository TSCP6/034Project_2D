using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine2D : MonoBehaviour
{
    [Header("????")]
    public float requiredTime = 5.0f; // ?????????????
    public string playerTag = "Player"; // ????????

    private float timer = 0f;
    private bool isTouching = false;

    void Update()
    {
        if (isTouching)
        {
            timer += Time.deltaTime;

            // ?????????????????????????????? UI ??????
            if (timer >= requiredTime)
            {
                LoadNextLevel();
            }
        }
    }

    // ???2D ????????? OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("??????...");
        }
    }

    // ???2D ????????? OnTriggerExit2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = false;
            timer = 0f; // ??????????
            Debug.Log("????????");
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
            Debug.LogWarning("????????????????????");
        }
    }
}