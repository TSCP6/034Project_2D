using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineLinkManager : MonoBehaviour
{
    [Header("Usage Settings")]
    public int maxUses = 5;
    private int remainingUses;

    [Header("Rod Settings")]
    public GameObject rodPrefab;    // Rod prefab (requires SpriteRenderer, BoxCollider2D, Rigidbody2D)
    public LayerMask targetLayer;
    public float maxLength = 10f;
    public float rodWidth = 0.2f;   // Rod visual width / collider size

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

        Debug.Log(isActive ? "Link mode activated" : "Link mode deactivated");
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
        previewRod.GetComponent<Collider2D>().enabled = false; // Disable collider on preview rod
        previewRod.GetComponent<Rigidbody2D>().isKinematic = true; // Prevent physics simulation on preview
        previewRenderer = previewRod.GetComponent<SpriteRenderer>();
    }

    void UpdateLinkPreview()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(firstWorldPoint, mousePos);

        // Clamp mouse position to max length
        if (dist > maxLength)
        {
            Vector2 dir = (mousePos - firstWorldPoint).normalized;
            mousePos = firstWorldPoint + dir * maxLength;
            dist = maxLength;
        }

        UpdateRodTransform(previewRod.transform, firstWorldPoint, mousePos, dist);

        // Show warning color when near max length
        previewRenderer.color = (dist >= maxLength * 0.95f) ? warningColor : normalColor;
    }

    void UpdateRodTransform(Transform trans, Vector2 start, Vector2 end, float length)
    {
        // Set position to midpoint between start and end
        trans.position = (start + end) / 2f;
        // Set rotation to face from start to end
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.Euler(0, 0, angle);
        // Set scale: X = length, Y = width, Z = 1
        trans.localScale = new Vector3(length, rodWidth, 1f);
    }

    void CompleteLink(Rigidbody2D secondBody, Vector2 secondWorldPoint)
    {
        float dist = Vector2.Distance(firstWorldPoint, secondWorldPoint);
        if (dist > maxLength) return;

        // 1. Instantiate the final rod at the correct position/rotation/scale
        GameObject finalRod = Instantiate(rodPrefab);
        UpdateRodTransform(finalRod.transform, firstWorldPoint, secondWorldPoint, dist);

        Rigidbody2D rodRb = finalRod.GetComponent<Rigidbody2D>();
        rodRb.isKinematic = false;

        // 2. Create hinge joint at the first body's contact point
        CreateHinge(finalRod, firstBody, finalRod.transform.InverseTransformPoint(firstWorldPoint));

        // 3. Create hinge joint at the second body's contact point
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
        // Convert anchor to the connected body's local space
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
        if (usageText != null) usageText.text = "Uses: " + remainingUses;
        if (linkButton != null) linkButton.interactable = (remainingUses > 0);
    }
}
