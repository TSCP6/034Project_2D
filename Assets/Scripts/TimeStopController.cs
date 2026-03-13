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
            Time.timeScale = 0.0f; // Time stopped
            Debug.Log("Time has been stopped.");
        }
        else
        {
            Time.timeScale = 1f; // Time resumed
            Debug.Log("Time resumed.");
        }
    }
}