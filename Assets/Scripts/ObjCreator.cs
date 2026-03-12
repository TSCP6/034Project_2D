using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // UI references

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

[System.Serializable]
// Group usage config, editable in Inspector
public class GroupUsageConfig
{
    public string groupName; // Corresponds to PrefabGroup name
    public int maxUses = 5;  // Maximum uses for this group
    public int remainingUses; // Remaining uses
    public Button uiButton;  // UI button for this group
}

public class ObjCreator : MonoBehaviour
{
    public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
    public List<GroupSpawnOrder> groupSpawnOrders = new List<GroupSpawnOrder>();

    [Header("Preview Settings")]
    public Material previewMaterial;
    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);
    public float rotateSpeedDegrees = 180f;
    public Vector2 defaultPos;

    [Header("Group Usage & Button Settings")]
    public List<GroupUsageConfig> groupUsageConfigs = new List<GroupUsageConfig>(); // Group usage configs
    public Color disabledButtonColor = new Color(1f, 1f, 1f, 0.7f); // Disabled button color

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private bool isPreviewActive;
    private bool waitMouseReleaseAfterButton;
    private Camera mainCam;
    private List<int> nextOrderIndices = new List<int>();
    private int pendingGroupIndex = -1;
    private Quaternion currentPreviewRotation = Quaternion.identity;

    void Start()
    {
        mainCam = Camera.main;
        InitOrderIndices();
        InitGroupUsage();
        UpdateAllButtonStates();
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
        previewObject.transform.rotation = currentPreviewRotation;

        if (Input.GetMouseButtonDown(1) && !IsPointerOverUi())
        {
            Destroy(previewObject);
        }

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            PlaceFinalObject();
            currentPreviewRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public void SelectPreviewByIndex(int groupIndex)
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
        waitMouseReleaseAfterButton = true;
    }

    private void InitGroupUsage()
    {
        foreach (var config in groupUsageConfigs)
        {
            config.remainingUses = config.maxUses;
        }
    }

    private bool CanUseGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groupUsageConfigs.Count)
        {
            return true;
        }
        var config = groupUsageConfigs[groupIndex];
        return config.remainingUses > 0;
    }

    private void ConsumeGroupUsage(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groupUsageConfigs.Count)
        {
            return;
        }
        var config = groupUsageConfigs[groupIndex];
        if (config.remainingUses > 0)
        {
            config.remainingUses--;
            UpdateButtonState(groupIndex);
        }
    }

    private void UpdateButtonState(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groupUsageConfigs.Count)
        {
            return;
        }
        var config = groupUsageConfigs[groupIndex];
        if (config.uiButton == null)
        {
            return;
        }

        if (config.remainingUses <= 0)
        {
            config.uiButton.interactable = false;
            Image btnImage = config.uiButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = disabledButtonColor;
            }
            Text btnText = config.uiButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = disabledButtonColor;
            }
        }
        else
        {
            config.uiButton.interactable = true;
            Image btnImage = config.uiButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = Color.white;
            }
            Text btnText = config.uiButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = Color.black;
            }
        }
    }

    private void UpdateAllButtonStates()
    {
        for (int i = 0; i < groupUsageConfigs.Count; i++)
        {
            UpdateButtonState(i);
        }
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

        previewObject = Instantiate(selectedPrefab, startPos, currentPreviewRotation);
        previewObject.name = selectedPrefab.name + "_Preview";

        ConfigurePreviewPhysics(previewObject);
        ApplyPreviewMaterial(previewObject);

        isPreviewActive = true;
        currentPreviewRotation = previewObject.transform.rotation;
    }

    private void ConfigurePreviewPhysics(GameObject obj)
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
        for (int i = 0; i < sprites.Length; i++)
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
            float deltaAngle = rotateInput * rotateSpeedDegrees * Time.unscaledDeltaTime;
            currentPreviewRotation = Quaternion.Euler(0f, 0f, currentPreviewRotation.eulerAngles.z + deltaAngle);
        }
    }

    private void PlaceFinalObject()
    {
        if (selectedPrefab == null || previewObject == null)
        {
            isPreviewActive = false;
            return;
        }

        Vector3 finalPos = previewObject.transform.position;
        Quaternion finalRot = previewObject.transform.rotation;
        Instantiate(selectedPrefab, finalPos, finalRot);
        if (pendingGroupIndex >= 0)
        {
            ConsumeGroupUsage(pendingGroupIndex);
        }

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

    private bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void ResetAllGroupUsage()
    {
        InitGroupUsage();
        UpdateAllButtonStates();
    }
}