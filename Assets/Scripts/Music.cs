using UnityEngine;

// 单例音频管理器：全局背景音乐播放
public class Music : MonoBehaviour
{
    // 单例实例（全局唯一）
    public static Music Instance { get; private set; }

    [Header("背景音乐设置")]
    public AudioClip bgmClip;          // 背景音乐文件
    public float bgmVolume = 0.5f;     // 背景音乐音量（0-1）
    public bool playOnStart = true;    // 启动游戏时自动播放
    public bool loopBgm = true;        // 是否循环播放

    private AudioSource bgmAudioSource; // 背景音乐音频源

    // 初始化单例 + 音频源
    private void Awake()
    {
        // 单例逻辑：确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            // 标记为跨场景不销毁
            DontDestroyOnLoad(gameObject);

            // 初始化音频源
            InitAudioSource();
        }
        else
        {
            // 如果已有实例，销毁重复的物体
            Destroy(gameObject);
        }
    }

    // 初始化音频源组件
    private void InitAudioSource()
    {
        // 添加音频源组件（如果没有）
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 配置音频源参数
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = loopBgm;
        bgmAudioSource.playOnAwake = false; // 手动控制播放，避免自动重复
    }

    // 游戏启动时自动播放（可选）
    private void Start()
    {
        if (playOnStart && bgmClip != null)
        {
            PlayBGM();
        }
    }

    // 播放背景音乐（外部可调用，比如手动触发播放）
    public void PlayBGM()
    {
        if (bgmAudioSource != null && bgmClip != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }

    // 暂停背景音乐
    public void PauseBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    // 停止背景音乐
    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    // 调整背景音乐音量（外部可调用，比如音量滑块）
    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
        {
            bgmVolume = Mathf.Clamp01(volume); // 限制音量在0-1之间
            bgmAudioSource.volume = bgmVolume;
        }
    }
}