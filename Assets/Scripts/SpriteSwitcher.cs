using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class SpriteLevel //sprite class for eash level
    {
        public Sprite sprite;
        public int level;
    }

    public SpriteLevel[] sprites;
    public Sprite sprite;

    public bool keepWorldSize = true;      // 切换精灵后保持当前世界尺寸
    public bool syncColliderShape = true;  // 切换精灵后同步常见 2D 碰撞体

    // Start is called before the first frame update
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        int curIndex = SceneManager.GetActiveScene().buildIndex;

        Sprite targetSprite = null;

        foreach (var s in sprites)
        {
            if (s.level == curIndex)
            {
                targetSprite = s.sprite;
                break;
            }
        }

        if (targetSprite == null)
        {
            targetSprite = sprite;
        }

        if (targetSprite == null)
        {
            return;
        }

        Vector2 oldWorldSize = sr.bounds.size;
        sr.sprite = targetSprite;

        if (keepWorldSize)
        {
            KeepRendererWorldSize(sr, oldWorldSize);
        }

        if (syncColliderShape)
        {
            SyncColliderToSprite(sr.sprite);
        }
    }

    private void KeepRendererWorldSize(SpriteRenderer sr, Vector2 oldWorldSize)
    {
        if (oldWorldSize.x <= 0f || oldWorldSize.y <= 0f)
        {
            return;
        }

        Vector2 newWorldSize = sr.bounds.size;
        if (newWorldSize.x <= 0f || newWorldSize.y <= 0f)
        {
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x *= oldWorldSize.x / newWorldSize.x;
        scale.y *= oldWorldSize.y / newWorldSize.y;
        transform.localScale = scale;
    }

    private void SyncColliderToSprite(Sprite currentSprite)
    {
        if (currentSprite == null)
        {
            return;
        }

        Vector2 spriteSize = currentSprite.bounds.size;
        Vector2 spriteCenter = currentSprite.bounds.center;

        if (TryGetComponent<BoxCollider2D>(out var box))
        {
            box.size = spriteSize;
            box.offset = spriteCenter;
        }

        if (TryGetComponent<CircleCollider2D>(out var circle))
        {
            circle.radius = Mathf.Max(spriteSize.x, spriteSize.y) * 0.5f;
            circle.offset = spriteCenter;
        }

        if (TryGetComponent<CapsuleCollider2D>(out var capsule))
        {
            capsule.size = spriteSize;
            capsule.offset = spriteCenter;
        }

        //三角形使用sprite mask处理
    }
}
