using UnityEngine;
using UnityEngine.Networking;

// a hotbar that spawns a different gbody
public class GBodySpawnHotbar : KittyNetworkBehaviour {

  public GBody prefab1;
  public GBody prefab2;
  public GBody prefab3;
  public GBody prefab4;
  public GBody prefab5;
  public GBody prefab6;
  public GBody prefab7;
  public GBody prefab8;
  public GBody prefab9;
  public GBody prefab0;

  public float spawnHeight = 1;

  public Player player { get; private set; }

  protected override void Awake() {
    base.Awake();
    player = GetComponent<Player>();
    if (prefab1 != null) ClientScene.RegisterPrefab(prefab1.gameObject);
    if (prefab2 != null) ClientScene.RegisterPrefab(prefab2.gameObject);
    if (prefab3 != null) ClientScene.RegisterPrefab(prefab3.gameObject);
    if (prefab4 != null) ClientScene.RegisterPrefab(prefab4.gameObject);
    if (prefab5 != null) ClientScene.RegisterPrefab(prefab5.gameObject);
    if (prefab6 != null) ClientScene.RegisterPrefab(prefab6.gameObject);
    if (prefab7 != null) ClientScene.RegisterPrefab(prefab7.gameObject);
    if (prefab8 != null) ClientScene.RegisterPrefab(prefab8.gameObject);
    if (prefab9 != null) ClientScene.RegisterPrefab(prefab9.gameObject);
    if (prefab0 != null) ClientScene.RegisterPrefab(prefab0.gameObject);
  }

  void Update() {
    if (isLocalPlayer) {
      int prefabIndex = -1;
      if (Input.GetKeyDown(KeyCode.Alpha1)) prefabIndex = 1;
      if (Input.GetKeyDown(KeyCode.Alpha2)) prefabIndex = 2;
      if (Input.GetKeyDown(KeyCode.Alpha3)) prefabIndex = 3;
      if (Input.GetKeyDown(KeyCode.Alpha4)) prefabIndex = 4;
      if (Input.GetKeyDown(KeyCode.Alpha5)) prefabIndex = 5;
      if (Input.GetKeyDown(KeyCode.Alpha6)) prefabIndex = 6;
      if (Input.GetKeyDown(KeyCode.Alpha7)) prefabIndex = 7;
      if (Input.GetKeyDown(KeyCode.Alpha8)) prefabIndex = 8;
      if (Input.GetKeyDown(KeyCode.Alpha9)) prefabIndex = 9;
      if (Input.GetKeyDown(KeyCode.Alpha0)) prefabIndex = 0;
      if (prefabIndex >= 0) {
        InteractionManager im = InteractionManager.instance;
        RaycastHit hit = im.targetHit;
        if (im.isTargetValid) {
          Vector3 point = hit.point + (hit.normal.normalized * spawnHeight);
          CmdSpawnPrefab(prefabIndex, point);
        }
      }
    }
  }

  [Command] void CmdSpawnPrefab(int prefabIndex, Vector3 point) {
    GBody prefab = null;
    switch (prefabIndex) {
      case 1: prefab = prefab1; break;
      case 2: prefab = prefab2; break;
      case 3: prefab = prefab3; break;
      case 4: prefab = prefab4; break;
      case 5: prefab = prefab5; break;
      case 6: prefab = prefab6; break;
      case 7: prefab = prefab7; break;
      case 8: prefab = prefab8; break;
      case 9: prefab = prefab9; break;
      case 0: prefab = prefab0; break;
    }
    if (prefab != null) {
      GBody gbody = Instantiate(prefab);
      gbody.transform.position = point;
      gbody.transform.up = -player.gfield.WorldPointToGravity(point);
      if (gbody.identity.localPlayerAuthority) {
        NetworkServer.SpawnWithClientAuthority(gbody.gameObject, player.gameObject);
      }
      else {
        NetworkServer.Spawn(gbody.gameObject);
      }
      gbody.SetGField(player.gfield);
    }
  }

}
