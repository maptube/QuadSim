using UnityEngine;
using System.Collections;

public class LapColliderScript : MonoBehaviour {
	//this is a script which is attached to all the collider objects which make up the lap so that we can do the timing
	GameObject _LapColliders;
	RaceTimerScript _RaceTimerScript;

	//settings
	public string MyName;
	public int SequenceNumber;

	// Use this for initialization
	void Start () {
		_LapColliders = GameObject.Find ("LapColliders");
		_RaceTimerScript = _LapColliders.GetComponent<RaceTimerScript> ();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter(Collider other) {
		//other is the quadcopter
		//Debug.Log ("COLLISION");
		_RaceTimerScript.LapTriggerPoint (MyName,SequenceNumber);
	}
}
