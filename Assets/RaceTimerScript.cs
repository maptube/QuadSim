using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RaceTimerScript : MonoBehaviour {
	float timer; //current timer value
	float splitTime; //time since last gate for split times
	string Message; //message displayed in addition to timer
	Text raceTimerText; //component to write the timer value and message to
	int LastSequenceNumber; //last sequence number gate that we went through

	List<float> LapTimes = new List<float>(); //list of lap times

	// Use this for initialization
	void Start () {
		GameObject goRaceTimerText = GameObject.Find ("RaceTimerText");
		raceTimerText = goRaceTimerText.GetComponent<Text> ();

		//set up the triggers which guide us around the lap
		//for
		//GameObject goLapColliders = GameObject.Find ("LapColliders");
		//Transform[] ts = gameObject.GetComponentsInChildren<Transform>();
		//foreach (Transform t in ts) {
		//	GameObject go = t.gameObject;
		//	go.GetComponent<>()
		//}
		//BoxCollider box = goLapColliders.GetComponentInChildren<BoxCollider> ();
		//box.OnTriggerEnter.Add (trigger);

		LastSequenceNumber = -1;
		Message = "";
	}
	
	// Update is called once per frame
	void Update () {
		if (LastSequenceNumber < 0)
			return; //don't start until you pass the start point
		timer += Time.deltaTime;
		raceTimerText.text = FormatLapTime (timer) + "\n" + Message;
	
	}

	//returns the current timer time formatted as mm:ss.000
	private static string FormatLapTime(float LapTime) {
		int mm = (int)Mathf.Floor (LapTime / 60);
		float ss = LapTime - (mm * 60);
		return mm.ToString () + ":" + ss.ToString ("00.000");
	}

	public void ResetTimer() {
		timer = 0;
		LastSequenceNumber = -1;
		LapTimes = new List<float> ();
	}

	//called when one of the colliders on the circuit triggers that you've gone through a gate or timing point
	//handles on screen messages about timing
	public void LapTriggerPoint(string Name, int SequenceNumber) {
		//Debug.Log ("Lap Trigger! "+Name);
		//TODO: you could check for going back through the start/finish gate immediately for a very quick lap?
		int SeqDelta = SequenceNumber - LastSequenceNumber;
		if (SeqDelta > 1) {
			Message = "Skipped Gate! " + ((SeqDelta-1) * 20) + "s penalty";
		}
		else if (SequenceNumber == 0) {
			//start finish gate
			if (timer>0) {
				//completed lap
				LapTimes.Add (timer);
				int NumLaps = LapTimes.Count;
				Message = "Lap "+NumLaps+": "+FormatLapTime (timer);
				if (NumLaps>1) {
					float lapDelta = timer-LapTimes[NumLaps-1]; //delta between this lap and last one, -ve is better
					Message+=" delta: "+FormatLapTime (lapDelta);
				}
				timer=0;
			}
			//else you've just started the first lap
		}
		else {
			//split time between last gate or timing point
			float SplitDelta = timer-splitTime;
			Message=Name+" "+FormatLapTime (SplitDelta);
		}
		LastSequenceNumber = SequenceNumber;
		splitTime = timer;
	}

}
