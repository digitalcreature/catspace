using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Player : KittyNetworkBehaviour {

  public GCharacter chr { get; private set; }
  public CameraRigFocusable focus { get; private set; }

  public Rigidbody body => chr.body;
  public GField gfield => chr.gfield;

  public static Player localPlayer { get; private set; }

  public Examinable backpack;

  protected override void Awake() {
    base.Awake();
    chr = GetComponent<GCharacter>();
    focus = GetComponent<CameraRigFocusable>();
  }

  public override void OnStartLocalPlayer() {
    localPlayer = this;
    // set the rig to target the player
    CameraRig rig = CameraRig.instance;
    rig.SetFocus(focus);
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

}
