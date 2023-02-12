using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Pooling;
using UnityEngine;

namespace OSFE.Scripts {
    public class Bullet : MonoBehaviour, IPooled {
        private Rigidbody rb;
        private SpriteRenderer rdr;
        private bool isActive;

        public void Go() {
            rb.velocity = Vector3.zero;
            rb.AddRelativeForce(transform.TransformDirection(Vector3.forward) * 150f);
            isActive = true;
            rdr.enabled = true;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private void OnCollisionEnter(Collision collision) {
            if (isActive) {
                IDamage dmg = collision.gameObject.GetComponent<IDamage>();
                if (dmg != null) dmg.OnAttack(0f);
                isActive = false;
                rdr.enabled = false;
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            rb = GetComponent<Rigidbody>();
            rdr = GetComponent<SpriteRenderer>();

            rdr.enabled = false;
        }
    }
}
