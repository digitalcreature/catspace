using UnityEngine;
using UnityEngine.Networking;

partial class GBody : KittyNetworkBehaviour {

  [Header("Rigidbody")]
  public float mass = 1;
  public float drag = 0;
  public float angularDrag = 0;
  public bool useGravity = true;
  public RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;
  public CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;
  public RigidbodyConstraints constraints;

  public Rigidbody body { get; private set; }

  public bool hasPhysics {
    get {
      return body != null;
    }
    set {
      if (value && body == null) {
        CreateRigidbody();
      }
      if (!value && body != null) {
        Destroy(body);
        body = null;
      }
    }
  }

  void CreateRigidbody() {
    body = gameObject.AddComponent<Rigidbody>();
    body.mass = mass;
    body.drag = drag;
    body.angularDrag = angularDrag;
    body.useGravity = useGravity;
    body.interpolation = interpolation;
    body.collisionDetectionMode = collisionDetection;
    body.constraints = constraints;
  }

  void OnSync_Rigidbody(NetworkSync sync) {
    bool hasPhysics = this.hasPhysics;
    sync.Sync(ref hasPhysics);
    this.hasPhysics = hasPhysics;
  }

}