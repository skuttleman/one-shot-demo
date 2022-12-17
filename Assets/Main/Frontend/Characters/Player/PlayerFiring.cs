using OSCore.Data.Enums;
using OSCore.System;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSFE.Characters.Player {
    public class PlayerFiring : ASystemInitializer<AttackModeChanged> {
        [SerializeField] private GameObject bulletPrefab;

        protected override void OnEvent(AttackModeChanged e) {
            if (e.mode == AttackMode.FIRING)
                Instantiate(bulletPrefab, transform.position, transform.rotation);
        }
    }
}
