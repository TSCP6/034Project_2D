using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjCreator : MonoBehaviour
{
    public GameObject prefab;

    public Material previewMaterial;

    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);

    public Vector2 defaultPos; // 点击图形后出现预制体的初始位置（UI位置）

    private GameObject previewObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePreviewObject()
    {
        //提前删除已有的预览预制体，防止与下一个冲突
        if(previewObject != null)
        {
            Destroy(previewObject);
        }

        if(prefab == null) return; //没有预制体的情况下不考虑生成

        Vector2 position = defaultPos;

        previewObject = Instantiate(prefab, position, Quaternion.identity);
        previewObject.name = "PreviewObject";

        if(previewObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.gravityScale = 0;
            rb.isKinematic = true;
        }
    }
}
