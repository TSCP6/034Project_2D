using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [Header("音效设置")]
    public AudioClip buttonClickClip;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private AudioSource audioSource;
    // 使用 HashSet 记录已经绑定过音效的按钮，解决 UnityEvent 无法遍历的问题
    private HashSet<Button> processedButtons = new HashSet<Button>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景切换后清空缓存，因为旧按钮引用已失效
        processedButtons.Clear();
        AutoAddSoundToAllButtons();
    }

    /// <summary>
    /// 自动扫描并为场景中所有按钮添加音效
    /// </summary>
    [ContextMenu("Auto Add Sound to All Buttons")]
    public void AutoAddSoundToAllButtons()
    {
        // 建议使用 Resources.FindObjectsOfTypeAll 或 FindObjectsByType (Unity 2022.3+)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        int addedCount = 0;

        foreach (Button btn in allButtons)
        {
            // 排除预制体资源，只处理场景中的物体
            if (btn.gameObject.scene.name == null) continue;

            if (AddSoundToButton(btn))
            {
                addedCount++;
            }
        }
        Debug.Log($"[ButtonSoundManager] 扫描完成: 场景共 {allButtons.Length} 个按钮, 新增绑定 {addedCount} 个");
    }

    /// <summary>
    /// 给指定按钮添加音效
    /// </summary>
    /// <returns>是否添加成功</returns>
    public bool AddSoundToButton(Button button)
    {
        if (button == null || processedButtons.Contains(button)) return false;

        // 检查持久化事件（Inspector里拖进去的）防止重复
        if (IsPersistentListenerAdded(button))
        {
            processedButtons.Add(button);
            return false;
        }

        // 添加监听
        button.onClick.AddListener(PlayButtonClickSound);
        processedButtons.Add(button);
        return true;
    }

    private bool IsPersistentListenerAdded(Button button)
    {
        int count = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == nameof(PlayButtonClickSound))
            {
                return true;
            }
        }
        return false;
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip != null)
        {
            audioSource.PlayOneShot(buttonClickClip, soundVolume);
        }
    }
}