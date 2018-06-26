using UnityEngine;
using UnityEngine.Networking;

public class VTOLShip : Vehicle {

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

  // public void PivotTo(Vector3 position){
  //   Vector3 offset = transform.position - position;
  //   foreach (Transform child in transform)
  //     child.transform.position += offset;
  //   transform.position = position;
  // }

  void FixedUpdate() {
    if (isServer) {
      GField gfield = gbody.gfield;
      Vector3 gravity = Vector3.zero;
      if (gfield != null) {
        gravity = gfield.WorldPointToGravity(transform.position);
      }
      if (hover) {
        body.AddForce(-gravity, ForceMode.Acceleration);
      }
      if (isDriven) {
        // apply thrust forces
        body.AddForce(transform.forward * controls.thrust.value * thrust, ForceMode.Acceleration);
        body.AddForce(transform.right * controls.strafe.value * thrustStrafe, ForceMode.Acceleration);
        body.AddForce(-transform.up * controls.lift.value * thrustVertical, ForceMode.Acceleration);
        // apply roll torque
        body.AddTorque(transform.forward * controls.roll.value * roll, ForceMode.Acceleration);
        // apply attitude control
        Vector3 attitudeTarget = controls.attitudeTarget;
        //get the angle between transform.forward and target delta
        float angleDiff = Vector3.Angle(transform.forward, attitudeTarget);
        // get its cross product, which is the axis of rotation to
        // get from one vector to the other
        Vector3 cross = Vector3.Cross(transform.forward, attitudeTarget);
        // apply torque along that axis according to the magnitude of the angle.
        body.AddTorque(cross * angleDiff * attitudeControlForce, ForceMode.Acceleration);

      }
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
