using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore;
using UnityEngine;

namespace OSFE.Camera {
    public class CameraController : MonoBehaviour {
        [SerializeField] CameraCfgSO cfg;

        void OnEnable() {
            FindObjectOfType<GameController>()
                .Send<IControllerBrainManager>(mngr =>
                    mngr.Ensure<ICameraControllerBrain>(transform)
                        .Init(cfg));
        }
    }
}
