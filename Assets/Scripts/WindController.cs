using UnityEngine;
using System.Collections.Generic;

public class WindController : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector2 windDirection = Vector2.right; // Initial wind direction
    public float windForce = 5f; // Wind force strength
    public float maxSpeed = 6f; // Max speed for affected objects
    public LayerMask windAffectedLayer; // Layer affected by wind

    [Header("Trigger Interval Settings")]
    public float triggerInterval = 5f; // Wind triggers every X seconds (core parameter)
    public float windDuration = 1f; // Wind duration after each trigger (customizable, e.g. 1s)

    private float timer;
    private bool isWindActive;
    private readonly HashSet<Rigidbody2D> bodiesInWind = new HashSet<Rigidbody2D>();
    public AudioSource audioSource;
    void Reset()
    {
        // Automatically set collider as trigger on reset
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

        // Logic: interval reached ¡ú trigger wind ¡ú wind lasts for set duration ¡ú repeat
        if (!isWindActive)
        {
            // Interval reached, trigger wind
            if (timer >= triggerInterval)
            {
                windDirection = -windDirection; // Reverse wind direction before each trigger
                isWindActive = true; // Activate wind
                timer = 0f; // Reset timer, start wind duration countdown
                audioSource.Play();

                Debug.Log($"Wind triggered! Current direction: {windDirection}");
            }
        }
        else
        {
            // Wind duration reached, deactivate wind
            if (timer >= windDuration)
            {
                isWindActive = false; // Deactivate wind
                timer = 0f; // Reset timer, start interval countdown
                Debug.Log("Wind deactivated, waiting interval");
            }
        }
    }

    void FixedUpdate()
    {
        // Only apply wind force when wind is active and affected objects exist
        if (!isWindActive || bodiesInWind.Count == 0) return;

        Vector2 force = windDirection.normalized * windForce;

        foreach (var rb in bodiesInWind)
        {
            if (rb == null) continue; // Prevent null reference if object destroyed

            rb.WakeUp(); // Wake up sleeping rigidbody
            rb.AddForce(force, ForceMode2D.Force); // Apply continuous force

            // Limit max speed to prevent infinite acceleration
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    #region Trigger Detection: Manage wind-affected rigidbodies
    void OnTriggerEnter2D(Collider2D collision)
    {
        AddRigidbodyIfValid(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Prevent missing detection due to runtime layer/rigidbody changes
        AddRigidbodyIfValid(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Rigidbody2D>(out var rb))
        {
            bodiesInWind.Remove(rb);
        }
    }

    private void AddRigidbodyIfValid(Collider2D collision)
    {
        // Check if layer matches
        if ((windAffectedLayer.value & (1 << collision.gameObject.layer)) == 0) return;
        // Check if has Rigidbody2D component
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        bodiesInWind.Add(rb);
    }
    #endregion

    #region Lifecycle: Clean up data
    void OnDisable()
    {
        bodiesInWind.Clear(); // Clear list when component disabled to prevent memory leak
    }

    void OnDestroy()
    {
        bodiesInWind.Clear(); // Clear list when component destroyed
    }
    #endregion
}