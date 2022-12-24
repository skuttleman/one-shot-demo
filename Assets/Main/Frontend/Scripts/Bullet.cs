using OSCore.Utils;
using UnityEngine;

namespace OSFE.Scripts {
    public class Bullet : MonoBehaviour {
        private Rigidbody rb;

        private void OnCollisionEnter(Collision collision) {
            EnemyDamage dmg = collision.gameObject.GetComponent<EnemyDamage>();
            if (dmg != null) dmg.OnAttack(0f);
            Destroy(gameObject);
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            rb = GetComponent<Rigidbody>();
            rb.AddRelativeForce(Vectors.FORWARD * 150f);
            StartCoroutine(Monos.After(5f, () => Destroy(gameObject)));
        }
    }
}
