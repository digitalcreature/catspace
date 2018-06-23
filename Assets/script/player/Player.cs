using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Player : NetworkBehaviour {

  public NetworkIdentity identity { get; private set; }
  public GCharacter chr { get; private set; }

  public Rigidbody body => chr.body;
  public GField gfield => chr.gfield;

  public static Player localPlayer { get; private set; }

  public Examinable backpack;

  void Awake() {
    chr = GetComponent<GCharacter>();
  }

  public override void OnStartLocalPlayer() {
    localPlayer = this;
    // set the rig to target the player
    CameraRig rig = CameraRig.instance;
    rig.Retarget(transform, gfield);
    StartCoroutine(PlaceOnNorthPoleSurfaceRoutine());
  }

  // quick and dirty: put this player on the north pole of the terrain it is attached to
  System.Collections.IEnumerator PlaceOnNorthPoleSurfaceRoutine() {
    while (gfield == null) {
      yield return null;
    }
    transform.position = gfield.WorldPointToSurface(gfield.transform.position + Vector3.up);
    TerrainBase terrain = gfield.GetComponent<TerrainBase>();
    if (terrain != null) {
      transform.position = terrain.WorldPointToSurface(terrain.transform.position + Vector3.up);
    }
  }

  void Update() {
    if (isLocalPlayer) {
      CameraRig.instance.SetGField(gfield);
    }
  }

  // assign this player's client as the authority for a NetworkIdentity
  public void AssignClientAuthority(NetworkIdentity identity) {
    if (isLocalPlayer) {
      CmdAssignClientAuthority(identity, this.identity);
    }
  }

  // remove this player's client as the authority for a NetworkIdentity
  public void RemoveClientAuthority(NetworkIdentity identity) {
    if (isLocalPlayer) {
      CmdRemoveClientAuthority(identity, this.identity);
    }
  }

  [Command]
  void CmdAssignClientAuthority(NetworkIdentity obj, NetworkIdentity auth) {
    obj.AssignClientAuthority(auth.connectionToClient);
  }

  [Command]
  void CmdRemoveClientAuthority(NetworkIdentity obj, NetworkIdentity auth) {
    obj.RemoveClientAuthority(auth.connectionToClient);
  }

}
