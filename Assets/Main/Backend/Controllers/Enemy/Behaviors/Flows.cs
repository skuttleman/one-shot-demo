using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites.Experimental;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Flows {
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
                new BNodeStepLockedAnd<EnemyAIStateDetails>(transform, nodes.ToArray()));
        }

        public static AStateNode<EnemyAIStateDetails> Curious(Transform transform) =>
            new BNodeParallelAll<EnemyAIStateDetails>(
                transform,
                new BNodeSpeak(transform, "..."),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeLookAtLKP(transform)));

        public static AStateNode<EnemyAIStateDetails> FollowLKP(
            Transform transform, float minDist) =>
            new BNodeDoWhile<EnemyAIStateDetails>(
                    transform,
                    details => Vector3.Distance(details.lastKnownPosition, transform.position) > minDist,
                    new BNodeGoto(
                        transform,
                        details => details.lastKnownPosition));


        public static AStateNode<EnemyAIStateDetails> Investigate(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeParallelAny<EnemyAIStateDetails>(
                    transform,
                    new BNodeSpeak(transform, "???"),
                    new BNodeRepeat<EnemyAIStateDetails>(
                        transform,
                        new BNodeLookAtLKP(transform))),
                FollowLKP(transform, 0.5f));

        public static AStateNode<EnemyAIStateDetails> ReturnToPassive(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                LookAround(transform),
                GiveUp(transform));

        public static AStateNode<EnemyAIStateDetails> LookAround(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeWait<EnemyAIStateDetails>(transform, 0.5f),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(5f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 0.75f),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(-5f, 0, 1f))),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(-5f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 1.5f),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(5f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 1f));

        public static AStateNode<EnemyAIStateDetails> ScanAround(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeWait<EnemyAIStateDetails>(transform, 0.25f),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(5f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 0.25f),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(-5f, 0, 1f))),
                new BNodeLookAt(transform, _ => transform.position
                    + transform.TransformDirection(new Vector3(-5f, 0, 1f))),
                new BNodeWait<EnemyAIStateDetails>(transform, 0.25f));

        /*
         * TEMPORARY
         */

        public static AStateNode<EnemyAIStateDetails> Harrass(Transform transform) =>
            new BNodeParallelAll<EnemyAIStateDetails>(
                transform,
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeAnd<EnemyAIStateDetails>(
                        transform,
                        new BNodeSpeak(transform, "!!!"),
                        new BNodeWait<EnemyAIStateDetails>(transform, 2f))),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeAnd<EnemyAIStateDetails>(
                        transform,
                        new BNodeDoWhile<EnemyAIStateDetails>(
                            transform,
                            details => Vector3.Distance(details.lastKnownPosition, transform.position) >= 2f,
                            FollowLKP(transform, 1.5f)),
                        new BNodeDoWhile<EnemyAIStateDetails>(
                            transform,
                            details => Vector3.Distance(details.lastKnownPosition, transform.position) < 2f,
                            new BNodeLookAtLKP(transform)))));

        public static AStateNode<EnemyAIStateDetails> GiveUp(Transform transform) =>
                new BNodeSpeak(transform, "¯\\_(  )_/¯");

        public static AStateNode<EnemyAIStateDetails> SearchHalfHeartedly(Transform transform) =>
            new BNodeParallelAll<EnemyAIStateDetails>(
                transform,
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeAnd<EnemyAIStateDetails>(
                        transform,
                        new BNodeSpeak(transform, "***"),
                        new BNodeWait<EnemyAIStateDetails>(transform, 7.5f))),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeOptional<EnemyAIStateDetails>(
                        transform,
                        new BNodeAnd<EnemyAIStateDetails>(
                            transform,
                            new BNodeGotoStatic(
                                transform,
                                details => Vectors.RandomPointWithinDistance(
                                    details.lastKnownPosition,
                                    new(2f, 0f, 2f),
                                    -1)),
                            ScanAround(transform)))));
    }
}
