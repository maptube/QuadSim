﻿using UnityEngine;
using System.Collections;

/// <summary>
/// PID controller.
/// Arducopter has defaults:
/// Rate Roll: P=0.168, I=0, D=0.008, IMAX=5.0
/// Rate Pitch: P=0.168, I=0.0, D=0.008, IMAX=5.0
/// Rate Yaw: P=0.25, I=0.015, D=0, IMAX=8.0
/// They also use Stabilise Roll, Pitch, Yaw and Throttle Rate and Altitude Hold
/// This is the best intro on PID:
/// http://brettbeauregard.com/blog/2011/04/improving-the-beginners-pid-introduction/
/// </summary>
public class PIDController {
	public float Kp=1.0f; //Proportional gain constant
	public float Ki=1.0f; //Integral gain constant
	public float Kd=1.0f; //Derivative gain constant
	public float Imax=5.0f; //max limit on I
	public float SampleTime = 0.01f; //seconds interval at which computation is done
	public float elapsedDeltaT = 0; //time since last update

	private float x0; //previous input value
	private float e0; //previous error value
	private float I; //Integral accumulator
	private float D; //Derivative accumulator
	private float u; //copy of last output value

	public PIDController(float Kp,float Ki,float Kd,float Imax) {
		this.Kp = Kp;
		this.Ki = Ki;
		this.Kd = Kd;
		this.Imax = Imax;
		x0 = 0;
		e0 = 0;
		I = 0;
		D = 0;
		u = 0;
	}

	public string debugToString() {
		return this.e0 + " " + this.I + " " + this.D;
	}

	/// <summary>
	/// Run the PID calculation with a new error value and delta time from the last calculation.
	/// </summary>
	/// //<param name="e">Error value</param>
	/// <param name="x">Current Value</param>
	/// <param name="y">Desired Value</param> 
	/// <param name="deltaT">Time elapsed since process was last called i.e. time between e and e0</param>
	/// <returns>u=Kp*e + Ki*I + Kd*D</returns>
	public float process(/*float e*/float x, float y,float deltaT) {
		//assert deltaT>0 ?
		elapsedDeltaT += deltaT;
		if (elapsedDeltaT >= SampleTime) {
			float e = y - x; //current error
			//I += (deltaT / 2.0f) * (e0 + e);
			//I += e * deltaT;
			I+=e*SampleTime;
			if (I > Imax)
				I = Imax;
			else if (I < -Imax)
				I = -Imax;
			//D += (e - e0) / deltaT;
			//D = (e - e0) / deltaT;
			//D = (e - e0) / SampleTime;
			float dx = (x - x0) / SampleTime; //change in input, or dInput/dt, this is the derivative kick fix
			//u = Kp * e + Ki * I + Kd * D; //standard PID formula
			u = Kp * e + Ki * I + Kd * dx; //derivative kick modification
			e0 = e;
			x0 = x;
			elapsedDeltaT=0; //or -=SampleTime?
		}
		return u;
	}
}

public class QuadcopterBehaviourScript : MonoBehaviour {
	Rect textArea = new Rect(0,0,Screen.width, Screen.height);
	Rigidbody rb;

	//PIDController pidAileron = new PIDController (0.5f, 0.0001f, 0.0001f);
	//PIDController pidElevator = new PIDController(0.5f,0.0001f,0.0001f);
	//PIDController pidRudder = new PIDController(0.5f,0.0001f,0.0001f);
	//arducopter
	PIDController pidAileron = new PIDController (0.4f, 0.002f, 0.01f, 5.0f); //0.168f, 0.0f, 0.0f, 5.0f
	PIDController pidElevator = new PIDController(0.4f, 0.002f, 0.01f, 5.0f);
	PIDController pidRudder = new PIDController(0.24f,0.008f,0.001f,5.0f); //1.2f,0.015f,0.001f,5.0f
	float aileron, elevator, rudder, throttle;
	float txRollAngle, txPitchAngle, txYawRate;
	float rollAngle, pitchAngle, yawRate;

	void OnGUI() {
		GUI.Label(textArea,"A: "+txRollAngle+" ("+rollAngle+")\n"
		          +"E: "+txPitchAngle+" ("+pitchAngle+")\n"
		          +"R: "+txYawRate+"("+yawRate+")\n"
		          +"T: "+throttle+"\n"
		          //+rb.inertiaTensor.x+" "+rb.inertiaTensor.y+" "+rb.inertiaTensor.z+"\n"
		          //+rb.inertiaTensorRotation.x+" "+rb.inertiaTensorRotation.y+" "+rb.inertiaTensorRotation.z);
		          +pidRudder.debugToString()
		);
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		// Change the mass of the object's Rigidbody.
		rb.mass = 1.0f;
		rb.drag = 0.4f;
		rb.angularDrag = 4.5f; //or inertiaTensor? what units is this in?
		rb.centerOfMass = new Vector3 (0, 0, 0);
		rb.inertiaTensor = new Vector3 (1.0f,1.0f,1.0f);
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

		//joystick - the MAD CATZ has left stick Horizontal/Vertical, right stick Yaw/Throttle
		//MadCatz has JoyAxis3=3rd Axis and JoyAxis4=4th Axis
		aileron = Input.GetAxis ("JoyAxis3");
		elevator = Input.GetAxis ("JoyAxis4");
		rudder = Input.GetAxis ("Horizontal");
		throttle = Input.GetAxis ("Vertical"); //NOTE: +-1.0

		//Speedlink NX has JoyAxis3=4th Axis and JoyAxis4=5th Axis

		//convert to requested angles etc
		float thrust = 10.0f+throttle*5.0f;
		//ailerons and elevator set requested angles to horizontal, rudder sets rotation speed
		txRollAngle = -aileron * 40.0f; //degrees - TODO: need to fix axis problem here
		txPitchAngle = elevator * 40.0f; //degrees
		txYawRate = rudder * 24.0f; //degrees per second (?)

		//current body angles
		Vector3 euler = transform.localEulerAngles; //was eulerAngles, but localEulerAngles seems better?
		rollAngle = euler.x;
		if (rollAngle > 180.0f)
			rollAngle = rollAngle - 360.0f;
		pitchAngle = euler.z;
		if (pitchAngle > 180.0f)
			pitchAngle = pitchAngle - 360.0f;
		yawRate = rb.angularVelocity.z; //degrees per second?

		//PID Calculations using errors
		//float P = pidAileron.process (txRollAngle - rollAngle, Time.deltaTime);
		//float Q = pidElevator.process (txPitchAngle - pitchAngle, Time.deltaTime);
		//float R = pidRudder.process (txYawRate - yawRate, Time.deltaTime);
		//PID Calculations using actual, desired which the error is calculated from
		float P = pidAileron.process (rollAngle, txRollAngle, Time.deltaTime);
		float Q = pidElevator.process (pitchAngle, txPitchAngle, Time.deltaTime);
		float R = pidRudder.process (yawRate, txYawRate, Time.deltaTime);

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
