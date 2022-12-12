using System.Collections.Generic;
using OSCore.Utils;
using UnityEngine;

public class Bullet : MonoBehaviour {
    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vectors.FORWARD * 150f);
        StartCoroutine(DieAfter(5f));
    }

    IEnumerator<YieldInstruction> DieAfter(float seconds) {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision) {
        EnemyDamage dmg = collision.gameObject.GetComponent<EnemyDamage>();
        if (dmg != null) dmg.OnAttack(0f);
        Destroy(gameObject);
    }
}
