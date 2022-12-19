using UnityEngine;

namespace OSFE.Scripts {
    public class EnemyFootstep : MonoBehaviour {
        private SpriteRenderer rdr;
        private float existence = 1f;

        void Start() {
            rdr = GetComponent<SpriteRenderer>();
            transform.localScale = new(0, 0, 0);
            rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, 0f);
        }

        void Update() {
            existence -= Time.deltaTime * 1.5f;
            if (existence <= 0) {
                Destroy(gameObject);
            } else {
                float size = (1 - existence) * 0.5f;
                transform.localScale = new(size, size, size);
                rdr.color = new(rdr.color.r, rdr.color.g, rdr.color.b, existence);
            }
        }
    }
}
