using OSCore.Data.Controllers;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSFE.Scripts {
    public class EnemyVision : ASystemInitializer {
        [SerializeField] private EnemyAICfgSO cfg;

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

        private void Update() {
            Visibility();
            rdr.color = new Color(1, 1, 1, Mathf.Clamp(0.75f - timeSinceSeeable, 0.25f, 1));
        }

        private void Visibility() {
            Vector3 eyes = transform.position;
            Vector3 playerEyes = Transforms
                .FindInActiveChildren(player, xform => xform.name == "head")
                .First()
                .position;
            controller.Handle(BuildLOS(eyes, playerEyes));

            if (Transforms.VisibilityFrom(playerEyes, coll) > 0f) timeSinceSeeable = 0f;
            else timeSinceSeeable += Time.fixedDeltaTime;
        }

        private PlayerLOS BuildLOS(Vector3 eyes, Vector3 playerEyes) {
            float angle2Player = Vector3.Angle(player.position - transform.position, transform.forward);
            float distance = Vector3.Distance(eyes, playerEyes);

            CapsuleCollider playerColl = player.GetComponentInChildren<CapsuleCollider>();

            return new PlayerLOS(
                Transforms.VisibilityFrom(eyes, playerColl),
                distance,
                angle2Player,
                player.position);
        }
    }
}
