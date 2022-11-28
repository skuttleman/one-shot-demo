using System;
using System.Collections.Generic;
using OSCore;
using OSCore.Events.Brains;
using OSCore.Interfaces.Brains;
using UnityEngine;

namespace OSFE {
    public class CameraController : MonoBehaviour {
        [SerializeField] CameraCfgSO cfg;

        void Start() {
            FindObjectOfType<GameController>()
                .Send<IControllerBrainManager>(mngr =>
                    mngr.Ensure(transform, EControllerBrainTag.CAMERA)
                        .OnMessage(new InitEvent<CameraCfgSO>(cfg)));
        }
    }
}
