using UnityEngine;

public class TimeStopController : MonoBehaviour
{
    private bool isStopped = false;

    void Update()
    {
        // Detect Space key press
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
            Time.timeScale = 0.1f;
            // 关键：物理步长也要缩小 10 倍，以保持平滑
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            Debug.Log("Time has been slowed down (Smoothly).");
        }
        else
        {
            Time.timeScale = 1f;
            // 恢复默认物理步长 (Unity 默认值是 0.02)
            Time.fixedDeltaTime = 0.02f;
            Debug.Log("Time resumed.");
        }
    }
}