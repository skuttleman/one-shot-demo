using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

namespace OSFE.Scripts {
    public class CeilingAlpha : ASystemInitializer {
        private IEnumerable<Tilemap> maps;
        private GameObject player;
        private Transform fov;

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player();
            fov = Transforms.FindInChildren(player.transform.parent, child => child.name == "fov")
                .First();
        }

        private void Start() {
            maps = Sequences.Transduce(
                transform.parent.parent.GetComponentsInChildren<Tilemap>(),
                Fns.Filter<Tilemap>(map => !map.transform.name.Contains("seethrough")));
        }

        private void OnTriggerStay(Collider other) {
            if (other.transform.IsChildOf(player.transform)) {
                foreach (Tilemap map in maps) {
                    float diff = Mathf.Abs(
                        map.transform.position.z
                        - player.transform.position.z);
                    float alpha = map.transform.position.z >= player.transform.position.z
                        ? 1f
                        : Mathf.Clamp(1f - (diff / 1.05f), 0f, 1f);
                    map.color = new(map.color.r, map.color.g, map.color.b, alpha);
                }
                fov.gameObject.SetActive(false);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.transform.IsChildOf(player.transform)) {
                foreach (Tilemap map in maps)
                    map.color = new(map.color.r, map.color.g, map.color.b, 1f);
                fov.gameObject.SetActive(true);
            }
        }
    }
}
