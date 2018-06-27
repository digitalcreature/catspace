using UnityEngine;

public class ThrusterParticleEffect : ThrusterEffect {

  public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
  public float startThreshold = 0.05f;  // the minimum throttle for this effect to turn on
  ParticleSystem system;


  void Awake() {
    system = GetComponentInChildren<ParticleSystem>();
  }

  protected override void Update() {
    UpdateParticles();
  }

  void UpdateParticles() {
    if (throttle > startThreshold) {
      if (system.isStopped) system.Play();
      system.transform.localScale = Vector3.one * scaleCurve.Evaluate(throttle);
    }
    else {
      if (system.isPlaying) system.Stop();
    }
  }
}
