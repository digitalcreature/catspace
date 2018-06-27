using UnityEngine;
using System.Collections.Generic;

public class Thruster : MonoBehaviour, INetworkSyncable {

  public float throttle;
  public float throttleSmoothTime = 0.25f;

  public float maxThrust = 15f; // the maximum thrust that this thruster can give (m/s2)

  ThrusterEffect[] effects;

  public Vector3 thrustDirection => transform.forward;

  float targetThrottle;
  float throttleVelocity;

  void Awake() {
    effects = GetComponentsInChildren<ThrusterEffect>();
  }

  void Update() {
    throttle = Mathf.SmoothDamp(throttle, targetThrottle, ref throttleVelocity, throttleSmoothTime);
    foreach (var effect in effects) {
      effect.throttle = throttle;
    }
  }

  public virtual void OnSync(NetworkSync sync) {
    sync.Sync(ref throttle);
  }

  public float ThrustForThrottle(float throttle) =>
    Mathf.Lerp(0, maxThrust, throttle);
  public float ThrottleForThrust(float thrust) =>
    Mathf.InverseLerp(0, maxThrust, thrust) ;

  public void SetTargetThrust(Vector3 totalThrust) {
    // figure out how much thrust we need to give in the direction that we are facing
    float thrust = Vector3.Dot(thrustDirection, -totalThrust);
    targetThrottle = ThrottleForThrust(thrust);
  }


}

[System.Serializable]
public class ThrusterGroup : INetworkSyncable {

  public List<Thruster> thrusters;

  // acceleration values
  Vector3 linearAcc;
  Vector3 angularAcc;

  public void ApplyThrust(Rigidbody body) {
    body.AddForce(linearAcc, ForceMode.Acceleration);
    body.AddTorque(angularAcc, ForceMode.Acceleration);
    foreach (var thruster in thrusters) {
      thruster.SetTargetThrust(linearAcc);
    }
    linearAcc = Vector3.zero;
    angularAcc = Vector3.zero;
  }

  public void AddLinearThrust(Vector3 acceleration) {
    linearAcc += acceleration;
  }

  public void AddAngularThrust(Vector3 acceleration) {
    angularAcc += acceleration;
  }

  public void OnSync(NetworkSync sync) {
    foreach (var thruster in thrusters) {
      sync.Sync(thruster);
    }
  }

}
