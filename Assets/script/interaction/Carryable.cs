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
        character.Carry(this);
      }
    }
  }

  void FixedUpdate() {
    if (carrier != null) {
      if (gbody.hasPhysics) {
        gbody.body.MovePosition(carrier.GetCarryPosition(this));
        gbody.body.MoveRotation(carrier.GetCarryRotation(this));
        if (carrier.hasPhysics) {
          gbody.body.velocity = carrier.body.velocity;
        }
        else {
          gbody.body.velocity = Vector3.zero;
        }
        gbody.body.angularVelocity = Vector3.zero;
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
