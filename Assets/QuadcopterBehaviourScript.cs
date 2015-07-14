using UnityEngine;
using System.Collections;

/// <summary>
/// PID controller.
/// </summary>
public class PIDController {
	public float Kp=1.0f; //Proportional gain constant
	public float Ki=1.0f; //Integral gain constant
	public float Kd=1.0f; //Derivative gain constant

	private float e0; //previous error value
	private float I; //Integral accumulator
	private float D; //Derivative accumulator

	public PIDController(float Kp,float Ki,float Kd) {
		this.Kp = Kp;
		this.Ki = Ki;
		this.Kd = Kd;
		e0 = 0;
		I = 0;
		D = 0;
	}

	/// <summary>
	/// Run the PID calculation with a new error value and delta time from the last calculation.
	/// </summary>
	/// <param name="e">Error value</param>
	/// <param name="deltaT">Time elapsed since process was last called i.e. time between e and e0</param>
	public float process(float e,float deltaT) {
		//assert deltaT>0 ?
		I += (deltaT / 2.0f) * (e0 + e);
		D += (e - e0) / deltaT;
		float u = Kp * e + Ki * I + Kd * D; //standard PID formula
		e0 = e;
		return u;
	}
}

public class QuadcopterBehaviourScript : MonoBehaviour {
	Rect textArea = new Rect(0,0,Screen.width, Screen.height);
	Rigidbody rb;

	PIDController pidAileron = new PIDController (0.5f, 0.0001f, 0.0001f);
	PIDController pidElevator = new PIDController(0.5f,0.0001f,0.0001f);
	PIDController pidRudder = new PIDController(0.5f,0.0001f,0.0001f);
	float aileron, elevator, rudder, throttle;
	float txRollAngle, txPitchAngle, txYawRate;
	float rollAngle, pitchAngle, yawRate;

	void OnGUI() {
		GUI.Label(textArea,"A: "+txRollAngle+" ("+rollAngle+")\n"
		          +"E: "+txPitchAngle+" ("+pitchAngle+")\n"
		          +"R: "+txYawRate+"("+yawRate+")\n"
		          +"T: "+throttle);
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		// Change the mass of the object's Rigidbody.
		rb.mass = 1.0f;
		rb.drag = 0.5f;
		rb.angularDrag = 4.0f; //or inertiaTensor? what units is this in?
	}
	
	// Update is called once per frame
	void Update () {
		//float speed = 1.0f;
		//float distance = 1.0f; //speed * Time.deltaTime * Input.GetAxis("Horizontal");
		//transform.Translate(Vector3.right * distance);
		//Rigidbody rb = GetComponent<Rigidbody>();
		//transform.Translate (rb.velocity);
	}

	//physics body pre-update
	void FixedUpdate() {
		//user control inputs
		//rudder = Input.GetAxis("Mouse X")-Screen.width/2;
		//elevator = Input.GetAxis ("Mouse Y"); //-Screen.height/2;

		aileron = 0; //(Input.mousePosition.x-Screen.width/2.0f)/Screen.width*2.0f; //todo: tidy formula
		elevator = -(Input.mousePosition.y-Screen.height/2.0f)/Screen.height*2.0f;
		rudder = (Input.mousePosition.x-Screen.width/2.0f)/Screen.width*2.0f;
		//convert to requested angles etc
		float thrust = 10.0f;
		//ailerons and elevator set requested angles to horizontal, rudder sets rotation speed
		txRollAngle = -aileron * 20.0f; //degrees - TODO: need to fix axis problem here
		txPitchAngle = elevator * 20.0f; //degrees
		txYawRate = rudder * 4.0f; //degrees per second (?)

		//current body angles
		Vector3 euler = transform.eulerAngles; //localEulerAngles?
		rollAngle = euler.x;
		if (rollAngle > 180.0f)
			rollAngle = rollAngle - 360.0f;
		pitchAngle = euler.z;
		if (pitchAngle > 180.0f)
			pitchAngle = pitchAngle - 360.0f;
		yawRate = rb.angularVelocity.z; //degrees per second?

		//PID Calculations using errors
		float P = pidAileron.process (txRollAngle - rollAngle, Time.deltaTime);
		float Q = pidElevator.process (txPitchAngle - pitchAngle, Time.deltaTime);
		float R = pidRudder.process (txYawRate - yawRate, Time.deltaTime);

		//float driveForce = 150.0f;
		//Vector3 force = transform.forward * driveForce * Input.GetAxis("Vertical");
		//GetComponent<Rigidbody>().AddForce(force);



		//todo: need PID controller here..

		//rb.AddForce(transform.up * thrust);
		rb.AddRelativeForce (new Vector3 (0, thrust, 0));
		//rb.AddTorque (new Vector3 (0.01f, 0f, 0f));
		//rb.AddTorque (new Vector3 (0f, 0.01f, 0f)); //spin upright
		//rb.AddTorque (new Vector3 (-0.004f*txPitchAngle, 0.0f/*0.3f*valueYaw*/, 0f)); //pitch, yaw, roll axis

		//rb.AddTorque (new Vector3 (0, 0, P)); //pitch, yaw, roll axis
		//rb.AddTorque (new Vector3 (-Q, 0, 0)); //pitch, yaw, roll axis
		//rb.AddTorque (new Vector3 (0, R, 0)); //pitch, yaw, roll axis

		//rb.AddTorque (new Vector3 (-Q, 0, P)); //elevator, aileron
		//rb.AddTorque(new Vector3(-Q,R,0)); //elevator, rudder

		rb.AddRelativeTorque(new Vector3(P,R,Q));
	}

}
