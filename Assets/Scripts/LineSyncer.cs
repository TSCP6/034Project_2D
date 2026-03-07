using UnityEngine;

public class LineSyncer : MonoBehaviour
{
    public Transform targetA;
    public Transform targetB;
    private LineRenderer lr;

    void Start() => lr = GetComponent<LineRenderer>();

    void LateUpdate()
    {
        if (targetA != null && targetB != null)
        {
            lr.SetPosition(0, targetA.position);
            lr.SetPosition(1, targetB.position);
        }
        else { Destroy(gameObject); }
    }
}