using UnityEngine;

public class GroundMove : MonoBehaviour
{
    [Header("Vertical Move Settings")]
    public float amplitude = 1f; // Max distance from start position
    public float period = 2f; // Seconds per full cycle
    [Range(0f, 1f)]
    public float phaseOffset = 0f; // 0-1 offset in cycle at start
    public bool useLocalPosition = true;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        if (period <= 0.0001f)
        {
            return;
        }

        float omega = (2f * Mathf.PI) / period;
        float t = Time.time + phaseOffset * period;
        float yOffset = Mathf.Sin(omega * t) * amplitude;

        Vector3 target = startPosition + Vector3.up * yOffset;
        if (useLocalPosition)
        {
            transform.localPosition = target;
        }
        else
        {
            transform.position = target;
        }
    }

    void OnValidate()
    {
        if (period < 0f)
        {
            period = 0f;
        }

        if (amplitude < 0f)
        {
            amplitude = 0f;
        }
    }
}
