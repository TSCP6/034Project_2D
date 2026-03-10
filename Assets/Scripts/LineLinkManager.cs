using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineLinkManager : MonoBehaviour
{
    [Header("Usage Limit (n)")]
    public int maxUses = 5;         // Total uses in this level
    private int remainingUses;      // Remaining uses

    [Header("Configuration")]
    public GameObject linePrefab;    // Line prefab (with LineRenderer and Rigidbody2D)
    public LayerMask targetLayer;    // Layer of target objects
    public float maxLength = 5f;     // Max line length

    [Header("UI References")]
    public Button linkButton;        // Linked UI button
    public Text usageText;           // (Optional) text for usage display

    [Header("Color Feedback")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color flashColor = Color.yellow; // Flash color when active

    [Header("Animation Settings")]
    public float flashSpeed = 5f;    // Button flash speed

    [Header("Current State")]
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

    // Public method for UI button click
    // Public method for UI button click
    public void EnableLinkOnce()
    {
        // 1. Return immediately if no uses remain
        if (remainingUses <= 0) return;

        // 2. If already active (isActive == true)
        if (isActive)
        {
            // If currently previewing a line (first click already made), cancel preview
            if (isLinking)
            {
                CancelLinking();
                Debug.Log("Line preview canceled.");
            }
            else
            {
                // If feature is on but first click has not happened, just reset state
                FinishAction();
                Debug.Log("Feature disabled.");
            }
        }
        else
        {
            // 3. If currently inactive, enable feature
            isActive = true;
            Debug.Log($"Feature enabled. Remaining uses: {remainingUses}");
        }
    }
    void Update()
    {
        // Handle button flashing visual effect
        HandleButtonFlashing();

        if (!isActive) return;

        // 1. Handle mouse click logic
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent UI click-through from creating lines behind UI
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

        // 2. Update line preview
        if (isLinking && currentLineRenderer != null)
        {
            UpdateLinkPreview();
        }

        // 3. Right click to cancel (does not consume uses)
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
            // Use unscaledTime so this still runs when Time.timeScale is 0
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
        // Physics binding
        FixedJoint2D j1 = currentLineObj.AddComponent<FixedJoint2D>();
        j1.connectedBody = firstBody;
        j1.enableCollision = false;

        FixedJoint2D j2 = currentLineObj.AddComponent<FixedJoint2D>();
        j2.connectedBody = secondBody;
        j2.enableCollision = false;

        // Add sync script
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
        if (usageText != null) usageText.text = "Remaining: " + remainingUses;
        if (linkButton != null && remainingUses <= 0)
        {
            linkButton.interactable = false;
        }
    }
}