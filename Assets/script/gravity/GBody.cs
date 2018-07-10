using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

// a rigidbody that experiences the physics of a gravitational field
public partial class GBody : KittyNetworkBehaviour {

  // the radius used when checking if a body should be loaded/unloaded
  // this should contain every collider in the body
  public float loadRadius = 0.5f;

  public static HashSet<GBody> loaded { get; private set; } = new HashSet<GBody>();

  public bool isLoaded => loaded.Contains(this);

  public GField gfield { get; private set; }  // the gravity field this object is in

  public Rigidbody body { get; private set; }
  public Vector3 gravity =>
    gfield == null ? Vector3.zero : gfield.WorldPointToGravity(transform.position);


  protected override void Awake() {
    base.Awake();
    body = GetComponent<Rigidbody>();
    AwakePositionSync();
  }

  public override void OnStartAuthority() {
    Load();
  }

  protected virtual void FixedUpdate() {
    if (hasAuthority) {
      AddGravity();
    }
  }

  protected virtual void Update() {
    UpdatePositionSync();
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
      ServerSync();
    }
    else if (isLocalPlayer) {
      CmdSetGField(gfield.Id());
    }
  }

  [Command] void CmdSetGField(Id gfieldId) => SetGField(gfieldId.Find<GField>());

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

  protected override void OnSync(NetworkSync sync) {
    GField gfield = this.gfield;
    sync.SyncBehaviour(ref gfield);
    if (sync.isReading) {
      SetGFieldLocal(gfield);
    }
    SyncPositionSync(sync);
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
