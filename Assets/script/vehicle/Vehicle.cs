using UnityEngine;
using UnityEngine.Networking;

public class Vehicle : KittyNetworkBehaviour {

  public PartNode driversSeatSpawn;

  public GBody gbody { get; private set; }

  public Seat driversSeat { get; private set; }
  public GCharacter driver { get; private set; }
  public PlayerVehicleController controller { get; private set; }

  // the center of mass of the vehicle, in world space
  public Vector3 centerOfMass
    => transform.TransformPoint(gbody.body.centerOfMass);

  public PlayerVehicleController.Controls controls
    => controller == null ? null : controller.controls;

  public bool isDriven
    => driver != null;

  protected override void Awake() {
    base.Awake();
    gbody = GetComponent<GBody>();
    driversSeatSpawn.EventOnChildSpawnedLocal += (part) => {
      driversSeat = part.GetComponent<Seat>();
      if (driversSeat == null) {
        Debug.LogError("Vehicle missing driver seat!");
      }
      else {
        driversSeat.SetDrivenVehicle(this);
      }
    };
  }

  public void OnDriveLocal(GCharacter driver) {
    this.driver = driver;
    if (driver == null) {
      controller = null;
    }
    else {
      controller = driver.GetComponent<PlayerVehicleController>();
    }
  }



}
