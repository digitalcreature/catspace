using UnityEngine;
using UnityEngine.Networking;

// an object that gcharacters can sit on
public class Seat : Interactable {

  // public bool changeOwner;        // if the sitting character is a player, give their client authority over this object while they are sitting

  public Transform anchor;        // the transform that the sitting character will parent to
  public Transform exit;          // where to position the character when they exit the seat

  public Vector3 exitPosition
    => exit == null ? anchor.position : exit.position;

  // a reference to the character sitting in this seat
  // the value is managed by GCharacter's sync operations
  public GCharacter sittingCharacter { get; set; }

  public bool isOccupied => sittingCharacter != null;

  // return true if the character can sit in this seat, false otherwise
  // only reliable on server side
  public bool CanSit(GCharacter character) {
    return (character.seat != this) && (!isOccupied);
  }

  // called once on the server and on all clients when the sitting character changes
  public void OnSitLocal() {
    // if (isServer) {
    //   if (changeOwner) {
    //     AssignClientOwner(sittingCharacter == null ? null : sittingCharacter.identity);
    //   }
    // }
  }

  protected override void OnInteractLocal(GCharacter character) {
    // we only need to call this once, on the server
    // the logic in GCharacter will make sure the change is serialized
    if (isServer) {
      character.Sit(this);
    }
  }

}
