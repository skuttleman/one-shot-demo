using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Pooling;
using UnityEngine;

namespace OSFE.Scripts {
    public class Bullet : MonoBehaviour, IPooled {
        private Rigidbody rb;
        private bool isActive;

        public void Go() {
            rb.velocity = Vector3.zero;
            rb.AddRelativeForce(Vector3.forward * 150f);
            isActive = true;
        }

        private void OnCollisionEnter(Collision collision) {
            if (isActive) {
                IDamage dmg = collision.gameObject.GetComponent<IDamage>();
                if (dmg != null) dmg.OnAttack(0f);
                isActive = false;
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            rb = GetComponent<Rigidbody>();
        }
    }
}
