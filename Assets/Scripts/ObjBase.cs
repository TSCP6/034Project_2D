using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBase : MonoBehaviour
{
    public LayerMask controllableLayer;
    public bool dragEnabled = true;

    protected Camera mainCam;

    private Rigidbody2D draggingRb;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool consumeNextLeftClick;


    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dragEnabled)
        {
            return;
        }

        Move();
    }

    public void SetDragEnabled(bool enabled)
    {
        dragEnabled = enabled;
        if (!enabled)
        {
            isDragging = false;
            draggingRb = null;
        }
    }

    // Treat the current interaction as a release and ignore the next left-click down event.
    public void ConsumeNextLeftClickAsRelease()
    {
        isDragging = false;
        draggingRb = null;
        consumeNextLeftClick = true;
    }

    //存在拖拽太快会穿过墙体的bug
    void Move() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (consumeNextLeftClick)
            {
                consumeNextLeftClick = false;
                return;
            }

            if (!isDragging)
            {
                Vector3 mouseWorld = GetMouseWorldPos();
                Collider2D hit = Physics2D.OverlapPoint(mouseWorld, controllableLayer);

                if (hit != null && hit.TryGetComponent(out Rigidbody2D rb))
                {
                    draggingRb = rb;
                    isDragging = true;
                    dragOffset = draggingRb.transform.position - mouseWorld;
                    Debug.Log("dragging");
                } 
            }
            else
            {
                isDragging = false;
                draggingRb = null;
                Debug.Log("released");
            }
        }
    }

    void FixedUpdate()
    {
        if (isDragging && draggingRb != null)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            Vector2 targetPos = mouseWorld + dragOffset;
            draggingRb.MovePosition(targetPos);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 p = Input.mousePosition;
        p.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(p);
    }
}
