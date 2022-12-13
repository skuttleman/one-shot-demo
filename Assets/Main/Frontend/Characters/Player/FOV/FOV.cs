using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;

public class FOV : MonoBehaviour {
    [SerializeField] private PlayerFOVCfgSO cfg;
    private IGameSystem system;
    private Mesh mesh;

    private void OnEnable() {
        system = FindObjectOfType<GameController>();
        mesh = new();
        GetComponent<MeshFilter>().mesh = mesh;

        system.Send<IControllerManager>(mngr =>
            mngr.Ensure<IPlayerFOVController>(transform)
                .Init(cfg, mesh));
    }
}
