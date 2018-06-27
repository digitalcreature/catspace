using UnityEngine;

public abstract class ThrusterEffect : MonoBehaviour {

  public float throttle;

  protected virtual void Start() {
    throttle = 0;
    Update();
  }

  protected abstract void Update();

}
