using UnityEngine;
using System.Collections.Generic;

public class TFragment : MonoBehaviour {

  public int detailLevelCount = 4; // how many levels of detali should this fragment generate for?

  public GridTerrain terrain { get; private set; }

  public Vector3 center { get; private set; }       // the center of this fragment, local to terrain

  public Vector3 worldCenter => terrain.transform.TransformPoint(center);

  public MeshFilter filter { get; private set; }
  public MeshCollider hull { get; private set; }
  public MeshRenderer render { get; private set; }

  public bool isLoaded { get; private set; } = false;
  public bool isVisible { get; private set; } = true;

  public bool hasGenerated { get; private set; } = false;

  public float detailFactor { get; private set; }

  public Vector3 a { get; private set; }
  public Vector3 b { get; private set; }
  public Vector3 c { get; private set; }

  // a set of all unloaded surfacebodies that should be loaded when this fragment is
  public HashSet<GBody> unloadedBodies { get; private set; } = new HashSet<GBody>();

  DetailMesh[] detailMeshes;    // collection of detail meshes for each detail level

  void Awake() {
    filter = GetComponent<MeshFilter>();
    hull = GetComponent<MeshCollider>();
    render = GetComponent<MeshRenderer>();
    detailMeshes = new DetailMesh[detailLevelCount];
    for (int i = 0; i < detailMeshes.Length; i ++) {
      detailMeshes[i] = new DetailMesh();
    }
  }

  void Update() {
    if (isLoaded) {
      // always use the highest detail level mesh for the hull
      Mesh hullMesh = GetDetailMesh(1, 1 - detailFactor);
      hull.enabled = hullMesh != null;
      if (hullMesh != null) {
        hull.sharedMesh = hullMesh;
      }
      UpdateDetailLevel(this.detailFactor);
    }
  }

  public TFragment Instantiate(GridTerrain terrain, Vector3 a, Vector3 b, Vector3 c) {
    TFragment frag = Instantiate(this);
    frag.gameObject.SetActive(true);
    frag.terrain = terrain;
    frag.name = name;
    frag.center = (a + b + c) / 3;
    frag.a = a;
    frag.b = b;
    frag.c = c;
    return frag;
  }

  public int DetailFactorToDetailLevel(float detailFactor) {
    detailFactor = Mathf.Clamp01(detailFactor);
    int detailLevel = (int) Mathf.Floor(detailFactor * (float) detailLevelCount);
    if (detailLevel >= detailLevelCount) detailLevel --;
    return detailLevel;
  }

  private TMeshData CreateSubGrid(Vector3 a, Vector3 b, Vector3 c, int n = 2) {
    int tLength = (n - 1) * (n - 1) * 3;
    Vector3[] vs = new Vector3[tLength];
    int[] ts = new int[tLength];
    for (int i = 0; i < tLength; i ++) {
      ts[i] = i;
    }
    Vector3 p = (b - a);
    Vector3 q = (c - a);
    int v = 0;
    for (int i = 0; i < n; i ++) {
      int colHeight = n - i;
      for (int j = 0; j < colHeight; j ++) {
        if (i > 0) {
          if (j > 0) {
            vs[v ++] = SubVert(a, p, q, n, i, j);
            vs[v ++] = SubVert(a, p, q, n, i - 1, j);
            vs[v ++] = SubVert(a, p, q, n, i, j - 1);
          }
          vs[v ++] = SubVert(a, p, q, n, i, j);
          vs[v ++] = SubVert(a, p, q, n, i - 1, j + 1);
          vs[v ++] = SubVert(a, p, q, n, i - 1, j);
        }
      }
    }
    return new TMeshData(vs, ts);
  }

  private Vector3 SubVert(Vector3 a, Vector3 p, Vector3 q, int n, int i, int j) {
    return (a +
      (p * ((float) i / (n - 1))) +
      (q * ((float) j / (n - 1)))
    );
  }

  public void Load() {
    if (!isLoaded) {
      isLoaded = true;
      gameObject.SetActive(true);
      // if (terrain.isServer) {
      //   foreach (GBody body in unloadedBodies) {
      //     body.Load();
      //   }
      //   unloadedBodies.Clear();
      // }
    }
  }

  public void Unload() {
    if (isLoaded) {
      isLoaded = false;
      // if (terrain.isServer) {
      //   foreach (GBody body in GBody.loaded) {
      //     if (SphereIsOverFragment(body.transform.position, body.loadRadius)) {
      //       unloadedBodies.Add(body);
      //     }
      //   }
      //   foreach (GBody body in unloadedBodies) {
      //     body.Unload();
      //   }
      // }
      // hull.enabled = false;
      gameObject.SetActive(false);
    }
  }

  RaycastHit[] hits = new RaycastHit[16];

  public bool SphereIsOverFragment(Vector3 position, float radius) {
    position = terrain.WorldPointToTop(position);
    int hitCount = terrain.SphereCastNonAlloc(position, radius, hits);
    for (int i = 0; i < hitCount; i ++) {
      if (hits[i].collider == hull) {
        return true;
      }
    }
    return false;
  }

  public void SetDetailFactor(float detailFactor) {
    this.detailFactor = detailFactor;
  }

  void UpdateDetailLevel(float detailFactor) {
    Mesh mesh = GetDetailMesh(detailFactor);
    if (mesh != null) {
      // only change the mesh if there is a mesh to change to
      filter.mesh = mesh;
    }
  }

  // get the mesh for certain detail level
  // if it isnt done generating, returns null
  // priority is used as the priority for the threaded task if the mesh needs to be generated
  public Mesh GetDetailMesh(float detailFactor, float priority) {
    int detailLevel = DetailFactorToDetailLevel(detailFactor);
    DetailMesh detailMesh = detailMeshes[detailLevel];
    Mesh mesh = detailMesh.mesh;
    if (mesh == null) {
      Task<TMeshData> meshTask = StartMeshGeneration(detailFactor, priority);
      if (meshTask.isDone) {
        mesh = meshTask.result.ToMesh();
        detailMesh.mesh = mesh;
      }
    }
    return mesh;
  }
  public Mesh GetDetailMesh(float detailFactor) => GetDetailMesh(detailFactor, 1 - detailFactor);

  // get the task responsible for generating the detail mesh
  // if the task isnt running yet, start it
  // pririty is used to set the priority of the generation task
  Task<TMeshData> StartMeshGeneration(float detailFactor, float priority) {
    int detailLevel = DetailFactorToDetailLevel(detailFactor);
    DetailMesh detailMesh = detailMeshes[detailLevel];
    Task<TMeshData> task = detailMesh.task;
    if (task == null) {
      task = TaskManager.Schedule(priority, () => {
        return GenerateMesh(detailLevel);
      });
      detailMesh.task = task;
    }
    return task;
  }
  Task<TMeshData> StartMeshGeneration(float detailFactor) => StartMeshGeneration(detailFactor, 1 - detailFactor);

  // mesh generation function
  // this is run in a worker thread
  public TMeshData GenerateMesh(int detailLevel) {
    int gridSize = (int) (terrain.gridSize * ((float) detailLevel / (float) detailLevelCount));
    if (gridSize < 2) gridSize = 2;
    TMeshData data = CreateSubGrid(a, b, c, gridSize);
    if (terrain.generator != null) {
      data = terrain.generator.Generate(data);
    }
    data.Clean();
    return data;
  }

  public void SetVisible(bool isVisible) {
    this.isVisible = isVisible;
    render.enabled = isVisible;
  }

  class DetailMesh {

    public Mesh mesh;
    public Task<TMeshData> task;

  }

}
