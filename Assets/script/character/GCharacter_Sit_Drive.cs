using UnityEngine;
using UnityEngine.Networking;

partial class GCharacter : GBody {

  public Seat seat { get; private set; }              // the seat the character is sitting in
  public Vehicle vehicle { get; private set; }        // the vehicle the character is currently driving

  public bool isSitting => seat != null;              // is the character sitting?
  public bool isDriving => vehicle != null;           // is the character driving?

  void SyncSit(NetworkSync sync) {
    Seat seat = this.seat;
    sync.SyncBehaviour(ref seat);
    if (sync.isReading) {
      SitLocal(seat);
    }
  }

  // make the character sit
  // can only be called on server or local player
  public void Sit(Seat seat) {
    if (isServer) {
      if (seat == null || seat.CanSit(this)) {
        SitLocal(seat);
        ServerSync();
      }
    }
    else if (isLocalPlayer) {
      CmdSit(seat.Id());
    }
  }

  [Command] void CmdSit(Id seatId) => Sit(seatId.Find<Seat>());

  public void LeaveSeat()
    => Sit(null);

  Transform oldParent;

  // make the character sit in a seat
  void SitLocal(Seat newSeat) {
    Seat oldSeat = seat;
    if (newSeat != oldSeat) {
      seat = newSeat;
      if (oldSeat != null) {
        oldSeat.OnSitLocal(null);
      }
      if (newSeat != null) {
        if (oldSeat == null) {
          // entering seat from standing
          oldParent = transform.parent;
        }
        if (!hasAuthority) {
          netTransform.enabled = false;
        }
        transform.parent = seat.anchor;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        body.isKinematic = true;
        hullTransform.gameObject.SetActive(false);
        newSeat.OnSitLocal(this);
      }
      else {
        // leaving seat
        if (!hasAuthority) {
          netTransform.enabled = true;
        }
        transform.parent = oldParent;
        transform.position = oldSeat.exit.position;
        // transform.rotation = oldSeat.exit.rotation;
        body.isKinematic = false;
        hullTransform.gameObject.SetActive(true);
      }
    }
    if (newSeat == null || !newSeat.isDriversSeat) {
      DriveLocal(null);
    }
    else {
      DriveLocal(newSeat.drivenVehicle);
    }
  }

  void DriveLocal(Vehicle newVehicle) {
    Vehicle oldVehicle = vehicle;
    if (newVehicle != oldVehicle) {
      vehicle = newVehicle;
      if (oldVehicle != null) {
        oldVehicle.OnDriveLocal(null);
      }
      if (newVehicle != null) {
        newVehicle.OnDriveLocal(this);
      }
    }
  }

}
