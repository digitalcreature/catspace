using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

// a rigidbody that experiences the physics of a gravitational field
public class GBody : NetworkBehaviour {

  // the radius used when checking if a body should be loaded/unloaded
  // this should contain every collider in the body
  public float loadRadius = 0.5f;

  public static HashSet<GBody> loaded { get; private set; } = new HashSet<GBody>();

  public bool isLoaded => loaded.Contains(this);

  public GField gfield { get; private set; }  // the gravity field this object is in
  [SyncVar] NetworkInstanceId gfieldId;

  public Rigidbody body { get; private set; }
  public Vector3 gravity =>
    gfield == null ? Vector3.zero : gfield.WorldPointToGravity(transform.position);

  public NetworkIdentity identity { get; private set; }
  public NetworkTransform netTransform { get; private set; }

  protected virtual void Awake() {
    body = GetComponent<Rigidbody>();
    identity = GetComponent<NetworkIdentity>();
    netTransform = GetComponent<NetworkTransform>();
  }

  public override void OnStartAuthority() {
    Load();
  }

  protected virtual void FixedUpdate() {
    if (hasAuthority) {
      AddGravity();
    }
  }

  protected void AddGravity() {
    if (body.useGravity) {
      body.AddForce(gravity, ForceMode.Acceleration);
    }
  }

  // set the gravity field this body responds to
  // must be called from server or local player
  public void SetGField(GField gfield) {
    if (isServer) {
      SetGFieldLocal(gfield);
      NetworkInstanceId gfieldId = gfield.GetInstanceId();
      if (gfieldId != this.gfieldId) {
        this.gfieldId = gfieldId;
      }
    }
    else if (isLocalPlayer) {
      CmdSetGField(gfield.GetInstanceId());
    }
  }

  [Command] void CmdSetGField(NetworkInstanceId gfieldId)
    => SetGField(gfieldId.FindLocalObject<GField>());

  void SetGFieldLocal(GField gfield) {
    if (gfield != this.gfield) {
      this.gfield = gfield;
      if (gfield != null) {
        transform.parent = gfield.transform;
      }
      else {
        transform.parent = null;
      }
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool init) {
    writer.Write(gfieldId);
    return true;
  }

  public override void OnDeserialize(NetworkReader reader, bool init) {
    gfieldId = reader.ReadNetworkId();
    GField gfield = gfieldId.FindLocalObject<GField>();
    SetGFieldLocal(gfield);
  }

  public void Load() {
    loaded.Add(this);
    body.isKinematic = false;
  }

  public void Unload() {
    loaded.Remove(this);
    body.isKinematic = true;
  }

  protected virtual void OnDrawGizmosSelected() {
    Color c = Gizmos.color;
    Gizmos.color = new Color(1f, 0.5f, 0.75f);
    Gizmos.DrawWireSphere(transform.position, loadRadius);
    Gizmos.color = c;
  }

}
