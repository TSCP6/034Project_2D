using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteObj : MonoBehaviour
{
    public LayerMask deletableLayer;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, deletableLayer);

            if (hit.collider != null)
            {
                GameObject objectToDelete = hit.collider.gameObject;
                DeleteObjectAndConnectedLinks(objectToDelete);
            }
        }
    }

    private void DeleteObjectAndConnectedLinks(GameObject target)
    {
        if (target == null) return;

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null)
        {
            Destroy(target);
            return;
        }

        HingeJoint2D[] allHinges = FindObjectsOfType<HingeJoint2D>();
        List<GameObject> linksToDelete = new List<GameObject>();

        foreach (var hinge in allHinges)
        {
            if (hinge.connectedBody == targetRb)
            {
                if (!linksToDelete.Contains(hinge.gameObject))
                {
                    linksToDelete.Add(hinge.gameObject);
                }
            }
        }

        foreach (var link in linksToDelete)
        {
            Destroy(link);
        }

        Destroy(target);
    }
}
