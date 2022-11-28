using System;
using System.Collections.Generic;
using OSCore;
using OSCore.Data.Enums;
using OSCore.Interfaces;
using OSCore.Interfaces.Tagging;
using UnityEngine;

namespace OSFE {
    public class CameraController : MonoBehaviour {
        [SerializeField] CameraCfgSO cfg;

        IGameSystem system;
        CinemachineCameraOffset camOffset;
        Transform target;

        bool isMoving;
        bool isScoping;
        bool isAiming;

        void Start() {
            system = FindObjectOfType<GameController>();
            camOffset = GetComponent<CinemachineCameraOffset>();
            target = system.Send<ITagRegistry, Transform>(registry =>
                registry.GetUnique(IdTag.PLAYER).transform);
        }

        void Update() {
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
