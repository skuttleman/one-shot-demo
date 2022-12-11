using OSCore.Data;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

namespace OSBE.Controllers {
    public class EnemyStateReducer : IEnemyStateReducer {
        IGameSystem system;
        Transform target;

        IStateReceiver<EnemyState> receiver = null;
        EnemyCfgSO cfg = null;
        EnemyState state = null;

        public EnemyStateReducer(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
            state = new EnemyState {
                isPlayerInView = false
            };
        }

        public void Init(IStateReceiver<EnemyState> receiver, EnemyCfgSO cfg) {
            this.receiver = receiver;
            this.cfg = cfg;
            receiver.OnStateChange(state);
        }

        public void OnAttackModeChanged(AttackMode attackMode) { }

        public void OnEnemyStep() { }

        public void OnMovementChanged(bool isMoving) { }

        public void OnPlayerSightChange(bool isInView) =>
            EmitState(state with { isPlayerInView = isInView });

        public void EmitState(EnemyState state) {
            if (this.state != state) {
                this.state = state;
                receiver.OnStateChange(state);
            }
        }
    }
}