using UnityEngine;

namespace OSFE.Scripts.UI {
    public class UIMain : MonoBehaviour {
        public void QuitGame() {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
    }
}
