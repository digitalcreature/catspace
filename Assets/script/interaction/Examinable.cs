using UnityEngine;
using System.Collections.Generic;

public class Examinable : InteractableModule {

  [Layer]
  public int defaultLayer;          // the layer that renderers will return to after examination
  public List<Renderer> renderers;  // renderers whose layers need to be changed during examination

  public Transform examineCenter; // where the camera should center on when examining this object
  public float boundingRadius = 0.5f;

  public bool lockCameraRotation = false;

  public ExaminationCameraRig observingRig { get; set; } // the camera rig currently examining this object

  public void SetRenderLayer(int layer) {
    foreach (Renderer renderer in renderers) {
      renderer.gameObject.layer = layer;
    }
  }

  public void ResetRenderLayer() {
    SetRenderLayer(defaultLayer);
  }

  void OnValidate() {
    if (examineCenter == null) {
      examineCenter = transform;
    }
  }

  void OnDrawGizmosSelected() {
    Color c = Gizmos.color;
    Gizmos.color = new Color(0.7f, 0.85f, 1f);
    Gizmos.DrawWireSphere(examineCenter.position, boundingRadius);
    Gizmos.color = c;
  }

  // this function is called once on the server
  // and then once on every client connected
  protected override void OnInteractLocal(GCharacter character, InteractionMode mode) {
    Player localPlayer = Player.localPlayer;
    if (mode == InteractionMode.Interact && localPlayer != null && localPlayer.chr == character) {
      if (observingRig == null) {
        InteractionManager.instance.Examine(this);
      }
      else {
        InteractionManager.instance.Examine(null);
      }
    }
  }

}
