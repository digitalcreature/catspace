using UnityEngine;
using UnityEngine.Networking;

public class VTOLShip : Vehicle {

  [Header("Thruster Groups")]
  // the pod thrusters
  public ThrusterGroup pods;

  [Header("Forces")]
  // all thrust values are in m/s2 (mass independant)
  // x: +right -left
  // y: +up -down
  // z: +forward -backward
  public Vector3 thrust;

  // roll rate in radians/s2
  public float roll = 5f;
  // attitude control rate in radians/s2
  public float attitudeControlForce = 1f;

  [Header("Drag")]
  // drag value to use for inertia dampening
  public float inertiaDampenerDrag = 2.5f;
  // drag value to use for gyro
  public float gyroDrag = 5f;


  public bool steeringOn { get; private set; }
  public bool hoverOn { get; private set; }
  public bool inertiaDampenerOn { get; private set; }
  public bool gyroOn { get; private set; }

  public Rigidbody body => gbody.body;

  public override void OnStartServer() {
    base.OnStartServer();
    steeringOn = true;
    inertiaDampenerOn = true;
    gyroOn = true;
  }

  protected override void OnSync(NetworkSync sync) {
    sync.Sync(pods);
  }

  void FixedUpdate() {
    if (isServer) {
      GField gfield = gbody.gfield;
      Vector3 gravity = Vector3.zero;
      if (gfield != null) {
        gravity = gfield.WorldPointToGravity(transform.position);
      }
      if (hoverOn) {
        float dot = Vector3.Dot(-gravity, transform.up);
        if (dot >= 0) {
          pods.AddLinearThrust(Vector3.Project(-gravity, transform.up));
        }
      }
      if (isDriven) {
        // apply thrust forces
        pods.AddLinearThrust(transform.right * controls.moveX.value * thrust.x);
        pods.AddLinearThrust(transform.up * controls.moveY.value * thrust.y);
        pods.AddLinearThrust(transform.forward * controls.moveZ.value * thrust.z);
        // apply roll torque
        pods.AddAngularThrust(transform.forward * controls.roll.value * roll);
        // apply attitude control
        if (steeringOn) {
          Vector3 attitudeTarget = controls.attitudeTarget;
          //get the angle between transform.forward and target delta
          float angleDiff = Vector3.Angle(transform.forward, attitudeTarget);
          // get its cross product, which is the axis of rotation to
          // get from one vector to the other
          Vector3 cross = Vector3.Cross(transform.forward, attitudeTarget);
          // apply torque along that axis according to the magnitude of the angle.
          pods.AddAngularThrust(cross * angleDiff * attitudeControlForce);
        }
      }
      if(pods.ApplyThrust(gbody)) {
        ServerSync();
      }
    }
  }

  void Update() {
    if (isServer) {
      if (isDriven) {
        if (controls.toggleSteering.down) {
          steeringOn = !steeringOn;
        }
        if (controls.toggleDampener.down) {
          inertiaDampenerOn = !inertiaDampenerOn;
        }
        if (controls.toggleHover.down) {
          hoverOn = !hoverOn;
        }
        if (controls.toggleGyro.down) {
          gyroOn = !gyroOn;
        }
      }
      if (inertiaDampenerOn) {
        body.drag = inertiaDampenerDrag;
      }
      else {
        body.drag = 0;
      }
      if (gyroOn) {
        body.angularDrag = gyroDrag;
      }
      else {
        body.angularDrag = 0;
      }
    }
  }

}
