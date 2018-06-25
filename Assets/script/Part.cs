using UnityEngine;
using UnityEngine.Networking;

public class Part : KittyNetworkBehaviour {

  public PartChild parent { get; private set; }

  // must be called from server
  // only called once, when first spawned
  public void SetParent(PartRoot root, int childId) {
    if (isServer) {
      SetParentLocal(root, childId);
      ServerSync();
    }
  }

  void SetParentLocal(PartRoot root, int childId) {
    if (root != null) {
      PartChild parent = root.children[childId];
      if (parent != this.parent) {
        this.parent = parent;
        transform.parent = parent.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
      }
    }
  }

  protected override void OnSync(NetworkSync sync) {
    if (sync.isWriting) {
      sync.Write(parent == null ? null : parent.root);
      sync.Write(parent == null ? -1 : parent.childId);
    }
    else {
      PartRoot root = null;
      int childId = 0;
      sync.ReadBehaviour(ref root);
      sync.Read(ref childId);
      SetParentLocal(root, childId);
    }
  }

}
