using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine2D : MonoBehaviour
{
    [Header("设置")]
    public float requiredTime = 5.0f; // 需要停留的总时间
    public string playerTag = "Player"; // 玩家的标签名

    private float timer = 0f;
    private bool isTouching = false;

    void Update()
    {
        if (isTouching)
        {
            timer += Time.deltaTime;

            // 可以在控制台查看进度，或者在这里更新 UI 进度条
            if (timer >= requiredTime)
            {
                LoadNextLevel();
            }
        }
    }

    // 注意：2D 环境下使用 OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("开始计时...");
        }
    }

    // 注意：2D 环境下使用 OnTriggerExit2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = false;
            timer = 0f; // 离开则清空进度
            Debug.Log("计时中断！");
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
            Debug.LogWarning("已经是最后一关，无法跳转！");
        }
    }
}