using UnityEngine;

public abstract class TMeshGenerator : MonoBehaviour {

  public TerrainBase terrain { get; private set; }

  public GField gfield => terrain.gfield;

  protected virtual void Awake() {}

  // initialize the generator so that it can be used
  public virtual void Initialize() {
    terrain = GetComponent<TerrainBase>();
  }

  // use the generator to generate a mesh (based on a previous iteration)
  public abstract TMeshData Generate(TMeshData mesh);

}


// intermidiate stage mesh generation utility
public struct TMeshData {

  public Vector3[] vert;
  public int[] tri;
  public Vector2[] uv;

  public TMeshData(Vector3[] vert, int[] tri, Vector2[] uv = null) {
    this.vert = vert;
    this.tri = tri;
    this.uv = uv;
  }

  public TMeshData(Mesh mesh) {
    vert = mesh.vertices;
    tri = mesh.triangles;
    uv = mesh.uv;
  }

  // remove data that corresponds to triangles that dont exist
  // also splits all triangles
  public void Clean() {
    int v = 0;
    Vector3[] vert = new Vector3[tri.Length];
    Vector2[] uv = new Vector2[tri.Length];
    for (int t = 0; t < tri.Length; t ++) {
      vert[v] = this.vert[tri[t]];
      uv[v] = this.uv[tri[t]];
      tri[t] = v;
      v ++;
    }
    this.vert = vert;
    this.uv = uv;
  }

  public Mesh ToMesh(Mesh mesh = null) {
    if (mesh == null) {
      mesh = new Mesh();
      mesh.name = "terrain mesh";
    }
    mesh.vertices = vert;
    mesh.triangles = tri;
    mesh.uv = uv;
    Vector3[] norm = new Vector3[tri.Length];
    for (int t = 0; t < tri.Length;) {
      Vector3 a = vert[tri[t + 0]];
      Vector3 b = vert[tri[t + 1]];
      Vector3 c = vert[tri[t + 2]];
      b -= a;
      c -= a;
      Vector3 normal = Vector3.Cross(b, c).normalized;
      norm[t++] = normal;
      norm[t++] = normal;
      norm[t++] = normal;
    }
    mesh.normals = norm;
    return mesh;
  }

}
