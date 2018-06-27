using UnityEngine;
using UnityEngine.Networking;

public class VTOLShip : Vehicle {

  // the thrusters this ship uses
  public ThrusterGroup thrusters;

  // all thrust values are in m/s2 (mass independant)
  public float thrust;                  // forward/backward
  public float thrustVertical;          // up/down
  public float thrustStrafe;            // left/right

  // roll rate in radians/s2
  public float roll = 5f;

  // drag value to use for inertia dampening
  public float inertiaDampenerDrag = 2.5f;
  // drag value to use for gyro
  public float gyroDrag = 5f;

  public float attitudeControlForce = 1f;

  public bool hover { get; private set; }
  public bool inertiaDampener { get; private set; }
  public bool gyro { get; private set; }

  public Rigidbody body => gbody.body;

  public override void OnStartServer() {
    base.OnStartServer();
    inertiaDampener = true;
    gyro = true;
  }

  protected override void OnSync(NetworkSync sync) {
    sync.Sync(thrusters);
  }

  void FixedUpdate() {
    if (isServer) {
      GField gfield = gbody.gfield;
      Vector3 gravity = Vector3.zero;
      if (gfield != null) {
        gravity = gfield.WorldPointToGravity(transform.position);
      }
      if (hover) {
        float dot = Vector3.Dot(-gravity, transform.up);
        if (dot >= 0) {
          thrusters.AddLinearThrust(Vector3.Project(-gravity, transform.up));
        }
      }
      if (isDriven) {
        // apply thrust forces
        thrusters.AddLinearThrust(transform.forward * controls.thrust.value * thrust);
        thrusters.AddLinearThrust(transform.right * controls.strafe.value * thrustStrafe);
        thrusters.AddLinearThrust(-transform.up * controls.lift.value * thrustVertical);
        // apply roll torque
        thrusters.AddAngularThrust(transform.forward * controls.roll.value * roll);
        // apply attitude control
        if (controls.steeringEnabled) {
          Vector3 attitudeTarget = controls.attitudeTarget;
          //get the angle between transform.forward and target delta
          float angleDiff = Vector3.Angle(transform.forward, attitudeTarget);
          // get its cross product, which is the axis of rotation to
          // get from one vector to the other
          Vector3 cross = Vector3.Cross(transform.forward, attitudeTarget);
          // apply torque along that axis according to the magnitude of the angle.
          thrusters.AddAngularThrust(cross * angleDiff * attitudeControlForce);
        }
      }
      thrusters.ApplyThrust(body);
    }
  }

  void Update() {
    if (isServer) {
      if (isDriven) {
        if (controls.toggleDampener.down) {
          inertiaDampener = !inertiaDampener;
        }
        if (controls.toggleHover.down) {
          hover = !hover;
        }
        if (controls.toggleGyro.down) {
          gyro = !gyro;
        }
      }
      if (inertiaDampener) {
        body.drag = inertiaDampenerDrag;
      }
      else {
        body.drag = 0;
      }
      if (gyro) {
        body.angularDrag = gyroDrag;
      }
      else {
        body.angularDrag = 0;
      }
    }
  }

}
