using UnityEngine;
using UnityEngine.Networking;

public class Interactable : KittyNetworkBehaviour {

  public bool isInteractable = true;

  public MeshFilter[] glowMeshFilters;  // the mesh filters to use for the glow effect

  // subscribe to this event to respond to interaction
  // this function is called once on the server
  // and then once on every client connected
  public event System.Action<GCharacter, InteractionMode> EventOnInteractLocal;

  // the interaction manager, for convenience
  public InteractionManager manager
    => InteractionManager.instance;

  // protected override void Awake() {
  //   base.Awake();
  // }

  public void DrawHighlight(Material highlightMaterial, Camera camera = null) {
    foreach (MeshFilter filter in glowMeshFilters) {
      if (filter != null) {
        Mesh mesh = filter.mesh;
        Matrix4x4 matrix = filter.transform.localToWorldMatrix;
        Graphics.DrawMesh(mesh, matrix, highlightMaterial, gameObject.layer, camera);
      }
    }
  }

  // this method is called when a player interacts with this object
  // only call on server!!! use GCharacter.Interact() to make a local player interact with something
  public void ServerInteract(GCharacter character, InteractionMode mode) {
    if (isServer) {
      if (character != null && isInteractable) {
        InteractLocal(character, mode);
        Id charId = character.Id();
        RpcInteract(charId, mode);
      }
    }
  }

  private void InteractLocal(GCharacter character, InteractionMode mode) {
    if (EventOnInteractLocal != null) {
      EventOnInteractLocal(character, mode);
    }
  }

  [ClientRpc]
  void RpcInteract(Id charId, InteractionMode mode) {
    // we only need to do this when on a client
    // if we are also the server, InteractLocal will already have been called in from ServerInteract!
    if (!isServer) {
      GCharacter character = charId.Find<GCharacter>();
      if (character != null) {
        InteractLocal(character, mode);
      }
    }
  }


  // override this to allow interaction
  // this function is called once on the server
  // and then once on every client connected
  // protected virtual void OnInteractLocal(GCharacter character) {}

}


public abstract class InteractableModule : KittyNetworkBehaviour {

  public Interactable interactable { get; private set; }

  // the interaction manager, for convenience
  public InteractionManager manager
    => InteractionManager.instance;

  protected override void Awake() {
    base.Awake();
    interactable = GetComponent<Interactable>();
    interactable.EventOnInteractLocal += OnInteractLocal;
  }

  protected abstract void OnInteractLocal(GCharacter character, InteractionMode mode);

}
