using UnityEngine;

public class PivotThruster : Thruster {

  public Transform pivot;         // the transform to rotate around it's local x axis
  public Transform parent;     // the parent transform that we are pivoting with respect to
  public float pivotSmoothTime = 0.35f;
  public float pivotThrustThreshold = 0.5f; // the minimum thrust required for the thruster to pivot

  float targetAngle;
  float angleVelocity;

  // the angle of the pivot, in radians
  public float angle {
    get {
      return ForwardToAngle(pivot.forward);
    }
    set {
      pivot.rotation = parent.rotation;
      pivot.Rotate(-value * Mathf.Rad2Deg, 0, 0);
    }
  }

  // given a forward vector, return the angle in radians needed to match it
  public float ForwardToAngle(Vector3 forward) {
    return Mathf.Atan2(
      Vector3.Dot(forward, parent.up) / parent.up.magnitude,
      Vector3.Dot(forward, parent.forward) / parent.forward.magnitude
    );
  }

  protected override void Awake() {
    base.Awake();
  }

  protected override void Update() {
    base.Update();
    angle = Mathf.SmoothDamp(angle, targetAngle, ref angleVelocity, pivotSmoothTime);
  }

  public override bool SetTargetThrust(Vector3 totalThrust) {
    if (Vector3.ProjectOnPlane(totalThrust, pivot.right).magnitude > pivotThrustThreshold) {
      targetAngle = ForwardToAngle(totalThrust);
    }
    else {
      targetAngle = 0;
    }
    base.SetTargetThrust(totalThrust);
    return true;
  }

  void OnValidate() {
    if (pivot == null) {
      pivot = transform;
    }
    if (parent == null) {
      if (pivot.parent == null) {
        parent = pivot;
      }
      else {
        parent = pivot.parent;
      }
    }
  }

  public override void OnSync(NetworkSync sync) {
    base.OnSync(sync);
    float angle = this.angle;
    sync.Sync(ref angle);
    this.angle = angle;
  }

}
