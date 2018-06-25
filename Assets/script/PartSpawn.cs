using UnityEngine;
using UnityEngine.Networking;

public class PartSpawn : MonoBehaviour {

  public Part partPrefab;

  public Part parent { get; private set; }

  public int childId { get; private set; }

  void Awake() {
    parent = GetComponentInParent<Part>();
  }

  // must be called on server
  public void SpawnChild(int childId) {
    this.childId = childId;
    Part part = Instantiate(partPrefab);
    part.transform.position = transform.position;
    part.transform.rotation = transform.rotation;
    NetworkServer.Spawn(part.gameObject);
    part.AttachToParent(parent, childId);
  }

}
