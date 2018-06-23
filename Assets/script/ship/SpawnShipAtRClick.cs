using UnityEngine;
using UnityEngine.Networking;

public class SpawnShipAtRClick : NetworkBehaviour {

  public bool spawned = false;
  public Ship prefab;
  public float spawnHeight = 1;


  public Player player { get; private set; }

  void Awake() {
    player = GetComponent<Player>();
  }

  void Update() {
    if (isLocalPlayer) {
      if ((Input.GetMouseButtonDown((int) MouseButton.Right)) && !spawned) {
        RaycastHit hit;
        if (InteractionManager.instance.MousePositionToObject(out hit)) {
          Vector3 point = hit.point + (hit.normal.normalized * spawnHeight);
          CmdSpawnPrefab(point);
          spawned = true;
        }
      }
    }
  }

  [Command]
  void CmdSpawnPrefab(Vector3 point) {
    Ship ship = Instantiate(prefab);
    ship.transform.position = point;
    NetworkServer.SpawnWithClientAuthority(ship.gameObject, player.gameObject);
    ship.SetOwner(player);
  }

}
