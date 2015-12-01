using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LookAtQuadcopter : MonoBehaviour {
	//attached to the camera, controls how the camera behaves in relation to the quadcopter, i.e. is it from where
	//a person is standing controlling the quad, or is it attached to the quad and flying with it (FPV)?
	//Handles keyboard and gamepad inputs to toggle between viewpoint types.
	//
	//Keys:
	//Currently using space on keyboard.
	//Gamepad:
	//TODO:

	//position of the person for 3rd person view mode
	const float PersonX=0, PersonY=1.8f, PersonZ=0;

	//Viewpoint types:
	//FPV is first person view with camera fixed on the front of the aircraft
	//Gimbal is a hanging camera which is stabilised to be level with the ground at all times (no roll)
	//3rdPerson is the normal RC viepoint of standing in the field looking at the aircraft in front of you
	//TODO: add chase cam etc?
	enum ViewpointType { vpFPV, vpGimbal, vp3rdPerson };

	ViewpointType _ViewType = ViewpointType.vpFPV;
	ViewpointType ViewType
	{
		get { return _ViewType; }
		set {
			_ViewType = value;
			if (_ViewType==ViewpointType.vp3rdPerson)
			{
				transform.position = new Vector3(PersonX,PersonY,PersonZ);
			}
		}
	}

	string ViewpointText() {
		switch (_ViewType) {
		case ViewpointType.vpFPV: return "FPV";
		case ViewpointType.vpGimbal: return "Gimbal";
		case ViewpointType.vp3rdPerson: return "3rd Person";
		}
		return "Unknown";
	}

	//Switches the view to the next one in the sequence.
	public void ToggleViewpoint() {
		switch (_ViewType) {
		case ViewpointType.vpFPV:
			_ViewType = ViewpointType.vpGimbal;
			break;
		case ViewpointType.vpGimbal:
			_ViewType=ViewpointType.vp3rdPerson;
			break;
		case ViewpointType.vp3rdPerson:
			_ViewType=ViewpointType.vpFPV;
			break;
		}
		//and set the button text
		GameObject but = GameObject.Find("ViewpointButton");
		but.GetComponentInChildren<Text>().text = ViewpointText() + "(A)";
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//UI stuff
		if (Input.GetKeyDown (KeyCode.A) || Input.GetButtonDown("SpeedlinkButtonA")) {
			//viewpoint type toggle
			ToggleViewpoint ();
		}
		//end UI stuff

		//Mesh quad = GameObject.Find("testquadcopter").GetComponent<Mesh> ();
		GameObject quad = GameObject.Find (/*"testquadcopter2"*/"F450");

		switch (_ViewType) {
		case ViewpointType.vpFPV:
			transform.position = quad.transform.position;
			transform.rotation = quad.transform.rotation; //look straight out the front of the quad
			transform.RotateAround(quad.transform.position,quad.transform.right,-20.0f); //FPV look up
			//NO transform.RotateAround(transform.position, Vector3.right, 20.0f); //FPV look up
			//transform.position=quad.transform.position;
			//transform.forward = quad.transform.forward; //make view look out the front
			//transform.up = quad.transform.up; //in this view, the camera is fixed to the frame and rolls with it
			//transform.right = quad.transform.right;
			break;
		case ViewpointType.vpGimbal:
			transform.position=quad.transform.position;
			transform.forward = quad.transform.forward; //make view look out the front (but no roll)
			break;
		case ViewpointType.vp3rdPerson:
			transform.LookAt(quad.transform);
			break;
		}
	}
}
