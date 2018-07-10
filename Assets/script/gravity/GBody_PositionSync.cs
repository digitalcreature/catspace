using UnityEngine;
using UnityEngine.Networking;

partial class GBody : KittyNetworkBehaviour {

  [Header("Position Sync")]
  public bool positionSyncEnabled = true;
  public float syncRate = 10;           //  updates per second

  float lastSyncTime;

  PositionSync lastPSync;
  PositionSync nextPSync;

  void AwakePositionSync() {
    lastPSync = nextPSync = PositionSync.Read(this);
  }

  void UpdatePositionSync() {
    if (positionSyncEnabled) {
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
  }

  void SyncPositionSync(NetworkSync sync) {
    sync.Sync(ref positionSyncEnabled);
    bool isKinematic = body.isKinematic;
    sync.Sync(ref isKinematic);
    body.isKinematic = isKinematic;
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
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public static PositionSync Read(GBody gbody) {
      return new PositionSync() {
        position = gbody.body.position,
        rotation = gbody.body.rotation,
        velocity = gbody.body.velocity,
        angularVelocity = gbody.body.angularVelocity
      };
    }

    public void Write(GBody gbody) {
      gbody.body.MovePosition(position);
      gbody.body.MoveRotation(rotation);
      gbody.body.velocity = velocity;
      gbody.body.angularVelocity = angularVelocity;
    }

    public static PositionSync Lerp(PositionSync a, PositionSync b, float t) {
      t = Mathf.Clamp01(t);
      PositionSync c = a;
      c.position = Vector3.Lerp(a.position, b.position, t);
      c.rotation = Quaternion.Lerp(a.rotation, b.rotation, t);
      c.velocity = Vector3.Lerp(a.velocity, b.velocity, t);
      c.angularVelocity = Vector3.Lerp(a.angularVelocity, b.angularVelocity, t);
      return c;
    }

  }

}
