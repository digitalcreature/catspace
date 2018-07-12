using UnityEngine;
using System;
using System.Collections;

// centralized manager that
public class InteractionManager : SingletonBehaviour<InteractionManager> {

  public float interactionRange = 5f;

  public KeyCode backpackKey = KeyCode.Tab;

  public KeyCode interactKey = KeyCode.F;
  public KeyCode carryKey = KeyCode.Mouse0;

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

  RaycastHit[] hits = new RaycastHit[8];

  void Update() {
    CameraRig rig = CameraRig.instance;
    if (Input.GetKeyDown(backpackKey)) {
      ToggleBackpack();
    }
    Player player = Player.localPlayer;
    if (player != null) {
      if (player.chr.isDriving) {
        // dont let the cursor show, or let the player interact while driving
        targetHit = new RaycastHit();
        // let them leave the seat theyre in still
        if (Input.GetKeyDown(interactKey)) {
          player.chr.Interact(null, InteractionMode.Interact);
        }
      }
      else {
        Ray ray = new Ray();
        if (rig.isFirstPerson) {
          ray.origin = rig.cam.transform.position;
          ray.direction = rig.cam.transform.forward;
        }
        else {
          ray = rig.cam.ScreenPointToRay(Input.mousePosition);
        }
        int count = Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity, interactableMask);
        Array.Sort(hits, 0, count, new RaycastHitComparer());
        for (int i = 0; i < count; i ++) {
          RaycastHit hit = hits[i];
          // dont interact with object being carried if in first person
          if (rig.isThirdPerson || hit.rigidbody == null || !hit.rigidbody.CompareTag("Carried")) {
            targetHit = hit;
            break;
          }
        }
        Interactable target = null;
        if (targetHit.collider != null) {
          target = targetHit.collider.GetComponentInParent<Interactable>();
          if (target != null) {
            float distance = (player.transform.position - target.transform.position).magnitude;
            if (!target.isInteractable || distance > interactionRange) {
              target = null;
            }
          }
        }
        SetTarget(target);
        if (target != null) {
          target.DrawHighlight(selectionGlowEffect);
        }
        if (Input.GetKeyDown(interactKey)) {
          player.chr.Interact(target, InteractionMode.Interact);
        }
        else if (Input.GetKeyDown(carryKey)) {
          player.chr.Interact(target, InteractionMode.Carry);
        }
        if (otherExaminationRig.target != null) {
          Vector3 targetPosition = otherExaminationRig.target.examineCenter.position;
          if (Vector3.Distance(targetPosition, player.transform.position) > interactionRange) {
            otherExaminationRig.SetTarget(null);
          }
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
