using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraOverlayController : ICameraOverlayController {
        readonly IGameSystem system;
        readonly Transform target;
        Transform player;
        CameraOverlayCfgSO cfg = null;
        SpriteRenderer rdr;
        bool isScoping = false;
        float alpha = 0f;

        public CameraOverlayController(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;

            system.Send<IPubSub>(pubsub => {
                pubsub.Subscribe<ScopingChanged>(ScopeChanged);
            });
            player = system.Send<ITagRegistry, GameObject>(registry =>
                registry.GetUnique(IdTag.PLAYER)).transform;
        }

        public void OnUpdate() {
            if (cfg != null) {
                if (isScoping) alpha += Time.deltaTime;
                else alpha -= Time.deltaTime;

                alpha = Mathf.Clamp(alpha, 0f, cfg.maxOverlayAlpha);
                rdr.color = new Color(rdr.color.r, rdr.color.g, rdr.color.b, alpha);
                if (isScoping)
                    target.rotation = Quaternion.Euler(
                        0f,
                        0f,
                        Vectors.AngleTo(player.position, target.parent.position));
            }
        }

        public void Init(CameraOverlayCfgSO cfg) {
            this.cfg = cfg;
            rdr = target.GetComponent<SpriteRenderer>();
        }

        void ScopeChanged(ScopingChanged ev) =>
            isScoping = ev.isScoping;
    }
}
