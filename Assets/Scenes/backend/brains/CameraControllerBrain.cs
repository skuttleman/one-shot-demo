using System;
using OSCore;
using OSCore.Data.Enums;
using OSCore.Events.Brains;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces.Events;
using OSCore.Interfaces.Tagging;
using UnityEngine;
using static OSCore.Events.Brains.Camera.CameraEvent;
using static OSCore.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Brains {
    public class CameraControllerBrain : IControllerBrain {
        readonly IGameSystem system;
        readonly Transform target;
        CameraCfgSO cfg = null;
        CinemachineCameraOffset camOffset = null;

        bool isMoving;
        bool isScoping;
        bool isAiming;

        public CameraControllerBrain(IGameSystem system, Transform camera) {
            this.system = system;
            target = system.Send<ITagRegistry, Transform>(registry =>
                registry.GetUnique(IdTag.PLAYER).transform);
            system.Send<IPubSub>(pubsub => {
                pubsub.Subscribe<AttackModeChanged>(UpdateState);
                pubsub.Subscribe<MovementChanged>(UpdateState);
                pubsub.Subscribe<ScopingChanged>(UpdateState);
            });
        }

        public void Handle(IEvent message) {
            switch (message) {
                case CameraInitEvent e:
                    cfg = e.cfg;
                    camOffset = e.offset;
                    break;
            }
        }

        public void Update() {
            if (cfg != null && camOffset != null)
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

        void UpdateState(AttackModeChanged ev) =>
            isAiming = ev.mode == PlayerAttackMode.WEAPON
                || ev.mode == PlayerAttackMode.FIRING;

        void UpdateState(MovementChanged ev) =>
            isMoving = ev.isMoving;

        void UpdateState(ScopingChanged ev) =>
            isScoping = ev.isScoping;
    }
}
