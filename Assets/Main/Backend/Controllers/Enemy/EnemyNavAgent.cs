using OSCore.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace OSBE.Controllers.Enemy {
    public class EnemyNavAgent : MonoBehaviour {
        public bool isMoving { get; private set; }
        public bool isTurning { get; private set; }

        private EnemyAnimator anim;
        private NavMeshAgent nav;
        private Vector3 turnPos;
        private Vector3 prevPos;
        private float buffer;

        public bool Goto(Vector3 location) {
            NavMeshPath path = new();
            if (nav.CalculatePath(location, path)) {
                anim.Transition(state => state with { isMoving = true });
                isMoving = true;
                buffer = -0.25f;
                nav.SetPath(path);

                return true;
            }
            return false;
        }

        public void Stop() {
            anim.Transition(state => state with { isMoving = false });
            isMoving = false;
            isTurning = false;
        }

        public void Face(Vector3 location) {
            isTurning = true;
            turnPos = location;
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
            // TODO - REMOVEME : CLICK MOVE FOR NOW
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Goto(hit.point);
                }
            }






            if (isTurning) {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(turnPos),
                    20f * Time.deltaTime);
            } else if (isMoving) {
                Vector3 movement = transform.position - prevPos;
                if (Vectors.NonZero(movement)) {
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        Quaternion.LookRotation(transform.position - prevPos),
                        10f * Time.deltaTime);
                }

                if (buffer > 0f && (nav.isStopped || nav.remainingDistance < 0.1f)) {
                    anim.Transition(state => state with { isMoving = false });
                    isMoving = false;
                    nav.isStopped = true;
                    nav.ResetPath();
                }
            }

            prevPos = transform.position;
            buffer += Time.deltaTime;
        }
    }
}