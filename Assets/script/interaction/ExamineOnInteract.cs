using UnityEngine;
using UnityEngine.Networking;

public class ExamineOnInteract : Interactable {

  public Examinable examinable { get; private set; }

  protected override void Awake() {
    base.Awake();
    examinable = GetComponent<Examinable>();
  }

  // this function is called once on the server
  // and then once on every client connected
  protected override void OnInteractLocal(GCharacter character) {
    Player localPlayer = Player.localPlayer;
    if (localPlayer != null && localPlayer.chr == character) {
      if (examinable.targeter == null) {
        InteractionManager.instance.Examine(examinable);
      }
      else {
        InteractionManager.instance.Examine(null);
      }
    }
  }

}
