using UnityEngine;
using System.Collections.Generic;

public class WindController : MonoBehaviour
{
    [Header("风力设置")]
    public Vector2 windDirection = Vector2.right; // 初始风向
    public float windForce = 5f; // 风力大小
    public float maxSpeed = 6f; // 受风力物体最大速度
    public LayerMask windAffectedLayer; // 受风力影响的层级

    [Header("触发间隔设置")]
    public float triggerInterval = 5f; // 每隔X秒触发一次风力（核心参数）
    public float windDuration = 1f; // 每次触发后风力持续时长（可自定义，比如1秒）

    private float timer; // 全局计时器
    private bool isWindActive; // 当前是否处于风力生效期
    private readonly HashSet<Rigidbody2D> bodiesInWind = new HashSet<Rigidbody2D>();

    void Reset()
    {
        // 重置时自动设置碰撞体为触发器
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        timer = 0f;
        isWindActive = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 逻辑：间隔时间到 → 触发风力 → 持续指定时长后关闭 → 循环
        if (!isWindActive)
        {
            // 间隔时间到，触发风力
            if (timer >= triggerInterval)
            {
                windDirection = -windDirection; // 每次触发前反转风向
                isWindActive = true; // 开启风力
                timer = 0f; // 重置计时器，开始计时风力持续时长
                Debug.Log($"风力触发！当前风向：{windDirection}");
            }
        }
        else
        {
            // 风力持续时长到，关闭风力
            if (timer >= windDuration)
            {
                isWindActive = false; // 关闭风力
                timer = 0f; // 重置计时器，开始计时触发间隔
                Debug.Log("风力关闭，进入等待间隔");
            }
        }
    }

    void FixedUpdate()
    {
        // 仅在风力生效期、且有受影响物体时施加风力
        if (!isWindActive || bodiesInWind.Count == 0) return;

        Vector2 force = windDirection.normalized * windForce;

        foreach (var rb in bodiesInWind)
        {
            if (rb == null) continue; // 防止物体被销毁导致空引用

            rb.WakeUp(); // 唤醒休眠的刚体
            rb.AddForce(force, ForceMode2D.Force); // 施加持续力

            // 限制最大速度，避免物体无限加速
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    #region 触发器检测：管理受风力影响的刚体
    void OnTriggerEnter2D(Collider2D collision)
    {
        AddRigidbodyIfValid(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // 防止运行时层级/刚体组件变化导致漏检
        AddRigidbodyIfValid(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Rigidbody2D>(out var rb))
        {
            bodiesInWind.Remove(rb);
        }
    }

    // 封装校验逻辑：层级匹配 + 有刚体组件
    private void AddRigidbodyIfValid(Collider2D collision)
    {
        // 检查层级是否匹配
        if ((windAffectedLayer.value & (1 << collision.gameObject.layer)) == 0) return;
        // 检查是否有刚体组件
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        bodiesInWind.Add(rb);
    }
    #endregion

    #region 生命周期：清理数据
    void OnDisable()
    {
        bodiesInWind.Clear(); // 组件禁用时清空列表，防止内存泄漏
    }

    void OnDestroy()
    {
        bodiesInWind.Clear(); // 组件销毁时清空列表
    }
    #endregion
}