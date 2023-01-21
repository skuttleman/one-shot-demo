using OSCore.ScriptableObjects;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Player {
    public class PlayerFOVController : MonoBehaviour {
        [SerializeField] private PlayerFOVCfgSO cfg;

        private Mesh mesh;
        private float timeout;

        private void DrawFOV() {
            Transform head = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "head")
                .First();

            Vector3[] vertices = new Vector3[cfg.RAY_COUNT + 1 + 1];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[cfg.RAY_COUNT * 3];

            vertices[0] = new(
                head.position.x.RoundTo(100f),
                head.position.y,
                head.position.z.RoundTo(100f));

            int triangleIdx = DrawTriangles(vertices, triangles, head);

            triangles[triangleIdx] = 0;
            triangles[triangleIdx + 1] = triangles[triangleIdx - 1];
            triangles[triangleIdx + 2] = 1;

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(vertices[0], Vector3.one * 1000f);
        }

        private int DrawTriangles(Vector3[] vertices, int[] triangles, Transform head) {
            int triangleIdx = -3;
            float angleIncrease = 360f / cfg.RAY_COUNT;
            float angle = head.rotation.eulerAngles.y;

            for (int vertexIdx = 1; vertexIdx < cfg.RAY_COUNT;) {
                bool isHit = Physics.Raycast(
                    vertices[0],
                    Vectors.ToVector3(angle),
                    out RaycastHit hit,
                    cfg.viewDistance,
                    cfg.layerMask);

                vertices[vertexIdx] = isHit
                    ? transform.InverseTransformPoint(hit.point)
                    : vertices[0] + Vectors.ToVector3(angle) * cfg.viewDistance;

                if (triangleIdx >= 0) {
                    triangles[triangleIdx] = 0;
                    triangles[triangleIdx + 1] = vertexIdx - 1;
                    triangles[triangleIdx + 2] = vertexIdx;
                }

                angle = (angle - angleIncrease) % 360f;
                vertexIdx += 1;
                triangleIdx += 3;
            }

            return triangleIdx;
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            mesh = new();
            timeout = 0f;
            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void Update() {
            if (timeout > 0f) {
                timeout -= Time.deltaTime;
            } else {
                timeout = cfg.secondsBetween;
                DrawFOV();
            }
        }
    }
}
