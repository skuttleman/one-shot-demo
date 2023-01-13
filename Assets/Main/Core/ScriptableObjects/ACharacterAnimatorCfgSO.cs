using System.Collections.Generic;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    public abstract class ACharacterAnimatorCfgSO<State, Details> : ScriptableObject
        where Details : AnimStateDetails<State> {
        [field: SerializeField] public RuntimeAnimatorController animator { get; private set; }

        public readonly IEnumerable<State> states;

        public abstract AnimNode<State, Details> Init();
    }
}
