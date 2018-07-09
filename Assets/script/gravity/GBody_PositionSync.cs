using UnityEngine;
using UnityEngine.Networking;

partial class GBody : KittyNetworkBehaviour {

  [Header("Position Sync")]
  public float syncRate = 10;           //  updates per second

  float lastSyncTime;

  PositionSync lastPSync;
  PositionSync nextPSync;

  void AwakePositionSync() {
    lastPSync = nextPSync = PositionSync.Read(this);
  }

  void UpdatePositionSync() {
    if (hasAuthority) {
      while (Time.time - lastSyncTime > (1 / syncRate)) {
        lastSyncTime = Time.time;
        SyncPosition(PositionSync.Read(this));
      }
    }
    else {
      PositionSync.Lerp(lastPSync, nextPSync, (Time.time - lastSyncTime) * syncRate).Write(this);
    }
  }

  void SyncPosition(PositionSync psync) {
    if (isServer) {
      RpcSyncPosition(psync);
    }
    else if (isLocalPlayer) {
      CmdSyncPosition(psync);
    }
  }

  [Command] void CmdSyncPosition(PositionSync psync) {
    SyncPositionLocal(psync);
    RpcSyncPosition(psync);
  }

  [ClientRpc] void RpcSyncPosition(PositionSync psync) {
    if (!hasAuthority && !isServer) {
      SyncPositionLocal(psync);
    }
  }

  void SyncPositionLocal(PositionSync psync) {
    lastSyncTime = Time.time;
    lastPSync = nextPSync;
    nextPSync = psync;
  }

  struct PositionSync {

    public Vector3 position;
    public Quaternion rotation;

    public static PositionSync Read(GBody gbody) {
      return new PositionSync() {
        position = gbody.body.position,
        rotation = gbody.body.rotation
      };
    }

    public void Write(GBody gbody) {
      gbody.body.isKinematic = true;
      gbody.body.MovePosition(position);
      gbody.body.MoveRotation(rotation);
    }

    public static PositionSync Lerp(PositionSync a, PositionSync b, float t) {
      t = Mathf.Clamp01(t);
      PositionSync c = a;
      c.position = Vector3.Lerp(a.position, b.position, t);
      c.rotation = Quaternion.Lerp(a.rotation, b.rotation, t);
      return c;
    }

  }

}
