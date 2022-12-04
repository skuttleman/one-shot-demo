using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Brains {
    public class PlayerFOVBrain : IPlayerFOVBrain {
        readonly IGameSystem system;
        readonly Transform target;
        PlayerFOVCfgSO cfg;
        Mesh mesh;
        Transform fov;

        public PlayerFOVBrain(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
        }

        public void Init(PlayerFOVCfgSO cfg, Mesh mesh, Transform fov) {
            this.cfg = cfg;
            this.mesh = mesh;
            this.fov = fov;
        }

        public void Update() {
            float angle = cfg.startingAngle;
            float angleIncrease = cfg.fov / cfg.RAY_COUNT;

            Vector3[] vertices = new Vector3[cfg.RAY_COUNT + 1 + 1];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[cfg.RAY_COUNT * 3];

            vertices[0] = Vector3.zero;

            Sequences.Iterate((1, -3), ((int a, int b) t) => (t.a + 1, t.b + 3))
                .Take(cfg.RAY_COUNT + 1)
                .ForEach(((int vertexIdx, int triangleIdx) t) => {
                    bool isHit = IsHit(angle, out RaycastHit hit);
                    vertices[t.vertexIdx] = isHit
                        ? fov.InverseTransformPoint(hit.point)
                        : vertices[0] + Vectors.ToVector3(angle) * cfg.viewDistance;

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
                fov.position,
                Vectors.ToVector3(angle + fov.rotation.eulerAngles.z),
                out hit,
                cfg.viewDistance,
                cfg.layerMask);
    }
}
