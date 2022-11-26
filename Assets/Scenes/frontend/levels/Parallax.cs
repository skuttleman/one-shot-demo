using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSCore.Utils;


namespace OSFE.Levels {
    public class Parallax : MonoBehaviour {
        private Vector3 realPosition;
        private Vector2 offset;
        BuildingParallax cfg;
        Transform focus;

        void Start() {
            realPosition = transform.position;
            BuildingController controller = GetComponentInParent<BuildingController>();
            cfg = controller.cfg;
            focus = controller.focus;
        }

        void FixedUpdate() {
            float diffZ = focus.position.z - realPosition.z;
            offset = cfg.customFactor * diffZ * (cfg.center.Upgrade() - focus.position);

            UpdatePosition(Mathf.Abs(diffZ));
        }

        void UpdatePosition(float diffZ) {
            transform.position = realPosition
                + Vectors.Clamp(offset.Upgrade(), -cfg.maxOffset * diffZ, cfg.maxOffset * diffZ);
        }
    }
}
