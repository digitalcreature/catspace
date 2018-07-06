using UnityEngine;
using UnityEngine.Networking;

public class Carryable : InteractableModule {

  public GCharacter carrier { get; private set; }
  public ConfigurableJoint joint { get; private set; }

  public GBody gbody { get; private set; }
  public Rigidbody body => gbody.body;

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

  // only call from GCharacter
  public void OnCarryLocal(GCharacter newChar) {
    GCharacter oldChar = carrier;
    if (newChar != oldChar) {
      carrier = newChar;
      if (oldChar != null) {
        Destroy(joint);
        joint = null;
      }
      if (newChar != null) {
        // connect with a joint
        transform.position = newChar.carryAnchor.position;
        transform.rotation = newChar.carryAnchor.rotation;
        joint = gameObject.AddComponent<ConfigurableJoint>();
        // limit angular motion
        joint.angularXMotion =
        joint.angularYMotion =
        joint.angularZMotion =
          ConfigurableJointMotion.Limited;
        // limit linear motion
        joint.xMotion =
        joint.yMotion =
        joint.zMotion =
          ConfigurableJointMotion.Limited;
        // set limits
        var angleLimit = new SoftJointLimit();
          angleLimit.limit = carrier.carryJointAngleLimit;
          angleLimit.contactDistance = 0;
          angleLimit.bounciness = 0;
        var linearLimit = angleLimit;
          linearLimit.limit = carrier.carryJointLinearLimit;
          linearLimit.bounciness = 0;
          linearLimit.contactDistance = 1;
        joint.highAngularXLimit =
        joint.lowAngularXLimit =
        joint.angularYLimit =
        joint.angularZLimit =
          angleLimit;
        joint.linearLimit =
          linearLimit;
        // set spring
        var spring = new SoftJointLimitSpring();
          spring.spring = carrier.carryJointSpringForce;
          spring.damper = carrier.carryJointSpringDamp;
        joint.linearLimitSpring =
        joint.angularXLimitSpring =
        joint.angularYZLimitSpring =
          spring;
        // configure projection
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = newChar.carryJointProjectionDistance;
        // configure rest of joint
        joint.autoConfigureConnectedAnchor = false;
        joint.enableCollision = false;
        joint.connectedAnchor = newChar.transform.InverseTransformPoint(newChar.carryAnchor.position);
        joint.massScale = body.mass;
        joint.connectedMassScale = newChar.body.mass;
        joint.connectedBody = newChar.body;
      }
    }
  }

}
