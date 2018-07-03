using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class GridTerrain : TerrainBase {

  [Header("Terrain")]
  public Material terrainMaterial;
  public float terrainTileLength = 15f; // the approximate edge length of a triangle of the final terrain mesh

  [Header("Grid")]
  public Mesh baseMesh;
  public float fragmentLength = 25f;     // the approximate edge length of an individual fragment mesh

  [Header("Fragments")]
  public float tileLength = 1f;         // the approximate edge length of an individual terrain triangle
  public int detailLevelCount = 4;      // how many levels of detail should fragments generate for?
  public AnimationCurve detailFactorCurve = AnimationCurve.Linear(0, 0, 1, 1);

  public Task<TMesh> gridMeshTask { get; private set; }   // the task generating the grid mesh
  public TMesh gridMesh { get; private set; }             // the grid mesh itself

  Transform terrainParent;
  Transform sectionsParent;
  Transform fragmentsParent;

  public int gridSize { get; private set; }

  List<TFragment> fragments;

  [HideInInspector] public TFragment fragmentPrefab;
  [HideInInspector] public MeshRenderer terrainSectionPrefab;

  ConcurrentQueue<TMesh> finishedSections;
  int sectionTasksRemaining;

  public override void Generate() {
    // start worker threads for fragments
    if (!TaskManager.workersAreRunning) {
      TaskManager.StartWorkers(4);
    }
    // initialize generator
    if (generator != null) {
      generator.Initialize();
    }
    // create heirarchy
    terrainParent = new GameObject("terrain").transform;
    sectionsParent = new GameObject("sections").transform;
    fragmentsParent = new GameObject("fragments").transform;
    terrainParent.parent = transform;
    sectionsParent.parent = terrainParent;
    fragmentsParent.parent = terrainParent;
    // start task to generate grid mesh
    TMesh baseMesh = new TMesh(this.baseMesh);
    baseMesh.Clean();
    // make sure the base mesh is the right scale
    Vector3[] vs = baseMesh.vert;
    for (int i = 0; i < vs.Length; i ++) {
      vs[i] = gfield.LocalPointToSurface(vs[i]);
    }
    gridMeshTask = TaskManager.Schedule(() => {
      return GenerateGridMesh(baseMesh, fragmentLength);
    });
    // start terrain section generation tasks
    finishedSections = new ConcurrentQueue<TMesh>();
    int sectionGridSize = MeshToGridSize(baseMesh, terrainTileLength);
    sectionTasksRemaining = baseMesh.tri.Length / 3;
    ForeachTriangle(baseMesh, (a, b, c) => {
      TaskManager.Schedule(() => {
        TMesh section = TMesh.CreateSubGrid(a, b, c, sectionGridSize);
        if (generator != null) {
          section = generator.Generate(section);
        }
        finishedSections.Enqueue(section);
      });
    });
  }

  void ForeachTriangle(TMesh mesh, Action<Vector3, Vector3, Vector3> action) {
    var vs = mesh.vert;
    var ts = mesh.tri;
    for (var i = 0; i < ts.Length;) {
      Vector3 a = vs[ts[i++]];
      Vector3 b = vs[ts[i++]];
      Vector3 c = vs[ts[i++]];
      action(a, b, c);
    }
  }

  void Update() {
    // once the grid mesh is complete, we can create the fragments
    if (gridMeshTask != null && gridMeshTask.isDone && gridMesh.isInvalid) {
      gridMesh = gridMeshTask.result;
      fragments = new List<TFragment>();
      // create a new fragment for each triangle in the grid mesh
      gridSize = MeshToGridSize(gridMesh, tileLength);
      ForeachTriangle(gridMesh, (a, b, c) => {
        var frag = fragmentPrefab.Instantiate(this, a, b, c, gridSize);
        frag.transform.parent = fragmentsParent;
        frag.gameObject.SetActive(false);
        fragments.Add(frag);
      });
    }
    // as the section mesh tasks complete, add them to the model
    if (sectionTasksRemaining > 0) {
      TMesh sectionMesh;
      while (finishedSections.TryDequeue(out sectionMesh)) {
        sectionTasksRemaining --;
        CreateTerrainSection(sectionMesh.ToMesh());
      }
    }
    // if we have the fragments, go ahead and update them
    if (fragments != null) {
      foreach (TFragment frag in fragments) {
        bool canBeSeenByLocalPlayer = false;
        float maxDetail = float.MinValue;
        foreach (TViewer view in TViewer.all) {
          if (view.isServer || view.isLocalPlayer) {
            float detail = view.GetDetailLevel(frag.worldCenter);
            if (detail >= maxDetail) {
              maxDetail = detail;
            }
            if (view.isLocalPlayer && detail > 0) {
              canBeSeenByLocalPlayer = true;
            }
          }
        }
        frag.SetDetailFactor(maxDetail);
        if (maxDetail > 0) {
          frag.Load();
          frag.SetVisible(canBeSeenByLocalPlayer);
        }
        else {
          frag.Unload();
        }
      }
    }
  }

  int MeshToGridSize(TMesh mesh, float edgeLength) {
    // assuming a mesh with regular spacing between vertices
    var vs = mesh.vert;
    var ts = mesh.tri;
    return SegmentToGridSize(vs[ts[0]], vs[ts[1]], edgeLength);
  }

  public int SegmentToGridSize(Vector3 a, Vector3 b, float edgeLength) {
    int n = 1 + (int) Mathf.Round((a - b).magnitude / edgeLength);
    return n < 2 ? 2 : n;
  }

  TMesh GenerateGridMesh(TMesh baseMesh, float edgeLength) {
    var vs = baseMesh.vert;
    Vector3 a = vs[0];
    Vector3 b = vs[1];
    int gridSize = SegmentToGridSize(a, b, edgeLength);
    return baseMesh.SubGrid(gridSize);
  }

  MeshRenderer CreateTerrainSection(Mesh mesh) {
    MeshRenderer renderer = Instantiate(terrainSectionPrefab);
    MeshFilter filter = renderer.GetComponent<MeshFilter>();
    renderer.name = terrainSectionPrefab.name;
    filter.sharedMesh = mesh;
    renderer.sharedMaterial = terrainMaterial;
    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetFloat("_CullSphereEnabled", 1);
    renderer.SetPropertyBlock(propertyBlock);
    renderer.transform.parent = sectionsParent;
    return renderer;
  }

}
