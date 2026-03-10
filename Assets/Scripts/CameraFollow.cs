using UnityEngine;

public class CameraVerticalFollow2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float camSpeed = 20f;       // Movement speed
    public float edgeSize = 30f;      // Edge trigger range (pixels)
    public bool useSmoothing = true;   // Enable smooth camera movement
    public float smoothTime = 0.1f;    // Smooth damping time

    [Header("Top Center Trigger Zone")]
    [Range(0.1f, 1f)]
    public float topCenterWidthPercent = 0.7f; // Width of top-center upward trigger area (screen width percent)

    [Header("Boundary Limits")]
    public float minY = -15f;
    public float maxY = 15f;

    private float _currentVelocityY;   // Internal velocity cache used by SmoothDamp

    void Update()
    {
        float moveDir = 0f;
        float mousePosX = Input.mousePosition.x;
        float mousePosY = Input.mousePosition.y;

        float clampedWidthPercent = Mathf.Clamp01(topCenterWidthPercent);
        float triggerWidth = Screen.width * clampedWidthPercent;
        float leftLimit = (Screen.width - triggerWidth) * 0.5f;
        float rightLimit = (Screen.width + triggerWidth) * 0.5f;
        bool isInTopCenterArea = mousePosX >= leftLimit && mousePosX <= rightLimit;

        // 1. Detect edge trigger
        if (mousePosY >= Screen.height - edgeSize && isInTopCenterArea)
        {
            moveDir = 1f;
        }
        else if (mousePosY <= edgeSize)
        {
            moveDir = -1f;
        }

        // 2. Compute target position
        if (moveDir != 0)
        {
            float targetY = transform.position.y + (moveDir * camSpeed * Time.unscaledDeltaTime);
            targetY = Mathf.Clamp(targetY, minY, maxY);

            if (useSmoothing)
            {
                // Smooth movement
                float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _currentVelocityY, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                // Linear hard movement
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }
        else
        {
            // Reset velocity cache when cursor leaves edge to avoid odd inertia
            _currentVelocityY = 0f;
        }
    }
}
