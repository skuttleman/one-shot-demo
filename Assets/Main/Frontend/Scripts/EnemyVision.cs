using OSCore.Data.Controllers;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSFE.Scripts {
    public class EnemyVision : ASystemInitializer {
        [SerializeField] private EnemyCfgSO cfg;

        private IController<EnemyControllerInput> controller;
        private Transform player;
        private SpriteRenderer rdr;
        private CapsuleCollider coll;
        private float timeSinceSeeable = 0f;

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player().transform;
        }

        private void Start() {
            Transform entity = Transforms.Body(transform);
            controller = entity.GetComponent<IController<EnemyControllerInput>>();
            rdr = entity.GetComponentInChildren<SpriteRenderer>();
            coll = entity.GetComponentInChildren<CapsuleCollider>();
        }

        private void FixedUpdate() {
            Visibility();
            rdr.color = new Color(1, 1, 1, Mathf.Clamp(0.75f - timeSinceSeeable, 0, 1));
        }

        private void Visibility() {
            Vector3 eyes = transform.position;
            Vector3 playerEyes = Transforms
                .FindInActiveChildren(player, xform => xform.name == "head")
                .First()
                .position;
            controller.On(BuildLOS(eyes, playerEyes));

            float enemyVisibility = Transforms.VisibilityFrom(playerEyes, coll);
            if (enemyVisibility > 0f) timeSinceSeeable = 0f;
            else timeSinceSeeable += Time.fixedDeltaTime;
        }

        private PlayerLOS BuildLOS(Vector3 eyes, Vector3 playerEyes) {
            float rotation = transform.parent.rotation.eulerAngles.y;
            float angle2Player = Vectors.AngleTo(transform.position - player.position);

            float periphery = Maths.AngleDiff(rotation, angle2Player) / cfg.fovAngle;
            float distance = Vector3.Distance(eyes, playerEyes);

            if (periphery <= 1f && distance <= cfg.fovDistance) {
                CapsuleCollider playerColl = player.GetComponentInChildren<CapsuleCollider>();
                float visibility = Transforms.VisibilityFrom(eyes, playerColl);

                return new PlayerLOS(visibility, distance, 1f - Mathf.Clamp(periphery, 0f, 1f));
            }
            return new PlayerLOS(0f, distance, 0f);
        }
    }
}
