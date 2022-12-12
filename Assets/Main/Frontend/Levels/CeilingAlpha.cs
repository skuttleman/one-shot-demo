using OSCore.Data.Enums;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine.Tilemaps;
using UnityEngine;
using OSCore.Utils;

public class CeilingAlpha : MonoBehaviour {
    IGameSystem system;
    Tilemap[] maps;
    GameObject player;
    Transform fov;

    void Start() {
        system = FindObjectOfType<GameController>();
        maps = transform.parent.GetComponentsInChildren<Tilemap>();
        player = system.Send<ITagRegistry, GameObject>(reg =>
            reg.GetUnique(IdTag.PLAYER));
        fov = Transforms.FindInChildren(player.transform.parent, child => child.name == "fov")
            .First();
    }

    void OnTriggerStay(Collider other) {
        if (other.transform.IsChildOf(player.transform)) {
            foreach (Tilemap map in maps) {
                float diff = Mathf.Abs(
                    map.transform.position.z
                    - player.transform.position.z);
                float alpha = map.transform.position.z >= player.transform.position.z
                    ? 1f
                    : Mathf.Clamp(1f - (diff / 1.5f), 0f, 1f);
                map.color = new(map.color.r, map.color.g, map.color.b, alpha);
            }
            fov.gameObject.SetActive(false);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.transform.IsChildOf(player.transform)) {
            foreach (Tilemap map in maps)
                map.color = new(map.color.r, map.color.g, map.color.b, 1f);
            fov.gameObject.SetActive(true);
        }
    }
}
