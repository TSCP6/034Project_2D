using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Random block drop manager from above.
/// Attach to any empty GameObject (e.g. DropManager).
/// </summary>
public class RandomBlockDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    public List<GameObject> dropPrefabs; // List of block prefabs to drop (supports multiple types)
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
    public float gravityScale = 0.5f;

    // Internal variables
    private List<GameObject> activeBlocks = new List<GameObject>();
    private Coroutine dropCoroutine;

    void Start()
    {
        // Validate parameters
        if (dropPrefabs == null || dropPrefabs.Count == 0)
        {
            Debug.LogError("Please add at least one prefab to dropPrefabs list!");
            return;
        }

        // Start drop coroutine
        dropCoroutine = StartCoroutine(DropLoopCoroutine());
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
        // 1. Randomly select a prefab
        int randomPrefabIndex = Random.Range(0, dropPrefabs.Count);
        GameObject selectedPrefab = dropPrefabs[randomPrefabIndex];
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