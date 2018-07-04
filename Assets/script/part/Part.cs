using UnityEngine;
using UnityEngine.Networking;

// this, and its companion class PartNode, allow us to get around the "no nested NetworkIdentity's" problem
// it also gives us something akin to nested prefabs, but with no ux to be seen whatsoever
public class Part : KittyNetworkBehaviour {

  public Part parent { get; private set; }
  public PartNode parentNode { get; private set; }

  public PartNode[] childNodes { get; private set; }

  protected override void Awake() {
    base.Awake();
    childNodes = GetComponentsInChildren<PartNode>();
    for (int i = 0; i < childNodes.Length; i ++) {
      childNodes[i].Initialize(this, i);
    }
  }

  public override void OnStartServer() {
    base.OnStartServer();
    foreach (var node in childNodes) {
      node.SpawnChild();
    }
  }

  // must be called from server
  // attach this part to a part node. if node is left null, detach from current parent
  public void AttachToParent(PartNode node) {
    if (isServer) {
      if (node == null) {
        AttachToParentLocal(null, -1);
      }
      else {
        AttachToParentLocal(node.parent, node.childId);
      }
      ServerSync();
    }
  }

  void AttachToParentLocal(Part parent, int childId) {
    this.parent = parent;
    PartNode parentNode;
    if (parent == null) {
      parentNode = null;
    }
    else {
      parentNode = parent.childNodes[childId];
    }
    if (parentNode != this.parentNode) {
      var oldNode = this.parentNode;
      var newNode = parentNode;
      this.parentNode = parentNode;
      if (oldNode != null) {
        oldNode.OnAttachChildLocal(null);
      }
      if (newNode != null) {
        transform.parent = newNode.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        newNode.OnAttachChildLocal(this);
      }
      else {
        transform.parent = null;
      }
    }
  }

  protected override void OnSync(NetworkSync sync) {
    if (sync.isWriting) {
      sync.Write(parentNode == null ? null : parentNode.parent);
      sync.Write(parentNode == null ? -1 : parentNode.childId);
    }
    else {
      Part parent = null;
      int childId = 0;
      sync.ReadBehaviour(ref parent);
      sync.Read(ref childId);
      AttachToParentLocal(parent, childId);
    }
  }

}
