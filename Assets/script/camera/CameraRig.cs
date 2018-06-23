using UnityEngine;

// the main camera rig
public class CameraRig : SingletonBehaviour<CameraRig> {

  public MouseLook mouseOrbit; // the mouse look
  public LayerMask distanceAdjustMask;  // the mask to use when adjusting the camera distance
  public float distance = 10;
  public float distanceBias = 0.5f;
  public float maxDistance = 15;
  public float zoomRate = 3f;
  public float zoomSmoothTime = 0.1f;
  public float firstPersonDistanceThreshold = 0.01f; // camera is considered first person when distance is smaller than this value

  public MouseButton mouseOrbitButton = MouseButton.Middle; // button to use for orbiting

  public bool isFirstPerson { get; private set; }
  public bool isThirdPerson {
    get { return !isFirstPerson; }
    private set { isFirstPerson = !value; }
  }

  public CameraRigFocusable focus { get; private set; }
  public GField gfield => focus == null ? null : focus.gfield;     // the gravity field to align to

  public Camera cam { get; private set; }

  public Transform gimbal => mouseOrbit.gimbal;

  float targetDistance;
  float zoomVelocity;

  void Awake() {
    cam = GetComponentInChildren<Camera>();
    targetDistance = distance;
  }

  void Update() {
    if (focus != null) {
      transform.position = focus.focusTarget.position;
    }
    if (gfield != null) {
      gfield.AlignTransformToGravity(transform);
    }
    if (isFirstPerson || Input.GetMouseButton((int) mouseOrbitButton)) {
      // update the gimbal based on mouse input
      // this is only done what the mouse button is being held down, or if the player is in first person
      mouseOrbit.UpdateGimbal(transform.up);
    }
    // handle scroll zoom
    targetDistance -= Input.mouseScrollDelta.y * zoomRate;
    targetDistance = Mathf.Clamp(targetDistance, 0f, maxDistance);
    distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, zoomSmoothTime);
    distance = Mathf.Clamp(distance, 0f, maxDistance);
    isFirstPerson = distance < firstPersonDistanceThreshold;
    if (focus != null) {
      focus.UpdateRenderers(isFirstPerson);
    }
    // adjust the distance of the camera
    RaycastHit hit;
    float d = distance;
    if (Physics.Raycast(gimbal.position, -gimbal.forward, out hit, d + distanceBias, distanceAdjustMask)) {
      d = hit.distance - distanceBias;
      if (d < 0f) {
        d = 0f;
      }
    }
    cam.transform.localPosition = -Vector3.forward * d;
  }


  public void SetFocus(CameraRigFocusable focus) {
    if (this.focus != null) {
      // if we are already focusing something, make sure it's renderers are in third person mode
      this.focus.UpdateRenderers(false);
    }
    this.focus = focus;
  }

}
