using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GridTerrain : TerrainBase {

  [Header("Terrain")]
  public Material terrainMaterial;
  public MeshFilter terrainMeshFilter;  // the terrain filter containing the mesh for the terrain at large

  [Header("Grid")]
  public Mesh baseMesh;

  public float fragmentLength = 1f;     // the approximate edge length of an individual fragment mesh
  public float tileLength = 1f;         // the approximate edge length of an individual terrain triangle

  [Header("Fragments")]
  public int detailLevelCount = 4;      // how many levels of detail should fragments generate for?
  public AnimationCurve detailFactorCurve = AnimationCurve.Linear(0, 0, 1, 1);


  public Task<TMesh> gridMeshTask { get; private set; }   // the task generating the grid mesh
  public TMesh gridMesh { get; private set; }             // the grid mesh itself

  public int gridSize { get; private set; }

  List<TFragment> fragments;


  public override void Generate() {
    if (generator != null) {
      generator.Initialize();
    }
    // start worker threads for fragments
    if (!TaskManager.workersAreRunning) {
      TaskManager.StartWorkers(4);
    }
    // start task to generate grid mesh
    TMesh gridMesh = new TMesh(baseMesh);
    gridMeshTask = TaskManager.Schedule(() => {
      this.gridMesh = GenerateGridMesh(gridMesh);
      if (generator != null) {
        return generator.Generate(this.gridMesh);
      }
      else {
        return gridMesh;
      }
    });
  }

  void Update() {
    // once the grid mesh is complete, we can create the fragments
    // as well as the mesh for the rest of the terrain
    if (gridMeshTask != null && gridMeshTask.isDone && fragments == null) {
      // create the mesh for the terrain at large
      MeshRenderer renderer = terrainMeshFilter.GetComponent<MeshRenderer>();
      renderer.sharedMaterial = terrainMaterial;
      // renderer.materials = new Material[] { renderer.sharedMaterial, terrainMaterial };
      terrainMeshFilter.mesh = gridMeshTask.result.ToMesh();

      fragments = new List<TFragment>();
      // create a new fragment for each triangle in the grid mesh
      gridSize = 0;
      var vs = gridMesh.vert;
      var ts = gridMesh.tri;
      for (var i = 0; i < ts.Length;) {
        Vector3 a = gfield.LocalPointToSurface(vs[ts[i++]]);
        Vector3 b = gfield.LocalPointToSurface(vs[ts[i++]]);
        Vector3 c = gfield.LocalPointToSurface(vs[ts[i++]]);
        if (gridSize == 0) {
          gridSize = SegmentToGridSize(a, b, tileLength);
          gridSize = gridSize < 2 ? 2 : gridSize;
        }
        var frag = TFragment.Create(this, a, b, c, gridSize);
        frag.transform.parent = terrainMeshFilter.transform;
        frag.gameObject.SetActive(false);
        fragments.Add(frag);
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

  public int SegmentToGridSize(Vector3 a, Vector3 b, float triLength) {
    int n = 1 + (int) Mathf.Round((a - b).magnitude / triLength);
    return n < 2 ? 2 : n;
  }

  public TMesh GenerateGridMesh(TMesh gridMesh) {
    Vector3[] vs = gridMesh.vert;
    for (int i = 0; i < vs.Length; i ++) {
      vs[i] = gfield.LocalPointToSurface(vs[i]);
    }
    gridMesh.vert = vs;
    Vector3 a = vs[0];
    Vector3 b = vs[1];
    int gridSize = SegmentToGridSize(a, b, fragmentLength);
    return gridMesh.SubGrid(gridSize);
  }

}
