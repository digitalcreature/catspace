using UnityEngine;
using UnityEngine.Rendering;

// an object that can be focused by the camera rig
public class CameraRigFocusable : MonoBehaviour {

  public Transform focusTarget;
  public Renderer[] thirdPersonOnlyRenderers;   // a list of renderers to disable/switch to shadow only when in first person

  public GBody gbody { get; private set; }  // optional. if present, the camerarig will use this to align itself to gravity
  public GField gfield => gbody == null ? null : gbody.gfield;
  public GCharacter character
    => gbody is GCharacter ? (GCharacter) gbody : null;

  bool isFirstPerson;

  void Awake() {
    gbody = GetComponent<GBody>();
  }

  public void UpdateRenderers(bool isFirstPerson) {
    if (this.isFirstPerson != isFirstPerson) {
      this.isFirstPerson = isFirstPerson;
      if (isFirstPerson) {
        // changing from third to first person
        foreach (Renderer renderer in thirdPersonOnlyRenderers) {
          switch (renderer.shadowCastingMode) {
            case ShadowCastingMode.On:
              renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
              break;
            case ShadowCastingMode.Off:
              renderer.enabled = false;
              break;
            default: break;
          }
        }
      }
      else {
        // changing from first to third person
        foreach (Renderer renderer in thirdPersonOnlyRenderers) {
          if (!renderer.enabled) {
            renderer.enabled = true;
          }
          else {
            renderer.shadowCastingMode = ShadowCastingMode.On;
          }
        }
      }
    }
  }

  void OnValidate() {
    if (focusTarget == null) {
      focusTarget = transform;
    }
  }

}
