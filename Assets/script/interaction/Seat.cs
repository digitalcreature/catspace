using UnityEngine;
using UnityEngine.Networking;

// an object that gcharacters can sit on
public class Seat : Interactable {

  public bool changeOwner;        // if the sitting character is a player, give their client authority over this object while they are sitting

  public Transform anchor;        // the transform that the sitting character will parent to
  public Transform exit;          // where to position the character when they exit the seat

  public Vector3 exitPosition
    => exit == null ? anchor.position : exit.position;

  public GCharacter sittingCharacter { get; private set; }
  [SyncVar] NetworkInstanceId sittingCharacterId;

  public bool isOccupied => sittingCharacter != null;

  // return true if the character can sit in this seat, false otherwise
  // only reliable on server side
  public bool CanSit(GCharacter character) {
    return (character.seat != this) && (!isOccupied);
  }

  // change which character is sitting in this seat
  // WARNING: NO CHECKS ARE MADE
  // ONLY CALL FROM SECURE CODE IN GCHARACTER
  // always called on server
  public void SetSittingCharacter(GCharacter character) {
    NetworkInstanceId id = character.GetInstanceId();
    if (id != sittingCharacterId) {
      sittingCharacterId = id;
    }
    if (identity.localPlayerAuthority && changeOwner) {
      if (character != null && character.identity.localPlayerAuthority) {
        NetworkConnection oldConnection = identity.clientAuthorityOwner;
        NetworkConnection newConnection = character.identity.connectionToClient;
        if (oldConnection != newConnection) {
          identity.RemoveClientAuthority(oldConnection);
          identity.AssignClientAuthority(newConnection);
        }
      }
    }
    this.sittingCharacter = character;
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll) {
    base.OnSerialize(writer, forceAll);
    writer.Write(sittingCharacterId);
    return true;
  }

  public override void OnDeserialize(NetworkReader reader, bool forceAll) {
    base.OnDeserialize(reader, forceAll);
    sittingCharacterId = reader.ReadNetworkId();
    sittingCharacter = sittingCharacterId.FindLocalObject<GCharacter>();
  }

  protected override void OnInteractLocal(GCharacter character) {
    // we only need to call this once, on the server
    // the logic in GCharacter will make sure the change is serialized
    if (isServer) {
      character.Sit(this);
    }
  }

}
