using UnityEngine;

public class RopeLine : MonoBehaviour
{
    // This script visualizes one or more ropes for level 2.
    public Transform anchor; // Optional shared hang point override
    public DistanceJoint2D[] joints;
    public LineRenderer[] ropeLines;
    public float lineWidth = 0.1f;
    public Material lineMaterial;
    public Color lineColor = Color.white;
    public string sortingLayerName = "Default";
    public int sortingOrder = 20;

    void Awake()
    {
        if (joints == null || joints.Length == 0)
        {
            return;
        }

        if (ropeLines == null || ropeLines.Length != joints.Length)
        {
            ropeLines = new LineRenderer[joints.Length];
        }

        for (int i = 0; i < joints.Length; i++)
        {
            if (ropeLines[i] == null)
            {
                GameObject lineObj = new GameObject($"RopeLine_{i}");
                lineObj.transform.SetParent(transform, false);
                ropeLines[i] = lineObj.AddComponent<LineRenderer>();
            }

            LineRenderer ropeLine = ropeLines[i];
            ConfigureRenderer(ropeLine);
        }
    }

    void LateUpdate()
    {
        if (joints == null || ropeLines == null)
        {
            return;
        }

        int count = Mathf.Min(joints.Length, ropeLines.Length);
        for (int i = 0; i < count; i++)
        {
            if (joints[i] == null || ropeLines[i] == null)
            {
                continue;
            }

            ConfigureRenderer(ropeLines[i]);

            Vector3 ropeStartWorld;

            // Prefer explicit anchor when provided.
            if (anchor != null)
            {
                ropeStartWorld = anchor.position;
            }
            // If connectedBody exists, connectedAnchor is local to connectedBody.
            else if (joints[i].connectedBody != null)
            {
                ropeStartWorld = joints[i].connectedBody.transform.TransformPoint(joints[i].connectedAnchor);
            }
            // If connectedBody is null, connectedAnchor is already world-space.
            else
            {
                ropeStartWorld = joints[i].connectedAnchor;
            }

            // Joint anchor is local-space on platform; convert to world-space.
            Vector3 platformAnchorWorld = joints[i].transform.TransformPoint(joints[i].anchor);

            ropeLines[i].SetPosition(0, ropeStartWorld);
            ropeLines[i].SetPosition(1, platformAnchorWorld);
        }
    }

    void ConfigureRenderer(LineRenderer ropeLine)
    {
        ropeLine.enabled = true;
        ropeLine.positionCount = 2;
        ropeLine.useWorldSpace = true;
        ropeLine.startWidth = Mathf.Max(0.02f, lineWidth);
        ropeLine.endWidth = Mathf.Max(0.02f, lineWidth);
        ropeLine.sortingLayerName = sortingLayerName;
        ropeLine.sortingOrder = sortingOrder;
        ropeLine.textureMode = LineTextureMode.Stretch;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(lineColor, 0f),
                new GradientColorKey(lineColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(lineColor.a, 0f),
                new GradientAlphaKey(lineColor.a, 1f)
            }
        );
        ropeLine.colorGradient = g;

        if (lineMaterial != null)
        {
            ropeLine.material = lineMaterial;
        }
        else if (ropeLine.material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            ropeLine.material = new Material(shader);
        }

        if (ropeLine.material != null)
        {
            ropeLine.material.color = Color.white;
        }
    }
}
