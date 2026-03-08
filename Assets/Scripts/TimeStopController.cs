using UnityEngine;

public class TimeStopController : MonoBehaviour
{
    private bool isStopped = false;

    void Update()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTimeStop();
        }
    }

    void ToggleTimeStop()
    {
        isStopped = !isStopped;

        if (isStopped)
        {
            Time.timeScale = 0f; // 时间停止
            Debug.Log("时间已停止");
        }
        else
        {
            Time.timeScale = 1f; // 时间恢复正常
            Debug.Log("时间恢复正常");
        }

        // 可选：调整固定步长，确保物理模拟在恢复后保持平滑
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}