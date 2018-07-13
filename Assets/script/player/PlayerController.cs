using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : GCController {

  public float baseWalkSpeed = 8f;
  public float velocitySmoothTime = 1;
  public float verticalLookDeadzone = 0.01f;  // deadzone to stop the player from freaking out when looking directly upward or downward

  [Range(0f, 1f)] public float airControlFactor = 0.5f; // how much slower are the controls when in the air?

  public float jumpSpeed = 20;

  public float jumpBoostDuration = 0.5f; // how long in seconds to boost forward velocity after jumping?


  public Player player { get; private set; }

  Vector3 targetVelocity;
  Vector3 acceleration;
  float jumpBoostTime = 0;

  protected override void Awake() {
    base.Awake();
    player = GetComponent<Player>();
  }

  void FixedUpdate() {
    if (isLocalPlayer && gfield != null) {
      Vector3 gravity = character.gravity;
      // get the input vector, aligned correctly with the view
      CameraRig rig = CameraRig.instance;
      Vector3 forward = Vector3.ProjectOnPlane(rig.gimbal.forward, -gravity);
      if (forward.magnitude < verticalLookDeadzone) {
        forward = Vector3.ProjectOnPlane(rig.gimbal.up, -gravity) * Mathf.Sign(Vector3.Dot(rig.gimbal.forward, gravity));
      }
      Vector3 right = Vector3.ProjectOnPlane(rig.gimbal.right, -gravity).normalized;
      Vector3 input = (
        (forward * Input.GetAxisRaw("Vertical")) + (right * Input.GetAxisRaw("Horizontal"))
      ).normalized;
      // if (character.isSitting) {
      //   // if the player tries to move, leave their seat
      //   if (input.magnitude > Mathf.Epsilon) {
      //     character.LeaveSeat();
      //     character.isGrounded = true;
      //   }
      // }
      if (!character.isSitting) {
        float speed = baseWalkSpeed;
        Vector3 velocity = body.velocity;
        Vector3 verticalVel = Vector3.Project(velocity, gravity);
        // Vector3 lateralVel = velocity - verticalVel;
        // if we are grounded, walk along the ground
        if (character.isGrounded) {
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
        if (rig.isThirdPerson) {
          if (input.magnitude > 0.001f) {
            character.facingDirection = input;
          }
        }
        else {
          character.facingDirection = forward;
        }
      }
    }
  }

  void Update() {
    if (isLocalPlayer && gfield != null) {
      if (!character.isSitting && character.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
        character.isGrounded = false;
        Vector3 gravity = gfield.WorldPointToGravity(transform.position);
        body.velocity += -gravity.normalized * jumpSpeed;
        // start the jump boost
        jumpBoostTime = jumpBoostDuration;
      }
      jumpBoostTime -= Time.deltaTime;
    }
  }

}
