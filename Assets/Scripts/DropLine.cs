using UnityEngine;
using UnityEngine.SceneManagement;

public class DropLine : MonoBehaviour
{
    // 如果不需要延迟，这两个变量也可以删掉以保持代码整洁
    // public float sleepTime = 1f; 
    public float alphaChange = 0.3f;

    private SpriteRenderer sr;
    private bool hasTriggered = false; // 防止单帧内多次触发

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 确保只触发一次，且碰撞体是玩家
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("物体触碰掉落线，立即重启。");

            // 改变透明度（虽然场景会立即重载，可能看不清颜色变化，但保留逻辑）
            if (sr != null)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a + alphaChange);
            }

            // 直接获取并重载当前场景
            int curIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(curIndex);
        }
    }
}