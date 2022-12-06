using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;
using OSCore.System.Interfaces.Brains.Patrol;
using System.Collections.Generic;

public class EnemyConnector : MonoBehaviour {
    [SerializeField] EnemyCfgSO cfg;

    IGameSystem system;
    IEnumerable<EnemyPatrol> patrol;


    void OnEnable() {
        system = FindObjectOfType<GameController>();
        patrol = Brain().Init(cfg);
    }

    void Start() {
        StartCoroutine(DoPatrol());
    }

    IEnumerator<YieldInstruction> DoPatrol() {
        foreach (EnemyPatrol step in patrol) {
            foreach (float wait in Brain().DoPatrolStep(step)) {
                if (wait > 0) yield return new WaitForSeconds(wait);
                else yield return new WaitForFixedUpdate();
            }
        }
    }

    IEnemyController Brain() =>
        system.Send<IControllerManager, IEnemyController>(mngr =>
            mngr.Ensure<IEnemyController>(transform));
}
