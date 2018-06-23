using UnityEngine;
using UnityEngine.Networking;

public class PartRoot : NetworkBehaviour {

  public PartChild[] children { get; private set; }

  void Awake() {
    children = GetComponentsInChildren<PartChild>();
  }

  public override void OnStartServer() {
    for (int i = 0; i < children.Length; i ++) {
      PartChild child = children[i];
      child.SpawnChild(i);
    }
  }

}
