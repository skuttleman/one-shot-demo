using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;
using OSCore.ScriptableObjects;

namespace OSFE.Camera {
public class CameraOverlay : MonoBehaviour {
        [SerializeField] private CameraOverlayCfgSO cfg;

        private IGameSystem system;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            system.Send<IControllerManager>(mngr =>
                mngr.Ensure<ICameraOverlayController>(transform)
                    .Init(cfg));
        }
    }
}
