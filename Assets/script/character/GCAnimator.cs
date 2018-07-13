using UnityEngine;
using UnityEngine.Networking;

public class GCAnimator : KittyNetworkBehaviour {

  public float walkVelocityScale = 1;

  public float limbIKPositionScale = 0.75f;

  public float handIKWeightSmoothTime = 0.25f;

	public GCharacter character { get; private set; }
	public Animator anim { get; private set; }

  int WALK_SPEED = Animator.StringToHash("walk_speed");
  int IS_GROUNDED = Animator.StringToHash("is_grounded");
  int IS_SITTING = Animator.StringToHash("is_sitting");

  int WALK_VEL_X = Animator.StringToHash("walk_vel_x");
  int WALK_VEL_Y = Animator.StringToHash("walk_vel_y");

  int PICKUP = Animator.StringToHash("pickup");

  public HandIKTargets handIKTargets { get; private set; }

  bool isCarrying;

  float handIKWeight;
  float handIKWeightVelocity;

	protected override void Awake() {
		base.Awake();
		character = GetComponent<GCharacter>();
		anim = GetComponent<Animator>();
	}

	void FixedUpdate() {
		if (isLocalPlayer) {
      // handle ik smoothing
      if (character.handIKTargets != null) {
        handIKTargets = character.handIKTargets;
      }
      float targetWeight;
      if (handIKTargets != null && character.handIKTargets == null) {
        targetWeight = 0;
      }
      else {
        targetWeight = 1;
      }
      handIKWeight = Mathf.SmoothDamp(handIKWeight, targetWeight, ref handIKWeightVelocity, handIKWeightSmoothTime);
      if (targetWeight == 0 && handIKWeight < 0.001f) {
        handIKTargets = null;
      }
      // handle pickup animation
      if (character.isCarrying && !isCarrying) {
        anim.SetTrigger(PICKUP);
      }
      isCarrying = character.isCarrying;
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

  void OnAnimatorIK(int layer) {
    handIKTargets.SetWeights(this, handIKWeight);
    handIKTargets.SetTargets(this);
  }

}

[System.Serializable]
public class HandIKTargets {

  public Transform leftHand;
  public Transform rightHand;

}

public static class HandIKTargetsMethods {

  public static void SetWeights(this HandIKTargets t, GCAnimator anim, float weight) {
    // position
    anim.anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, (t == null || t.leftHand == null) ? 0 : weight);
    anim.anim.SetIKPositionWeight(AvatarIKGoal.RightHand, (t == null || t.rightHand == null) ? 0 : weight);
    // rotation
    anim.anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, (t == null || t.leftHand == null) ? 0 : weight);
    anim.anim.SetIKRotationWeight(AvatarIKGoal.RightHand, (t == null || t.rightHand == null) ? 0 : weight);
  }

  public static void SetTargets(this HandIKTargets t, GCAnimator anim) {
    // position
    if (t != null) {
      Vector3 position;
      if (t.leftHand != null) {
        position = t.leftHand.position;
        position = anim.transform.InverseTransformPoint(position) / anim.limbIKPositionScale;
        position = anim.transform.TransformPoint(position);
        anim.anim.SetIKPosition(AvatarIKGoal.LeftHand, position);
      }
      else {
        anim.anim.SetIKPosition(AvatarIKGoal.LeftHand, Vector3.zero);
      }
      if (t.rightHand != null) {
        position = t.rightHand.position;
        position = anim.transform.InverseTransformPoint(position) / anim.limbIKPositionScale;
        position = anim.transform.TransformPoint(position);
        anim.anim.SetIKPosition(AvatarIKGoal.RightHand, position);
      }
      else {
        anim.anim.SetIKPosition(AvatarIKGoal.RightHand, Vector3.zero);
      }
    }
    else {
      anim.anim.SetIKPosition(AvatarIKGoal.LeftHand, Vector3.zero);
      anim.anim.SetIKPosition(AvatarIKGoal.RightHand, Vector3.zero);
    }
    // rotation
    anim.anim.SetIKRotation(AvatarIKGoal.LeftHand, (t == null || t.leftHand == null) ? Quaternion.identity : t.leftHand.rotation);
    anim.anim.SetIKRotation(AvatarIKGoal.RightHand, (t == null || t.rightHand == null) ? Quaternion.identity : t.rightHand.rotation);
  }

}
