using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GridTerrain : TerrainBase {

  public Mesh gridMesh;
  public TFragment fragmentPrefab;
  public float segmentLength = 1f;

  public int gridSize { get; private set; }

  List<TFragment> fragments = new List<TFragment>();

  public Transform fragmentParent;

  public override void Generate() {
    // start worker threads for fragments
    if (!TaskManager.workersAreRunning) {
      TaskManager.StartWorkers(4);
    }
    // iterate over triangles in gridMesh
    gridSize = 0;
    var vs = gridMesh.vertices;
    var ts = gridMesh.triangles;
    for (var i = 0; i < ts.Length;) {
      Vector3 a = gfield.LocalPointToSurface(vs[ts[i++]]);
      Vector3 b = gfield.LocalPointToSurface(vs[ts[i++]]);
      Vector3 c = gfield.LocalPointToSurface(vs[ts[i++]]);
      if (gridSize == 0) {
        gridSize = SegmentToGridSize(a, b);
        gridSize = gridSize < 2 ? 2 : gridSize;
      }
      var frag = fragmentPrefab.Instantiate(this, a, b, c);
      frag.transform.parent = fragmentParent;
      frag.gameObject.SetActive(false);
      fragments.Add(frag);
    }
    if (generator != null) {
      generator.Initialize();
    }
  }

  void Update() {
    foreach (TFragment frag in fragments) {
      bool canBeSeenByLocalPlayer = false;
      float maxDetail = 0;
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

  public int SegmentToGridSize(Vector3 a, Vector3 b) {
    int n = 1 + (int) Mathf.Round((a - b).magnitude / segmentLength);
    return n < 2 ? 2 : n;
  }


}
