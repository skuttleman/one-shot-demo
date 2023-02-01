using System.Collections.Generic;
using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Main {
    public static class EnemyBehaviors {
        public static AStateNode<EnemyAIStateDetails> TransformPatrol(Transform transform) {
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

            return new BNodeRepeat<EnemyAIStateDetails>(
                transform,
                new BNodeAnd<EnemyAIStateDetails>(transform, nodes.ToArray()));
        }

        public static AStateNode<EnemyAIStateDetails> Curious(Transform transform) =>
            new BNodeParallel<EnemyAIStateDetails>(
                transform,
                new BNodeSpeak(transform, "..."),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeLookAtLKP(transform)));

        public static AStateNode<EnemyAIStateDetails> Investigate(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
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

        public static AStateNode<EnemyAIStateDetails> ReturnToPassive(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                LookAround(transform),
                new BNodeSpeak(transform, "¯\\_( )_/¯"));

        public static AStateNode<EnemyAIStateDetails> LookAround(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeWait<EnemyAIStateDetails>(transform, 0.5f),
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
}