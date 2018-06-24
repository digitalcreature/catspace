using UnityEngine;
using UnityEngine.Networking;

public class PlayerAnimation : KittyNetworkBehaviour {

	public Player player { get; private set; }
	public Animator anim { get; private set; }

	protected override void Awake() {
		base.Awake();
		player = GetComponent<Player>();
		anim = GetComponent<Animator>();
	}

	void FixedUpdate() {
		if (isLocalPlayer) {
			GField gfield = player.gfield;
			if (gfield != null) {
				Vector3 velocity = player.body.velocity;
				Vector3 gravity = gfield.WorldPointToGravity(player.body.position).normalized;
				float walk_speed = Vector3.ProjectOnPlane(velocity, -gravity).magnitude;
				anim.SetFloat("walk_speed", walk_speed);
				anim.SetBool("is_grounded", player.chr.isGrounded);
				anim.SetBool("is_sitting", player.chr.isSitting);
			}
		}
	}

}
