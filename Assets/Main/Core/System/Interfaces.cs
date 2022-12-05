using System;
using System.Collections.Generic;
using OSCore.Data.Enums;
using OSCore.Data.Events.Brains;
using OSCore.ScriptableObjects;
using UnityEngine;

namespace OSCore.System.Interfaces {
    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent;
    }

    public interface IGameSystemComponent {
        public void OnStart() { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnDestroy() { }
    }

    namespace Tagging {
        public interface ITagRegistry : IGameSystemComponent {
            public void Register(IdTag tag, GameObject obj);
            public void RegisterUnique(IdTag tag, GameObject obj);
            public ISet<GameObject> Get(IdTag tag);
            public GameObject GetUnique(IdTag tag);
        }
    }

    namespace Events {
        public interface IPubSub : IGameSystemComponent {
            void Publish<T>(T item) where T : IEvent;
            long Subscribe<T>(Action<T> action) where T : IEvent;
            void Unsubscribe(long id);
        }
    }

    namespace Brains {
        public interface IControllerManager : IGameSystemComponent {
            public T Ensure<T>(Transform target) where T : IGameSystemComponent;
        }

        public interface IPlayerController : IGameSystemComponent {
            public void Init(PlayerCfgSO cfg);
            public void OnMovementInput(Vector2 direction);
            public void OnSprintInput(bool isSprinting);
            public void OnLookInput(Vector2 direction, bool isMouse);
            public void OnStanceInput(float holdDuration);
            public void OnAimInput(bool isAiming);
            public void OnAttackInput(bool isAttacking);
            public void OnScopeInput(bool isScoping);
            public void OnStanceChanged(PlayerStance stance);
            public void OnAttackModeChanged(AttackMode attackMode);
            public void OnMovementChanged(bool isMoving);
            public void OnScopingChanged(bool isScoping);
            public void OnPlayerStep();
        }

        public interface IPlayerFOVController : IGameSystemComponent {
            public void Init(PlayerFOVCfgSO cfg, Mesh mesh, Transform fov);
        }

        public interface IEnemyController : IGameSystemComponent {
            public void OnAttackModeChanged(AttackMode attackMode);
            public void OnMovementChanged(bool isMoving);
            public void OnEnemyStep();
        }

        public interface ICameraController : IGameSystemComponent {
            public void Init(CameraCfgSO cfg);
        }

        public interface ICameraOverlayController : IGameSystemComponent {
            public void Init(CameraOverlayCfgSO cfg);
        }
    }
}
