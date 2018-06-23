using UnityEngine;

// the main camera rig
public class CameraRig : SingletonBehaviour<CameraRig> {

  public MouseLook mouseOrbit; // the mouse look
  public LayerMask distanceAdjustMask;  // the mask to use when adjusting the camera distance
  public float maxDistance = 10;
  public float distanceBias = 0.5f;

  public MouseButton mouseOrbitButton = MouseButton.Middle; // button to use for orbiting


  public Transform target { get; private set; }  // the object to target
  public GField gfield { get; private set; }     // the gravity fiel to align to (optional)

  public Camera cam { get; private set; }

  public Transform gimbal => mouseOrbit.gimbal;

  void Awake() {
    cam = GetComponentInChildren<Camera>();
  }

  void Update() {
    if (target != null) {
      transform.position = target.position;
    }
    if (gfield != null) {
      gfield.AlignTransformToGravity(transform);
    }
    if (Input.GetMouseButton((int) mouseOrbitButton)) {
      // update the gimbal based on mouse input
      // this is only done what the mouse button is being held down
      mouseOrbit.UpdateGimbal(transform.up);
    }
    // adjust the distance of the camera
    RaycastHit hit;
    float distance = maxDistance;
    if (Physics.Raycast(gimbal.position, -gimbal.forward, out hit, maxDistance + distanceBias, distanceAdjustMask)) {
      distance = hit.distance - distanceBias;
      if (distance < 0f) {
        distance = 0f;
      }
    }
    cam.transform.localPosition = -Vector3.forward * distance;
  }


  // retarget the camera rig to follow [target]
  // [gfield] is optional; if it is not provided, the camera rig will stop
  // aligning to any gravity field. use RetargetKeepGravity() if you want to leave
  // the field unchanged instead
  public void Retarget(Transform target, GField gfield = null) {
    this.target = target;
    SetGField(gfield);
  }

  // retarget the camera rig, but leave it's gravity field unchanged
  public void RetargetKeepGravity(Transform target) {
    Retarget(target, this.gfield);
  }

  // set the gravity field
  public void SetGField(GField gfield) {
    this.gfield = gfield;
  }

}
