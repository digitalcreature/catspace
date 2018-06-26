using UnityEngine;
using UnityEngine.Networking;

public abstract class KittyMessageBase : MessageBase {

  public override void Serialize(NetworkWriter writer)
    { OnSync(new NetworkSync(writer)); }
  public override void Deserialize(NetworkReader reader)
    { OnSync(new NetworkSync(reader)); }

  // override and implement to customize syncronization
  protected abstract void OnSync(NetworkSync sync);

}
