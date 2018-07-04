using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Part))]
public class PartEditor : Editor {

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    Part part = (Part) target;
    if (GUILayout.Button("Spawn Parts")) {
      foreach (PartNode node in part.GetComponentsInChildren<PartNode>()) {
        if (node.spawnPart != null) {
          Part child = Instantiate(node.spawnPart);
          child.transform.parent = node.transform;
          child.transform.localPosition = Vector3.zero;
          child.transform.localRotation = Quaternion.identity;
        }
      }
    }
    if (GUILayout.Button("Destroy Parts")) {
      foreach (PartNode node in part.GetComponentsInChildren<PartNode>()) {
        for (int i = 0; i < node.transform.childCount; i ++) {
          DestroyImmediate(node.transform.GetChild(i).gameObject);
        }
      }
    }
  }

}

[CustomEditor(typeof(PartNode))]
public class PartNodeEditor : Editor {

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    PartNode node = (PartNode) target;
    if (node.transform.childCount == 0) {
      if (GUILayout.Button("Spawn")) {
        if (node.spawnPart != null) {
          Part part = Instantiate(node.spawnPart);
          part.transform.parent = node.transform;
          part.transform.localPosition = Vector3.zero;
          part.transform.localRotation = Quaternion.identity;
        }
      }
    }
    else {
      if (GUILayout.Button("Destroy")) {
        for (int i = 0; i < node.transform.childCount; i ++) {
          DestroyImmediate(node.transform.GetChild(i).gameObject);
        }
      }
    }
  }

}
