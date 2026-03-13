using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Storm : MonoBehaviour
{
    [Header("同步对齐")]
    [Tooltip("音频开头空白的时长（秒）。增加此值会让闪电延后，减少此值会让闪电提前。")]
    public float audioLeadTime = 0.3f;

    [Header("闪电频率")]
    public float minBreakTime = 2f;
    public float maxBreakTime = 5f;

    [Header("视觉表现")]
    public float lightningTime = 0.08f;      // 纯白持续时间
    public float whiteToBlackTime = 0.15f;   // 闪烁渐隐到目标透明度的时间
    public float darkenTime = 2.5f;          // 最终完全回黑的时间
    [Range(0f, 1f)]
    public float targetAlpha = 0.6f;         // 渐隐后的中间层透明度

    [Header("细节抖动")]
    public float flickerStrength = 0.1f;
    public float flickerFrequency = 30f;

    [Header("音频引用")]
    public AudioSource thunderSource;
    public AudioClip thunderClip;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // 容错处理
        if (sr == null)
        {
            Debug.LogError("物体上缺少 SpriteRenderer 组件！");
            return;
        }

        // 初始状态：全黑
        sr.color = Color.black;

        // 开启天气循环
        StartCoroutine(StormLoop());
    }

    IEnumerator StormLoop()
    {
        // --- 启动后的第一次等待 ---
        // 为了避免刚进游戏等太久，这里只给一个极短的固定延迟
        yield return new WaitForSecondsRealtime(0.5f);

        while (true)
        {
            // --- 1. 触发音频 ---
            if (thunderSource != null && thunderClip != null)
            {
                thunderSource.PlayOneShot(thunderClip);
            }

            // --- 2. 对齐音频空白期 ---
            // 这里等待 audioLeadTime 秒，直到雷声真正的爆发点
            yield return new WaitForSecondsRealtime(audioLeadTime);

            // --- 3. 闪电爆发 (瞬间变白) ---
            sr.color = Color.white;
            yield return new WaitForSecondsRealtime(lightningTime);

            // --- 4. 余辉与快速闪烁阶段 ---
            float elapsed = 0f;
            while (elapsed < whiteToBlackTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / whiteToBlackTime);
                float smoothT = t * t * (3f - 2f * t); // 平滑插值

                // 从白色渐变到半透明黑色
                Color baseColor = Color.Lerp(Color.white, new Color(0, 0, 0, targetAlpha), smoothT);

                // 模拟电火花高频闪烁，随时间推移强度减弱
                float flicker = Mathf.Sin(Time.unscaledTime * flickerFrequency) * flickerStrength * (1f - t);
                sr.color = new Color(baseColor.r + flicker, baseColor.g + flicker, baseColor.b + flicker, baseColor.a);

                yield return null;
            }

            // --- 5. 缓慢恢复全黑阶段 ---
            elapsed = 0f;
            while (elapsed < darkenTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / darkenTime);
                float smoothT = t * t * (3f - 2f * t);

                // alpha 从 targetAlpha 回到 1 (全黑)
                sr.color = new Color(0, 0, 0, Mathf.Lerp(targetAlpha, 1f, smoothT));
                yield return null;
            }

            // --- 6. 周期性随机等待 ---
            // 确保回到纯黑状态后，再进行下一轮的随机等待
            sr.color = Color.black;
            float nextWait = Random.Range(minBreakTime, maxBreakTime);
            yield return new WaitForSecondsRealtime(nextWait);
        }
    }

    // 可选：方便你在 Inspector 面板调试时手动触发测试
    [ContextMenu("Test Flash Now")]
    public void TestFlash()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(StormLoop());
        }
    }
}