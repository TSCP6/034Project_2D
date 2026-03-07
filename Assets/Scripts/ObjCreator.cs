using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjCreator : MonoBehaviour
{
    public List<GameObject> prefabList = new List<GameObject>(); //预制体列表

    public Material previewMaterial;
    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);

    public Vector2 defaultPos; //初始生成位置

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private bool isPreviewActive;
    private bool waitMouseReleaseAfterButton; //用于确认是否是二次点击防止物体
    private Camera mainCam;
    private ObjBase objBaseController;

    void Start()
    {
        mainCam = Camera.main;
        objBaseController = FindObjectOfType<ObjBase>();
    }

    void Update()
    {
        if (!isPreviewActive || previewObject == null)
        {
            return;
        }

        if (waitMouseReleaseAfterButton)
        {
            if (!Input.GetMouseButton(0))
            {
                waitMouseReleaseAfterButton = false;
            }
            return;
        }

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            PlaceFinalObject();
        }
    }

    public void SelectPreviewByIndex(int prefabIndex) //根据索引来选择预制体
    {
        if (prefabIndex < 0 || prefabIndex >= prefabList.Count)
        {
            Debug.LogWarning($"Prefab index out of range: {prefabIndex}");
            return;
        }

        if (prefabList[prefabIndex] == null)
        {
            Debug.LogWarning($"Prefab at index {prefabIndex} is null.");
            return;
        }

        selectedPrefab = prefabList[prefabIndex];
        CreatePreviewObject();
        waitMouseReleaseAfterButton = true; //已生成预览物体，且为第一次点击
    }

    private void CreatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        if (selectedPrefab == null)
        {
            isPreviewActive = false;
            return;
        }

        Vector3 startPos = GetMouseWorldPos();
        if (mainCam == null)
        {
            startPos = defaultPos;
        }

        previewObject = Instantiate(selectedPrefab, startPos, Quaternion.identity);
        previewObject.name = selectedPrefab.name + "_Preview";

        ConfigurePreviewPhysics(previewObject);
        ApplyPreviewMaterial(previewObject);

        isPreviewActive = true;
    }

    private void ConfigurePreviewPhysics(GameObject obj) //配置预览预制体刚体属性
    { 
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody2D[] rigidbodies = obj.GetComponentsInChildren<Rigidbody2D>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].velocity = Vector2.zero;
            rigidbodies[i].angularVelocity = 0f;
            rigidbodies[i].gravityScale = 0f;
            rigidbodies[i].isKinematic = true;
        }
    }

    private void ApplyPreviewMaterial(GameObject obj)
    {
        SpriteRenderer[] sprites = obj.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sprites.Length; i++) //设置为预览材质
        {
            Color c = sprites[i].color;
            c.r = previewColor.r;
            c.g = previewColor.g;
            c.b = previewColor.b;
            c.a = previewColor.a;
            sprites[i].color = c;
        }

        if (previewMaterial != null)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = previewMaterial;
                }
                renderers[i].materials = mats;
            }
        }
    }

    private void UpdatePreviewPosition()
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        if (mainCam == null)
        {
            return;
        }

        previewObject.transform.position = mouseWorld;
    }

    private void PlaceFinalObject() //放置物体
    {
        if (selectedPrefab == null || previewObject == null)
        {
            isPreviewActive = false;
            return;
        }

        Vector3 finalPos = previewObject.transform.position;
        Quaternion finalRot = previewObject.transform.rotation;
        Instantiate(selectedPrefab, finalPos, finalRot);

        Destroy(previewObject);
        previewObject = null;
        selectedPrefab = null;
        isPreviewActive = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        if (mainCam == null)
        {
            return defaultPos;
        }

        Vector3 p = Input.mousePosition;
        p.z = -mainCam.transform.position.z;
        Vector3 world = mainCam.ScreenToWorldPoint(p);
        world.z = 0f;
        return world;
    }

    private bool IsPointerOverUi() //检测鼠标是否在UI上，物体不能放在UI的位置
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
