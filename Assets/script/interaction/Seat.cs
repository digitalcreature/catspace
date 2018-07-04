using UnityEngine;
using UnityEngine.Networking;
using System;

// an object that gcharacters can sit on
public class Seat : InteractableModule {

  public event Action<GCharacter> EventOnSitLocal;

  public Transform anchor;        // the transform that the sitting character will parent to
  public Transform exit;          // where to position the character when they exit the seat

  public Vehicle drivenVehicle { get; private set; }  // the vehicle, if any, that this seat drives
  public bool isDriversSeat => drivenVehicle != null;

  public Vector3 exitPosition
    => exit == null ? anchor.position : exit.position;

  public GCharacter sittingCharacter { get; set; }    // a reference to the character sitting in this seat

  public bool isOccupied => sittingCharacter != null;

  // return true if the character can sit in this seat, false otherwise
  // only reliable on server side
  public bool CanSit(GCharacter character) {
    return (character.seat != this) && (!isOccupied);
  }

  // called once on the server and on all clients when the sitting character changes
  public void OnSitLocal(GCharacter character) {
    sittingCharacter = character;
    if (EventOnSitLocal != null) {
      EventOnSitLocal(sittingCharacter);
    }
  }

  public void SetDrivenVehicle(Vehicle vehicle) {
    drivenVehicle = vehicle;
  }

  protected override void OnInteractLocal(GCharacter character, InteractionMode mode) {
    // we only need to call this once, on the server
    // the logic in GCharacter will make sure the change is serialized
    if (mode == InteractionMode.Interact && isServer) {
      character.Sit(this);
    }
  }

}
