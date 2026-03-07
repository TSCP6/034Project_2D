using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class PrefabGroup
{
    public string groupName;
    public List<GameObject> prefabs = new List<GameObject>();
}

[System.Serializable]
public class GroupSpawnOrder
{
    public string groupName;
    public List<int> prefabOrder = new List<int>();
}

public class ObjCreator : MonoBehaviour
{
    public List<PrefabGroup> prefabGroups = new List<PrefabGroup>(); // 三类预制体分组
    public List<GroupSpawnOrder> groupSpawnOrders = new List<GroupSpawnOrder>(); // 每组组内生成顺序

    public Material previewMaterial;
    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);
    public float rotateSpeedDegrees = 180f;

    public Vector2 defaultPos; //初始生成位置

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private bool isPreviewActive;
    private bool waitMouseReleaseAfterButton; //用于确认是否是二次点击防止物体
    private Camera mainCam;
    private List<int> nextOrderIndices = new List<int>();
    private int pendingGroupIndex = -1;

    void Start()
    {
        mainCam = Camera.main;
        InitOrderIndices();
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
        HandlePreviewRotation();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            PlaceFinalObject();
        }
    }

    public void SelectPreviewByIndex(int groupIndex) //根据组索引按该组顺序选择预制体
    {
        if (groupIndex < 0 || groupIndex >= prefabGroups.Count)
        {
            Debug.LogWarning($"Prefab group index out of range: {groupIndex}");
            return;
        }

        PrefabGroup group = prefabGroups[groupIndex];
        if (group == null || group.prefabs == null || group.prefabs.Count == 0)
        {
            Debug.LogWarning($"Prefab group {groupIndex} is empty.");
            return;
        }

        EnsureOrderIndicesSize();

        if (groupIndex >= groupSpawnOrders.Count || groupSpawnOrders[groupIndex] == null)
        {
            Debug.LogWarning($"Spawn order for group {groupIndex} is missing.");
            return;
        }

        GroupSpawnOrder orderConfig = groupSpawnOrders[groupIndex];
        if (orderConfig.prefabOrder == null || orderConfig.prefabOrder.Count == 0)
        {
            Debug.LogWarning($"Spawn order for group {groupIndex} is empty.");
            return;
        }

        int orderIndex = nextOrderIndices[groupIndex];
        if (orderIndex >= orderConfig.prefabOrder.Count)
        {
            Debug.Log($"Group {groupIndex} sequence is exhausted and cannot generate more.");
            return;
        }

        int prefabIndex = orderConfig.prefabOrder[orderIndex];
        if (prefabIndex < 0 || prefabIndex >= group.prefabs.Count)
        {
            Debug.LogWarning($"Prefab index {prefabIndex} is out of range in group {groupIndex}.");
            return;
        }

        if (group.prefabs[prefabIndex] == null)
        {
            Debug.LogWarning($"Prefab group {groupIndex} index {prefabIndex} is null.");
            return;
        }

        selectedPrefab = group.prefabs[prefabIndex];
        pendingGroupIndex = groupIndex;
        CreatePreviewObject();
        waitMouseReleaseAfterButton = true; //已生成预览物体，且为第一次点击
    }

    private void InitOrderIndices()
    {
        nextOrderIndices.Clear();
        for (int i = 0; i < prefabGroups.Count; i++)
        {
            nextOrderIndices.Add(0);
        }
    }

    private void EnsureOrderIndicesSize()
    {
        while (nextOrderIndices.Count < prefabGroups.Count)
        {
            nextOrderIndices.Add(0);
        }
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

    private void HandlePreviewRotation()
    {
        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateInput += 1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotateInput -= 1f;
        }

        if (rotateInput != 0f)
        {
            float deltaAngle = rotateInput * rotateSpeedDegrees * Time.deltaTime;
            previewObject.transform.Rotate(0f, 0f, deltaAngle);
        }
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

        if (pendingGroupIndex >= 0 && pendingGroupIndex < nextOrderIndices.Count)
        {
            nextOrderIndices[pendingGroupIndex]++;
        }
        pendingGroupIndex = -1;

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
        // return EventSystem.current != null  && EventSystem.current.IsPointerOverGameObject();
        return false;
    }
}
