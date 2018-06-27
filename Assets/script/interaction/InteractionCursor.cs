using UnityEngine;

public class InteractionCursor : MonoBehaviour {

  public float normalOffset = 0.05f;
  Renderer render;

  void Awake() {
    render = GetComponent<Renderer>();
  }

  void OnEnable() {
    // if (!Application.isEditor) {
    //   Cursor.visible = false;
    // }
  }

  void OnDisable() {
    // if (!Application.isEditor) {
    //   Cursor.visible = true;
    // }
  }

  void LateUpdate() {
    InteractionManager im = InteractionManager.instance;
    if (im.isTargetValid) {
      transform.position = im.targetHit.point + (im.targetHit.normal.normalized * normalOffset);
      render.enabled = true;
    }
    else {
      render.enabled = false;
    }
  }

}
