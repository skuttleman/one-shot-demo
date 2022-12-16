using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Tagging;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraOverlayController : ASystemInitializer<ScopingChanged> {
        [SerializeField] private CameraOverlayCfgSO cfg;

        private Transform player;
        private SpriteRenderer rdr;
        private bool isScoping = false;
        private float alpha = 0f;

        protected override void OnEvent(ScopingChanged e) =>
            isScoping = e.isScoping;

        private void Start() {
            player = system.Send<ITagRegistry, GameObject>(registry =>
                registry.GetUnique(IdTag.PLAYER)).transform;
            rdr = transform.GetComponent<SpriteRenderer>();
        }

        private void Update() {
            if (cfg != null) {
                if (isScoping) alpha += Time.deltaTime;
                else alpha -= Time.deltaTime;

                alpha = Mathf.Clamp(alpha, 0f, cfg.maxOverlayAlpha);
                rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, alpha);
                if (isScoping)
                    transform.rotation = Quaternion.Euler(
                        0f,
                        0f,
                        Vectors.AngleTo(player.position, transform.parent.position));
            }
        }
    }
}
