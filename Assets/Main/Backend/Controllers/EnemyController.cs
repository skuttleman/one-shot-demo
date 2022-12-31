using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Patrol;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine.AI;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.Data.Patrol.EnemyPatrol;

namespace OSBE.Controllers {
    public class EnemyController : ASystemInitializer<MovementChanged>, IController<EnemyControllerInput>, IStateReceiver<EnemyAnim> {
        [SerializeField] private EnemyCfgSO cfg;
        [SerializeField] GameObject footstep;
        private static readonly float SEEN_THRESHOLD = 5f;

        private GameObject player;
        private EnemyAnimator anim;
        private TextMeshPro speech;
        private NavMeshAgent nav;
        private float timeSinceSeenPlayer = 0f;
        private float timeSincePlayerMoved = 0f;
        private bool isPlayerMoving = false;
        private EnemyState state = null;

        public IEnumerable<float> DoPatrolStep(EnemyPatrol step) {
            IEnumerable<float> waits = step switch {
                PatrolWait msg => Wait(msg.seconds),
                PatrolFace msg => Face(msg.rotation),
                PatrolGoto msg => Goto(msg.position),
                PatrolRotate msg => Face(Translate(msg.rotation)),
                _ => new float[] { }
            };

            foreach (float wait in waits)
                yield return wait;
        }

        public void On(EnemyControllerInput e) {
            switch (e) {
                case DamageInput:
                    speech.text = "OUCH!";
                    state = state with { isPlayerInView = true };
                    timeSinceSeenPlayer = 0f;
                    break;
                case PlayerLOS ev:
                    state = state with { isPlayerInView = ev.isInView };
                    if (ev.isInView) timeSinceSeenPlayer = 0f;
                    break;
            }
        }

        public void OnStep() {
            if (timeSincePlayerMoved > 0.5f)
                Instantiate(footstep, transform.position, Quaternion.identity);
        }

        protected override void OnEvent(MovementChanged e) {
            isPlayerMoving = Maths.NonZero(e.speed);
        }

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player();
        }

        private void Start() {
            anim = GetComponentInChildren<EnemyAnimator>();
            nav = GetComponent<NavMeshAgent>();
            nav.updateRotation = false;

            speech = Transforms.Body(transform)
                .parent
                .gameObject
                .GetComponentInChildren<TextMeshPro>();
            speech.text = "";

            state = new() {
                isPlayerInView = false
            };

            StartCoroutine(DoPatrol());
            StartCoroutine(SpeakUp());
        }

        private void FixedUpdate() {
            speech.transform.position = transform.position + new Vector3(0f, 0f, 0.75f);
            if (!state.isPlayerInView) timeSinceSeenPlayer += Time.fixedDeltaTime;
            if (isPlayerMoving) timeSincePlayerMoved = 0f;
            else timeSincePlayerMoved += Time.fixedDeltaTime;
        }

        private IEnumerator<YieldInstruction> SpeakUp() {
            while (!state.isPlayerInView)
                yield return new WaitForSeconds(0.1f);
            while (true) {
                if (state.isPlayerInView || timeSinceSeenPlayer <= SEEN_THRESHOLD) {
                    speech.text = "I see you, Bro.";
                    speech.enabled = true;
                    yield return new WaitForSeconds(2f);
                    speech.enabled = false;
                    yield return new WaitForSeconds(1f);
                    if (!state.isPlayerInView && timeSinceSeenPlayer > SEEN_THRESHOLD) {
                        speech.text = "Where'd they go?";
                        speech.enabled = true;
                        float elapsed = 0f;
                        while (!state.isPlayerInView && elapsed < 2f) {
                            elapsed += Time.fixedDeltaTime;
                            yield return new WaitForFixedUpdate();
                        }
                        speech.enabled = false;
                    }
                    speech.text = "";
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator<YieldInstruction> DoPatrol() {
            foreach (EnemyPatrol step in Patrol()) {
                foreach (float wait in DoPatrolStep(step)) {
                    float waitAmount = wait;
                    while (waitAmount > 0f) {
                        waitAmount -= Time.fixedDeltaTime;
                        yield return new WaitForFixedUpdate();
                        while (state.isPlayerInView || timeSinceSeenPlayer <= SEEN_THRESHOLD) {
                            anim.Transition(state => state with { isMoving = false });
                            DoFace(player.transform.position);
                            yield return new WaitForFixedUpdate();
                        }
                    }
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        private IEnumerable<EnemyPatrol> Patrol() {
            return Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .Reduce((tpl, xform) => {
                    float waitTime = xform.localScale.z;
                    bool notFirstStep = tpl.transform != xform;

                    return (
                        xform,
                        tpl.Item2.Concat(Sequences.Empty<EnemyPatrol>()
                            .ConsIf(waitTime > 0, new PatrolWait(waitTime))
                            .ConsIf(waitTime > 0, new PatrolRotate(xform.rotation.eulerAngles.y))
                            .ConsIf(notFirstStep, new PatrolGoto(xform.position))));
                },
                (transform, Sequences.Empty<EnemyPatrol>()))
                .Item2
                .Cycle();
        }

        private IEnumerable<float> Wait(float seconds) {
            yield return seconds;
        }

        private IEnumerable<float> Face(Vector3 rotation) {
            float diff;
            do {
                diff = Mathf.Abs(transform.rotation.eulerAngles.y - Quaternion.LookRotation(rotation - transform.position).eulerAngles.y);
                diff = diff > 180f ? 360f - diff : diff;
                DoFace(rotation);
                yield return 0f;
            } while (diff > cfg.rotationSpeed * Time.fixedDeltaTime);
        }

        private void DoFace(Vector3 position) {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(position - transform.position),
                cfg.rotationSpeed * Time.fixedDeltaTime);
        }

        private Vector3 Translate(float rotation) {
            return transform.position + Vectors.ToVector3(rotation);
        }

        private IEnumerable<float> Goto(Vector3 position) {
            float diff = 0;
            do {
                diff = Mathf.Abs(transform.rotation.eulerAngles.y - Quaternion.LookRotation(position - transform.position).eulerAngles.y);
                diff = diff > 180f ? 360f - diff : diff;
                DoFace(position);
                yield return 0f;
            } while (diff > 60f);

            nav.SetDestination(position);
            anim.Transition(state => state with { isMoving = true });
            float timeout = 0f;

            while (timeout < 0.1f || nav.remainingDistance > nav.stoppingDistance) {
                DoFace(position);
                yield return 0f;
                timeout += Time.fixedDeltaTime;
            }
            anim.Transition(state => state with { isMoving = false });
        }
    }
}
