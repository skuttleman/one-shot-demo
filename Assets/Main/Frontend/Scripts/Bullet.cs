using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Scripts {
    public class Bullet : MonoBehaviour {
        private Rigidbody rb;

        private void OnCollisionEnter(Collision collision) {
            IDamage dmg = collision.gameObject.GetComponent<IDamage>();
            if (dmg != null) dmg.OnAttack(0f);
            Destroy(gameObject);
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            rb = GetComponent<Rigidbody>();
            rb.AddRelativeForce(Vector3.forward * 150f);
            StartCoroutine(Monos.After(5f, () => Destroy(gameObject)));
        }
    }
}
