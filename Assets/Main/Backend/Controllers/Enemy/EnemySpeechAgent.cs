using OSCore.ScriptableObjects;
using TMPro;
using UnityEngine;

public class EnemySpeechAgent : MonoBehaviour {
    [SerializeField] private TextMeshPro speech;
    [SerializeField] private EnemyAICfgSO cfg;

    public bool isSpeaking { get; private set; }
    public string message => speech.text;

    private float elapsed;

    public void Say(string message) {
        isSpeaking = true;
        speech.text = message.Trim();
        elapsed = 0f;
    }

    public void Stop() {
        speech.text = "";
        isSpeaking = false;
    }

    /*
     * Lifecyle Methods
     */

    private void Start() {
        speech.text = "";
        elapsed = 0f;
    }

    private void Update() {
        if (isSpeaking) {
            speech.transform.position = transform.position + new Vector3(0f, 0f, 0.75f);

            elapsed += Time.deltaTime;
            float duration = speech.text.Length * cfg.speechSpeed;

            if (speech.text.Length > 0 && elapsed >= duration) {
                Stop();
            }
        }
    }
}
