using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore;
using UnityEngine;
using static OSCore.Data.Events.Brains.Camera.CameraEvent;

namespace OSFE.Camera {
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
