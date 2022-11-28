using System;
using OSCore;
using OSCore.Data.Enums;
using OSCore.Events.Brains;
using OSCore.Interfaces;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces.Tagging;
using UnityEngine;

namespace OSBE.Brains {
    public class CameraControllerBrain : IControllerBrain {

        IGameSystem system;
        CinemachineCameraOffset camOffset;
        Transform target;
        CameraCfgSO cfg = null;

        bool isMoving;
        bool isScoping;
        bool isAiming;

        public CameraControllerBrain(IGameSystem system, Transform camera) {
            this.system = system;
            camOffset = camera.GetComponent<CinemachineCameraOffset>();
            target = system.Send<ITagRegistry, Transform>(registry =>
                registry.GetUnique(IdTag.PLAYER).transform);
        }

        public void OnMessage(IEvent message) {
            switch (message) {
                case InitEvent<CameraCfgSO> e:
                    cfg = e.cfg;
                    break;
            }
        }

        public void Update() {
            if (cfg != null)
                SetOffset();
        }

        void SetOffset() {
            float lookAhead = 0f;
            if (isScoping) lookAhead += cfg.scopeOffset;
            else if (isMoving) lookAhead += cfg.moveOffset;
            if (isAiming) lookAhead += cfg.aimOffset;

            lookAhead = Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead);

            camOffset.m_Offset = Vector3.Lerp(
                camOffset.m_Offset,
                target.rotation * new Vector3(0, lookAhead, 0),
                cfg.orbitSpeed * Time.deltaTime);
        }
    }
}
