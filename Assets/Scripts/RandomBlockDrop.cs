using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 天上随机掉落方块管理器
/// 挂载到任意空物体（比如DropManager）即可
/// </summary>
public class RandomBlockDrop : MonoBehaviour
{
    [Header("掉落基础设置")]
    public List<GameObject> dropPrefabs; // 要掉落的方块预制体列表（支持多个类型）
    public Vector2 dropAreaX = new Vector2(-5f, 5f); // 掉落的水平范围（X轴）
    public float dropY = 8f; // 掉落的初始Y坐标（天上的高度）
    public float minDropInterval = 1f; // 最小掉落间隔（秒）
    public float maxDropInterval = 3f; // 最大掉落间隔（秒）
    public float minDropSpeed = 2f; // 方块初始下落速度（最小）
    public float maxDropSpeed = 5f; // 方块初始下落速度（最大）

    [Header("进阶设置")]
    public int maxActiveBlocks = 20; // 场景中最大活跃方块数量（防止堆积）
    public float destroyY = -5f; // 方块落到该Y坐标以下自动销毁
    public bool isDropLoop = true; // 是否持续掉落
    public float startDelay = 1f; // 游戏启动后延迟多久开始掉落

    // 内部变量
    private List<GameObject> activeBlocks = new List<GameObject>();
    private Coroutine dropCoroutine;

    void Start()
    {
        // 校验参数
        if (dropPrefabs == null || dropPrefabs.Count == 0)
        {
            Debug.LogError("请至少添加一个掉落的预制体到dropPrefabs列表！");
            return;
        }

        // 启动掉落协程
        dropCoroutine = StartCoroutine(DropLoopCoroutine());
    }

    /// <summary>
    /// 核心：循环掉落方块的协程
    /// </summary>
    private IEnumerator DropLoopCoroutine()
    {
        // 启动延迟
        yield return new WaitForSeconds(startDelay);

        while (isDropLoop)
        {
            // 检查最大活跃数量，超出则等待
            if (activeBlocks.Count >= maxActiveBlocks)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 随机生成掉落间隔
            float randomInterval = Random.Range(minDropInterval, maxDropInterval);
            yield return new WaitForSeconds(randomInterval);

            // 生成一个方块
            SpawnOneBlock();
        }
    }

    /// <summary>
    /// 生成单个掉落方块
    /// </summary>
    private void SpawnOneBlock()
    {
        // 1. 随机选择一个预制体
        int randomPrefabIndex = Random.Range(0, dropPrefabs.Count);
        GameObject selectedPrefab = dropPrefabs[randomPrefabIndex];
        if (selectedPrefab == null) return;

        // 2. 随机生成掉落位置（X轴在指定范围，Y轴固定）
        float randomX = Random.Range(dropAreaX.x, dropAreaX.y);
        Vector3 spawnPos = new Vector3(randomX, dropY, 0f);

        // 3. 实例化方块
        GameObject block = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        block.name = $"{selectedPrefab.name}_Drop_{activeBlocks.Count}";

        // 4. 设置方块下落速度（随机）
        Rigidbody2D rb = block.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomSpeed = Random.Range(minDropSpeed, maxDropSpeed);
            rb.velocity = new Vector2(0f, -randomSpeed); // 向下掉落（Y轴负方向）
        }

        // 5. 加入活跃列表，绑定销毁检测
        activeBlocks.Add(block);
        StartCoroutine(CheckBlockDestroy(block));
    }

    /// <summary>
    /// 检测方块是否需要销毁（超出范围/被销毁）
    /// </summary>
    private IEnumerator CheckBlockDestroy(GameObject block)
    {
        while (block != null)
        {
            // 超出销毁Y坐标 → 销毁
            if (block.transform.position.y <= destroyY)
            {
                Destroy(block);
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // 从活跃列表移除
        if (activeBlocks.Contains(block))
        {
            activeBlocks.Remove(block);
        }
    }

    /// <summary>
    /// 手动停止掉落（可选，比如游戏结束时调用）
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
    /// 手动清空所有掉落的方块（可选，比如重启场景时调用）
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

    // 场景销毁时清空方块
    void OnDestroy()
    {
        ClearAllBlocks();
    }
}