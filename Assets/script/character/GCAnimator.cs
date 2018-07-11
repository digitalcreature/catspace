using UnityEngine;
using UnityEngine.Networking;

public class GCAnimator : KittyNetworkBehaviour {

  public float walkVelocityScale = 1;

	public GCharacter character { get; private set; }
	public Animator anim { get; private set; }

  int WALK_SPEED = Animator.StringToHash("walk_speed");
  int IS_GROUNDED = Animator.StringToHash("is_grounded");
  int IS_SITTING = Animator.StringToHash("is_sitting");

  int WALK_VEL_X = Animator.StringToHash("walk_vel_x");
  int WALK_VEL_Y = Animator.StringToHash("walk_vel_y");

	protected override void Awake() {
		base.Awake();
		character = GetComponent<GCharacter>();
		anim = GetComponent<Animator>();
	}

	void FixedUpdate() {
		if (isLocalPlayer) {
			GField gfield = character.gfield;
			if (gfield != null) {
        if (character.hasPhysics) {
          Vector3 velocity = character.body.velocity;
          Vector3 gravity = gfield.WorldPointToGravity(character.transform.position).normalized;
          Vector3 walkVelocity = Vector3.ProjectOnPlane(velocity, -gravity);
          // find walk velocity, local to facing direction
          Vector3 forward = character.facingDirectionSmooth.normalized;
          Vector3 right = Vector3.Cross(forward, gravity).normalized;
          Vector2 localWalkVelocity = new Vector2(
          Vector3.Dot(right, walkVelocity) * walkVelocityScale,
          Vector3.Dot(forward, walkVelocity) * walkVelocityScale
          );
          anim.SetFloat(WALK_VEL_X, localWalkVelocity.x);
          anim.SetFloat(WALK_VEL_Y, localWalkVelocity.y);
          anim.SetFloat(WALK_SPEED, walkVelocity.magnitude);
        }
				anim.SetBool(IS_GROUNDED, character.isGrounded);
				anim.SetBool(IS_SITTING, character.isSitting);
			}
		}
	}

}
