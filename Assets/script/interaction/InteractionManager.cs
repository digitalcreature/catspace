using UnityEngine;

// centralized manager that
public class InteractionManager : SingletonBehaviour<InteractionManager> {

  public float interactionRange = 5f;

  public KeyCode backpackKey = KeyCode.Tab;
  public KeyCode interactionKey = KeyCode.F;

  public LayerMask interactableMask;
  public Material selectionGlowEffect;
  public ExaminationCameraRig backpackExaminationRig;

  public Vector3 cursor { get; private set; }         // the world space position of the cursor (Vector3.zero if the cursor isnt over anything)
  public Interactable target { get; private set; }    // the target interactable currently being hovered over

  public void OpenBackpack() {
    Player player = Player.localPlayer;
    if (player != null) {
      if (backpackExaminationRig.target == null) {
        backpackExaminationRig.SetTarget(player.backpack);
      }
    }
  }

  public void CloseBackpack() {
    Player player = Player.localPlayer;
    if (player != null) {
      if (backpackExaminationRig.target == player.backpack) {
        backpackExaminationRig.SetTarget(null);
      }
    }
  }

  public void ToggleBackpack() {
    Player player = Player.localPlayer;
    if (player != null) {
      if (backpackExaminationRig.target == player.backpack) {
        CloseBackpack();
      }
      else {
        OpenBackpack();
      }
    }
  }

  void Update() {
    if (Input.GetKeyDown(backpackKey)) {
      ToggleBackpack();
    }
    Player player = Player.localPlayer;
    if (player != null) {
      RaycastHit hit;
      MousePositionToObject(out hit);
      float distance = (player.transform.position - hit.point).magnitude;
      Interactable target = null;
      if (hit.collider != null) {
        cursor = hit.point;
        target = hit.collider.GetComponent<Interactable>();
        if (target != null) {
          if (!target.isInteractable || distance > interactionRange) {
            target = null;
          }
        }
      }
      else {
        cursor = Vector3.zero;
      }
      SetTarget(target);
      if (Input.GetKeyDown(interactionKey)) {
        player.chr.Interact(target);
      }
    }
  }

  void SetTarget(Interactable target) {
    if (this.target != target) {
      if (this.target != null) {
        this.target.SetEffectMaterial(null);
      }
      if (target != null) {
        target.SetEffectMaterial(selectionGlowEffect);
      }
      this.target = target;
    }
  }

  // return a world space location that is under the a given screen point
  public bool ScreenPointToObject(Vector3 screenPoint, out RaycastHit hit) {
    CameraRig rig = CameraRig.instance;
    Ray ray = rig.cam.ScreenPointToRay(screenPoint);
    return Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask);
  }

  public bool MousePositionToObject(out RaycastHit hit) {
    return ScreenPointToObject(Input.mousePosition, out hit);
  }

}
