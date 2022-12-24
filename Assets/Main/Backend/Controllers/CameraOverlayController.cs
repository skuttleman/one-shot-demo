using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraOverlayController : ASystemInitializer<ScopingChanged> {
        [SerializeField] private CameraOverlayCfgSO cfg;

        private Transform player;
        private SpriteRenderer rdr;
        private bool isScoping = false;
        private float alpha = 0f;

        protected override void OnEvent(ScopingChanged e) {
            isScoping = e.isScoping;
        }

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player().transform;
        }

        private void Start() {
            rdr = transform.GetComponent<SpriteRenderer>();
        }

        private void LateUpdate() {
            if (cfg != null) {
                float delta = Time.deltaTime / 2;
                if (isScoping) alpha += delta;
                else alpha -= delta;
                alpha = Mathf.Clamp(alpha, -0.1f, cfg.maxOverlayAlpha);

                float overlayAngle = Vectors.AngleTo(player.position, transform.parent.position);
                if (Vectors.NonZero(player.position.Downgrade() - transform.parent.position.Downgrade())) {
                    transform.rotation = Quaternion.Euler(
                        0f,
                        0f,
                        overlayAngle);
                } else {
                    transform.rotation = player.rotation;
                }

                rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, Mathf.Max(0f, alpha));
            }
        }
    }
}
