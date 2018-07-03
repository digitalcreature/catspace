using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoSplash : MonoBehaviour {

  public Gradient fadeGradient;
  public float duration = 3;

  public string nextSceneName;

  float time;

  public Graphic graphic { get; private set; }

  void Awake() {
    graphic = GetComponent<Graphic>();
  }

  void Start() {
    time = 0;
  }

  void Update() {
    time += Time.deltaTime;
    graphic.color = fadeGradient.Evaluate(time / duration);
    if (time >= duration || Input.anyKeyDown) {
      SceneManager.LoadScene(nextSceneName);
    }
  }


}
