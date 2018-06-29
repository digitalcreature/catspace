using UnityEngine;
using UnityEngine.Networking;

public class PlayerVehicleController : KittyNetworkBehaviour {

  [Range(1, 16)]
  public float sendRate = 12; // how often to send updates to the server (updates per second)

  public Controls controls;

  public const short controlsUpdateMessageType = 413;

  float t;

  public GCharacter character { get; private set; }

  protected override void Awake() {
    base.Awake();
    character = GetComponent<GCharacter>();
  }

  public override void OnStartServer() {
    if (!isLocalPlayer) {
      identity.connectionToClient.RegisterHandler(controlsUpdateMessageType, (message) => {
        controls.OnSync(new NetworkSync(message.reader));
      });
    }
  }

  void Update() {
    if (isLocalPlayer && character.isDriving) {
      // update the controls locally
      controls.Update(CameraRig.instance.lookDirection);
      // if we arent the server, send the update to the server
      if (!isServer) {
        if (t <= 0) {
          SendControlsUpdate();
          t += 1 / sendRate;
        }
        t -= Time.deltaTime;
      }
    }
  }

  void SendControlsUpdate() {
    if (isLocalPlayer & !isServer) {
      NetworkWriter writer = new NetworkWriter();
      writer.StartMessage(controlsUpdateMessageType);
      controls.OnSync(new NetworkSync(writer));
      writer.FinishMessage();
      identity.connectionToServer.SendWriter(writer, Channels.DefaultReliable);
      controls.Reset();
    }
  }

  [System.Serializable]
  public class Controls : INetworkSyncable {

    public Axis moveX = new Axis(KeyCode.D, KeyCode.A);
    public Axis moveY = new Axis(KeyCode.Space, KeyCode.LeftShift);
    public Axis moveZ = new Axis(KeyCode.W, KeyCode.S);
    public Axis roll = new Axis(KeyCode.Q, KeyCode.E);
    public Button toggleSteering = new Button(KeyCode.Z);
    public Button toggleHover = new Button(KeyCode.H);
    public Button toggleDampener = new Button(KeyCode.I);
    public Button toggleGyro = new Button(KeyCode.G);


    public Vector3 attitudeTarget { get; private set; }

    public void OnSync(NetworkSync sync) {
      sync.Sync(moveX);
      sync.Sync(moveY);
      sync.Sync(moveZ);
      sync.Sync(roll);
      sync.Sync(toggleSteering);
      sync.Sync(toggleHover);
      sync.Sync(toggleDampener);
      sync.Sync(toggleGyro);
      Vector3 at = attitudeTarget;
      sync.Sync(ref at);
      attitudeTarget = at;
    }

    public void Reset() {
      toggleSteering.Reset();
      toggleHover.Reset();
      toggleDampener.Reset();
      toggleGyro.Reset();
    }

    public void Update(Vector3 attitudeTarget) {
      moveX.Update();
      moveY.Update();
      moveZ.Update();
      roll.Update();
      toggleSteering.Update();
      toggleHover.Update();
      toggleDampener.Update();
      toggleGyro.Update();
      this.attitudeTarget = attitudeTarget;
    }

  }


  [System.Serializable]
  public class Axis : INetworkSyncable {

    public KeyCode positive;
    public KeyCode negative;

    public float value { get; private set; }

    public Axis(KeyCode positive, KeyCode negative) {
      this.positive = positive;
      this.negative = negative;
    }

    public void Update() {
      value = 0;
      if (Input.GetKey(positive)) value += 1;
      if (Input.GetKey(negative)) value -= 1;
    }

    public void OnSync(NetworkSync sync) {
      float v = value;
      sync.Sync(ref v);
      value = v;
    }

  }

  [System.Serializable]
  public class Button : INetworkSyncable {

    public KeyCode key;

    bool _down;
    public bool down {
      get {
        if (_down) {
          Reset();
          return true;
        }
        return false;
      }
      private set {
        _down = value;
      }
    }
    public bool held { get; private set; }

    public Button(KeyCode key) {
      this.key = key;
    }

    public void Reset() {
      down = false;
    }

    public void Update() {
      if (Input.GetKeyDown(key)) down = true;
      held = Input.GetKey(key);
    }

    public void OnSync(NetworkSync sync) {
      bool d = down;
      bool h = held;
      sync.Sync(ref d);
      sync.Sync(ref h);
      down = d;
      held = h;
    }

  }

}
