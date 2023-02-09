using OSCore.Data.Enums;
using OSCore.System;
using OSCore.System.Interfaces.Pooling;
using OSCore.System.Pooling;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSFE.Scripts {
    public class PlayerFiring : ASystemInitializer<AttackModeChanged> {
        [SerializeField] private GameObject bulletPrefab;
        private IPool bulletPool;

        protected override void OnEvent(AttackModeChanged e) {
            if (e.mode == AttackMode.FIRING) {
                bulletPool.Instantiate(transform.position, transform.rotation);
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            bulletPool = new SlidingPool(bulletPrefab);
        }
    }
}
