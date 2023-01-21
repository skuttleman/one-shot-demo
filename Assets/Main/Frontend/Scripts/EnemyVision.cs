using OSCore.Data.Controllers;
using OSCore.System.Interfaces.Controllers;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSFE.Scripts {
    public class EnemyVision : ASystemInitializer {
        private IController<EnemyControllerInput> controller;
        private Transform player;
        private SpriteRenderer rdr;
        private bool trackPlayer = false;
        private float timeSinceSeen = 0f;
        private float timeInSight = 0f;

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player().transform;
        }

        private void Start() {
            Transform entity = Transforms.Body(transform);
            controller = entity.GetComponent<IController<EnemyControllerInput>>();
            rdr = entity.GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate() {
            //Vector3 playerPos = player.position;
            //Vector3 playerEyes =
            //    Transforms.FindInActiveChildren(player, xform => xform.name == "head")
            //        .First()
            //        .position;
            //Vector3 enemyEyes = transform.parent.position;
            //bool los = false;

            //if (Physics.Raycast(
            //        enemyEyes,
            //        playerEyes - enemyEyes,
            //        out RaycastHit losHit,
            //        Vector3.Distance(enemyEyes, player.position),
            //        ~LayerMask.GetMask("Enemies", "Geometry"))) {
            //    if (losHit.transform.IsChildOf(player)) {
            //        los = true;
            //    }
            //}

            //rdr.color = new Color(1, 1, 1, Mathf.Clamp(1 - timeSinceSeeable, 0, 1));
            //controller.On(new PlayerLOS(seesPlayer && los));

            //bool isBlocked = Physics.Raycast(
            //    playerEyes,
            //    enemyEyes - playerEyes,
            //    out RaycastHit blockedHit,
            //    Vector3.Distance(transform.parent.parent.position, playerEyes),
            //    LayerMask.GetMask("Opaque", "InsideOpaque"));

            //if (isBlocked) timeSinceSeeable += Time.fixedDeltaTime;
            //else timeSinceSeeable = -0.25f;
        }

        private void OnTriggerStay(Collider other) {
            if (other.transform.IsChildOf(player)) {
                trackPlayer = true;
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.transform.IsChildOf(player)) {
                trackPlayer = false;
            }
        }
    }
}
