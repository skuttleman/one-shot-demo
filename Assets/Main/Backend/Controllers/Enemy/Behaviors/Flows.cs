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

        protected override void Process(EnemyAIStateDetails details) {
            Process(child, details);
            status = child.status;
        }

        public override void Init() {
            List<AStateNode<EnemyAIStateDetails>> nodes = new();

            Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .ForEach(xform => {
                    float waitTime = xform.localScale.y;
                    float rotation = xform.rotation.eulerAngles.y;
                    Vector3 direction = Vectors.ToVector3(xform.rotation.eulerAngles.y);

                    nodes.Add(new BNodeGotoLocation(transform, xform.position));

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
            tree = new BNodeParallel(
                transform,
                new BNodeSpeak(transform, "???"),
                new BNodeRepeat(transform, new BNodeLookAtLKP(transform)));
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(tree, details);
            status = tree.status;
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            tree.Init();
        }
    }

    public class EnemyInvestigating : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> tree;

        public EnemyInvestigating(Transform transform) : base(transform) {
            tree = new BNodeRepeat(
                transform,
                new BNodeParallel(
                    transform,
                    new BNodeSpeak(transform, "I'm investigating now"),
                    new BNodeGoto(transform, details => details.lastKnownPosition)));
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(tree, details);
            status = tree.status;
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            tree.Init();
        }
    }
}