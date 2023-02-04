using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites.Experimental;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Flows {
    public static class EnemyBehaviors {
        public static IBehaviorNodeFactory<EnemyAIStateDetails> TransformPatrol(Transform transform) {
            List<IBehaviorNodeFactory<EnemyAIStateDetails>> children = new();

            Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .ForEach(xform => {
                    float waitTime = xform.localScale.y;
                    float rotation = xform.rotation.eulerAngles.y;
                    Vector3 direction = Vectors.ToVector3(xform.rotation.eulerAngles.y);

                    List<IBehaviorNodeFactory<EnemyAIStateDetails>> group = new();

                    group.Add(BNodeGotoLocation.Of(xform.position));

                    if (waitTime > 0) {
                        group.Add(BNodeLookAt.Of((transform, _) => transform.position + direction));
                        group.Add(BNodeWait<EnemyAIStateDetails>.Of(waitTime));
                    }
                    children.Add(BNodeAnd<EnemyAIStateDetails>.Of(group.ToArray()));
                });

            return BNodeRepeat<EnemyAIStateDetails>.Of(
                BNodeStepLockedAnd<EnemyAIStateDetails>.Of(children.ToArray()));
        }

        public static IBehaviorNodeFactory<EnemyAIStateDetails> Curious() =>
            BNodeParallelAll<EnemyAIStateDetails>.Of(
                BNodeSpeak.Of("..."),
                BNodeRepeat<EnemyAIStateDetails>.Of(
                    BNodeLookAt.LKP()));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> FollowLKP(float minDist) =>
            BNodeDoWhile<EnemyAIStateDetails>.Of(
                (transform, details) =>
                    Vector3.Distance(details.lastKnownPosition, transform.position) > minDist,
                BNodeGoto.Of((_, details) => details.lastKnownPosition));


        public static IBehaviorNodeFactory<EnemyAIStateDetails> Investigate() =>
            BNodeAnd<EnemyAIStateDetails>.Of(
                BNodeParallelAny<EnemyAIStateDetails>.Of(
                    BNodeSpeak.Of("???"),
                    BNodeRepeat<EnemyAIStateDetails>.Of(
                        BNodeLookAt.LKP())),
                FollowLKP(0.5f));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> ReturnToPassive() =>
            BNodeAnd<EnemyAIStateDetails>.Of(
                LookAround(),
                GiveUp());

        public static IBehaviorNodeFactory<EnemyAIStateDetails> ReturnToAlert() =>
            BNodeAnd<EnemyAIStateDetails>.Of(
                LookAround(),
                LookAround(),
                BNodeSpeak.Of("---"));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> LookAround() {
            return BNodeAnd<EnemyAIStateDetails>.Of(
                BNodeWait<EnemyAIStateDetails>.Of(1f),
                Turn(80f),
                BNodeWait<EnemyAIStateDetails>.Of(1.25f),
                Turn(-160f),
                BNodeWait<EnemyAIStateDetails>.Of(2f),
                Turn(80f),
                BNodeWait<EnemyAIStateDetails>.Of(1f));
        }

        public static IBehaviorNodeFactory<EnemyAIStateDetails> ScanAround() =>
            BNodeAnd<EnemyAIStateDetails>.Of(
                BNodeWait<EnemyAIStateDetails>.Of(0.75f),
                Turn(-80f),
                BNodeWait<EnemyAIStateDetails>.Of(0.75f),
                Turn(160f),
                BNodeWait<EnemyAIStateDetails>.Of(0.75f));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> Turn(float angle) =>
            BNodeLookAt.Of(
                (transform, _) => transform.position
                    + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);

        /*
         * TEMPORARY
         */

        public static IBehaviorNodeFactory<EnemyAIStateDetails> Harrass() =>
            BNodeParallelAll<EnemyAIStateDetails>.Of(
                BNodeRepeat<EnemyAIStateDetails>.Of(
                    BNodeAnd<EnemyAIStateDetails>.Of(
                        BNodeSpeak.Of("!!!"),
                        BNodeWait<EnemyAIStateDetails>.Of(2f))),
                BNodeRepeat<EnemyAIStateDetails>.Of(
                    BNodeAnd<EnemyAIStateDetails>.Of(
                        BNodeDoWhile<EnemyAIStateDetails>.Of(
                            (transform, details) =>
                                Vector3.Distance(
                                    details.lastKnownPosition,
                                    transform.position
                                ) >= 2f,
                            BNodeOptional<EnemyAIStateDetails>.Of(FollowLKP(1.5f))),
                        BNodeDoWhile<EnemyAIStateDetails>.Of(
                            (transform, details) =>
                                Vector3.Distance(
                                    details.lastKnownPosition,
                                    transform.position
                                ) < 2f,
                            BNodeLookAt.LKP()))));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> GiveUp() =>
                BNodeSpeak.Of("¯\\_(  )_/¯");

        public static IBehaviorNodeFactory<EnemyAIStateDetails> SearchHalfHeartedly() =>
            BNodeParallelAll<EnemyAIStateDetails>.Of(
                BNodeRepeat<EnemyAIStateDetails>.Of(
                    BNodeAnd<EnemyAIStateDetails>.Of(
                        BNodeSpeak.Of("***"),
                        BNodeWait<EnemyAIStateDetails>.Of(7.5f))),
                BNodeRepeat<EnemyAIStateDetails>.Of(
                    BNodeOptional<EnemyAIStateDetails>.Of(
                        BNodeAnd<EnemyAIStateDetails>.Of(
                            BNodeGoto.Of(
                                (_, details) => Vectors.RandomPointWithinDistance(
                                    details.lastKnownPosition,
                                    new(2f, 0f, 2f),
                                    -1)),
                            ScanAround()))));

        public class Noop<T> : ABehaviorNode<T> {
            public static readonly Noop<T> noop = new();

            private Noop() : base(default) { }

            protected override void Continue(T details) {
                status = StateNodeStatus.SUCCESS;
            }
        }
    }
}
