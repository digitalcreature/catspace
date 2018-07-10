using UnityEngine;
using UnityEngine.Networking;

public partial class GCharacter : GBody {

  public Transform hullTransform;             // the root of the hull transform; used when a character gets parented to another object
  public float turnSpeed = 15f;             // speed of character rotation in degrees/s


  [Header("Ground Checking")]
  public LayerMask groundMask;
  public Vector3 groundCheckStartPosition;  // local space ground check start position
  public float groundCheckRadius = 0.5f;
  public float groundCheckDistance = 2f;

  [HideInInspector] public bool isGrounded;             // is the character grounded?
  public Vector3 groundNormal { get; private set; }     // world space normal of ground

  [HideInInspector] public Vector3 facingDirection;           // the direction the character is facing
  public Vector3 facingDirectionSmooth { get; private set; }  // the direction the character is facing, but smoothed

  [Range(0f, 90f)] public float maxSlopeAngle = 60f;  // the max slope that can be traversed

  Vector3 facingDirectionSmoothVelocity;

  protected override void Awake() {
    base.Awake();
    facingDirection = transform.forward;
    facingDirectionSmooth = transform.forward;
    Awake_Sit_Drive();
    Awake_Carry();
  }

  protected override void FixedUpdate() {
    if (isLocalPlayer && gfield != null) {
      if (!isSitting) {
        if (isGrounded) {
          Ray position;
          Vector3 gravity;
          gravity = this.gravity;
          // if the ground is too far for even the ground cast to hit, or its too steep to walk on, we arent grounded
          bool groundPresent = SphereCastGround(transform.TransformPoint(groundCheckStartPosition), out position);
          float slopeAngle = GetSlopeAngle(position.direction, gravity);
          if (!groundPresent || slopeAngle > maxSlopeAngle) {
            isGrounded = false;
            // take it again from the top! we arent grounded!! (this wont infinite loop unless you change stuff;
            // yes i know this is hacky bs but it works and this is a game script so shut up)
            FixedUpdate();
            return;
          }
          if (body != null) {
            Vector3 velocity = body.velocity;
            float speed = velocity.magnitude;
            velocity = Vector3.ProjectOnPlane(velocity, -gravity);
            velocity = velocity.normalized * speed;
            body.velocity = velocity;
          }
          body.MovePosition(position.origin);
        }
        else {
          Vector3 next = body.position + body.velocity * Time.fixedDeltaTime;
          Vector3 nextCheckStart = transform.TransformPoint(groundCheckStartPosition) + body.velocity * Time.fixedDeltaTime;
          Ray nextGround;
          SphereCastGround(nextCheckStart, out nextGround);
          Vector3 nextGravity = gfield.WorldPointToGravity(next);
          // if the next position lies in (below) the ground, and it is
          // not steeper than we can walk on, are grounded now
          if ((Vector3.Dot(next - nextGround.origin, nextGravity) > 0) && GetSlopeAngle(nextGround.direction, nextGravity) < maxSlopeAngle) {
            isGrounded = true;
            body.velocity = Vector3.ProjectOnPlane(body.velocity, -nextGravity);
          }
          else {
            // apply gravity
            AddGravity();
          }
        }
        UpdateFacingDirection();
        // body.rotation = transform.rotation;
        Ray ground;
        if (SphereCastGround(transform.TransformPoint(groundCheckStartPosition), out ground)) {
          groundNormal = ground.direction;
        }
      }
    }
  }

  // update the facing direction of the character
  public void UpdateFacingDirection() {
    facingDirection = facingDirection.normalized;
    Vector3 dir = Vector3.RotateTowards(
      facingDirectionSmooth, facingDirection, Time.fixedDeltaTime * turnSpeed * Mathf.Deg2Rad, 0
    ).normalized;
    if (gfield != null) {
      Vector3 gravity = gfield.WorldPointToGravity(transform.position);
      Vector3 forward = gfield.AlignRayToGravity(new Ray(transform.position, dir)).direction;
      Debug.DrawRay(transform.position, dir, Color.red);
      Debug.DrawRay(transform.position, facingDirection, Color.green);
      Debug.DrawRay(transform.position, facingDirectionSmooth, Color.blue);
      if (forward != Vector3.zero) {
        body.MoveRotation(Quaternion.LookRotation(forward, -gravity));
      }
    }
    else {
      // if we are in zero-g, just face wherever, man
      body.MoveRotation(Quaternion.LookRotation(dir, transform.up));
    }
    facingDirectionSmooth = dir;
  }

  float GetSlopeAngle(Vector3 groundNormal, Vector3 gravity) {
    return Vector3.Angle(groundNormal, -gravity);
  }

  // DEPRECATED: this is a spherecast only event, go home
  bool RaycastGround(Vector3 point, out Vector3 hit) {
    Vector3 gravity = gfield.WorldPointToGravity(point);
    RaycastHit h;
    if (Physics.Raycast(point, gravity.normalized, out h, groundCheckDistance, groundMask)) {
      hit = h.point;
      return true;
    }
    else {
      hit = gfield.WorldPointToSurface(point);
      return false;
    }
  }

  // sphere cast against the ground
  // hit is the point that point should be moved to in order to be aligned with the top of the ground
  // and the normal of the ground at that point
  bool SphereCastGround(Vector3 point, out Ray hit) {
    Vector3 gravity = gfield.WorldPointToGravity(point).normalized;
    RaycastHit h;
    if (Physics.SphereCast(point, groundCheckRadius, gravity, out h, groundCheckDistance, groundMask)) {
      hit = new Ray(Vector3.Project(h.point - point, gravity) + point, h.normal);
      return true;
    }
    else {
      hit = new Ray(gfield.WorldPointToSurface(point), -gravity);
      return false;
    }
  }

  // sphere cast the ground, but just output the raycast hit
  bool SphereCastGround(Vector3 point, out RaycastHit hit) {
    Vector3 gravity = gfield.WorldPointToGravity(point);
    return Physics.SphereCast(point, groundCheckRadius, gravity.normalized, out hit, groundCheckDistance, groundMask);
  }

  protected override void OnDrawGizmosSelected() {
    base.OnDrawGizmosSelected();
    Color c = Gizmos.color;
    Gizmos.color = new Color(1f, 0.5f, 0.5f);
    Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckStartPosition), groundCheckRadius);
    Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckStartPosition - (Vector3.up * (groundCheckDistance - groundCheckRadius))), groundCheckRadius);
    Gizmos.color = c;
  }

  protected override void OnSync(NetworkSync sync) {
    base.OnSync(sync);
    OnSync_Sit_Drive(sync);
    OnSync_Carry(sync);
  }

  // interact with an interactable object
  public void Interact(Interactable interactable, InteractionMode mode) {
    if (mode == InteractionMode.Interact && isSitting && (interactable == null || interactable == seat)) {
      LeaveSeat();
    }
    if (mode == InteractionMode.Carry && isCarrying && interactable == null) {
      DropCarried();
    }
    else if (interactable != null) {
      if (isServer) {
        interactable.ServerInteract(this, mode);
      }
      else if (isLocalPlayer) {
        Id interactableId = interactable.Id();
        CmdInteract(interactableId, mode);
      }
    }
  }

  [Command] void CmdInteract(Id interactableId, InteractionMode mode) => Interact(interactableId.Find<Interactable>(), mode);

}
