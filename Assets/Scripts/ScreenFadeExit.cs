using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFadeExit : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2f;
    public bool autoStartOnLoad = true; // 是否一进关卡就自动触发

    void Start()
    {
        // 只有勾选了自动触发，且 Image 已经分配时才开始
        if (autoStartOnLoad && fadeImage != null)
        {
            StartFadeEffect();
        }
    }

    [ContextMenu("Start Fade and Exit")]
    public void StartFadeEffect()
    {
        // 开启协程前，确保 Image 是激活的
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeToWhiteAndExit());
    }

    private IEnumerator FadeToWhiteAndExit()
    {
        float timer = 0f;
        Color startColor = new Color(1, 1, 1, 0);
        Color endColor = new Color(1, 1, 1, 1);

        // 逐渐变白
        while (timer < fadeDuration)
        {
            // 使用 unscaledDeltaTime，防止 TimeScale 为 0 导致卡死
            timer += Time.unscaledDeltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        fadeImage.color = endColor;

        // 使用不受时间缩放影响的等待
        yield return new WaitForSecondsRealtime(0.5f);

        Debug.Log("Game Exiting...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}