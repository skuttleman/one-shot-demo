using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers {
    public class CharacterAnimationController : MonoBehaviour {
        [SerializeField] private PlayerAnimationCfgSO cfg;

        private readonly Queue<PlayerAnimSignal> signals;
        private AStateNode<PlayerAnim, PlayerAnimSignal> state = null;
        private IStateReceiver<PlayerAnim> receiver;
        private float timeInState;

        public CharacterAnimationController() {
            signals = new();
        }

        public void Init(IStateReceiver<PlayerAnim> receiver) {
            this.receiver = receiver;
            cfg.Init();
            state = cfg.tree;
            timeInState = 0f;
            receiver.OnStateEnter(state.state);
        }

        public void Send(PlayerAnimSignal signal) =>
            signals.Enqueue(signal);

        private void Update() {
            if (state is null) return;
            timeInState += Time.deltaTime;
            if (timeInState < state.minTime) return;

            if (state != state.next)
                Transition(state.next);
            else if (signals.TryDequeue(out PlayerAnimSignal signal))
                Transition(state.Next(signal));
        }

        private void Transition(AStateNode<PlayerAnim, PlayerAnimSignal> state) {
            if (this.state != state) {
                receiver.OnStateExit(this.state.state);
                this.state = state;
                timeInState = 0f;
                receiver.OnStateEnter(this.state.state);
            }
        }
    }
}