using UnityEngine;

public class ExaminationCameraRig : MonoBehaviour {

  [Layer]
  public int examinationLayer;

  public float startDistance = 100;
  public float endDistance = 0;

  public float scale = 0.5f;  // how much of the vertical height of the viewport should the target take up? (approximate)

  public float rotateSpeed = 30f;

  public Transform gimbal;

  public Camera cam { get; private set; }

  public Examinable target { get; private set; }

  public Tween zoomTween;

  void Awake() {
    cam = GetComponentInChildren<Camera>();
  }

  void Update() {
    zoomTween.Update();
    if (zoomTween.isAtStart) {
      if (zoomTween.speed < 0) {
        cam.enabled = false;
        if (target != null) {
          target.ResetRenderLayer();
          target.targeter = null;
          target = null;
        }
      }
    }
    else {
      if (target != null) {
        if (target.targeter == null) {
          target.targeter = this;
          target.SetRenderLayer(examinationLayer);
        }
        cam.enabled = true;
        Vector3 difference = gimbal.forward * zoomTween.Lerp(startDistance, endDistance);
        transform.position = target.examineCenter.position - difference;
        if (target.lockCameraRotation) {
          if (zoomTween.isAtEnd) {
            gimbal.rotation = target.examineCenter.rotation;
          }
        }
        else {
          gimbal.Rotate(target.transform.up, rotateSpeed * Time.deltaTime, Space.Self);
        }
        float fov = cam.fieldOfView;
        float camDistance = (target.boundingRadius / scale) / Mathf.Sin(fov / 2 * Mathf.Deg2Rad);
        cam.transform.localPosition = new Vector3(0f, 0f, -camDistance);
      }
      else {
        cam.enabled = false;
      }
    }
  }

  public void SetTarget(Examinable target) {
    if (target != this.target) {
      if (target == null) {
        zoomTween.speed = -1;
      }
      else {
        if (this.target == null) {
          zoomTween.speed = 1;
        }
        else {
          zoomTween.speed = 0;
          zoomTween.t = 1;
          this.target.targeter = null;
          this.target.ResetRenderLayer();
        }
        this.target = target;
        gimbal.rotation = target.examineCenter.rotation;
      }
    }
    else {
      if (zoomTween.speed < 0) {
        zoomTween.speed = 1;
      }
    }
  }

}
