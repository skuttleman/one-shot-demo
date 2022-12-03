using System;
using System.Collections.Generic;
using OSCore;
using OSCore.Interfaces.Brains;
using UnityEngine;
using static OSCore.Events.Brains.Camera.CameraEvent;

namespace OSFE {
    public class CameraController : MonoBehaviour {
        [SerializeField] CameraCfgSO cfg;

        void OnEnable() {
            CinemachineCameraOffset offset = GetComponent<CinemachineCameraOffset>();
            FindObjectOfType<GameController>()
                .Send<IControllerBrainManager>(mngr =>
                    mngr.Ensure(EControllerBrainTag.CAMERA, transform)
                        .Handle(new CameraInitEvent(cfg, offset)));
        }
    }
}
