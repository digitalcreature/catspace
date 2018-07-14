using UnityEngine;

public class Atmosphere : MonoBehaviour {

  public float maxAltitude = 200;
  [MinMax(0, 1)]
  public FloatRange surfaceFadeRange = new FloatRange(0.125f, 0.85f);

  float radius => gfield.radius + maxAltitude;

  public GSphereField gfield { get; private set; }

  public MeshRenderer render { get; private set; }
  public Light sun { get; private set; }

  int SURFACE_FADE;
  int RADIUS;
  int SUN_DIRECTION;

  MaterialPropertyBlock block;

  void Awake() {
    render = GetComponent<MeshRenderer>();
    block = new MaterialPropertyBlock();
    SURFACE_FADE = Shader.PropertyToID("_SurfaceFade");
    RADIUS = Shader.PropertyToID("_Radius");
    SUN_DIRECTION = Shader.PropertyToID("_SunDirection");
    gfield = GetComponentInParent<GSphereField>();
    sun = GameObject.FindWithTag("Sun").GetComponent<Light>();
  }

  void Start() {
    transform.localScale = radius * Vector3.one;
    block.SetFloat(RADIUS, radius);
    render.SetPropertyBlock(block);
  }

  void Update() {
    Camera camera = CameraRig.instance.cam;
    float altitude = (camera.transform.position - transform.position).magnitude - gfield.radius;
    float factor = surfaceFadeRange.InverseLerp(altitude / maxAltitude);
    block.SetFloat(SURFACE_FADE, 1 - factor);
    block.SetVector(SUN_DIRECTION, sun.transform.forward);
    render.SetPropertyBlock(block);
  }

}
