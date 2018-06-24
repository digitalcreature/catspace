using UnityEngine;
using UnityEngine.Networking;

public class Planet : KittyNetworkBehaviour {

  public GField gfield { get; private set; }
  public TerrainBase terrain { get; private set; }

  protected override void Awake() {
    base.Awake();
    gfield = GetComponent<GField>();
    terrain = GetComponent<TerrainBase>();
  }

  void Start() {
    terrain.Generate();
  }

}
