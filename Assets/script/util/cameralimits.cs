using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimits : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		Vector3 eulerAngles = transform.rotation.eulerAngles;
		Debug.Log("transform.rotation angles x: " + eulerAngles.x + " y: " + eulerAngles.y + " z: " + eulerAngles.z); 
	}
}
