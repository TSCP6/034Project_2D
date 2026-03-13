using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
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

        sr.sprite = targetSprite;
    }
}