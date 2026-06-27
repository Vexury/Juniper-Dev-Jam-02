using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIConnector : Graphic
{
    public RectTransform pointA;
    public RectTransform pointB;
    public float thickness = 4f;

    [Tooltip("Pull each endpoint inward so the line stops at the element edge instead of its center.")]
    public float edgeInset = 0f;

    Vector3 _lastA, _lastB;

    void Update()
    {
        if (pointA == null || pointB == null) return;

        if (pointA.position != _lastA || pointB.position != _lastB)
        {
            _lastA = pointA.position;
            _lastB = pointB.position;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (pointA == null || pointB == null) return;

        Vector2 a = rectTransform.InverseTransformPoint(pointA.position);
        Vector2 b = rectTransform.InverseTransformPoint(pointB.position);

        Vector2 dir = (b - a).normalized;
        if (edgeInset > 0f)
        {
            a += dir * edgeInset;
            b -= dir * edgeInset;
        }

        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        AddVert(vh, a - normal);
        AddVert(vh, a + normal);
        AddVert(vh, b + normal);
        AddVert(vh, b - normal);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    void AddVert(VertexHelper vh, Vector2 pos)
    {
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        v.position = pos;
        vh.AddVert(v);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
