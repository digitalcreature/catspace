using UnityEngine;
using UnityEngine.Networking;

public class Interactable : KittyNetworkBehaviour {

  [SyncVar] public bool isInteractable = true;

  MeshFilter[] filters;

  // the interaction manager, for convenience
  public InteractionManager manager
    => InteractionManager.instance;

  protected override void Awake() {
    base.Awake();
    filters = GetComponentsInChildren<MeshFilter>();
  }

  public void DrawHighlight(Material highlightMaterial, Camera camera = null) {
    foreach (MeshFilter filter in filters) {
      if (filter != null) {
        Mesh mesh = filter.mesh;
        Matrix4x4 matrix = filter.transform.localToWorldMatrix;
        Graphics.DrawMesh(mesh, matrix, highlightMaterial, gameObject.layer, camera);
      }
    }
  }

  // this method is called when a player interacts with this object
  // only call on server!!! use GCharacter.Interact() to make a local player interact with something
  public void ServerInteract(GCharacter character) {
    if (isServer) {
      if (character != null && isInteractable) {
        InteractLocal(character);
        Id charId = character.Id();
        RpcInteract(charId);
      }
    }
  }

  private void InteractLocal(GCharacter character) {
    OnInteractLocal(character);
  }

  [ClientRpc]
  void RpcInteract(Id charId) {
    // we only need to do this when on a client
    // if we are also the server, InteractLocal will already have been called in from ServerInteract!
    if (!isServer) {
      GCharacter character = charId.Find<GCharacter>();
      if (character != null) {
        InteractLocal(character);
      }
    }
  }


  // override this to allow interaction
  // this function is called once on the server
  // and then once on every client connected
  protected virtual void OnInteractLocal(GCharacter character) {}

}
