using UnityEngine;
using System.Collections;

public class LookAtQuadcopter : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Mesh quad = GameObject.Find("testquadcopter").GetComponent<Mesh> ();
		GameObject quad = GameObject.Find ("testquadcopter");
		transform.LookAt(quad.transform);
	}
}
