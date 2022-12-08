using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace OSBE.Controllers {
    public class PlayerFOVController : IPlayerFOVController {
        readonly IGameSystem system;
        readonly Transform fov;
        PlayerFOVCfgSO cfg;
        Mesh mesh;

        public PlayerFOVController(IGameSystem system, Transform target) {
            this.system = system;
            fov = target;
        }

        public void Init(PlayerFOVCfgSO cfg, Mesh mesh) {
            this.cfg = cfg;
            this.mesh = mesh;
        }

        public void OnUpdate() {
            Transform head = Transforms
                .FindInChildren(fov.parent, xform => xform.gameObject.activeInHierarchy && xform.name == "head")
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
                float distance = Mathf.Lerp(cfg.viewDistance - cfg.angleDither, cfg.viewDistance, percent);

                bool isHit = Physics.Raycast(
                    head.position,
                    Vectors.ToVector2(angle + fov.rotation.eulerAngles.z).Upgrade(),
                    out RaycastHit hit,
                    distance,
                    cfg.layerMask);

                vertices[vertexIdx] = isHit
                    ? fov.InverseTransformPoint(hit.point)
                    : vertices[0] + Vectors.ToVector3(angle) * distance;

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
