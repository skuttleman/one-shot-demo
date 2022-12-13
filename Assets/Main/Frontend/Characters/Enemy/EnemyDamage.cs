using OSCore;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

public class EnemyDamage : MonoBehaviour, IDamage {
    private IGameSystem system;

    public void OnAttack(float damage) =>
        Brain().OnDamage(damage);

    private void OnEnable() {
        system = FindObjectOfType<GameController>();
    }

    private IEnemyStateReducer Brain() =>
        system.Send<IControllerManager, IEnemyStateReducer>(mngr =>
            mngr.Ensure<IEnemyStateReducer>(transform));
}
