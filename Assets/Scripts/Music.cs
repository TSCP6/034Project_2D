using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入，用于检测关卡切换

// 单例音频管理器：全局背景音乐播放
public class Music : MonoBehaviour
{
    // 单例实例（全局唯一）
    public static Music Instance { get; private set; }

    [Header("背景音乐设置")]
    public AudioClip bgmClip;          // 背景音乐文件
    public float bgmVolume = 0.5f;     // 背景音乐音量（0-1）
    public bool playOnStart = true;    // 启动游戏时是否自动播放
    public bool loopBgm = true;        // 是否循环播放

    [Header("特殊关卡设置")]
    public string silentSceneName = "Level10"; // 不播放音乐的场景名称

    private AudioSource bgmAudioSource; // 背景音乐音频源

    private void Awake()
    {
        // 单例逻辑
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitAudioSource()
    {
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = loopBgm;
        bgmAudioSource.playOnAwake = false;
    }

    // --- 核心修改部分：监听场景加载 ---

    private void OnEnable()
    {
        // 注册场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 注销场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 判断是否是第10关（或者你指定的静音关卡）
        if (scene.name == silentSceneName)
        {
            StopBGM();
        }
        else
        {
            // 如果不是第10关，且勾选了自动播放，则确保音乐响起
            if (playOnStart)
            {
                PlayBGM();
            }
        }
    }

    // --- 播放控制方法 ---

    public void PlayBGM()
    {
        if (bgmAudioSource != null && bgmClip != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }

    public void PauseBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmAudioSource.volume = bgmVolume;
        }
    }
}