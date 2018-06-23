using UnityEngine;

public class MoveOnInteract : Interactable {

  public float moveSpeed = 15;
  public Vector2 moveDirection = new Vector2(1f, 1f); // x is forward, y is up

  public GBody gbody { get; private set; }

  protected override void Awake() {
    base.Awake();
    gbody = GetComponent<GBody>();
  }

  protected override void OnInteractLocal(GCharacter character) {
    if (hasAuthority) {
      Vector3 up = -gbody.gravity.normalized;
      Vector3 forward = (transform.position - character.transform.position);
      forward = Vector3.ProjectOnPlane(forward, up).normalized;
      Vector3 velocity = moveDirection.normalized;
      velocity = (forward * velocity.x) + (up * velocity.y);
      velocity = velocity.normalized * moveSpeed;
      gbody.body.AddForceAtPosition(velocity, cursor, ForceMode.VelocityChange);
    }
  }

}
