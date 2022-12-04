using OSCore.Utils;
using UnityEngine;

public class FOV : MonoBehaviour {
    static readonly int RAY_COUNT = 50;

    [SerializeField] LayerMask layerMask;
    [SerializeField] float fov;
    [SerializeField] float viewDistance;
    [SerializeField] float startingAngle;

    Mesh mesh;

    void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate() {
        float angle = startingAngle;
        float angleIncrease = fov / RAY_COUNT;

        Vector3[] vertices = new Vector3[RAY_COUNT + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[RAY_COUNT * 3];

        vertices[0] = Vector3.zero;

        Sequences.Iterate((1, -3), ((int a, int b) t) => (t.a + 1, t.b + 3))
            .Take(RAY_COUNT + 1)
            .ForEach(((int vertexIdx, int triangleIdx) t) => {
                bool isHit = IsHit(angle, out RaycastHit hit);
                vertices[t.vertexIdx] = isHit
                    ? transform.InverseTransformPoint(hit.point)
                    : vertices[0] + Vectors.ToVector3(angle) * viewDistance;

                if (t.triangleIdx >= 0) {
                    triangles[t.triangleIdx] = 0;
                    triangles[t.triangleIdx + 1] = t.vertexIdx - 1;
                    triangles[t.triangleIdx + 2] = t.vertexIdx;
                }

                angle -= angleIncrease;
            });

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(vertices[0], Vector3.one * 1000f);
    }

    bool IsHit(float angle, out RaycastHit hit) =>
        Physics.Raycast(
            transform.position,
            Vectors.ToVector3(angle + transform.rotation.eulerAngles.z),
            out hit,
            viewDistance,
            layerMask);
}
