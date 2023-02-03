﻿using OSCore.Data.AI;
using OSCore.Utils;
using UnityEngine.AI;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyNavAgent : MonoBehaviour {
        public bool isMoving { get; private set; }
        public bool isTurning { get; private set; }
        public float remainingDistance => nav == null ? -1f : nav.remainingDistance;

        private EnemyAnimator anim;
        private NavMeshAgent nav;
        private BehaviorConfig cfg;

        private Vector3 turnPos;
        private Quaternion faceTarget;
        private float turnStartTime;
        private Vector3 prevPos;
        private float buffer;

        public bool Goto(Vector3 location, BehaviorConfig cfg) {
            NavMeshPath path = new();

            if (!location.IsNegativeInfinity()
                    && nav.CalculatePath(location, path)
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

        public void Face(Vector3 location, BehaviorConfig cfg) {
            this.cfg = cfg;
            faceTarget = transform.rotation;
            turnStartTime = Time.time;
            turnPos = location;
            isTurning = true;
        }

        private void UpdateTurn() {
            Quaternion rotation = transform.rotation;
            Quaternion face = Quaternion.LookRotation(turnPos - transform.position);
            float diff = Maths.AngleDiff(rotation.eulerAngles.y, face.eulerAngles.y);

            if (diff >= 1f) {
                DoTurn(face);
            } else {
                isTurning = false;
            }
        }

        private void UpdateMove() {
            Vector3 movement = transform.position - prevPos;
            if (Vectors.NonZero(movement)) {
                DoTurn(Quaternion.LookRotation(transform.position - prevPos));
            }

            if (isMoving && buffer > 0f && (nav.isStopped || nav.remainingDistance < 0.025f)) {
                anim.Transition(state => state with { isMoving = false });
                isMoving = false;
                nav.isStopped = true;
                nav.ResetPath();
            }
        }

        private void DoTurn(Quaternion toward) {
            transform.rotation = Quaternion.Lerp(
                isTurning ? faceTarget : transform.rotation,
                toward,
                cfg.rotationSpeed
                * (isTurning ? (Time.time - turnStartTime) : Time.deltaTime));
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