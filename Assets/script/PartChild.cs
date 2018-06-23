using UnityEngine;
using UnityEngine.Networking;

public class PartChild : MonoBehaviour {

  public Part partPrefab;

  PartRoot root;

  void Awake() {
    root = GetComponentInParent<PartRoot>();
  }

  // must be called on server
  public void SpawnChild(int childId) {
    Part part = Instantiate(partPrefab);
    part.transform.position = transform.position;
    part.transform.rotation = transform.rotation;
    NetworkServer.Spawn(part.gameObject);
    part.SetParent(root, childId);
  }

}
