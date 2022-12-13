using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;
using OSCore.Utils;
using System.Collections.Generic;

public class EnemyVision : MonoBehaviour {
    private IGameSystem system;
    private GameObject player;
    private Renderer rdr;
    private bool seesPlayer = false;
    private float timeSinceSeen = 1f;

    private void OnEnable() {
        system = FindObjectOfType<GameController>();
        player = system.Send<ITagRegistry, GameObject>(reg =>
            reg.GetUnique(OSCore.Data.Enums.IdTag.PLAYER));
        rdr = transform.parent.parent.parent.GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate() {
        bool los = false;
        Vector3 playerPos = player.transform.position + new Vector3(0f, 0f, -0.25f);

        IEnumerable<RaycastHit> hits = Physics.RaycastAll(
            transform.parent.position,
            playerPos - transform.parent.position,
            Vector3.Distance(playerPos, transform.parent.position))
            .Remove(hit => transform.IsChildOf(hit.transform));
        if (!hits.IsEmpty() && hits.First().transform.IsChildOf(player.transform))
            los = true;

        Brain().OnPlayerSightChange(seesPlayer && los);

        if (!los) {
            timeSinceSeen += Time.fixedDeltaTime;
            if (timeSinceSeen > 1f) rdr.enabled = false;
        } else rdr.enabled = true;
    }

    private void OnTriggerStay(Collider other) {
        if (other.transform.IsChildOf(player.transform))
            seesPlayer = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.IsChildOf(player.transform))
            seesPlayer = false;
    }

    private IEnemyStateReducer Brain() =>
        system.Send<IControllerManager, IEnemyStateReducer>(mngr =>
            mngr.Ensure<IEnemyStateReducer>(transform.parent.parent.parent));
}
