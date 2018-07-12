using UnityEngine;
using UnityEngine.Networking;

partial class GCharacter : GBody {

  [Header("Carry")]
  public Transform carryAnchor;               // the point in local space where carried objects are held
  public float carryLiftLimit = 0.5f;         // the highest that the carried object can be raised to avoid terrain
  public float carryLiftSpacing = 0.15f;      // the size of the gap between the bottom of the carried object and the ground
  public float carryLiftSmoothTime = 0.15f;
  public float carryLiftRadiusBuffer = 0.15f; // how much larger should the radius used for the spherecast be? (adds an extra buffer)

  public Carryable carried { get; private set; }  // the object currently being carried

  public bool isCarrying => carried != null;

  SyncRef<Carryable> carriedRef;

  float carryLiftVelocity = 0;
  float carryLift = 0;

  public Vector3 GetCarryPosition(Carryable obj) {
    Vector3 pos = carryAnchor.position + carryAnchor.forward * obj.boundingRadius;
    float radius = obj.boundingRadius + carryLiftRadiusBuffer;
    Vector3 up = transform.up;
    float carryLiftTarget;
    RaycastHit hit;
    if (Physics.SphereCast(pos + up * carryLiftLimit, radius, -up, out hit, carryLiftLimit + carryLiftSpacing, groundMask)) {
      carryLiftTarget = carryLiftLimit - (hit.distance - carryLiftSpacing) - carryLiftRadiusBuffer;
    }
    else {
      carryLiftTarget = 0;
    }
    carryLift = Mathf.SmoothDamp(carryLift, carryLiftTarget, ref carryLiftVelocity, carryLiftSmoothTime);
    return pos + up * carryLift;
  }

  public Quaternion GetCarryRotation(Carryable obj) {
    return carryAnchor.rotation;
  }

  void Awake_Carry() {
    carriedRef = new SyncRef<Carryable>(this, CarryLocal);
  }

  void OnSync_Carry(NetworkSync sync) {
    carriedRef.Sync(sync, carried);
  }

  // make the character start carrying an object
  // only call from server or local player
  public void Carry(Carryable obj) {
    if (isServer) {
      if (obj != null && obj.isCarried) {
        obj.carrier.DropCarried();
      }
      CarryLocal(obj);
      ServerSync();
    }
    else {
      if (isLocalPlayer) {
        CmdCarry(obj.Id());
      }
    }
  }
  [Command] void CmdCarry(Id objId) => Carry(objId.Find<Carryable>());

  void CarryLocal(Carryable newObj) {
    Carryable oldObj = carried;
    if (oldObj != newObj) {
      carried = newObj;
      if (oldObj != null) {
        oldObj.OnCarryLocal(null);
      }
      if (newObj != null) {
        newObj.OnCarryLocal(this);
      }
    }
  }

  public void DropCarried() => Carry(null);

}
