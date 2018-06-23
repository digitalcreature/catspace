using UnityEngine;
using UnityEngine.Networking;

public class KittyNetworkManager : NetworkManager {

  public Planet planetPrefab;

  public Planet planet { get; private set; }

  void Awake() {
    ClientScene.RegisterPrefab(planetPrefab.gameObject);
  }

  public override void OnStartServer() {
    base.OnStartServer();
    planet = Instantiate(planetPrefab);
  }

  public override void OnServerConnect(NetworkConnection conn) {
    base.OnServerConnect(conn);
    NetworkServer.Spawn(planet.gameObject);
  }

  public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
    Player player = Instantiate(playerPrefab, Vector3.up, Quaternion.identity).GetComponent<Player>();
    NetworkServer.AddPlayerForConnection(conn, player.gameObject, playerControllerId);
    player.chr.SetGField(planet.gfield);
  }

  public override void OnStopServer() {
    Destroy(planet.gameObject);
    planet = null;
    base.OnStopServer();
  }

}
