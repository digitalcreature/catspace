using UnityEngine;

public class ThrusterScaleEffect : ThrusterEffect {

  public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
  public Transform model;

  protected override void Update() {
    model.localScale = Vector3.one * scaleCurve.Evaluate(throttle);
  }

  void OnValidate() {
    if (model == null) model = transform;
  }
}
