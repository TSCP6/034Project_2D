using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine2D : MonoBehaviour
{
    [Header("通关设置")]
    public float requiredTime = 3.0f; // 需要持续停留的时间（秒）
    public string playerTag = "Player"; // 玩家物体的标签

    private float timer = 0f;
    private bool isTouching = false;

    void Update()
    {
        if (isTouching)
        {
            timer += Time.deltaTime;

            // 停留时间达到要求后切换下一关（你也可以在这里先播放 UI 倒计时）
            if (timer >= requiredTime)
            {
                LoadNextLevel();
            }
        }
    }

    // 进入 2D 触发器时触发 OnTriggerEnter2D
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("玩家已进入终点区域，开始计时...");
        }
    }

    // 离开 2D 触发器时触发 OnTriggerExit2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = false;
            timer = 0f; // 离开后重置计时
            Debug.Log("玩家离开终点区域，计时重置");
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
            Debug.LogWarning("当前已是最后一关，无法继续加载下一关");
        }
    }
}