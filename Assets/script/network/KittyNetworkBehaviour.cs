using UnityEngine;
using UnityEngine.Networking;

public abstract class KittyNetworkBehaviour : NetworkBehaviour {

  public NetworkIdentity identity { get; private set; }

  protected virtual void Awake() {
    identity = GetComponent<NetworkIdentity>();
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    { OnSync(new NetworkSync(writer)); return true; }
  public override void OnDeserialize(NetworkReader reader, bool forceAll)
    { OnSync(new NetworkSync(reader)); }

  // override and implement to customize syncronization
  protected virtual void OnSync(NetworkSync sync) {}

  // call this on the server to manually force syncronization
  public void ServerSync() {
    SetDirtyBit(~0u);
  }

}
