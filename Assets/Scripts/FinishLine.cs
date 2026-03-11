using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine2D : MonoBehaviour
{
    [Header("Level Completion Settings")]
    public float requiredTime = 3.0f; // Required continuous stay time (seconds)
    public string playerTag = "Player"; // Player object tag
    public float alphaChange = 0.3f;

    private float timer = 0f;
    private bool isTouching = false;
    private bool alphaAdd = false;
    private bool alphaSub = false;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        alphaAdd = false;
        alphaSub = false;
    }

    void Update()
    {
        if (isTouching)
        {
            timer += Time.deltaTime;

            // Load next level after required stay time is reached
            if (timer >= requiredTime)
            {
                LoadNextLevel();
            }
        }
    }

    // Triggered while inside a 2D trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("Player entered finish zone. Timer started...");
            if (!alphaAdd)
            {
                ChangeAlpha();
                alphaAdd = true;
                alphaSub = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = true;
            Debug.Log("Player entered finish zone. Timer started...");
            if (!alphaAdd)
            {
                ChangeAlpha();
                alphaAdd = true;
                alphaSub = false;
            }
        }
    }

    // Triggered when exiting a 2D trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isTouching = false;
            timer = 0f; // Reset timer after leaving
            Debug.Log("Player left finish zone. Timer reset.");
            if (!alphaSub)
            {
                ChangeAlpha();
                alphaAdd = false;
                alphaSub = true;
            }
        }
    }

    void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("This is already the last level. Cannot load next level.");
        }
    }

    void ChangeAlpha()
    {
        float change = isTouching ? alphaChange : -alphaChange;
        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, c.a + change);
    }
}