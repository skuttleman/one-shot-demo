using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;

public class EnemyVision : MonoBehaviour {
    IGameSystem system;
    GameObject player;
    bool seesPlayer = false;

    void OnEnable() {
        system = FindObjectOfType<GameController>();
        player = system.Send<ITagRegistry, GameObject>(reg =>
            reg.GetUnique(OSCore.Data.Enums.IdTag.PLAYER));
    }

    void FixedUpdate() {
        Transform head = transform.parent;
        if (seesPlayer && Physics.Raycast(
            head.position,
            (player.transform.position - head.position).normalized,
            out RaycastHit hit,
            100f)) {
            Brain().OnPlayerSightChange(hit.collider.gameObject.transform.IsChildOf(player.transform));
        } else {
            Brain().OnPlayerSightChange(false);
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.transform.IsChildOf(player.transform))
            seesPlayer = true;
    }

    void OnTriggerExit(Collider other) {
        if (other.transform.IsChildOf(player.transform))
            seesPlayer = false;
    }

    IEnemyStateReducer Brain() =>
        system.Send<IControllerManager, IEnemyStateReducer>(mngr =>
            mngr.Ensure<IEnemyStateReducer>(transform.parent.parent.parent));
}
