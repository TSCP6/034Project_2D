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
    }
}