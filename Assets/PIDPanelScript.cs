using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PIDPanelScript : MonoBehaviour {
	GameObject quad;
	QuadcopterBehaviourScript script;

	// Use this for initialization
	void Start () {
		//GameObject quad = GameObject.Find (/*"testquadcopter2"*/"F450");
		quad = GameObject.Find ("F450");
		script = quad.GetComponent<QuadcopterBehaviourScript> ();
		//set initial values here...
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void AileronPChange(float P) {
		script.setAileronP (P);
	}

}
