using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers {
    public class PlayerFOVController : IPlayerFOVController {
        readonly IGameSystem system;
        readonly Transform fov;
        PlayerFOVCfgSO cfg;
        Mesh mesh;
        float timeout;

        public PlayerFOVController(IGameSystem system, Transform target) {
            this.system = system;
            fov = target;
            timeout = 0f;
        }

        public void Init(PlayerFOVCfgSO cfg, Mesh mesh) {
            this.cfg = cfg;
            this.mesh = mesh;
        }

        public void OnUpdate() {
            if (timeout > 0f) {
                timeout -= Time.deltaTime;
            } else if (cfg is not null) {
                timeout = cfg.secondsBetween;
                DrawFOV();
            }
        }
        private void DrawFOV() {
            Transform head = Transforms
                .FindInActiveChildren(fov.parent, xform => xform.name == "head")
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
                    Vectors.ToVector3(angle + fov.rotation.eulerAngles.z),
                    out RaycastHit hit,
                    cfg.viewDistance,
                    cfg.layerMask);
                vertices[vertexIdx] = isHit
                    ? fov.InverseTransformPoint(hit.point)
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
