using UnityEngine;

// centralized manager that
public class InteractionManager : SingletonBehaviour<InteractionManager> {

  public float interactionRange = 5f;

  public KeyCode backpackKey = KeyCode.Tab;
  public KeyCode interactionKey = KeyCode.F;

  public LayerMask interactableMask;
  public Material selectionGlowEffect;
  public ExaminationCameraRig backpackExaminationRig;
  public ExaminationCameraRig otherExaminationRig;

  public Interactable target { get; private set; }    // the target interactable currently being hovered over
  public RaycastHit targetHit { get; private set; }   // the hit result of the current target (contains extra values like distance, normal, collider, etc)
  public bool isTargetValid => targetHit.collider != null;

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

  public void Examine(Examinable examinable) {
    otherExaminationRig.SetTarget(examinable);
  }

  void Update() {
    CameraRig rig = CameraRig.instance;
    if (Input.GetKeyDown(backpackKey)) {
      ToggleBackpack();
    }
    Player player = Player.localPlayer;
    if (player != null) {
      Ray ray = new Ray();
      if (rig.isFirstPerson) {
        ray.origin = rig.cam.transform.position;
        ray.direction = rig.cam.transform.forward;
      }
      else {
        ray = rig.cam.ScreenPointToRay(Input.mousePosition);
      }
      RaycastHit hit;
      Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask);
      targetHit = hit;
      float distance = (player.transform.position - hit.point).magnitude;
      Interactable target = null;
      if (hit.collider != null) {
        target = hit.collider.GetComponentInParent<Interactable>();
        if (target != null) {
          if (!target.isInteractable || distance > interactionRange) {
            target = null;
          }
        }
      }
      SetTarget(target);
      if (target != null) {
        target.DrawHighlight(selectionGlowEffect);
      }
      if (Input.GetKeyDown(interactionKey)) {
        player.chr.Interact(target);
      }
      if (otherExaminationRig.target != null) {
        Vector3 targetPosition = otherExaminationRig.target.examineCenter.position;
        if (Vector3.Distance(targetPosition, player.transform.position) > interactionRange) {
          otherExaminationRig.SetTarget(null);
        }
      }
    }
  }

  void SetTarget(Interactable target) {
    if (this.target != target) {
      this.target = target;
    }
  }

}
