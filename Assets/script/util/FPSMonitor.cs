using UnityEngine;

public class FPSMonitor : MonoBehaviour {

  void OnGUI() {
    GUILayout.Label(string.Format("{0} fps", (int) (1 / Time.unscaledDeltaTime)));
  }
}
