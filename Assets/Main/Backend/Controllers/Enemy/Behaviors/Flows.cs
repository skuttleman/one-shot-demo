using System.Collections.Generic;
using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Flows {
    public class TransformPatrol : AStateNode<EnemyAIStateDetails> {
        private AStateNode<EnemyAIStateDetails> child;

        public TransformPatrol(Transform transform) : base(transform) { }

        public override StateNodeStatus Process(EnemyAIStateDetails details) {
            return child.Process(details);
        }

        public override void Init() {
            List<AStateNode<EnemyAIStateDetails>> nodes = new();

            Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .ForEach(xform => {
                    float waitTime = xform.localScale.y;
                    float rotation = xform.rotation.eulerAngles.y;
                    Vector3 direction = Vectors.ToVector3(xform.rotation.eulerAngles.y);

                    nodes.Add(new BNodeGoto(transform, xform.position));

                    if (waitTime > 0) {
                        nodes.Add(new BNodeLookAtDirection(transform, direction));
                        nodes.Add(new BNodeWait(transform, waitTime));
                    }
                });

            child = new BNodeRepeat(transform, new BNodeAnd(transform, nodes.ToArray()));
            child.Init();
        }
    }

    public class EnemyCurious : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> tree;

        public EnemyCurious(Transform transform) : base(transform) {
            tree = new BNodeDoFor(
                transform,
                new BNodeRepeat(transform, new BNodeLookAtLKL(transform)),
                6f);
        }

        public override StateNodeStatus Process(EnemyAIStateDetails details) {
            StateNodeStatus status = tree.Process(details);

            if (status == StateNodeStatus.SUCCESS) {
                tree.Init();
                return StateNodeStatus.RUNNING;
            }

            return status;
        }

        public override void Init() {
            tree.Init();
        }
    }
}