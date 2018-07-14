using UnityEngine;

public class Atmosphere : MonoBehaviour {

  public float radius = 3000;

  public GSphereField gfield { get; private set; }

  public MeshRenderer render { get; private set; }

  int FRESNEL_FACTOR;

  MaterialPropertyBlock block;

  void Awake() {
    render = GetComponent<MeshRenderer>();
    block = new MaterialPropertyBlock();
    FRESNEL_FACTOR = Shader.PropertyToID("_FresnelFactor");
    gfield = GetComponentInParent<GSphereField>();
  }

  void Start() {
    transform.localScale = radius * Vector3.one;
    render.SetPropertyBlock(block);
  }

  void Update() {
    Camera camera = CameraRig.instance.cam;
    float distance = (camera.transform.position - transform.position).magnitude;
    float factor = Mathf.InverseLerp(gfield.radius, radius, distance);
    block.SetFloat(FRESNEL_FACTOR, factor);
    render.SetPropertyBlock(block);
  }

}
