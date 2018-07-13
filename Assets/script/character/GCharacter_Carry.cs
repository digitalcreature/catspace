using UnityEngine;
using UnityEngine.Networking;

partial class GCharacter : GBody {

  [Header("Carry")]
  public CarryFields carry;

  [System.Serializable]
  public class CarryFields {
    public Transform anchor;               // the point in local space where carried objects are held
    public float liftLimit = 0.5f;         // the highest that the carried object can be raised to avoid terrain
    public float liftSpacing = 0.15f;      // the size of the gap between the bottom of the carried object and the ground
    public float liftSmoothTime = 0.15f;
    public float liftRadiusBuffer = 0.15f; // how much larger should the radius used for the spherecast be? (adds an extra buffer)

    public float angularSpring = 5;

    [HideInInspector] public float liftVelocity = 0;
    [HideInInspector] public float lift = 0;

  }

  public Carryable carried { get; private set; }  // the object currently being carried

  public bool isCarrying => carried != null;

  SyncRef<Carryable> carriedRef;


  public Vector3 GetCarryPosition(Carryable obj) {
    Vector3 pos = carry.anchor.position + carry.anchor.forward * obj.boundingRadius;
    float radius = obj.boundingRadius + carry.liftRadiusBuffer;
    Vector3 up = transform.up;
    float liftTarget;
    RaycastHit hit;
    if (Physics.SphereCast(pos + up * carry.liftLimit, radius, -up, out hit, carry.liftLimit + carry.liftSpacing, groundMask)) {
      liftTarget = carry.liftLimit - (hit.distance - carry.liftSpacing) - carry.liftRadiusBuffer;
    }
    else {
      liftTarget = 0;
    }
    carry.lift = Mathf.SmoothDamp(carry.lift, liftTarget, ref carry.liftVelocity, carry.liftSmoothTime);
    return pos + up * carry.lift;
  }

  public Quaternion GetCarryRotation(Carryable obj) {
    return carry.anchor.rotation;
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
