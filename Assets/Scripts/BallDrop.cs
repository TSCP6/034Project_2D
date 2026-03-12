using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDrop : MonoBehaviour
{
    public ObjCreator objc;
    public bool autoFindObjCreator = true;
    private Rigidbody2D rb;
    private bool startedDrop;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("BallDrop requires a Rigidbody2D on the same GameObject.");
            enabled = false;
            return;
        }

        if (objc == null && autoFindObjCreator)
        {
            objc = FindObjectOfType<ObjCreator>();
        }

        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (startedDrop)
        {
            return;
        }

        if (objc == null && autoFindObjCreator)
        {
            objc = FindObjectOfType<ObjCreator>();
        }

        if (objc == null)
        {
            rb.isKinematic = true;
            return;
        }

        if (objc.AreAllObjectsCreated())
        {
            rb.isKinematic = false;
            startedDrop = true;
        }
        else
        {
            rb.isKinematic = true;
        }
    }
}
