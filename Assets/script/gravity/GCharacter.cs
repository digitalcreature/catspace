using UnityEngine;
using UnityEngine.Networking;

public class GCharacter : GBody {

  public LayerMask groundMask;

  public float surfaceHeight = 0f;
  public bool alignToGravity = true;
  public Transform hullTransform;           // the root of the hull transform; used when a character gets parented to another object

  [Header("Ground Checking")]
  public Vector3 groundCheckStartPosition;  // local space ground check start position
  public float groundCheckRadius = 0.5f;
  public float groundCheckDistance = 2f;

  public bool isGrounded { get; set; }                // is the character grounded?
  public Vector3 groundNormal { get; private set; }   // world space normal of ground
  public bool isSitting => seat != null;              // is the character sitting?

  public Seat seat { get; private set; }          // the seat the character is sitting in

  [Range(0f, 90f)] public float maxSlopeAngle = 60f;  // the max slope that can be traversed

  protected override void FixedUpdate() {
    if (isLocalPlayer && gfield != null) {
      if (!isSitting) {
        if (isGrounded) {
          // position.origin -= gravity * surfaceHeight;
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
          transform.position = position.origin;
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
        if (alignToGravity) {
          gfield.AlignTransformToGravity(transform);
        }
        body.rotation = transform.rotation;
        Ray ground;
        if (SphereCastGround(transform.TransformPoint(groundCheckStartPosition), out ground)) {
          groundNormal = ground.direction;
        }
      }
    }
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
    Seat seat = this.seat;
    sync.SyncBehaviour(ref seat);
    if (sync.isReading) {
      SitLocal(seat);
    }
  }

  // interact with an interactable object
  public void Interact(Interactable interactable) {
    if (isSitting && (interactable == null || interactable == seat)) {
      LeaveSeat();
    }
    else if (interactable != null) {
      if (isServer) {
        interactable.ServerInteract(this);
      }
      else if (isLocalPlayer) {
        NetworkInstanceId interactableId = interactable.GetInstanceId();
        CmdInteract(interactableId);
      }
    }
  }

  [Command] void CmdInteract(NetworkInstanceId interactableId)
    => Interact(interactableId.FindLocalObject<Interactable>());

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
      CmdSit(seat.GetInstanceId());
    }
  }

  [Command] void CmdSit(NetworkInstanceId seatId)
    => Sit(seatId.FindLocalObject<Seat>());

  public void LeaveSeat()
    => Sit(null);

  Transform oldParent;

  // make the character sit in a seat
  public void SitLocal(Seat newSeat) {
    Seat oldSeat = this.seat;
    if (newSeat != oldSeat) {
      this.seat = newSeat;
      if (oldSeat != null) {
        oldSeat.sittingCharacter = null;
      }
      if (newSeat != null) {
        newSeat.sittingCharacter = this;
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
  }

}
