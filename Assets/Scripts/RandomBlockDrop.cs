using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Random block drop manager from above.
/// Attach to any empty GameObject (e.g. DropManager).
/// </summary>
public class RandomBlockDrop : MonoBehaviour
{
    [Header("Drop Source Settings")]
    public ObjCreator objCreator; // Source of prefabGroups and groupSpawnOrders
    public bool useObjCreatorData = true; // Use ObjCreator data instead of local dropPrefabs
    public int sourceGroupIndex = -1; // -1 means use all groups, otherwise use a specific group index
    public bool randomGroupEachSpawn = true; // When using all groups, pick a random group each spawn
    public List<GameObject> dropPrefabs; // Fallback local prefabs when ObjCreator data is not used

    [Header("Drop Settings")]
    public Vector2 dropAreaX = new Vector2(-5f, 5f); // Horizontal drop range (X axis)
    public float dropY = 8f; // Initial Y coordinate for drop (height above)
    public float minDropInterval = 1f; // Minimum drop interval (seconds)
    public float maxDropInterval = 3f; // Maximum drop interval (seconds)
    public float minDropSpeed = 2f; // Minimum initial drop speed
    public float maxDropSpeed = 5f; // Maximum initial drop speed

    [Header("Advanced Settings")]
    public int maxActiveBlocks = 20; // Max number of active blocks in scene (prevent stacking)
    public float destroyY = -5f; // Destroy block if below this Y coordinate
    public bool isDropLoop = true; // Should blocks keep dropping
    public float startDelay = 1f; // Delay before dropping starts after game launch
    public float gravityScale = 0.1f;

    // Internal variables
    private List<GameObject> activeBlocks = new List<GameObject>();
    private readonly List<GameObject> orderedDropPrefabs = new List<GameObject>();
    private readonly List<List<GameObject>> groupedDropPrefabs = new List<List<GameObject>>();
    private readonly List<int> groupedNextIndices = new List<int>();
    private Coroutine dropCoroutine;
    private int nextOrderedPrefabIndex;

    void Start()
    {
        // Build drop source from ObjCreator if enabled.
        if (useObjCreatorData)
        {
            if (!BuildOrderedPrefabsFromObjCreator())
            {
                Debug.LogError("Failed to build drop sequence from ObjCreator. Check prefabGroups/groupSpawnOrders configuration.");
                return;
            }
        }
        else
        {
            // Validate fallback local prefab list.
            if (dropPrefabs == null || dropPrefabs.Count == 0)
            {
                Debug.LogError("Please add at least one prefab to dropPrefabs list!");
                return;
            }
        }

        // Start drop coroutine
        dropCoroutine = StartCoroutine(DropLoopCoroutine());
    }

    private bool BuildOrderedPrefabsFromObjCreator()
    {
        orderedDropPrefabs.Clear();
        groupedDropPrefabs.Clear();
        groupedNextIndices.Clear();
        nextOrderedPrefabIndex = 0;

        if (objCreator == null)
        {
            return false;
        }

        if (objCreator.prefabGroups == null || objCreator.groupSpawnOrders == null)
        {
            return false;
        }

        if (sourceGroupIndex >= 0)
        {
            AppendGroupOrder(sourceGroupIndex);
        }
        else
        {
            int maxGroupCount = Mathf.Min(objCreator.prefabGroups.Count, objCreator.groupSpawnOrders.Count);
            for (int i = 0; i < maxGroupCount; i++)
            {
                AppendGroupOrder(i);
            }
        }

        return orderedDropPrefabs.Count > 0;
    }

    private void AppendGroupOrder(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= objCreator.prefabGroups.Count) return;
        if (groupIndex >= objCreator.groupSpawnOrders.Count) return;

        PrefabGroup group = objCreator.prefabGroups[groupIndex];
        GroupSpawnOrder order = objCreator.groupSpawnOrders[groupIndex];
        if (group == null || order == null || group.prefabs == null || order.prefabOrder == null) return;

        List<GameObject> oneGroupSequence = new List<GameObject>();

        for (int i = 0; i < order.prefabOrder.Count; i++)
        {
            int prefabIndex = order.prefabOrder[i];
            if (prefabIndex < 0 || prefabIndex >= group.prefabs.Count) continue;

            GameObject prefab = group.prefabs[prefabIndex];
            if (prefab != null)
            {
                orderedDropPrefabs.Add(prefab);
                oneGroupSequence.Add(prefab);
            }
        }

        if (sourceGroupIndex < 0 && oneGroupSequence.Count > 0)
        {
            groupedDropPrefabs.Add(oneGroupSequence);
            groupedNextIndices.Add(0);
        }
    }

    private GameObject GetNextPrefab()
    {
        if (useObjCreatorData)
        {
            if (sourceGroupIndex < 0 && randomGroupEachSpawn && groupedDropPrefabs.Count > 0)
            {
                int randomGroup = Random.Range(0, groupedDropPrefabs.Count);
                List<GameObject> groupSequence = groupedDropPrefabs[randomGroup];
                if (groupSequence == null || groupSequence.Count == 0)
                {
                    return null;
                }

                int groupIndex = groupedNextIndices[randomGroup];
                GameObject groupedPrefab = groupSequence[groupIndex];

                groupIndex++;
                if (groupIndex >= groupSequence.Count)
                {
                    groupIndex = 0;
                }
                groupedNextIndices[randomGroup] = groupIndex;

                return groupedPrefab;
            }

            if (orderedDropPrefabs.Count == 0)
            {
                return null;
            }

            GameObject prefab = orderedDropPrefabs[nextOrderedPrefabIndex];
            nextOrderedPrefabIndex++;

            if (nextOrderedPrefabIndex >= orderedDropPrefabs.Count)
            {
                nextOrderedPrefabIndex = 0;
            }

            return prefab;
        }

        int randomPrefabIndex = Random.Range(0, dropPrefabs.Count);
        return dropPrefabs[randomPrefabIndex];
    }

    /// <summary>
    /// Core: Coroutine for dropping blocks in a loop
    /// </summary>
    private IEnumerator DropLoopCoroutine()
    {
        // Start delay
        yield return new WaitForSeconds(startDelay);

        while (isDropLoop)
        {
            // Check max active blocks, wait if exceeded
            if (activeBlocks.Count >= maxActiveBlocks)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Random drop interval
            float randomInterval = Random.Range(minDropInterval, maxDropInterval);
            yield return new WaitForSeconds(randomInterval);

            // Spawn a block
            SpawnOneBlock();
        }
    }

    /// <summary>
    /// Spawn a single dropped block
    /// </summary>
    private void SpawnOneBlock()
    {
        // 1. Select prefab from ordered source (or fallback random list)
        GameObject selectedPrefab = GetNextPrefab();
        if (selectedPrefab == null) return;

        // 2. Random drop position (X in range, Y fixed)
        float randomX = Random.Range(dropAreaX.x, dropAreaX.y);
        Vector3 spawnPos = new Vector3(randomX, dropY, 0f);

        // 3. Instantiate block
        GameObject block = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        block.name = $"{selectedPrefab.name}_Drop_{activeBlocks.Count}";

        // 4. Set block drop speed (random)
        Rigidbody2D rb = block.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomSpeed = Random.Range(minDropSpeed, maxDropSpeed);
            rb.velocity = new Vector2(0f, -randomSpeed); // Drop downwards (negative Y)
            rb.gravityScale = gravityScale;
        }

        // 5. Add to active list, bind destroy check
        activeBlocks.Add(block);
        StartCoroutine(CheckBlockDestroy(block));
    }

    /// <summary>
    /// Check if block needs to be destroyed (out of range/destroyed)
    /// </summary>
    private IEnumerator CheckBlockDestroy(GameObject block)
    {
        while (block != null)
        {
            // Destroy if below destroyY
            if (block.transform.position.y <= destroyY)
            {
                Destroy(block);
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // Remove from active list
        if (activeBlocks.Contains(block))
        {
            activeBlocks.Remove(block);
        }
    }

    /// <summary>
    /// Manually stop dropping (optional, e.g. call on game over)
    /// </summary>
    public void StopDrop()
    {
        isDropLoop = false;
        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
        }
    }

    /// <summary>
    /// Manually clear all dropped blocks (optional, e.g. call on scene restart)
    /// </summary>
    public void ClearAllBlocks()
    {
        foreach (var block in activeBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        activeBlocks.Clear();
    }

    // Clear blocks when scene is destroyed
    void OnDestroy()
    {
        ClearAllBlocks();
    }
}