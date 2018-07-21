using UnityEngine;
using UnityEngine.Networking;

partial class GCharacter : GBody {

  [Header("Carry")]
  public CarryFields carry;

  [System.Serializable]
  public class CarryFields {
    public Transform anchor;                // the point in local space where carried objects are held
    public float liftLimit = 0.5f;          // the highest that the carried object can be raised to avoid terrain
    public float liftSpacing = 0.15f;       // the size of the gap between the bottom of the carried object and the ground

    public float linearSpring = 15;
    public float angularSpring = 15;

    public float angularSnapThreshold = 90; // if the anchor turns more than this many degrees per second, snap the rotation

  }

  public Carryable carried { get; private set; }  // the object currently being carried

  public bool isCarrying => carried != null;

  SyncRef<Carryable> carriedRef;


  public Vector3 GetCarryPosition(Carryable obj) {
    float radius = obj.boundingRadius;
    Vector3 pos = carry.anchor.position + carry.anchor.forward * radius;
    Vector3 up = transform.up;
    float lift;
    RaycastHit hit;
    if (Physics.SphereCast(pos + up * carry.liftLimit, radius, -up, out hit, carry.liftLimit + carry.liftSpacing, groundMask)) {
      lift = carry.liftLimit - (hit.distance - carry.liftSpacing);
    }
    else {
      lift = 0;
    }
    return pos + up * lift;
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
        if (CameraRig.instance.isThirdPerson) {
          Vector3 directionTo = newObj.transform.position - transform.position;
          facingDirection = directionTo.normalized;
          SnapFacingDirection();
        }
      }
    }
  }

  public void DropCarried() => Carry(null);

}
