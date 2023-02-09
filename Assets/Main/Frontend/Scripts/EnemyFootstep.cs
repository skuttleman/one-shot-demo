using OSCore.System.Interfaces.Pooling;
using UnityEngine;

namespace OSFE.Scripts {
    public class EnemyFootstep : MonoBehaviour, IPooled {
        private SpriteRenderer rdr;
        private float existence;
        private bool isActive;

        public void Go() {
            transform.localScale = new(0, 0, 0);
            Invisible();
            existence = 1f;
            isActive = true;
        }

        private void Invisible() {
            rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, 0f);
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            rdr = GetComponent<SpriteRenderer>();
            Invisible();
        }

        private void Update() {
            if (isActive) {
                existence -= Time.deltaTime * 1.5f;

                if (existence <= 0) {
                    Invisible();
                    isActive = false;
                } else {
                    float size = (1 - existence) * 0.5f;
                    transform.localScale = new(size, size, size);
                    rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, existence);
                }
            }
        }
    }
}
