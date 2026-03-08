using UnityEngine;

public class CameraVerticalFollow2D : MonoBehaviour
{
    [Header("移动设置")]
    public float camSpeed = 20f;       // 移动速度
    public float edgeSize = 30f;      // 边缘触发范围（像素）
    public bool useSmoothing = true;   // 是否开启平滑平移
    public float smoothTime = 0.1f;    // 平滑缓冲时间

    [Header("边界限制")]
    public float minY = -15f;
    public float maxY = 15f;

    private float _currentVelocityY;   // 用于 SmoothDamp 的内部变量

    void Update()
    {
        float moveDir = 0f;
        float mousePosY = Input.mousePosition.y;

        // 1. 检测边缘触发
        if (mousePosY >= Screen.height - edgeSize)
        {
            moveDir = 1f;
        }
        else if (mousePosY <= edgeSize)
        {
            moveDir = -1f;
        }

        // 2. 计算目标位置
        if (moveDir != 0)
        {
            float targetY = transform.position.y + (moveDir * camSpeed * Time.unscaledDeltaTime);
            targetY = Mathf.Clamp(targetY, minY, maxY);

            if (useSmoothing)
            {
                // 平滑移动效果
                float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _currentVelocityY, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                // 线性硬移动
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }
        else
        {
            // 当鼠标离开边缘时，重置速度缓存，防止奇怪的惯性
            _currentVelocityY = 0f;
        }
    }
}
