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


    [Header("Auto Link on Scene Start (Multiple objects + custom anchor points)")]
    // Configure multiple link groups, each group contains: two objects + their custom anchor points
    public List<InitialLinkGroup> initialLinkGroups = new List<InitialLinkGroup>();

    // New class: defines a group of initial links
    [System.Serializable]
    public class InitialLinkGroup
    {
        public Rigidbody2D bodyA;          // First object
        public Vector2 anchorA;            // Custom anchor point for object A (local coordinates)
        public Rigidbody2D bodyB;          // Second object
        public Vector2 anchorB;            // Custom anchor point for object B (local coordinates)
        public bool enableThisLink = true; // Enable this link group
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

        // On game start, batch create all configured initial links
        CreateAllInitialLinks();
    }

    /// <summary>
    /// Core: batch create all initial links (supports multiple groups)
    /// </summary>
    private void CreateAllInitialLinks()
    {
        foreach (var linkGroup in initialLinkGroups)
        {
            if (!linkGroup.enableThisLink) continue;

            if (linkGroup.bodyA == null || linkGroup.bodyB == null)
            {
                continue;
            }

            if (linkGroup.bodyA == linkGroup.bodyB)
            {
                continue;
            }

            Vector2 worldPosA = linkGroup.bodyA.transform.TransformPoint(linkGroup.anchorA);
            Vector2 worldPosB = linkGroup.bodyB.transform.TransformPoint(linkGroup.anchorB);

            float dist = Vector2.Distance(worldPosA, worldPosB);
            if (dist > maxLength)
            {
                continue;
            }

            GameObject rod = Instantiate(rodPrefab);
            UpdateRodTransform(rod.transform, worldPosA, worldPosB, dist);

            Rigidbody2D rodRb = rod.GetComponent<Rigidbody2D>();
            rodRb.isKinematic = false;

            CreateHinge(rod, linkGroup.bodyA, rod.transform.InverseTransformPoint(worldPosA));
            CreateHinge(rod, linkGroup.bodyB, rod.transform.InverseTransformPoint(worldPosB));
        }
    }

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