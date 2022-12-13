using OSCore.ScriptableObjects;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers {
    public class PlayerFOVController : MonoBehaviour {
        [SerializeField] private PlayerFOVCfgSO cfg;

        private Mesh mesh;
        private float timeout;

        private void OnEnable() {
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
        private void DrawFOV() {
            Transform head = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "head")
                .First();
            float angle = head.rotation.eulerAngles.z;
            float angleIncrease = 360f / cfg.RAY_COUNT;

            Vector3[] vertices = new Vector3[cfg.RAY_COUNT + 1 + 1];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[cfg.RAY_COUNT * 3];

            vertices[0] = head.position;
            float facing = head.rotation.eulerAngles.z;

            int triangleIdx = -3;
            for (int vertexIdx = 1; vertexIdx < cfg.RAY_COUNT;) {
                float diff = Mathf.Abs(facing + 90f - angle);
                if (diff > 180f) diff = Mathf.Abs(360f - diff);

                float percent = Mathf.Max(0.333f, (180f - diff) / 180f);
                bool isHit = Physics.Raycast(
                    head.position,
                    Vectors.ToVector3(angle + transform.rotation.eulerAngles.z),
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

            triangles[triangleIdx] = 0;
            triangles[triangleIdx + 1] = triangles[triangleIdx - 1];
            triangles[triangleIdx + 2] = 1;

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(vertices[0], Vector3.one * 1000f);
        }
    }
}
