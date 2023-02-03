using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites.Experimental;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Flows {
    public static class EnemyBehaviors {
        public static ABehaviorNode<EnemyAIStateDetails> TransformPatrol(Transform transform) {
            List<ABehaviorNode<EnemyAIStateDetails>> nodes = new();

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

        public static ABehaviorNode<EnemyAIStateDetails> Curious(Transform transform) =>
            new BNodeParallelAll<EnemyAIStateDetails>(
                transform,
                new BNodeSpeak(transform, "..."),
                new BNodeRepeat<EnemyAIStateDetails>(
                    transform,
                    new BNodeLookAtLKP(transform)));

        public static ABehaviorNode<EnemyAIStateDetails> FollowLKP(
            Transform transform, float minDist) =>
            new BNodeDoWhile<EnemyAIStateDetails>(
                    transform,
                    details => Vector3.Distance(details.lastKnownPosition, transform.position) > minDist,
                    new BNodeGoto(
                        transform,
                        details => details.lastKnownPosition));


        public static ABehaviorNode<EnemyAIStateDetails> Investigate(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                new BNodeParallelAny<EnemyAIStateDetails>(
                    transform,
                    new BNodeSpeak(transform, "???"),
                    new BNodeRepeat<EnemyAIStateDetails>(
                        transform,
                        new BNodeLookAtLKP(transform))),
                FollowLKP(transform, 0.5f));

        public static ABehaviorNode<EnemyAIStateDetails> ReturnToPassive(Transform transform) =>
            new BNodeAnd<EnemyAIStateDetails>(
                transform,
                LookAround(transform),
                GiveUp(transform));

        public static ABehaviorNode<EnemyAIStateDetails> LookAround(Transform transform) =>
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

        public static ABehaviorNode<EnemyAIStateDetails> ScanAround(Transform transform) =>
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

        public static ABehaviorNode<EnemyAIStateDetails> Harrass(Transform transform) =>
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

        public static ABehaviorNode<EnemyAIStateDetails> GiveUp(Transform transform) =>
                new BNodeSpeak(transform, "¯\\_(  )_/¯");

        public static ABehaviorNode<EnemyAIStateDetails> SearchHalfHeartedly(Transform transform) =>
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
