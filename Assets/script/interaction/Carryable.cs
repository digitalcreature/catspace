using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Carryable : InteractableModule {

  public GCharacter carrier { get; private set; }
  public ConfigurableJoint joint { get; private set; }

  public Transform leftHandIKTarget;
  public Transform rightHandIKTarget;

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
    if (carrier != null) {
      if (gbody.hasPhysics) {
        body.MovePosition(carrier.GetCarryPosition(this));
        // body.MoveRotation(carrier.GetCarryRotation(this));
        Quaternion target = carrier.GetCarryRotation(this);

        // Rotations stack right to left,
        // so first we undo our rotation, then apply the target.
        Quaternion delta = target * Quaternion.Inverse(body.rotation);

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
          Vector3 angular = (0.9f * Mathf.Deg2Rad * angle) * axis.normalized * carrier.carry.angularSpring;

          body.angularVelocity = angular;
        }


        // Vector3 targetForward = target * Vector3.forward;
        // Vector3 forward = body.rotation * Vector3.forward;
        // float angle = Vector3.Angle(forward, targetForward);
        // Vector3 axis = Vector3.Cross(forward, targetForward).normalized;
        // body.AddTorque(axis * angle * carrier.carry.angularSpring, ForceMode.Acceleration);
        // Vector3 targetUp = target * Vector3.up;
        // Vector3 up = body.rotation * Vector3.up;
        if (carrier.hasPhysics) {
          body.velocity = carrier.body.velocity;
        }
        else {
          body.velocity = Vector3.zero;
        }
        // body.angularVelocity = Vector3.zero;
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
