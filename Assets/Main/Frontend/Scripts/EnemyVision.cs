using OSCore.Data.Controllers;
using OSCore.System.Interfaces.Controllers;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSFE.Scripts {
    public class EnemyVision : ASystemInitializer {
        private IController<EnemyControllerInput> controller;
        private GameObject player;
        private SpriteRenderer rdr;
        private bool seesPlayer = false;
        private float timeSinceSeeable = -0.25f;

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player();
        }

        private void Start() {
            Transform entity = Transforms.Entity(transform);
            controller = entity.GetComponent<IController<EnemyControllerInput>>();
            rdr = entity.GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate() {
            bool los = false;
            Vector3 playerPos = player.transform.position + new Vector3(0f, 0f, -0.1f); // TODON'T
            rdr.color = new Color(1, 1, 1, Mathf.Clamp(1 - timeSinceSeeable, 0, 1));

            IEnumerable<RaycastHit> hits = Sequences.Transduce(
                Physics.RaycastAll(
                    transform.parent.position,
                    playerPos - transform.parent.position,
                    Vector3.Distance(playerPos, transform.parent.position)),
                Fns.Filter<RaycastHit>(hit => !transform.IsChildOf(hit.transform)));

             if (!hits.IsEmpty() && hits.First().transform.IsChildOf(player.transform))
                los = true;

            controller.On(new PlayerLOS(seesPlayer && los));

            Vector3 position = transform.parent.parent.position + new Vector3(0, 0, -0.1f); // TODON'T
            Vector3 playerEyes = Transforms.FindInActiveChildren(
                player.transform,
                xform => xform.name == "head")
                .First().position;

            bool isBlocked = Physics.Raycast(
                playerEyes,
                position - playerEyes,
                out RaycastHit hit,
                Vector3.Distance(transform.parent.parent.position, playerEyes),
                1 << LayerMask.NameToLayer("Walls"));

            if (isBlocked) timeSinceSeeable += Time.fixedDeltaTime;
            else timeSinceSeeable = -0.25f;
        }

        private void OnTriggerStay(Collider other) {
            if (other.transform.IsChildOf(player.transform))
                seesPlayer = true;
        }

        private void OnTriggerExit(Collider other) {
            if (other.transform.IsChildOf(player.transform))
                seesPlayer = false;
        }
    }
}
