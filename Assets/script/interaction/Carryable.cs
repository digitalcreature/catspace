using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Carryable : InteractableModule {

  public GCharacter carrier { get; private set; }
  public ConfigurableJoint joint { get; private set; }

  public HandIKTargets handIKTargets;

  public GBody gbody { get; private set; }
  public Rigidbody body => gbody.body;

  public bool isCarried => carrier != null;
  public float boundingRadius => gbody.boundingRadius;

  protected override void Awake() {
    base.Awake();
    gbody = GetComponent<GBody>();
  }

  // this function is called once on the server
  // and then once on every client connected
  protected override void OnInteractLocal(GCharacter character, InteractionMode mode) {
    if (isServer && mode == InteractionMode.Carry) {
      if (character == null) {
        if (carrier != null) {
          carrier.DropCarried();
        }
      }
      else {
        if (character.isCarrying) {
          character.DropCarried();
        }
        else {
          character.Carry(this);
        }
      }
    }
  }

  void FixedUpdate() {
    if (!gbody.positionSyncEnabled && !isCarried) {
      gbody.SnapPosition();
    }
    gbody.positionSyncEnabled = !isCarried;
    if (isCarried) {
      if (gbody.hasPhysics) {
        Vector3 targetPosition = carrier.GetCarryPosition(this);
        Quaternion targetRotation = carrier.GetCarryRotation(this);
        Vector3 localPosition = body.position - targetPosition;
        Vector3 velocity = 0.9f * - localPosition * carrier.carry.linearSpring;
        if (carrier.hasPhysics) {
          velocity += carrier.body.velocity;
        }
        body.velocity = velocity;
        float deltaAngle = Quaternion.Angle(targetRotation, body.rotation);
        if (deltaAngle > carrier.carry.angularSnapThreshold) {
          body.MoveRotation(targetRotation);
        }
        else {
          Quaternion delta = targetRotation * Quaternion.Inverse(body.rotation);
          float angle;
          Vector3 axis;
          delta.ToAngleAxis(out angle, out axis);
          // We get an infinite axis in the event that our rotation is already aligned.
          if (!float.IsInfinity(axis.x)) {
            if (angle > 180f) {
              angle -= 360f;
            }
            // Here I drop down to 0.9f times the desired movement,
            // since we'd rather undershoot and ease into the correct angle
            // than overshoot and oscillate around it in the event of errors.
            Vector3 angular = 0.9f * Mathf.Deg2Rad * angle * axis.normalized * carrier.carry.angularSpring;
            body.angularVelocity = angular;
          }
        }
      }
    }
  }

  // only call from GCharacter
  public void OnCarryLocal(GCharacter newChar) {
    GCharacter oldChar = carrier;
    if (newChar != oldChar) {
      if (oldChar != null) {
        Destroy(joint);
        joint = null;
      }
      if (newChar != null) {
        StartCoroutine(JointRoutine(newChar));
      }
      else {
        carrier = newChar;
        tag = "Untagged";
      }
    }
  }

  // we need to handle this in a sperate routine to prevent a wierd glitch where if a character is too close
  // to an object when it picks it up, they start sliding backwards indefinitely
  IEnumerator JointRoutine(GCharacter newChar) {
    // make the object kinematic for a frame, thus resetting contact forces
    body.isKinematic = true;
    yield return new WaitForFixedUpdate();
    joint = gameObject.AddComponent<ConfigurableJoint>();
    // unlimit motion
    joint.angularXMotion =
    joint.angularYMotion =
    joint.angularZMotion =
    joint.xMotion =
    joint.yMotion =
    joint.zMotion =
      ConfigurableJointMotion.Free;
    // connect to joint
    joint.autoConfigureConnectedAnchor = false;
    joint.enableCollision = false;
    joint.connectedBody = newChar.body;
    // make the object not kinematic again
    body.isKinematic = false;
    carrier = newChar;
    tag = "Carried";
  }

}
