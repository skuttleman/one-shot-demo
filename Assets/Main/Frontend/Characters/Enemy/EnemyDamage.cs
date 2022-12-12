using OSCore;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

public class EnemyDamage : MonoBehaviour, IDamage {
    IGameSystem system;

    void OnEnable() {
        system = FindObjectOfType<GameController>();
    }

    public void OnAttack(float damage) =>
        Brain().OnDamage(damage);

    IEnemyStateReducer Brain() =>
        system.Send<IControllerManager, IEnemyStateReducer>(mngr =>
            mngr.Ensure<IEnemyStateReducer>(transform));
}
