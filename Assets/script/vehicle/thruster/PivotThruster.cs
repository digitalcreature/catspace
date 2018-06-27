using UnityEngine;

public class PivotThruster : Thruster {

  public Transform pivot;         // the transform to rotate around it's local x axis
  public Transform restAngle;     // the transform to use as a reference when too little thrust is present
  public float pivotSpeed = 90f;  // the speed the thruster pivots at in degrees/s
  public float pivotSmoothTime = 0.35f;
  public float pivotThrustThreshold = 0.5f; // the minimum thrust required for the thruster to pivot

  Vector3 targetForward;  // the target to pivot to (where the forward vector of the pivot should end up)
  Vector3 pivotVelocity;

  protected override void Awake() {
    base.Awake();
    targetForward = pivot.forward;
  }

  protected override void Update() {
    base.Update();
    UpdateForward(targetForward);
  }

  public override bool SetTargetThrust(Vector3 totalThrust) {
    if (Vector3.ProjectOnPlane(totalThrust, pivot.right).magnitude > pivotThrustThreshold) {
      targetForward = totalThrust;
    }
    else {
      targetForward = (restAngle == null ? vehicle.transform : restAngle).forward;
    }
    base.SetTargetThrust(totalThrust);
    return true;
  }

  void UpdateForward(Vector3 targetForward) {
    Vector3 nextForward = Vector3.SmoothDamp(
      pivot.forward, Vector3.ProjectOnPlane(targetForward, pivot.right).normalized,
      ref pivotVelocity, pivotSmoothTime
    ).normalized;
    // i think this is making it break (its very janky anyway, need to fix)
    pivot.rotation = Quaternion.LookRotation(nextForward, pivot.right);
    pivot.Rotate(0, 0, 90);
  }

  void OnValidate() => pivot = pivot == null ? transform : pivot;

  public override void OnSync(NetworkSync sync) {
    base.OnSync(sync);
    sync.Sync(ref targetForward);
  }

}
