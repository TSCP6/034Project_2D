using UnityEngine;
using System.Collections.Generic;

public class WindController : MonoBehaviour
{
    public Vector2 windDirection = Vector2.right;
    public float windForce = 5f;
    public float maxSpeed = 6f;
    public LayerMask windAffectedLayer;
    public float exitTime = 5f; // Wind active duration
    public float breakTime = 5f; // Wind pause duration

    private float curTime;
    private bool isWinding = true;
    private readonly HashSet<Rigidbody2D> bodiesInWind = new HashSet<Rigidbody2D>();

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        curTime = 0f;
        isWinding = true;
    }

    void Update()
    {
        curTime += Time.deltaTime;

        if (isWinding)
        {
            if (curTime >= exitTime)
            {
                isWinding = false;
                curTime = 0f;
            }
        }
        else
        {
            windDirection = -windDirection;
            if (curTime >= breakTime)
            {
                isWinding = true;
                curTime = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isWinding || bodiesInWind.Count == 0) return;

        Vector2 force = windDirection.normalized * windForce;

        foreach (var rb in bodiesInWind)
        {
            if (rb == null) continue;

            rb.WakeUp();
            rb.AddForce(force, ForceMode2D.Force);

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((windAffectedLayer.value & (1 << collision.gameObject.layer)) == 0) return;
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        bodiesInWind.Add(rb);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Prevent missing bodies when Layer changes or Rigidbody is added at runtime
        if ((windAffectedLayer.value & (1 << collision.gameObject.layer)) == 0) return;
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        bodiesInWind.Add(rb);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        bodiesInWind.Remove(rb);
    }

    void OnDisable()
    {
        bodiesInWind.Clear();
    }
}
