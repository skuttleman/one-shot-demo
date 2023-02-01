using TMPro;
using UnityEngine;

public class EnemySpeechAgent : MonoBehaviour {
    [SerializeField] private TextMeshPro speech;
    [SerializeField] private float speechSpeed;

    public bool isSpeaking { get; private set; }
    public string message => speech.text;

    private float elapsed;

    public void Say(string message) {
        message = message.Trim();
        if (message.Length > 0) {
            isSpeaking = true;
            speech.text = message.Trim();
            elapsed = 0f;
        }
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
            float duration = Mathf.Max(speech.text.Length * speechSpeed, 1.5f);

            if (elapsed >= duration) {
                Stop();
            }
        }
    }
}
