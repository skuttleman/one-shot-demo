using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;

public class FOV : MonoBehaviour {
    [SerializeField] PlayerFOVCfgSO cfg;
    IGameSystem system;
    Mesh mesh;

    void OnEnable() {
        system = FindObjectOfType<GameController>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        system.Send<IControllerManager>(mngr =>
            mngr.Ensure<IPlayerFOVController>(transform.parent.parent.parent)
                .Init(cfg, mesh, transform));
    }
}
