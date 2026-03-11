using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 新增：UI相关命名空间

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

// 新增：分组次数配置（序列化，Inspector可编辑）
[System.Serializable]
public class GroupUsageConfig
{
    public string groupName; // 对应PrefabGroup的名称
    public int maxUses = 5;  // 该分组最大使用次数
    public int remainingUses; // 剩余次数
    public Button uiButton;  // 该分组对应的UI按钮（用于变灰）
}

public class ObjCreator : MonoBehaviour
{
    public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
    public List<GroupSpawnOrder> groupSpawnOrders = new List<GroupSpawnOrder>();

    [Header("预览设置")]
    public Material previewMaterial;
    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);
    public float rotateSpeedDegrees = 180f;
    public Vector2 defaultPos;

    [Header("次数限制设置【新增】")]
    public List<GroupUsageConfig> groupUsageConfigs = new List<GroupUsageConfig>(); // 分组次数配置
    public Color disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 按钮变灰后的颜色

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
        InitGroupUsage(); // 初始化分组次数
        UpdateAllButtonStates(); // 初始化按钮状态
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

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            PlaceFinalObject();
            currentPreviewRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public void SelectPreviewByIndex(int groupIndex)
    {
        // 新增：检查次数，次数为0直接返回
        if (!CanUseGroup(groupIndex))
        {
            Debug.LogWarning($"分组{groupIndex}次数已用完，无法生成预览！");
            return;
        }

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

    // 新增：初始化分组次数
    private void InitGroupUsage()
    {
        foreach (var config in groupUsageConfigs)
        {
            config.remainingUses = config.maxUses; // 剩余次数初始化为最大次数
        }
    }

    // 新增：检查分组是否可使用（次数>0）
    private bool CanUseGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groupUsageConfigs.Count)
        {
            return true; // 未配置次数的分组默认可使用
        }
        var config = groupUsageConfigs[groupIndex];
        return config.remainingUses > 0;
    }

    // 新增：扣减分组次数并更新按钮状态
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
            UpdateButtonState(groupIndex); // 更新对应按钮状态
            Debug.Log($"分组{groupIndex}剩余次数：{config.remainingUses}");
        }
    }

    // 新增：更新单个分组的按钮状态（变灰/禁用）
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

        // 次数为0时：按钮不可交互 + 变灰
        if (config.remainingUses <= 0)
        {
            config.uiButton.interactable = false;
            Image btnImage = config.uiButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = disabledButtonColor;
            }
            // 如果按钮有文字，也可以同步变灰
            Text btnText = config.uiButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = disabledButtonColor;
            }
        }
        else
        {
            // 次数>0时：恢复可交互 + 恢复原颜色
            config.uiButton.interactable = true;
            Image btnImage = config.uiButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = Color.white; // 可改为默认颜色变量
            }
            Text btnText = config.uiButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.color = Color.black; // 可改为默认文字颜色
            }
        }
    }

    // 新增：更新所有分组的按钮状态
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

        // 新增：放置物体后扣减对应分组次数
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

    // 新增：重置所有分组次数和按钮状态（可选，比如重启场景时调用）
    public void ResetAllGroupUsage()
    {
        InitGroupUsage();
        UpdateAllButtonStates();
        Debug.Log("所有分组次数已重置，按钮状态已恢复！");
    }
}