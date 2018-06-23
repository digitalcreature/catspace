using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

  public bool faceVelocity = true;
  public float baseWalkSpeed = 8f;
  public float velocitySmoothTime = 1;

  [Range(0f, 1f)] public float airControlFactor = 0.5f; // how much slower are the controls when in the air?

  public float jumpSpeed = 20;

  public float jumpBoostDuration = 0.5f; // how long in seconds to boost forward velocity after jumping?

  public Player player { get; private set; }

  public GCharacter chr => player.chr;
  public GField gfield => player.gfield;
  public Rigidbody body => player.body;

  Vector3 targetVelocity;
  Vector3 acceleration;
  float jumpBoostTime = 0;

  void Awake() {
    player = GetComponent<Player>();
  }

  void FixedUpdate() {
    if (isLocalPlayer && gfield != null) {
      Vector3 gravity = chr.gravity;
      // get the input vector, aligned correctly with the view
      CameraRig rig = CameraRig.instance;
      Vector3 forward = Vector3.ProjectOnPlane(rig.gimbal.forward, -gravity).normalized;
      if (forward == Vector3.zero) {
        forward = Vector3.ProjectOnPlane(rig.gimbal.up, -gravity).normalized;
      }
      Vector3 right = Vector3.ProjectOnPlane(rig.gimbal.right, -gravity).normalized;
      Vector3 input = (
        (forward * Input.GetAxisRaw("Vertical")) + (right * Input.GetAxisRaw("Horizontal"))
      ).normalized;
      // if (chr.isSitting) {
      //   // if the player tries to move, leave their seat
      //   if (input.magnitude > Mathf.Epsilon) {
      //     chr.LeaveSeat();
      //     chr.isGrounded = true;
      //   }
      // }
      if (!chr.isSitting) {
        float speed = baseWalkSpeed;
        Vector3 velocity = body.velocity;
        Vector3 verticalVel = Vector3.Project(velocity, gravity);
        // Vector3 lateralVel = velocity - verticalVel;
        // if we are grounded, walk along the ground
        if (chr.isGrounded) {
          targetVelocity = (input * speed) + verticalVel;
          velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref acceleration, velocitySmoothTime);
        }
        // otherwise, do air control
        else {
          // reduce speed based on where we are in the jump boost window
          // if we are in the beginning, dont lower speed at all
          // if we are at the end or beyond, reduce it completely
          speed *= Mathf.Lerp(airControlFactor, 1f, jumpBoostTime / jumpBoostDuration);
          targetVelocity = (input * speed) + verticalVel;
          velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref acceleration, velocitySmoothTime);
        }
        body.velocity = velocity;
        // align the transform so that its rotated towards the direction the player is moving
        velocity = gfield.AlignRayToGravity(new Ray(transform.position, velocity)).direction;
        if (velocity != Vector3.zero) {
          transform.forward = velocity;
        }
        gfield.AlignTransformToGravity(transform);
      }
    }
  }

  void Update() {
    if (isLocalPlayer && gfield != null) {
      if (!chr.isSitting && chr.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
        chr.isGrounded = false;
        Vector3 gravity = gfield.WorldPointToGravity(transform.position);
        body.velocity += -gravity.normalized * jumpSpeed;
        // start the jump boost
        jumpBoostTime = jumpBoostDuration;
      }
      jumpBoostTime -= Time.deltaTime;
    }
  }

}
