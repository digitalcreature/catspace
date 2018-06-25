using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Part))]
public class PartEditor : Editor {

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    Part part = (Part) target;
    if (GUILayout.Button("Spawn Parts")) {
      foreach (PartSpawn spawn in part.GetComponentsInChildren<PartSpawn>()) {
        if (spawn.partPrefab != null) {
          Part child = Instantiate(spawn.partPrefab);
          child.transform.parent = spawn.transform;
          child.transform.localPosition = Vector3.zero;
          child.transform.localRotation = Quaternion.identity;
        }
      }
    }
    if (GUILayout.Button("Destroy Parts")) {
      foreach (PartSpawn spawn in part.GetComponentsInChildren<PartSpawn>()) {
        for (int i = 0; i < spawn.transform.childCount; i ++) {
          DestroyImmediate(spawn.transform.GetChild(i).gameObject);
        }
      }
    }
  }

}

[CustomEditor(typeof(PartSpawn))]
public class PartSpawnEditor : Editor {

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    PartSpawn spawn = (PartSpawn) target;
    if (spawn.transform.childCount == 0) {
      if (GUILayout.Button("Spawn")) {
        if (spawn.partPrefab != null) {
          Part part = Instantiate(spawn.partPrefab);
          part.transform.parent = spawn.transform;
          part.transform.localPosition = Vector3.zero;
          part.transform.localRotation = Quaternion.identity;
        }
      }
    }
    else {
      if (GUILayout.Button("Destroy")) {
        for (int i = 0; i < spawn.transform.childCount; i ++) {
          DestroyImmediate(spawn.transform.GetChild(i).gameObject);
        }
      }
    }
  }

}
