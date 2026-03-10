using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineLinkManager1 : MonoBehaviour
{
    [Header("次数限制")]
    public int maxUses = 5;
    private int remainingUses;

    [Header("配置")]
    public GameObject rodPrefab;    // 连杆预制体 (需包含 SpriteRenderer, BoxCollider2D, Rigidbody2D)
    public LayerMask targetLayer;
    public float maxLength = 10f;
    public float rodWidth = 0.2f;   // 杆子的宽度/厚度

    [Header("UI 引用")]
    public Button linkButton;
    public Text usageText;

    [Header("颜色反馈")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color flashColor = Color.yellow;
    public float flashSpeed = 5f;

    [Header("当前状态")]
    public bool isActive = false;

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
    }

    public void ToggleLinkMode()
    {
        if (remainingUses <= 0) return;

        isActive = !isActive;
        if (!isActive && isLinking) CancelLinking();

        Debug.Log(isActive ? "功能激活" : "功能关闭");
    }

    void Update()
    {
        HandleButtonFlashing();

        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, targetLayer);

            if (hit.collider != null && hit.rigidbody != null)
            {
                if (!isLinking)
                {
                    StartLinking(hit.rigidbody, hit.point);
                }
                else if (hit.rigidbody != firstBody)
                {
                    CompleteLink(hit.rigidbody, hit.point);
                }
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
        previewRod.GetComponent<Collider2D>().enabled = false; // 预览时关闭碰撞
        previewRod.GetComponent<Rigidbody2D>().isKinematic = true; // 预览时不受物理影响
        previewRenderer = previewRod.GetComponent<SpriteRenderer>();
    }

    void UpdateLinkPreview()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(firstWorldPoint, mousePos);

        // 长度限制
        if (dist > maxLength)
        {
            Vector2 dir = (mousePos - firstWorldPoint).normalized;
            mousePos = firstWorldPoint + dir * maxLength;
            dist = maxLength;
        }

        UpdateRodTransform(previewRod.transform, firstWorldPoint, mousePos, dist);

        // 变色反馈
        previewRenderer.color = (dist >= maxLength * 0.95f) ? warningColor : normalColor;
    }

    void UpdateRodTransform(Transform trans, Vector2 start, Vector2 end, float length)
    {
        // 定位在两点中心
        trans.position = (start + end) / 2f;
        // 计算旋转
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.Euler(0, 0, angle);
        // 设置缩放 (假设预制体原始长度为1个单位)
        trans.localScale = new Vector3(length, rodWidth, 1f);
    }

    void CompleteLink(Rigidbody2D secondBody, Vector2 secondWorldPoint)
    {
        float dist = Vector2.Distance(firstWorldPoint, secondWorldPoint);
        if (dist > maxLength) return;

        // 1. 生成最终的硬质连杆
        GameObject finalRod = Instantiate(rodPrefab);
        UpdateRodTransform(finalRod.transform, firstWorldPoint, secondWorldPoint, dist);

        Rigidbody2D rodRb = finalRod.GetComponent<Rigidbody2D>();
        rodRb.isKinematic = false;

        // 2. 创建铰链连接 (连接第一个物体和杆件端点)
        CreateHinge(finalRod, firstBody, finalRod.transform.InverseTransformPoint(firstWorldPoint));

        // 3. 创建铰链连接 (连接第二个物体和杆件另一端)
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
        // 将连接目标的锚点转换为本地坐标
        hinge.connectedAnchor = connectedTarget.transform.InverseTransformPoint(rod.transform.TransformPoint(anchorOnRod));
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
            float lerp = Mathf.PingPong(Time.unscaledTime * flashSpeed, 1.0f);
            buttonImage.color = Color.Lerp(originalBtnColor, flashColor, lerp);
        }
        else
        {
            buttonImage.color = originalBtnColor;
        }
    }

    void UpdateUI()
    {
        if (usageText != null) usageText.text = "剩余: " + remainingUses;
        if (linkButton != null) linkButton.interactable = (remainingUses > 0);
    }
}