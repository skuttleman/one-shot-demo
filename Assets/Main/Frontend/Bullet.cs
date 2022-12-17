using System.Collections.Generic;
using OSCore.Utils;
using OSFE.Characters.Enemy;
using UnityEngine;

namespace OSFE {
    public class Bullet : MonoBehaviour {
        private Rigidbody rb;

        private void Start() {
            rb = GetComponent<Rigidbody>();
            rb.AddRelativeForce(Vectors.FORWARD * 150f);
            StartCoroutine(Monos.After(5f, () => Destroy(gameObject)));
        }

        void OnCollisionEnter(Collision collision) {
            EnemyDamage dmg = collision.gameObject.GetComponent<EnemyDamage>();
            if (dmg != null) dmg.OnAttack(0f);
            Destroy(gameObject);
        }
    }
}
