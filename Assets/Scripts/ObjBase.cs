using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjBase : MonoBehaviour
{
    // public LayerMask controllableLayer;

    protected Camera mainCam;

    private Rigidbody2D draggingRb;
    private Vector3 dragOffset;
    private bool isDragging;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Move();
        if (gameObject.transform.position.magnitude > 100f)
            Destroy(gameObject);
    }

    // Known issue: dragging too fast may pass through walls
    // void Move()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         if (!isDragging)
    //         {
    //             Vector3 mouseWorld = GetMouseWorldPos();
    //             Collider2D hit = Physics2D.OverlapPoint(mouseWorld, controllableLayer);

    //             if (hit != null && hit.TryGetComponent(out Rigidbody2D rb))
    //             {
    //                 draggingRb = rb;
    //                 isDragging = true;
    //                 dragOffset = draggingRb.transform.position - mouseWorld;
    //                 Debug.Log("dragging");
    //             }
    //         }
    //         else
    //         {
    //             isDragging = false;
    //             draggingRb = null;
    //             Debug.Log("released");
    //         }
    //     }
    // }

    // void FixedUpdate()
    // {
    //     if (isDragging && draggingRb != null)
    //     {
    //         Vector3 mouseWorld = GetMouseWorldPos();
    //         Vector2 targetPos = mouseWorld + dragOffset;
    //         draggingRb.MovePosition(targetPos);
    //     }
    // }

    // private Vector3 GetMouseWorldPos()
    // {
    //     Vector3 p = Input.mousePosition;
    //     p.z = -mainCam.transform.position.z;
    //     return mainCam.ScreenToWorldPoint(p);
    // }
}
