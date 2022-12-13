using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraOverlayController : MonoBehaviour {
        [SerializeField] private CameraOverlayCfgSO cfg;

        private IGameSystem system;
        private Transform player;
        private SpriteRenderer rdr;
        private bool isScoping = false;
        private float alpha = 0f;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            system.Send<IPubSub>(pubsub => {
                pubsub.Subscribe<ScopingChanged>(ScopeChanged);
            });
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

        private void ScopeChanged(ScopingChanged ev) =>
            isScoping = ev.isScoping;
    }
}
