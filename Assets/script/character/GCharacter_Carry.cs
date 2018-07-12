using UnityEngine;
using UnityEngine.Networking;

partial class GCharacter : GBody {

  [Header("Carry")]
  public Transform carryAnchor;         // the point in local space where carried objects are held


  public Carryable carried { get; private set; }  // the object currently being carried

  public bool isCarrying => carried != null;

  SyncRef<Carryable> carriedRef;

  public Vector3 GetCarryPosition(Carryable obj) {
    return carryAnchor.position + carryAnchor.forward * obj.gbody.boundingRadius;
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
