using System.Collections.Generic;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Brains.Patrol;
using OSCore.Utils;
using UnityEngine;
using static OSCore.System.Interfaces.Brains.Patrol.EnemyPatrol;

namespace OSBE.Controllers {
    public class EnemyController : IEnemyController {
        readonly IGameSystem system;
        readonly Transform target;
        Animator anim;
        EnemyCfgSO cfg = null;

        public EnemyController(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
            anim = target.GetComponentInChildren<Animator>();
        }

        public IEnumerable<EnemyPatrol> Init(EnemyCfgSO cfg) {
            this.cfg = cfg;
            return Transforms
                .FindInChildren(target.parent, node => node.name.Contains("position"))
                .Reduce((tpl, xform) => {
                    float waitTime = xform.localScale.z;
                    bool notFirstStep = tpl.target != xform;
                    bool isFarEnough = Vector3.Distance(xform.position, tpl.target.position) > 0.01f;

                    return (
                        xform,
                        tpl.Item2.Concat(Sequences.Empty<EnemyPatrol>()
                            .ConsIf(waitTime > 0, new PatrolWait(waitTime))
                            .Cons(new PatrolRotate(xform.rotation.eulerAngles.z))
                            .ConsIf(notFirstStep && isFarEnough, new PatrolGoto(xform.position))
                            .ConsIf(notFirstStep && isFarEnough, new PatrolFace(xform.position))));
                },
                (target, Sequences.Empty<EnemyPatrol>()))
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
            Face(Vectors.AngleTo(target.position, rotation));

        IEnumerable<float> Face(float rotationZ) {
            while (Mathf.Abs(target.rotation.eulerAngles.z - rotationZ) % 360 > cfg.rotationSpeed * Time.fixedDeltaTime) {
                target.rotation = Quaternion.Lerp(
                        target.rotation,
                        Quaternion.Euler(0f, 0f, rotationZ),
                        cfg.rotationSpeed * Time.fixedDeltaTime);

                yield return 0;
            }
        }

        IEnumerable<float> Goto(Vector3 position) {
            anim.SetBool("isMoving", true);

            while (Vector2.Distance(position, target.position) > Time.fixedDeltaTime) {
                Vector3 direction =
                    cfg.moveSpoeed
                    * Time.fixedDeltaTime
                    * (position - target.position).Sign();
                direction = direction.Clamp(position - target.position, target.position - position);

                target.position += direction;
                yield return 0;
            }

            anim.SetBool("isMoving", false);
        }
    }
}
