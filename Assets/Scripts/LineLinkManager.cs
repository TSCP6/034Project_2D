using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineLinkManager : MonoBehaviour
{
    [Header("次数限制 (n)")]
    public int maxUses = 5;         // 本局总次数
    private int remainingUses;      // 剩余次数

    [Header("配置")]
    public GameObject linePrefab;    // 连线预制体 (带 LineRenderer 和 Rigidbody2D)
    public LayerMask targetLayer;    // 目标物体的 Layer
    public float maxLength = 5f;     // 线段最大长度

    [Header("UI 引用")]
    public Button linkButton;        // 关联的 UI 按钮
    public Text usageText;           // (可选) 用于显示次数的文本

    [Header("颜色反馈")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color flashColor = Color.yellow; // 激活时按钮闪烁的颜色

    [Header("动画设置")]
    public float flashSpeed = 5f;    // 按钮闪烁速度

    [Header("当前状态")]
    public bool isActive = false;

    private Rigidbody2D firstBody;
    private GameObject currentLineObj;
    private LineRenderer currentLineRenderer;
    private bool isLinking = false;
    private bool isOverLength = false;

    private Image buttonImage;
    private Color originalBtnColor;
    private Vector2 firstLocalOffset;
    private Vector2 secondLocalOffset;
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

    // --- 给 UI 按钮调用的公共方法 ---
    // --- 给 UI 按钮调用的公共方法 ---
    public void EnableLinkOnce()
    {
        // 1. 如果没有使用次数了，直接返回
        if (remainingUses <= 0) return;

        // 2. 如果当前已经是激活状态 (isActive == true)
        if (isActive)
        {
            // 如果正在连线预览中（已经点了第一下），调用取消逻辑
            if (isLinking)
            {
                CancelLinking();
                Debug.Log("连线预览已取消。");
            }
            else
            {
                // 如果只是开启了功能还没点第一下，直接重置状态
                FinishAction();
                Debug.Log("功能已关闭。");
            }
        }
        else
        {
            // 3. 如果当前是关闭状态，则开启功能
            isActive = true;
            Debug.Log($"功能激活。剩余次数: {remainingUses}");
        }
    }
    void Update()
    {
        // 处理按钮闪烁视觉效果
        HandleButtonFlashing();

        if (!isActive) return;

        // 1. 处理鼠标点击逻辑
        if (Input.GetMouseButtonDown(0))
        {
            // UI 穿透检测，防止点按钮时直接在后面连线
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, targetLayer);

            if (hit != null && hit.attachedRigidbody != null)
            {
                if (!isLinking)
                {
                    StartLinking(hit.attachedRigidbody);
                }
                else if (hit.attachedRigidbody != firstBody && !isOverLength)
                {
                    CompleteLink(hit.attachedRigidbody);
                }
            }
        }

        // 2. 连线预览更新
        if (isLinking && currentLineRenderer != null)
        {
            UpdateLinkPreview();
        }

        // 3. 右键取消 (不扣次数)
        if (Input.GetMouseButtonDown(1) && isLinking)
        {
            CancelLinking();
        }
    }

    void HandleButtonFlashing()
    {
        if (buttonImage == null || remainingUses <= 0) return;

        if (isActive)
        {
            // 使用 unscaledTime 确保在 Time.timeScale 为 0 时逻辑依然运行
            float lerp = Mathf.PingPong(Time.unscaledTime * flashSpeed, 1.0f);
            buttonImage.color = Color.Lerp(originalBtnColor, flashColor, lerp);
        }
        else if (buttonImage.color != originalBtnColor)
        {
            buttonImage.color = originalBtnColor;
        }
    }

    void StartLinking(Rigidbody2D rb)
    {
        firstBody = rb;
        isLinking = true;
        currentLineObj = Instantiate(linePrefab);
        currentLineRenderer = currentLineObj.GetComponent<LineRenderer>();
        currentLineRenderer.positionCount = 2;
    }

    void UpdateLinkPreview()
    {
        Vector3 startPos = firstBody.transform.position;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        float dist = Vector2.Distance(startPos, mousePos);
        isOverLength = dist > maxLength;

        Color c = isOverLength ? warningColor : normalColor;
        currentLineRenderer.startColor = c;
        currentLineRenderer.endColor = c;

        if (isOverLength)
        {
            Vector3 dir = (mousePos - startPos).normalized;
            mousePos = startPos + (dir * maxLength);
        }

        currentLineRenderer.SetPosition(0, startPos);
        currentLineRenderer.SetPosition(1, mousePos);
    }

    void CompleteLink(Rigidbody2D secondBody)
    {
        // 物理绑定
        FixedJoint2D j1 = currentLineObj.AddComponent<FixedJoint2D>();
        j1.connectedBody = firstBody;
        j1.enableCollision = false;

        FixedJoint2D j2 = currentLineObj.AddComponent<FixedJoint2D>();
        j2.connectedBody = secondBody;
        j2.enableCollision = false;

        // 赋予同步脚本
        var syncer = currentLineObj.AddComponent<LineSyncer>();
        syncer.targetA = firstBody.transform;
        syncer.targetB = secondBody.transform;

        remainingUses--;
        FinishAction();
    }

    void CancelLinking()
    {
        if (currentLineObj != null) Destroy(currentLineObj);
        FinishAction();
    }

    void FinishAction()
    {
        isActive = false;
        isLinking = false;
        firstBody = null;
        currentLineObj = null;
        currentLineRenderer = null;

        if (buttonImage != null) buttonImage.color = originalBtnColor;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (usageText != null) usageText.text = "剩余: " + remainingUses;
        if (linkButton != null && remainingUses <= 0)
        {
            linkButton.interactable = false;
        }
    }
}