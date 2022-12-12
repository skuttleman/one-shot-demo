using OSCore;
using OSCore.Data.Enums;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Events;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

public class PlayerFiring : MonoBehaviour {
    [SerializeField] GameObject bulletPrefab;

    IGameSystem system;

    void OnEnable() {
        system = FindObjectOfType<GameController>();

        system.Send<IPubSub>(pubsub => pubsub.Subscribe<AttackModeChanged>(OnFire));
    }

    void OnFire(AttackModeChanged ev) {
        if (ev.mode == AttackMode.FIRING)
            Instantiate(bulletPrefab, transform.position, transform.rotation);
    }
}
