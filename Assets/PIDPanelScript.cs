using UnityEngine;
using UnityEngine.UI;
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

		//set initial values  and wire up change events here
		//TODO: the ailePSlider.value = script.aileronP bit could do with improving. It's going to read the value from the quadcopter PID,
		//set the slider value, trigger the slider's change event, write the label (which is what we want) and then write the same value back
		//to the quadcopter PID again.
		GameObject gm;
		gm = GameObject.Find ("AilePSlider");
		Slider ailePSlider = gm.GetComponent<Slider> ();
		ailePSlider.onValueChanged.RemoveAllListeners ();
		ailePSlider.onValueChanged.AddListener(AileronPChange);
		ailePSlider.value = script.aileronP;

		gm = GameObject.Find ("AileISlider");
		Slider aileISlider = gm.GetComponent<Slider> ();
		aileISlider.onValueChanged.RemoveAllListeners ();
		aileISlider.onValueChanged.AddListener(AileronIChange);
		aileISlider.value = script.aileronI;

		gm = GameObject.Find ("AileDSlider");
		Slider aileDSlider = gm.GetComponent<Slider> ();
		aileDSlider.onValueChanged.RemoveAllListeners ();
		aileDSlider.onValueChanged.AddListener(AileronDChange);
		aileDSlider.value = script.aileronD;

		gm = GameObject.Find ("ElePSlider");
		Slider elePSlider = gm.GetComponent<Slider> ();
		elePSlider.onValueChanged.RemoveAllListeners ();
		elePSlider.onValueChanged.AddListener(ElevatorPChange);
		elePSlider.value = script.elevatorP;

		gm = GameObject.Find ("EleISlider");
		Slider eleISlider = gm.GetComponent<Slider> ();
		eleISlider.onValueChanged.RemoveAllListeners ();
		eleISlider.onValueChanged.AddListener(ElevatorIChange);
		eleISlider.value = script.elevatorI;

		gm = GameObject.Find ("EleDSlider");
		Slider eleDSlider = gm.GetComponent<Slider> ();
		eleDSlider.onValueChanged.RemoveAllListeners ();
		eleDSlider.onValueChanged.AddListener(ElevatorDChange);
		eleDSlider.value = script.elevatorD;

		gm = GameObject.Find ("RudPSlider");
		Slider rudPSlider = gm.GetComponent<Slider> ();
		rudPSlider.onValueChanged.RemoveAllListeners ();
		rudPSlider.onValueChanged.AddListener(RudderPChange);
		rudPSlider.value = script.rudderP;

		gm = GameObject.Find ("RudISlider");
		Slider rudISlider = gm.GetComponent<Slider> ();
		rudISlider.onValueChanged.RemoveAllListeners ();
		rudISlider.onValueChanged.AddListener(RudderIChange);
		rudISlider.value = script.rudderI;

		gm = GameObject.Find ("RudDSlider");
		Slider rudDSlider = gm.GetComponent<Slider> ();
		rudDSlider.onValueChanged.RemoveAllListeners ();
		rudDSlider.onValueChanged.AddListener(RudderDChange);
		rudDSlider.value = script.rudderD;

	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void AileronPChange(float P) {
		script.aileronP = P;
		GameObject gm = GameObject.Find ("AilePText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = P.ToString ("0.000");
	}

	public void AileronIChange(float I) {
		script.aileronI = I;
		GameObject gm = GameObject.Find ("AileIText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = I.ToString ("0.0000");
	}

	public void AileronDChange(float D) {
		script.aileronD = D;
		GameObject gm = GameObject.Find ("AileDText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = D.ToString ("0.0000");
	}

	public void ElevatorPChange(float P) {
		script.elevatorP = P;
		GameObject gm = GameObject.Find ("ElePText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = P.ToString ("0.000");
	}

	public void ElevatorIChange(float I) {
		script.elevatorI = I;
		GameObject gm = GameObject.Find ("EleIText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = I.ToString ("0.0000");
	}

	public void ElevatorDChange(float D) {
		script.elevatorD = D;
		GameObject gm = GameObject.Find ("EleDText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = D.ToString ("0.0000");
	}

	public void RudderPChange(float P) {
		script.rudderP = P;
		GameObject gm = GameObject.Find ("RudPText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = P.ToString ("0.000");
	}

	public void RudderIChange(float I) {
		script.rudderI = I;
		GameObject gm = GameObject.Find ("RudIText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = I.ToString ("0.0000");
	}

	public void RudderDChange(float D) {
		script.rudderD = D;
		GameObject gm = GameObject.Find ("RudDText");
		Text theText = gm.GetComponent<Text> ();
		theText.text = D.ToString ("0.0000");
	}

}
