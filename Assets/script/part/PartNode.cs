using UnityEngine;
using UnityEngine.Networking;
using System;

public class PartNode : MonoBehaviour {

  public Part spawnPart;  // optional part to spawn when the parent part spawns

  public Part attachedPart { get; private set; }

  public Part parent { get; private set; }
  public int childId { get; private set; }

  public event Action<Part> EventOnChildSpawnedLocal;

  public void Initialize(Part parent, int childId) {
    this.parent = parent;
    this.childId = childId;
  }

  // must be called on server
  public void SpawnChild() {
    if (spawnPart != null) {
      this.childId = childId;
      Part part = Instantiate(spawnPart);
      part.transform.position = transform.position;
      part.transform.rotation = transform.rotation;
      NetworkServer.Spawn(part.gameObject);
      part.AttachToParent(this);
    }
  }

  public void OnAttachChildLocal(Part part) {
    if (EventOnChildSpawnedLocal != null) EventOnChildSpawnedLocal(part);
    attachedPart = part;
  }

}
