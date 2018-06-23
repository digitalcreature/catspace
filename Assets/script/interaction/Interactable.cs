using UnityEngine;
using UnityEngine.Networking;

public class Interactable : NetworkBehaviour {

  [SyncVar] public bool isInteractable = true;

  Renderer[] renderers;

  // the interaction manager, for convenience
  public InteractionManager manager
    => InteractionManager.instance;

    // the position of the cursor, for convenience
  public Vector3 cursor
    => manager.cursor;

  public NetworkIdentity identity { get; private set; }

  protected virtual void Awake() {
    identity = GetComponent<NetworkIdentity>();
    renderers = GetComponentsInChildren<Renderer>();
    // make sure each renderer has an extra material slot open for an effect material
    // this is so that we can add the an effect to interactable objects when the player hovers over them etc
    foreach (Renderer renderer in renderers) {
      Material[] materials = renderer.materials;
      if (materials[materials.Length - 1] != null) {
        Material[] newMaterials = new Material[materials.Length + 1];
        for (int i = 0; i < materials.Length; i ++) {
          newMaterials[i] = materials[i];
        }
        materials = newMaterials;
        renderer.materials = materials;
      }
    }
  }

  public void SetEffectMaterial(Material material) {
    foreach (Renderer renderer in renderers) {
      Material[] materials = renderer.materials;
      materials[materials.Length - 1] = material;
      renderer.materials = materials;
    }
  }

  // this method is called when a player interacts with this object
  // only call on server!!! use GCharacter.Interact() to make a local player interact with something
  public void ServerInteract(GCharacter character) {
    if (isServer) {
      if (character != null && isInteractable) {
        InteractLocal(character);
        NetworkInstanceId charId = character.GetInstanceId();
        if (!isClient) {  // dont call InteractLocal more than once if we are hosting
          RpcInteract(charId);
        }
      }
    }
  }

  private void InteractLocal(GCharacter character) {
    OnInteractLocal(character);
  }

  [ClientRpc]
  void RpcInteract(NetworkInstanceId charId) {
    GCharacter character = charId.FindLocalObject<GCharacter>();
    if (character != null) {
      InteractLocal(character);
    }
  }


  // override this to allow interaction
  // this function is called once on the server
  // and then once on every client connected
  protected virtual void OnInteractLocal(GCharacter character) {}

}
