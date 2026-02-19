using UnityEngine;

/// <summary>
/// Рисует "ткань" как один Mesh-стрип по точкам (points top->bottom).
/// - Нет дыр между сегментами (одна геометрия).
/// - Ширина меняется по длине (widthTop -> widthBottom).
/// - UV по длине тайлится в "world units" (tileWorldUnits).
/// - Подходит для 2D (XY), без нормалей/освещения по умолчанию.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class ClothStripMesh2D : MonoBehaviour
{
    [SerializeField] private bool forceDrawMesh = true;
    [SerializeField] private Material forceMaterial;

    [SerializeField] private bool flipWinding = false;

    [Header("Path points (top -> bottom)")]
    [SerializeField] private Transform[] points;

    [Header("Width (world units)")]
    [SerializeField, Min(0f)] private float widthTop = 0.20f;
    [SerializeField, Min(0f)] private float widthBottom = 0.35f;

    [Header("UV tiling")]
    [Tooltip("Сколько world-units приходится на 1 повтор текстуры по длине. Меньше = чаще повтор.")]
    [SerializeField, Min(0.01f)] private float tileWorldUnits = 0.40f;

    [Tooltip("Сдвиг UV по длине (можно анимировать для эффекта 'текущей ткани').")]
    [SerializeField] private float vOffset = 0f;

    [Header("2D sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 0;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = false;

    private Mesh _mesh;
    private Vector3[] _verts;
    private Vector2[] _uv;
    private int[] _tris;

    private MeshRenderer _mr;

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        ApplySorting();

        _mesh = new Mesh { name = "ClothStripMesh2D" };
        GetComponent<MeshFilter>().sharedMesh = _mesh;

        RebuildIfNeeded();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        ApplySorting();
        RebuildIfNeeded();
    }

    private void ApplySorting()
    {
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        // MeshRenderer тоже умеет sortingLayer/order
        _mr.sortingLayerName = sortingLayerName;
        _mr.sortingOrder = sortingOrder;
    }

    private void LateUpdate()
    {
        if (points == null || points.Length < 2) return;

        RebuildIfNeeded();
        UpdateMesh();
        if (forceDrawMesh && forceMaterial != null && _mesh != null)
        {
            Graphics.DrawMesh(
                _mesh,
                transform.localToWorldMatrix,
                forceMaterial,
                gameObject.layer
            );
        }

        if (Time.frameCount % 30 == 0)
        {
            var b = _mesh.bounds;
            Debug.Log($"[ClothMesh] verts={_mesh.vertexCount} boundsCenter={b.center} size={b.size} pos={transform.position}");
        }
    }

    private void RebuildIfNeeded()
    {
        int n = points != null ? points.Length : 0;
        if (n < 2) return;

        int vertCount = n * 2;
        int triCount = (n - 1) * 6;

        if (_verts != null && _verts.Length == vertCount && _tris != null && _tris.Length == triCount && _uv != null && _uv.Length == vertCount)
            return;

        _verts = new Vector3[vertCount];
        _uv = new Vector2[vertCount];
        _tris = new int[triCount];

        int ti = 0;
        for (int i = 0; i < n - 1; i++)
        {
            int a = i * 2;
            int b = i * 2 + 1;
            int c = (i + 1) * 2;
            int d = (i + 1) * 2 + 1;

            if (!flipWinding)
            {
                _tris[ti++] = a; _tris[ti++] = b; _tris[ti++] = c;
                _tris[ti++] = b; _tris[ti++] = d; _tris[ti++] = c;
            }
            else
            {
                _tris[ti++] = a; _tris[ti++] = c; _tris[ti++] = b;
                _tris[ti++] = b; _tris[ti++] = c; _tris[ti++] = d;
            }
        }

        _mesh.Clear();
        _mesh.vertices = _verts;
        _mesh.uv = _uv;
        _mesh.triangles = _tris;
        _mesh.RecalculateBounds();
    }

    private void UpdateMesh()
    {
        int n = points.Length;
        if (n < 2) return;

        float totalV = 0f;

        // точки и меш в одной локальной системе (CloakPivot)
        Vector3 prevP = points[0].localPosition;
        prevP.z = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 p = points[i].localPosition;
            p.z = 0f;

            // tangent в локале
            Vector3 t;
            if (i == 0)
            {
                Vector3 next = points[i + 1].localPosition;
                next.z = 0f;
                t = next - p;
            }
            else if (i == n - 1)
            {
                Vector3 prev = points[i - 1].localPosition;
                prev.z = 0f;
                t = p - prev;
            }
            else
            {
                Vector3 next = points[i + 1].localPosition;
                Vector3 prev = points[i - 1].localPosition;
                next.z = 0f;
                prev.z = 0f;
                t = next - prev;
            }

            t.z = 0f;
            if (t.sqrMagnitude < 1e-6f) t = Vector3.down;
            else t.Normalize();

            Vector3 normal = new Vector3(-t.y, t.x, 0f);

            float tt = (float)i / (n - 1);
            float halfW = Mathf.Lerp(widthTop, widthBottom, tt) * 0.5f;

            int vi = i * 2;
            _verts[vi + 0] = p - normal * halfW;
            _verts[vi + 1] = p + normal * halfW;

            if (i > 0)
            {
                float segLen = (p - prevP).magnitude;
                totalV += segLen / Mathf.Max(0.0001f, tileWorldUnits);
                prevP = p;
            }

            float v = totalV + vOffset;
            _uv[vi + 0] = new Vector2(0f, v);
            _uv[vi + 1] = new Vector2(1f, v);
        }

        _mesh.vertices = _verts;
        _mesh.uv = _uv;
        _mesh.RecalculateBounds();
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || points == null || points.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i] == null || points[i + 1] == null) continue;
            Gizmos.DrawLine(points[i].position, points[i + 1].position);
        }
    }
}