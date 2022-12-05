using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore;
using UnityEngine;

namespace OSFE.Camera {
    public class CameraConnector : MonoBehaviour {
        [SerializeField] CameraCfgSO cfg;

        void OnEnable() {
            FindObjectOfType<GameController>()
                .Send<IControllerManager>(mngr =>
                    mngr.Ensure<ICameraController>(transform)
                        .Init(cfg));
        }
    }
}
