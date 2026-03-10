using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DropLine : MonoBehaviour
{
    public float sleepTime = 1f; //interval time when obj touches the drop line
    public float alphaChange = 0.3f; //alpha change when obj touched dropline

    private float curTime;
    private bool touched;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        curTime = 0f;
        touched = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (touched)
        {
            curTime += Time.deltaTime;
            int curIndex = SceneManager.GetActiveScene().buildIndex;
            if (curTime >= sleepTime)
                SceneManager.LoadScene(curIndex); //reload cur scene
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (touched) return;

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Obj touches drop line, restart.");
            touched = true;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a + alphaChange);
        }
    }
}
