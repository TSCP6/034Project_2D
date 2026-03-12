using UnityEngine;

public class Storm : MonoBehaviour
{
    [Header("Storm Timing")]
    public float breakTime = 3f; // Full black duration
    public float lightningTime = 0.08f; // White flash duration
    public float whiteToBlackTime = 0.08f;
    public float darkenTime = 3f; // Fade from targetAlpha black to full black
    public float flickerStrength = 0.08f; // Subtle flicker while fading from white to black
    public float flickerFrequency = 28f;

    [Range(0f, 1f)]
    public float targetAlpha = 0.7f; // Black alpha right after flash

    private SpriteRenderer sr;
    private float curTime;
    private Color c;

    // Start is called before the first frame update
    void Start()
    {
        curTime = 0f;
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("Storm requires a SpriteRenderer on the same GameObject.");
            enabled = false;
            return;
        }

        sr.color = new Color(0, 0, 0, 1f);
        c = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        float safeLightningTime = Mathf.Max(0.0001f, lightningTime);
        float safeDarkenTime = Mathf.Max(0.0001f, darkenTime);
        float safeDurationTime = Mathf.Max(0.0001f, whiteToBlackTime);
        float totalCycle = breakTime + safeLightningTime + safeDurationTime + safeDarkenTime;

        if (totalCycle <= 0f)
        {
            return;
        }

        float cycleTime = curTime % totalCycle;

        // Phase 1: fully black
        if (cycleTime < breakTime)
        {
            c = new Color(0f, 0f, 0f, 1f);
        }
        // Phase 2: sudden white flash
        else if (cycleTime < breakTime + safeLightningTime)
        {
            c = new Color(1f, 1f, 1f, 1f);
        }
        // Phase 3: smooth white -> black(targetAlpha), with slight decaying flicker
        else if (cycleTime < breakTime + safeLightningTime + safeDurationTime)
        {
            float phaseTime = cycleTime - breakTime - safeLightningTime;
            float t = phaseTime / safeDurationTime;

            // Smoothstep: softer than linear interpolation.
            t = t * t * (3f - 2f * t);

            Color from = new Color(1f, 1f, 1f, 1f);
            Color to = new Color(0f, 0f, 0f, targetAlpha);
            c = Color.Lerp(from, to, t);

            float flickerEnvelope = 1f - t;
            float flicker = Mathf.Sin(curTime * flickerFrequency) * flickerStrength * flickerEnvelope;
            c.r = Mathf.Clamp01(c.r + flicker);
            c.g = Mathf.Clamp01(c.g + flicker);
            c.b = Mathf.Clamp01(c.b + flicker);
        }
        // Phase 4: black returns from targetAlpha to full black
        else
        {
            float t = (cycleTime - breakTime - safeLightningTime - safeDurationTime) / safeDarkenTime;
            t = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(targetAlpha, 1f, t);
            c = new Color(0f, 0f, 0f, alpha);
        }

        sr.color = c;
        curTime += Time.unscaledDeltaTime;
    }
}
