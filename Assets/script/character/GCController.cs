using UnityEngine;
using UnityEngine.Networking;

public abstract class GCController : KittyNetworkBehaviour {

  public GCharacter character { get; private set; }
  public GField gfield => character.gfield;
  public Rigidbody body => character.body;

  protected override void Awake() {
    base.Awake();
    character = GetComponent<GCharacter>();
  }

}
