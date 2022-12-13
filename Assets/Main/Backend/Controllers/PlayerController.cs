using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers {
    public class PlayerController : MonoBehaviour, IStateReceiver<PlayerState> {
        [SerializeField] private PlayerCfgSO cfg;

        private IGameSystem system;
        private Rigidbody rb;
        private Animator anim;
        private PlayerState state;
        private bool isGrounded = false;
        private RaycastHit ground;

        public void OnStateChange(PlayerState state) =>
            this.state = state;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            rb = GetComponent<Rigidbody>();
            anim = GetComponentInChildren<Animator>();

            system.Send<IControllerManager>(mngr =>
                mngr.Ensure<IPlayerStateReducer>(transform).Init(this, cfg));
        }

        private void Update() {
            RotatePlayer(MoveCfg());
        }

        private void FixedUpdate() {
            isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out ground,
                cfg.groundedDist);

            anim.SetBool("isGrounded", isGrounded);

            if (!isGrounded) {
                anim.SetBool("isAiming", false);
                anim.SetBool("isScoping", false);
                anim.SetInteger("stance", 0);
            }

            MovePlayer(MoveCfg());
        }

        private void RotatePlayer(MoveConfig moveCfg) {
            Vector2 direction;

            if (Vectors.NonZero(state.facing)
                && (state.stance != PlayerStance.CRAWLING || !Vectors.NonZero(state.movement)))
                direction = state.facing;
            else if (state.mouseLookTimer <= 0f && Vectors.NonZero(state.movement))
                direction = state.movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    moveCfg.rotationSpeed * Time.deltaTime);
        }

        private void MovePlayer(MoveConfig moveCfg) {
            if (isGrounded && PCUtils.IsMovable(state.stance, state)) {
                float speed = moveCfg.moveSpeed;
                float forceZ = ground.transform.rotation != Quaternion.identity && Vectors.NonZero(state.movement)
                    ? (Vector3.Angle(ground.normal, state.movement) - 90f) / 90f
                    : 0f;

                if (PCUtils.IsAiming(state.attackMode)) speed *= cfg.aimFactor;
                else if (state.isScoping) speed *= cfg.scopeFactor;

                float movementSpeed = Mathf.Max(
                    Mathf.Abs(state.movement.x),
                    Mathf.Abs(state.movement.y));
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;

                if (Vectors.NonZero(state.facing)) {
                    float mov = (360f + Vectors.AngleTo(transform.position, transform.position - state.movement.Upgrade())) % 360;
                    float fac = (360f + Vectors.AngleTo(transform.position, transform.position - state.facing.Upgrade())) % 360;
                    float diff = Mathf.Abs(mov - fac);

                    if (diff > 180f) diff = Mathf.Abs(diff - 360f);

                    speed *= Mathf.Lerp(moveCfg.lookSpeedInhibiter, 1f, 1f - diff / 180f);
                }

                Vector3 dir = speed * state.movement.Upgrade(-forceZ);

                float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;
                if (velocityDiff < moveCfg.maxVelocitydamper)
                    dir *= velocityDiff / moveCfg.maxVelocitydamper;

                if (isForceable && Vectors.NonZero(state.movement)) {
                    anim.speed = movementSpeed * speed * Time.fixedDeltaTime * moveCfg.animFactor;

                    if (state.isSprinting)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private MoveConfig MoveCfg() =>
                state.stance switch {
                    PlayerStance.CROUCHING => cfg.crouching,
                    PlayerStance.CRAWLING => cfg.crawling,
                    _ => state.isSprinting ? cfg.sprinting : cfg.standing
                };
    }
}
