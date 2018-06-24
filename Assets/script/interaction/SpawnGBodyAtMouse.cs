using UnityEngine;
using UnityEngine.Networking;

public class SpawnGBodyAtMouse : KittyNetworkBehaviour {

  public GBody prefab;
  public float spawnHeight = 1;

  public Player player { get; private set; }

  protected override void Awake() {
    base.Awake();
    player = GetComponent<Player>();
  }

  void Update() {
    if (isLocalPlayer) {
      if (Input.GetMouseButtonDown((int) MouseButton.Left)) {
        InteractionManager im = InteractionManager.instance;
        RaycastHit hit = im.targetHit;
        if (im.isTargetValid) {
          Vector3 point = hit.point + (hit.normal.normalized * spawnHeight);
          CmdSpawnPrefab(point);
        }
      }
    }
  }

  [Command]
  void CmdSpawnPrefab(Vector3 point) {
    GBody gbody = Instantiate(prefab);
    gbody.transform.position = point;
    if (gbody.identity.localPlayerAuthority) {
      NetworkServer.SpawnWithClientAuthority(gbody.gameObject, player.gameObject);
    }
    else {
      NetworkServer.Spawn(gbody.gameObject);
    }
    gbody.SetGField(player.gfield);
  }

}
