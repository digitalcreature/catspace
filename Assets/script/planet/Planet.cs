using UnityEngine;
using UnityEngine.Networking;

public class Planet : NetworkBehaviour {

  public GField gfield { get; private set; }
  public TerrainBase terrain { get; private set; }

  protected virtual void Awake() {
    gfield = GetComponent<GField>();
    terrain = GetComponent<TerrainBase>();
  }

  void Start() {
    terrain.Generate();
  }

}
