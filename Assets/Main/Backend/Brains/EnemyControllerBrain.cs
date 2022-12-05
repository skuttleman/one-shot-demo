using System.Collections;
using System.Collections.Generic;
using OSCore.Data.Enums;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

public class EnemyControllerBrain : IEnemyControllerBrain {
    readonly IGameSystem system;
    readonly Transform target;

    public EnemyControllerBrain(IGameSystem system, Transform target) {
        this.system = system;
        this.target = target;
    }

    public void OnAttackModeChanged(AttackMode attackMode) { }

    public void OnEnemyStep() { }

    public void OnMovementChanged(bool isMoving) { }
}
