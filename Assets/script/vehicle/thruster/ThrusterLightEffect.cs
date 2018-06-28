using UnityEngine;

public class ThrusterLightEffect : ThrusterEffect {

  public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);

  public Light lamp { get; private set; }

  void Awake() {
    lamp = GetComponent<Light>();
  }

  protected override void Update() {
    lamp.intensity = intensityCurve.Evaluate(throttle);
  }

}
