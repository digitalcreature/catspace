using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PivotThruster : Thruster {

  public Transform pivot;         // the transform to rotate around it's local x axis
  public Transform parent;     // the parent transform that we are pivoting with respect to
  public float pivotSmoothTime = 0.35f;
  public float pivotThrustThreshold = 0.5f; // the minimum thrust required for the thruster to pivot

  public float pivotRest = 90;    // the rest angle of the pivot
  public float pivotZero = 0;     // the zero angle of the pivot
  public float pivotRange = 180;  // the range of motion of the pivot

  float targetAngle;
  float angleVelocity;

  // the angle of the pivot
  public float angle {
    get {
      return ForwardToAngle(pivot.forward);
    }
    set {
      pivot.rotation = parent.rotation;
      pivot.Rotate(-(value + pivotZero), 0, 0);
    }
  }

  // given a forward vector, return the angle in degrees needed to match it
  public float ForwardToAngle(Vector3 forward) {
    return Mathf.Repeat((Mathf.Atan2(
      Vector3.Dot(forward, parent.up) / parent.up.magnitude,
      Vector3.Dot(forward, parent.forward) / parent.forward.magnitude
    ) * Mathf.Rad2Deg) - pivotZero, 360);
  }

  public Vector3 AngleToForward(float angle) {
    return Mathf.Cos((pivotZero + angle) * Mathf.Deg2Rad) * parent.forward
         + Mathf.Sin((pivotZero + angle) * Mathf.Deg2Rad) * parent.up;
  }

  protected override void Awake() {
    base.Awake();
    angle = targetAngle = pivotRest;
  }

  protected override void Update() {
    base.Update();
    targetAngle = Mathf.Clamp(targetAngle, 0, pivotRange);
    angle = Mathf.SmoothDamp(angle, targetAngle, ref angleVelocity, pivotSmoothTime);
  }

  public override bool SetTargetThrust(Vector3 totalThrust, Vector3 totalTorque) {
    // figure out the tangent force for torque
    float oldTargetAngle = targetAngle;
    Vector3 tangent = GetTangentThrust(pivot.position, totalTorque) * maxThrust;
    // limit torque pivoting to only go down, never up
    Vector3 pivotThrust = totalThrust + (tangent * torqueWeight * (Vector3.Dot(tangent, parent.up) <= 0 ? 0 : 1));
    if (Vector3.ProjectOnPlane(pivotThrust, parent.right).magnitude > pivotThrustThreshold) {
      targetAngle = ForwardToAngle(pivotThrust + tangent);
    }
    else {
      targetAngle = pivotRest;
    }
    return base.SetTargetThrust(totalThrust + tangent, Vector3.zero) || oldTargetAngle != targetAngle;
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
    pivotRest = Mathf.Clamp(pivotRest, 0, pivotRange);
  }

  public override void OnSync(NetworkSync sync) {
    base.OnSync(sync);
    float angle = this.angle;
    sync.Sync(ref angle);
    this.angle = angle;
  }

  #if UNITY_EDITOR

  void OnDrawGizmosSelected() {
    Color color = Handles.color;
    Handles.color = Color.white;
    Handles.DrawWireArc(pivot.position, parent.right, AngleToForward(0), -pivotRange, 1);
    Handles.DrawSolidDisc(pivot.position + AngleToForward(pivotRest), AngleToForward(pivotRest + 90), 0.1f);
    Handles.color = color;
  }

  #endif

}
