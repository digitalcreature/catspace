using UnityEngine;
using System.Collections.Generic;

public class Examinable : MonoBehaviour {

  [Layer]
  public int defaultLayer;

  public Transform examineCenter; // where the camera should center on when examining this object

  public List<GameObject> children; // a list of children gameobjects to toggle layers on

  public Vector3 centerPosition => examineCenter == null ? transform.position : examineCenter.position;
  public Quaternion centerRotation => examineCenter == null ? transform.rotation : examineCenter.rotation;

  public void SetLayer(int layer) {
    gameObject.layer = layer;
    foreach (GameObject child in children) {
      child.layer = layer;
    }
  }

  public void ResetLayer() {
    SetLayer(defaultLayer);
  }

}
