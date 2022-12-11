using System.Collections.Generic;
using OSCore;
using OSCore.Data;
using OSCore.Data.Enums;
using OSCore.Data.Patrol;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Tagging;
using OSCore.Utils;
using TMPro;
using UnityEngine;
using static OSCore.Data.Patrol.EnemyPatrol;


namespace OSBE.Controllers {
    public class EnemyController : MonoBehaviour, IStateReceiver<EnemyState> {
        [SerializeField] EnemyCfgSO cfg;
        IGameSystem system;
        GameObject player;
        Animator anim;
        TextMeshPro speech;

        float timeSinceSeenPlayer = 0f;
        EnemyState state;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
            anim = GetComponentInChildren<Animator>();

            system.Send<IControllerManager>(mngr =>
               mngr.Ensure<IEnemyStateReducer>(transform).Init(this, cfg));
            player = system.Send<ITagRegistry, GameObject>(reg => reg.GetUnique(IdTag.PLAYER));
            speech = transform.parent.parent.gameObject.GetComponentInChildren<TextMeshPro>();
            speech.text = "";
        }

        void Start() {
            StartCoroutine(DoPatrol());
            StartCoroutine(SpeakUp());
        }

        void FixedUpdate() {
            speech.transform.position = transform.position + new Vector3(0f, 0.75f, 0f);
            if (!state.isPlayerInView) timeSinceSeenPlayer += Time.fixedDeltaTime;
            else timeSinceSeenPlayer = 0f;
        }

        IEnumerator<YieldInstruction> SpeakUp() {
            while (!state.isPlayerInView)
                yield return new WaitForSeconds(0.1f);
            while (true) {
                if (state.isPlayerInView || timeSinceSeenPlayer <= 1f) {
                    speech.text = "I see you, Bro.";
                    speech.enabled = true;
                    yield return new WaitForSeconds(2f);
                    speech.enabled = false;
                    yield return new WaitForSeconds(1f);
                    speech.text = "";
                    if (!state.isPlayerInView && timeSinceSeenPlayer > 1f) {
                        speech.text = "Where'd they go?";
                        speech.enabled = true;
                        float elapsed = 0f;
                        while (!state.isPlayerInView && elapsed < 2f) {
                            elapsed += Time.fixedDeltaTime;
                            yield return new WaitForFixedUpdate();
                        }
                        speech.enabled = false;
                        speech.text = "";
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        IEnumerator<YieldInstruction> DoPatrol() {
            foreach (EnemyPatrol step in Patrol()) {
                foreach (float wait in DoPatrolStep(step)) {
                    if (wait > 0) yield return new WaitForSeconds(wait);
                    else yield return new WaitForFixedUpdate();

                    while (state.isPlayerInView || timeSinceSeenPlayer <= 1f) {
                        anim.SetBool("isMoving", false);
                        DoFace(player.transform.position);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
        }

        IEnumerable<EnemyPatrol> Patrol() {
            return Transforms
                .FindInChildren(transform.parent, node => node.name.Contains("position"))
                .Reduce((tpl, xform) => {
                    float waitTime = xform.localScale.z;
                    bool notFirstStep = tpl.transform != xform;
                    bool isFarEnough = Vector3.Distance(xform.position, tpl.transform.position) > 0.01f;

                    return (
                        xform,
                        tpl.Item2.Concat(Sequences.Empty<EnemyPatrol>()
                            .ConsIf(waitTime > 0, new PatrolWait(waitTime))
                            .Cons(new PatrolRotate(xform.rotation.eulerAngles.z))
                            .ConsIf(notFirstStep && isFarEnough, new PatrolGoto(xform.position))));
                },
                (transform, Sequences.Empty<EnemyPatrol>()))
                .Item2
                .Cycle();
        }

        public IEnumerable<float> DoPatrolStep(EnemyPatrol step) =>
            step switch {
                PatrolWait msg => Wait(msg.seconds),
                PatrolFace msg => Face(msg.rotation),
                PatrolRotate msg => Face(msg.rotation),
                PatrolGoto msg => Goto(msg.position),
                _ => new float[] { }
            };

        public void OnMovementChanged(bool isMoving) { }
        public void OnAttackModeChanged(AttackMode attackMode) { }
        public void OnEnemyStep() { }

        IEnumerable<float> Wait(float seconds) {
            yield return seconds;
        }

        IEnumerable<float> Face(Vector3 rotation) =>
            Face(Vectors.AngleTo(transform.position, rotation));

        IEnumerable<float> Face(float rotationZ) {
            while (Mathf.Abs(transform.rotation.eulerAngles.z - rotationZ) % 360 > cfg.rotationSpeed * Time.fixedDeltaTime) {
                DoFace(rotationZ);
                yield return 0;
            }
        }

        void DoFace(Vector3 rotation) =>
            DoFace(Vectors.AngleTo(transform.position, rotation));

        void DoFace(float rotationZ) =>
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, rotationZ),
                cfg.rotationSpeed * Time.fixedDeltaTime);

        IEnumerable<float> Goto(Vector3 position) {
            while (Vector2.Distance(position, transform.position) > Time.fixedDeltaTime) {
                DoFace(position);

                Vector3 direction =
                    cfg.moveSpeed
                    * Time.fixedDeltaTime
                    * (position - transform.position).Sign();
                direction = direction.Clamp(position - transform.position, transform.position - position);

                float diff = Mathf.Abs(Vectors.AngleTo(transform.position - position) - transform.rotation.eulerAngles.z);
                diff = diff > 180f ? 360f - diff : diff;
                if (diff < 45f) {
                    anim.SetBool("isMoving", true);
                    transform.position += direction;
                }

                yield return 0;
            }

            anim.SetBool("isMoving", false);
        }

        public void OnStateChange(EnemyState state) =>
            this.state = state;
    }
}
