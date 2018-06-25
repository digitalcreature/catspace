using UnityEngine;
using UnityEngine.Networking;

public class PartRoot : KittyNetworkBehaviour {

  public PartChild[] children { get; private set; }

  protected override void Awake() {
    base.Awake();
    children = GetComponentsInChildren<PartChild>();
  }

  public override void OnStartServer() {
    base.OnStartServer();
    for (int i = 0; i < children.Length; i ++) {
      PartChild child = children[i];
      child.SpawnChild(i);
    }
  }

}
