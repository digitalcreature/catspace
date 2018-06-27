using UnityEngine;
using System.Collections.Generic;

public class Thruster : MonoBehaviour, INetworkSyncable {

  public float throttleSmoothTime = 0.25f;
  public float maxThrust = 15f; // the maximum thrust that this thruster can give (m/s2)

  public float throttle { get; private set; }
  public Vector3 thrustDirection => transform.forward;

  ThrusterEffect[] effects;

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
    sync.Sync(ref targetThrottle);
  }

  public float ThrustForThrottle(float throttle) =>
    Mathf.Lerp(0, maxThrust, throttle);
  public float ThrottleForThrust(float thrust) =>
    Mathf.InverseLerp(0, maxThrust, thrust) ;

  // return true if any values changed that need to be syncronized over the network
  public bool SetTargetThrust(Vector3 totalThrust) {
    // figure out how much thrust we need to give in the direction that we are facing
    float thrust = Vector3.Dot(thrustDirection, -totalThrust);
    float targetThrottle = ThrottleForThrust(thrust);
    if (targetThrottle != this.targetThrottle) {
      this.targetThrottle = targetThrottle;
      return true;
    }
    return false;
  }


}

[System.Serializable]
public class ThrusterGroup : INetworkSyncable {

  public List<Thruster> thrusters;

  // acceleration values
  Vector3 linearAcc;
  Vector3 angularAcc;

  // return true if any values changed that need to be syncronized over the network
  public bool ApplyThrust(Rigidbody body) {
    body.AddForce(linearAcc, ForceMode.Acceleration);
    body.AddTorque(angularAcc, ForceMode.Acceleration);
    bool dirty = false;
    foreach (var thruster in thrusters) {
      dirty = thruster.SetTargetThrust(linearAcc) || dirty;
    }
    linearAcc = Vector3.zero;
    angularAcc = Vector3.zero;
    return dirty;
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