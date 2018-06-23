using UnityEngine;
using UnityEngine.Networking;

// select a random material from a list and apply it to all renderers given
// syncronize the material chosen over the network
public class RandomNetworkedMaterial : NetworkBehaviour {

  public Renderer[] renderers;
  public Material[] materials;

  public NetworkIdentity id { get; private set; }

  [SyncVar] int m;

  void Awake() {
    id = GetComponent<NetworkIdentity>();
  }

  public override void OnStartServer() {
    base.OnStartServer();
    m = Random.Range(0, materials.Length);
    UpdateMaterial(m);
  }

  public override bool OnSerialize(NetworkWriter writer, bool initialState) {
    if (initialState) {
      writer.Write(m);
    }
    return true;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState) {
    if (initialState) {
      m = reader.ReadInt32();
    }
    UpdateMaterial(m);
  }

  void UpdateMaterial(int m) {
    Material material = materials[m];
    foreach (Renderer render in renderers) {
      if (render != null) {
        Material[] mats = render.materials;
        mats[0] = material;
        // make sure the rest of the materials (used for effects) arent accidentally left on
        // (temp workaround bc i dont feel like finding a real fix)
        for (int i = 1; i < mats.Length; i ++) {
          mats[i] = null;
        }
        render.materials = mats;
      }
    }
  }


}
