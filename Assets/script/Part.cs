using UnityEngine;
using UnityEngine.Networking;

public class Part : NetworkBehaviour {

  public PartChild parent { get; private set; }
  [SyncVar] NetworkInstanceId rootId;
  [SyncVar] int childId;

  // must be called from server
  // only called once, when first spawned
  public void SetParent(PartRoot root, int childId) {
    if (isServer) {
      SetParentLocal(root, childId);
      this.rootId = root.GetInstanceId();
      this.childId = childId;
    }
  }

  void SetParentLocal(PartRoot root, int childId) {
    PartChild parent = root.children[childId];
    if (parent != this.parent) {
      this.parent = parent;
      transform.parent = parent.transform;
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll) {
    writer.Write(rootId);
    writer.Write(childId);
    return true;
  }

  public override void OnDeserialize(NetworkReader reader, bool forceAll) {
    rootId = reader.ReadNetworkId();
    childId = reader.ReadInt32();
    PartRoot root = rootId.FindLocalObject<PartRoot>();
    if (root != null) {
      SetParentLocal(root, childId);
    }
  }

}
