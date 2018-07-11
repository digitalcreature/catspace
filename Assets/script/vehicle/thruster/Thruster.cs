using UnityEngine;
using System.Collections.Generic;

public class Thruster : MonoBehaviour, INetworkSyncable {

  public float throttleSmoothTime = 0.25f;
  public float maxThrust = 15f; // the maximum thrust that this thruster can give (m/s2)

  public float torqueWeight = 1f;

  public float throttle { get; private set; }
  public float thrust => ThrustForThrottle(throttle);
  public Vector3 thrustDirection => transform.forward;

  ThrusterEffect[] effects;

  protected float targetThrottle;
  float throttleVelocity;

  public Vehicle vehicle { get; private set; }

  protected virtual void Awake() {
    vehicle = GetComponentInParent<Vehicle>();
    effects = GetComponentsInChildren<ThrusterEffect>();
  }

  protected virtual void Update() {
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
  public virtual bool SetTargetThrust(Vector3 totalThrust, Vector3 totalTorque) {
    // figure out the tangent force for torque
    totalThrust += GetTangentThrust(transform.position, totalTorque) * maxThrust * torqueWeight;
    // figure out how much thrust we need to give in the direction that we are facing
    float thrust = Vector3.Dot(thrustDirection, -totalThrust);
    float targetThrottle = ThrottleForThrust(thrust);
    if (targetThrottle != this.targetThrottle) {
      this.targetThrottle = targetThrottle;
      return true;
    }
    return false;
  }

  // given a world space position and a torque, find the tangent force (also world space)
  public Vector3 GetTangentThrust(Vector3 position, Vector3 torque) {
    Vector3 r = position - vehicle.centerOfMass;
    return Vector3.Cross(torque, r) / r.sqrMagnitude;
  }


}

[System.Serializable]
public class ThrusterGroup : INetworkSyncable {

  public List<Thruster> thrusters;

  // acceleration values
  Vector3 linearAcc;
  Vector3 angularAcc;

  // return true if any values changed that need to be syncronized over the network
  public bool ApplyThrust(GBody gbody) {
    if (gbody.hasPhysics) {
      gbody.body.AddForce(linearAcc, ForceMode.Acceleration);
      gbody.body.AddTorque(angularAcc, ForceMode.Acceleration);
      bool dirty = false;
      foreach (var thruster in thrusters) {
        dirty = thruster.SetTargetThrust(linearAcc, angularAcc) || dirty;
      }
      linearAcc = Vector3.zero;
      angularAcc = Vector3.zero;
      return dirty;
    }
    return false;
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
