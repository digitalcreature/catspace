using UnityEngine;

public class InteractionCursor : MonoBehaviour {

  Renderer render;

  void Awake() {
    render = GetComponent<Renderer>();
  }

  void Update() {
    InteractionManager im = InteractionManager.instance;
    render.enabled = im.cursor != Vector3.zero;
    transform.position = im.cursor;
  }

}
