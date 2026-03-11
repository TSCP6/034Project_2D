using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class LineLinkManager : MonoBehaviour
{
    [Header("Usage Settings")]
    public int maxUses = 5;
    private int remainingUses;

    [Header("Rod Settings")]
    public GameObject rodPrefab;
    public LayerMask targetLayer;
    public float maxLength = 10f;
    public float rodWidth = 0.2f;

    [Header("UI References")]
    public Button linkButton;
    public Text usageText;

    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color flashColor = Color.yellow;
    public float flashSpeed = 5f;

    [Header("State")]
    public bool isActive = false;

    [Header("场景启动时自动连接【多物体+自定义连接点】")]
    // 可配置多组连接，每组包含：两个物体 + 各自的自定义连接点
    public List<InitialLinkGroup> initialLinkGroups = new List<InitialLinkGroup>();

    // 【新增类：定义一组初始连接】
    [System.Serializable]
    public class InitialLinkGroup
    {
        public Rigidbody2D bodyA;          // 第一个物体
        public Vector2 anchorA;            // 物体A的自定义连接点（本地坐标）
        public Rigidbody2D bodyB;          // 第二个物体
        public Vector2 anchorB;            // 物体B的自定义连接点（本地坐标）
        public bool enableThisLink = true; // 是否启用这组连接
    }

    private Rigidbody2D firstBody;
    private Vector2 firstWorldPoint;
    private GameObject previewRod;
    private SpriteRenderer previewRenderer;

    private bool isLinking = false;
    private Image buttonImage;
    private Color originalBtnColor;

    void Start()
    {
        remainingUses = maxUses;

        if (linkButton != null)
        {
            buttonImage = linkButton.GetComponent<Image>();
            originalBtnColor = buttonImage.color;
        }

        UpdateUI();

        // 游戏启动时，批量创建所有配置好的初始连接
        CreateAllInitialLinks();
    }

    /// <summary>
    /// 核心：批量创建所有初始连接（支持多组）
    /// </summary>
    private void CreateAllInitialLinks()
    {
        foreach (var linkGroup in initialLinkGroups)
        {
            // 跳过未启用的连接组
            if (!linkGroup.enableThisLink) continue;

            // 校验：两个物体都不能为空
            if (linkGroup.bodyA == null || linkGroup.bodyB == null)
            {
                Debug.LogWarning("初始连接失败：某组连接的bodyA/bodyB为空！");
                continue;
            }

            // 校验：不是同一个物体
            if (linkGroup.bodyA == linkGroup.bodyB)
            {
                Debug.LogWarning("初始连接失败：某组连接的bodyA和bodyB是同一个物体！");
                continue;
            }

            // 本地坐标 → 世界坐标（关键：自定义连接点的转换）
            Vector2 worldPosA = linkGroup.bodyA.transform.TransformPoint(linkGroup.anchorA);
            Vector2 worldPosB = linkGroup.bodyB.transform.TransformPoint(linkGroup.anchorB);

            // 校验：距离不超过最大长度
            float dist = Vector2.Distance(worldPosA, worldPosB);
            if (dist > maxLength)
            {
                Debug.LogWarning($"初始连接失败：{linkGroup.bodyA.name}和{linkGroup.bodyB.name}距离超过maxLength！");
                continue;
            }

            // 创建连接杆
            GameObject rod = Instantiate(rodPrefab);
            UpdateRodTransform(rod.transform, worldPosA, worldPosB, dist);

            // 配置连接杆的刚体
            Rigidbody2D rodRb = rod.GetComponent<Rigidbody2D>();
            rodRb.isKinematic = false;

            // 创建铰链关节（连接A）
            CreateHinge(rod, linkGroup.bodyA, rod.transform.InverseTransformPoint(worldPosA));
            // 创建铰链关节（连接B）
            CreateHinge(rod, linkGroup.bodyB, rod.transform.InverseTransformPoint(worldPosB));

            Debug.Log($"成功创建初始连接：{linkGroup.bodyA.name} ↔ {linkGroup.bodyB.name}");
        }
    }

    // 【保留原有方法：手动连接逻辑】
    public void ToggleLinkMode()
    {
        if (remainingUses <= 0) return;

        isActive = !isActive;
        if (!isActive && isLinking) CancelLinking();
    }

    void Update()
    {
        HandleButtonFlashing();
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, targetLayer);

            if (hit.collider != null && hit.rigidbody != null)
            {
                if (!isLinking)
                    StartLinking(hit.rigidbody, hit.point);
                else if (hit.rigidbody != firstBody)
                    CompleteLink(hit.rigidbody, hit.point);
            }
        }

        if (isLinking) UpdateLinkPreview();
        if (Input.GetMouseButtonDown(1)) CancelLinking();
    }

    void StartLinking(Rigidbody2D rb, Vector2 point)
    {
        firstBody = rb;
        firstWorldPoint = point;
        isLinking = true;

        previewRod = Instantiate(rodPrefab);
        previewRod.GetComponent<Collider2D>().enabled = false;
        previewRod.GetComponent<Rigidbody2D>().isKinematic = true;
        previewRenderer = previewRod.GetComponent<SpriteRenderer>();
    }

    void UpdateLinkPreview()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(firstWorldPoint, mousePos);

        if (dist > maxLength)
        {
            Vector2 dir = (mousePos - firstWorldPoint).normalized;
            mousePos = firstWorldPoint + dir * maxLength;
            dist = maxLength;
        }

        UpdateRodTransform(previewRod.transform, firstWorldPoint, mousePos, dist);
        previewRenderer.color = dist >= maxLength * 0.95f ? warningColor : normalColor;
    }

    void UpdateRodTransform(Transform trans, Vector2 start, Vector2 end, float length)
    {
        trans.position = (start + end) / 2f;
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.Euler(0, 0, angle);
        trans.localScale = new Vector3(length, rodWidth, 1f);
    }

    void CompleteLink(Rigidbody2D secondBody, Vector2 secondWorldPoint)
    {
        float dist = Vector2.Distance(firstWorldPoint, secondWorldPoint);
        if (dist > maxLength) return;

        GameObject finalRod = Instantiate(rodPrefab);
        UpdateRodTransform(finalRod.transform, firstWorldPoint, secondWorldPoint, dist);

        Rigidbody2D rodRb = finalRod.GetComponent<Rigidbody2D>();
        rodRb.isKinematic = false;

        CreateHinge(finalRod, firstBody, finalRod.transform.InverseTransformPoint(firstWorldPoint));
        CreateHinge(finalRod, secondBody, finalRod.transform.InverseTransformPoint(secondWorldPoint));

        remainingUses--;
        Destroy(previewRod);
        FinishAction();
    }

    void CreateHinge(GameObject rod, Rigidbody2D connectedTarget, Vector2 anchorOnRod)
    {
        HingeJoint2D hinge = rod.AddComponent<HingeJoint2D>();
        hinge.connectedBody = connectedTarget;
        hinge.anchor = anchorOnRod;
        hinge.connectedAnchor = connectedTarget.transform.InverseTransformPoint(
            rod.transform.TransformPoint(anchorOnRod));
    }

    void CancelLinking()
    {
        if (previewRod != null) Destroy(previewRod);
        FinishAction();
    }

    void FinishAction()
    {
        isActive = false;
        isLinking = false;
        firstBody = null;
        if (buttonImage != null) buttonImage.color = originalBtnColor;
        UpdateUI();
    }

    void HandleButtonFlashing()
    {
        if (buttonImage == null || remainingUses <= 0) return;
        if (isActive)
        {
            float lerp = Mathf.PingPong(Time.unscaledTime * flashSpeed, 1f);
            buttonImage.color = Color.Lerp(originalBtnColor, flashColor, lerp);
        }
        else
        {
            buttonImage.color = originalBtnColor;
        }
    }

    void UpdateUI()
    {
        if (usageText != null) usageText.text = "Uses: " + remainingUses;
        if (linkButton != null) linkButton.interactable = remainingUses > 0;
    }
}