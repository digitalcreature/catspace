using UnityEngine;

public class ExaminationCameraRig : MonoBehaviour {

  [Layer]
  public int interactionLayer;

  public float cameraFOV = 30;

  public Camera cam { get; private set; }

  public Examinable target { get; private set; }

  // public Tween zoomTween;

  void Awake() {
    cam = GetComponentInChildren<Camera>();
    Camera.onPreCull += HandlePreCull;
    Camera.onPostRender += HandlePostRender;
  }

  void Update() {
    if (target == null) {
      cam.enabled = false;
    }
    else {
      cam.enabled = true;
      transform.position = target.centerPosition;
      transform.rotation = target.centerRotation;
    }
    // if (target != null) {
    //   // transform.position = target.centerPosition;
    //   // transform.rotation = target.centerRotation;
    //   zoomTween.Update();
    //   cam.enabled = true;
    //   if (!zoomTween.isAtEnd) {
    //     Camera main = CameraRig.instance.cam;
    //     cam.fieldOfView = zoomTween.Lerp(main.fieldOfView, cameraFOV);
    //     transform.position = zoomTween.Lerp(main.transform.position, target.centerPosition);
    //     transform.rotation = zoomTween.Lerp(main.transform.rotation, target.centerRotation);
    //     // zoomTween.SlerpPos(cam.transform, main.transform, camPosition);
    //     // zoomTween.LerpRot(cam.transform, main.transform, camPosition);
    //   }
    //   if (zoomTween.isAtStart) {
    //     target = null;
    //     cam.enabled = false;
    //   }
    // }
    // else {
    //   cam.enabled = false;
    // }
  }

  public void SetTarget(Examinable target) {
    this.target = target;
    // if (target == null) {
    //   zoomTween.speed = -1;
    // }
    // else {
    //   zoomTween.speed = 1;
    //   this.target = target;
    // }
  }

  void HandlePreCull(Camera camera) {
    if (camera == cam) {
      if (target != null) {
        target.SetLayer(interactionLayer);
      }
    }
  }

  void HandlePostRender(Camera camera) {
    if (camera == cam) {
      if (target != null) {
        target.ResetLayer();
      }
    }
  }

}
