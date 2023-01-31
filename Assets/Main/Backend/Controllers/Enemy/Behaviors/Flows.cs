using System;
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

        protected override void Init() {
            if (child == null) {
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
                            nodes.Add(new BNodeWait<EnemyAIStateDetails>(transform, waitTime));
                        }
                    });

                child = new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeAnd<EnemyAIStateDetails>(transform, nodes.ToArray()));
            }
        }

        protected override void ReInit() {
            ReInit(child);
        }
    }

    public class EnemyCurious : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> tree;

        public EnemyCurious(Transform transform) : base(transform) {
            tree = new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeSpeak(transform, "..."),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeLookAtLKP(transform)));
        }

        protected override void ReInit() {
            ReInit(tree);
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(tree, details);
            status = tree.status;
        }
    }

    public class EnemyLookAround : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> tree;

        public EnemyLookAround(Transform transform) : base(transform) {
            tree = new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeLookAt(transform, _ =>
                    transform.position
                        + transform.TransformDirection(new Vector3(3f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 0.75f),
                new BNodeLookAt(transform, _ =>
                    transform.position
                        + transform.TransformDirection(new Vector3(-3f, 0, 1f))),
                new BNodeLookAt(transform, _ =>
                    transform.position
                        + transform.TransformDirection(new Vector3(-3f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 1.5f),
                new BNodeLookAt(transform, _ =>
                    transform.position
                        + transform.TransformDirection(new Vector3(3f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 1f));
        }

        protected override void ReInit() {
            ReInit(tree);
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(tree, details);
            status = tree.status;
        }
    }

    public class EnemyInvestigating : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> tree;

        public EnemyInvestigating(Transform transform) : base(transform) {
            tree = new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeParallelAny<EnemyAIStateDetails>(
                    transform,
                    new BNodeSpeak(transform, "???"),
                    new BNodeRepeat<EnemyAIStateDetails>(
                        transform,
                        new BNodeLookAtLKP(transform))),
                new BNodeDoWhile<EnemyAIStateDetails>(
                    transform,
                    new BNodeGoto(
                        transform,
                        details => details.lastKnownPosition),
                    details => Vector3.Distance(details.lastKnownPosition, transform.position) < 0.5f));
        }

        protected override void ReInit() {
            ReInit(tree);
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(tree, details);
            status = tree.status;
        }
    }
}