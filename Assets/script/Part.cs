using UnityEngine;
using UnityEngine.Networking;

// this, and its companion class PartSpawn, allow us to get around the "no nested NetworkIdentity's" problem
// it also gives us something akin to nested prefabs, but with no ux to be seen whatsoever
public class Part : KittyNetworkBehaviour {

  public PartSpawn spawn { get; private set; }

  public PartSpawn[] childSpawns { get; private set; }

  protected override void Awake() {
    base.Awake();
    childSpawns = GetComponentsInChildren<PartSpawn>();
  }

  public override void OnStartServer() {
    base.OnStartServer();
    for (int i = 0; i < childSpawns.Length; i ++) {
      PartSpawn childSpawn = childSpawns[i];
      childSpawn.SpawnChild(i);
    }
  }

  // must be called from server
  // only called once, when first spawned
  public void AttachToParent(Part parent, int childId) {
    if (isServer) {
      AttachToParentLocal(parent, childId);
      ServerSync();
    }
  }

  void AttachToParentLocal(Part parent, int childId) {
    if (parent != null) {
      PartSpawn spawn = parent.childSpawns[childId];
      if (spawn != this.spawn) {
        this.spawn = spawn;
        transform.parent = spawn.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
      }
    }
  }

  protected override void OnSync(NetworkSync sync) {
    if (sync.isWriting) {
      sync.Write(spawn == null ? null : spawn.parent);
      sync.Write(spawn == null ? -1 : spawn.childId);
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
