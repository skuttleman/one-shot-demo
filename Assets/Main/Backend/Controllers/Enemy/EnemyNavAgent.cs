using OSCore.Data.AI;
using OSCore.Utils;
using UnityEngine.AI;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyNavAgent : MonoBehaviour {
        public bool isMoving { get; private set; }
        public bool isTurning { get; private set; }

        private EnemyAnimator anim;
        private NavMeshAgent nav;
        private StateConfig cfg;

        private Vector3 turnPos;
        private Vector3 prevPos;
        private float buffer;

        public bool Goto(Vector3 location, StateConfig cfg) {
            NavMeshPath path = new();

            if (nav.CalculatePath(location, path)
                && Vector3.Distance(location, nav.pathEndPosition) > 0.025f
            ) {
                anim.Transition(state => state with { isMoving = true });
                anim.SetSpeed(cfg.moveSpeed / 2.5f);

                this.cfg = cfg;
                isMoving = true;
                buffer = -0.25f;

                nav.SetPath(path);
                nav.speed = cfg.moveSpeed;

                return true;
            }
            return false;
        }

        public void Stop() {
            if (nav != null) nav.ResetPath();
            if (anim != null) {
                anim.Transition(state => state with { isMoving = false });
            }

            isMoving = false;
            isTurning = false;
        }

        public void Face(Vector3 location, StateConfig cfg) {
            this.cfg = cfg;
            isTurning = true;
            turnPos = location;
        }

        private void UpdateTurn() {
            Quaternion rotation = transform.rotation;
            Quaternion face = Quaternion.LookRotation(turnPos - transform.position);
            float diff = Maths.AngleDiff(rotation.eulerAngles.y, face.eulerAngles.y);

            if (diff >= 1f) {
                transform.rotation = Quaternion.Lerp(rotation, face, cfg.rotationSpeed * Time.deltaTime);
            } else {
                isTurning = false;
            }
        }

        private void UpdateMove() {
            Vector3 movement = transform.position - prevPos;
            if (Vectors.NonZero(movement)) {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(transform.position - prevPos),
                    cfg.rotationSpeed * Time.deltaTime);
            }

            if (isMoving && buffer > 0f && (nav.isStopped || nav.remainingDistance < 0.025f)) {
                anim.Transition(state => state with { isMoving = false });
                isMoving = false;
                nav.isStopped = true;
                nav.ResetPath();
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            anim = GetComponentInChildren<EnemyAnimator>();
            nav = GetComponent<NavMeshAgent>();
            nav.updateRotation = false;
            prevPos = transform.position;
        }

        private void Update() {
            if (isTurning) {
                UpdateTurn();
            } else if (isMoving) {
                UpdateMove();
            }

            prevPos = transform.position;
            buffer += Time.deltaTime;
        }
    }
}