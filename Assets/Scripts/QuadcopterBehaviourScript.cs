using UnityEngine;
using UnityEngine.UI;
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
			//removed imax term
			//if (I > Imax)
			//	I = Imax;
			//else if (I < -Imax)
			//	I = -Imax;
			//end of removal

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

	//Quadcopter physical data (Diatone Blue Ghost)
	float Mass = 0.48f; //KG
    //propeller (TODO: add efficiency?)
    //float PropPitch = 0.1f; //inches
    //float PropDiameter = 0.1f; //inches
    //motor parameters
    //float Kv = 2280.0f; //from the emax box - max thrust is 460g
    //float I0 = 0.6f; //I0 and Rs from Internet data, I0 also given as 0.3A
    //float Rs = 0.117f; //fron Internet - looks high?
    //float VBatt=11.1f; //nominally 3S LiPo
    //physical dimensions
    //float LArm = 0.125f; //moment arm from CG to prop centre in metres
    //other motor consts related to thrust and moment based on rotational speed
    //float Ixx=0.002f, Iyy=0.002f, Izz=0.004f, Izx=0f; //moments of inertia - these are wrong, should be at least times 10
    //Drag?
    float MaxThrust = 10.0f; //Maximum thrust from all four motors - just set this rather than messing about with the motor constants above

    //Control Rates
    const float AxisRateRoll = 25.0f; //this is how many degrees tilt you get for full stick deflection
	const float AxisRatePitch = 25.0f;
	const float AxisRateYaw = 2.0f;
    const float ExpoRoll = 6f; //Y=X^6
    const float ExpoPitch = 6f;
    const float ExpoYaw = 6f;
	//PID parameters	
	const float PID_Aileron_P = 0.5f, PID_Aileron_I = 0.5f, PID_Aileron_D = 0.001f;
	const float PID_Elevator_P = 0.5f, PID_Elevator_I = 0.5f, PID_Elevator_D = 0.001f;
	const float PID_Rudder_P = 0.6f, PID_Rudder_I = 0.4f, PID_Rudder_D = 0.0f;

    float Thrust = 0; //total thrust from all four props in Newtons

	//calculated values which are saved

    //thrust required to hover and joystick multiplier (calculated)
    private float HoverThrust = 0;
    //These are the constants for a two part throttle curve split on the hoverThrust at the joystick mid point.
    private float ThrustConstantLow = 0; //multiplier used with HoverThrust to set hover at joystick centre (t=0), zero at joystick min (t=-1) and max thrust at joystick max (t=+1)
    private float ThrustConstantHigh = 0; //same as above

	//////////////////////////////////////////////////////////////////////////////////////////////////////////
	//PID Controllers for aileron, elevator, rudder and altitude. Associated angles.

	//PIDController pidAileron = new PIDController (0.5f, 0.0001f, 0.0001f);
	//PIDController pidElevator = new PIDController(0.5f,0.0001f,0.0001f);
	//PIDController pidRudder = new PIDController(0.5f,0.0001f,0.0001f);
	//arducopter
	//PIDController pidAileron = new PIDController (0.4f, 0.002f, 0.01f, 5.0f); //0.168f, 0.0f, 0.0f, 5.0f
	//PIDController pidElevator = new PIDController(0.4f, 0.002f, 0.01f, 5.0f); //0.4 0.002 0.01 5.0
	//PIDController pidRudder = new PIDController(0.24f,0.008f,0.001f,5.0f); //1.2f,0.015f,0.001f,5.0f
	//PIDController pidAltitude = new PIDController(0.8f,0.01f,0,5.0f);
	//new
	PIDController pidAileron = new PIDController (PID_Aileron_P,PID_Aileron_I,PID_Aileron_D,5.0f);
	PIDController pidElevator = new PIDController (PID_Elevator_P,PID_Elevator_I,PID_Elevator_D,5.0f);
	PIDController pidRudder = new PIDController(PID_Rudder_P,PID_Rudder_I,PID_Rudder_D,5.0f);
	PIDController pidAltitude = new PIDController(0.8f, 0.01f, 0.0f, 1.0f); //original new PIDController (0.8f, 0.01f, 0, 5.0f);
	

	bool AltitudeHoldModeEnabled = false;
	float AltitudeHold = 0; //altitude to hold to
	float aileron, elevator, rudder, throttle; //read from the joysitck [-1..+1]

    //simulated MEMs sensor angles and rates
	Vector3 gyroAngles;
	Vector3 gyroRates;

	//float E0,E1,E2,E3; //ESC inputs

	//////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Transmitter and control data
	//what are aileron and txAileron?
	//float MaxRollAngle = 40.0f; //roll angle in degrees corresponding to full stick deflection
	//float MaxPitchAngle = 40.0f; //degrees
	//float MaxYawRate = 48.0f; //degrees per second
	

	//reset the quad back to the home position i.e. start again
	public void resetHome() {
		AltitudeHoldModeEnabled = false;
		//rb.position = new Vector3 (6.09f, 2.03f, 15.3f); //original reset position for desert terrain
		rb.position = new Vector3 (-6.77f,-29.7f,-194.23f); //Lima reset position in the green area at the bottom
		rb.velocity = new Vector3 (0, 0, 0);
		rb.angularVelocity = new Vector3 (0, 0, 0);
		//zero pid controllers?
		//put viewpoint back?
	}

	//toggle the altitude hold controller on or off
	public void ToggleAltHold() {
		if (AltitudeHoldModeEnabled) {
			//it's currently on, so switch it off
			AltitudeHoldModeEnabled=false;
			GameObject but = GameObject.Find("AltHoldButton");
			but.GetComponentInChildren<Text>().text = "Alt Hold OFF (B)";
		}
		else {
			AltitudeHoldModeEnabled=true;
			AltitudeHold=rb.position.y;
			GameObject but = GameObject.Find("AltHoldButton");
			but.GetComponentInChildren<Text>().text = "Alt Hold ON (B)";
		}
	}

	public float aileronP {
		get { return pidAileron.Kp; }
		set { pidAileron.Kp = value; }
	}
	public float aileronI {
		get { return pidAileron.Ki; }
		set { pidAileron.Ki = value; }
	}
	public float aileronD {
		get { return pidAileron.Kd; }
		set { pidAileron.Kd = value; }
	}
	public float elevatorP {
		get { return pidElevator.Kp; }
		set { pidElevator.Kp = value; }
	}
	public float elevatorI {
		get { return pidElevator.Ki; }
		set { pidElevator.Ki = value; }
	}
	public float elevatorD {
		get { return pidElevator.Kd; }
		set { pidElevator.Kd = value; }
	}
	public float rudderP {
		get { return pidRudder.Kp; }
		set { pidRudder.Kp = value; }
	}
	public float rudderI {
		get { return pidRudder.Ki; }
		set { pidRudder.Ki = value; }
	}
	public float rudderD {
		get { return pidRudder.Kd; }
		set { pidRudder.Kd = value; }
	}
	//TODO: imax too?
	
	void OnGUI() {
		/*GUI.Label(textArea,"A: "+txRollAngle+" ("+rollAngle+")\n"
		          +"E: "+txPitchAngle+" ("+pitchAngle+")\n"
		          +"R: "+txYawRate+"("+yawRate+")\n"
		          +"T: "+throttle+"\n"
		          //+rb.inertiaTensor.x+" "+rb.inertiaTensor.y+" "+rb.inertiaTensor.z+"\n"
		          //+rb.inertiaTensorRotation.x+" "+rb.inertiaTensorRotation.y+" "+rb.inertiaTensorRotation.z);
		          +pidRudder.debugToString()
		);*/
		GUI.Label (textArea,
		           "A: " + aileron + "\n"
		           + "RollAngle: "+gyroAngles.x+" Target: "+aileron*AxisRateRoll+"\n"
		           +"E: "+elevator+"\n"
		           +"R: "+rudder+"\n"
                   +"T: "+throttle+"\n"
		           +"YawRateTarget: "+rudder*AxisRateYaw+" GyroRate: "+gyroRates.z+"\n"
		           +"gyroRates: "+gyroRates.x.ToString ("N2")+" "+gyroRates.y.ToString ("N2")+" "+gyroRates.z.ToString ("N2")+"\n"
                   +"thrust: "+Thrust+"\n"
                   +"AltHold: "+AltitudeHold+"\n"
		);
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		// Change the mass of the object's Rigidbody.
		rb.mass = this.Mass; //1.0f;
		rb.drag = 0.4f; //was 0.8
        rb.angularDrag = 1.0f; // 3.5f; //0.0001f; //4.5f; //or inertiaTensor? what units is this in?
		rb.centerOfMass = new Vector3 (0, 0, 0);
        //rb.inertiaTensor = new Vector3 (Ixx,Iyy,Izz); /*new Vector3 (1.0f,1.0f,1.0f);*/

        //create an electric motor model from the parameters and determine its max thurst from static RPM
        //ElectricMotorScript EMotor = new ElectricMotorScript (Kv, I0, Rs, VBatt);
        //float StaticRPM = EMotor.SolveStaticRPM (PropDiameter, PropPitch);
        //this.MaxThrust = 5.0f; //EMotor.CalcThrust (PropDiameter, PropPitch, StaticRPM);

        //calculate thrust required to hover and make sure throttle centre equates to this thrust level (t=0), while t=-1 gives zero thrust and t=+1 gives max
        this.HoverThrust = this.Mass * 9.81f;
        this.ThrustConstantLow = this.HoverThrust;
        this.ThrustConstantHigh = (this.MaxThrust - this.HoverThrust);
    }
	
	// Update is called once per frame
	void Update () {
		//UI stuff
		if (Input.GetButtonDown ("SpeedlinkButtonB")) {
			ToggleAltHold ();
		}
	}

    /// <summary>
    /// Take a linear stick position and apply the relevant exponential to it.
    /// </summary>
    /// <param name="val"></param>
    /// <param name="expo"></param>
    /// <returns></returns>
    float ApplyExpo(float val,float expo)
    {
        float Y = Mathf.Pow(Mathf.Abs(val), expo);
        if (val < 0) Y = -1;
        return Y;
    }

	//physics body pre-update
	void FixedUpdate() {
        //Axis system is defined as +Z forwards, +X right and +Y up
        //Aileron rotation is Z, Elevator rotation is X and Rudder rotation is Y

        //motor orientation - note M1=rpm[0]
        //M4 M2 (4 turns clockwise)
        //M3 M1

        //Propeller RPM values from throttle setting - used for visual animation of propeller disks
        float[] propthrottle = new float[] { (throttle+1)/2, (throttle+1)/2, (throttle+1)/2, (throttle+1)/2 }; //as throttle = [-1..+1]
        if (propthrottle[0] > 0) //if the throttle's at zero, then don't spin any props
        {
            propthrottle[0] = propthrottle[0] - aileron * 0.5f - elevator * 0.5f;
            propthrottle[1] = propthrottle[1] - aileron * 0.5f + elevator * 0.5f;
            propthrottle[2] = propthrottle[2] + aileron * 0.5f - elevator * 0.5f;
            propthrottle[3] = propthrottle[3] + aileron * 0.5f + elevator * 0.5f;
        }
        for (int i = 0; i < 4; i++)
        {
            //Component propellerM1 = GetComponent("PropellerM1");
            GameObject propellerM = GameObject.Find("PropellerM"+(i+1)); //PropellerM1, M2, M3,M4
            if (propellerM != null)
            {
                float rpm = 0;
                if (propthrottle[i] > 0.0f) { rpm = propthrottle[i] * 10.0f + 40.0f; }
                if (rpm > 80.0f) { rpm = 80.0f; }
                propellerM.transform.Rotate(new Vector3(0, 0, 1), rpm);
            }
        }

		//user control inputs
		//rudder = Input.GetAxis("Mouse X")-Screen.width/2;
		//elevator = Input.GetAxis ("Mouse Y"); //-Screen.height/2;

		aileron = 0; //(Input.mousePosition.x-Screen.width/2.0f)/Screen.width*2.0f; //todo: tidy formula
		elevator = -(Input.mousePosition.y-Screen.height/2.0f)/Screen.height*2.0f;
		//rudder = (Input.mousePosition.x-Screen.width/2.0f)/Screen.width*2.0f;
		throttle = 0;


        //TODO: need to check platform (Android touch controller), then if Windows, test for joystick (which one?), then fall back to mouse control

        //this is the live control block
        //joystick - the MAD CATZ has left stick Horizontal/Vertical, right stick Yaw/Throttle
        //MadCatz has JoyAxis3=3rd Axis and JoyAxis4=4th Axis
        //Speedlink NX has JoyAxis3=4th Axis and JoyAxis4=5th Axis
        //aileron = Input.GetAxis ("SpeedlinkAxisAileron"); //or "SpeedlinkAxisAileron" or "MadCatzAxisAileron" or "TaranisAileron"
        //elevator = Input.GetAxis ("SpeedlinkAxisElevator"); //or "SpeedlinkAxisElevator" or "MadCatzAxisElevator"
        //rudder = Input.GetAxis ("Horizontal"); //Horizontal, TaranisRudder
        //throttle = Input.GetAxis ("Vertical"); //NOTE: +-1.0 Vertical, TaranisThrottle

        //serial joystick controller
        //aileron = SerialJoystickScript.Aileron;
        //elevator = SerialJoystickScript.Elevator;
        //rudder = SerialJoystickScript.Rudder;
        //throttle = -SerialJoystickScript.Throttle;

        //DirectInput joystick controller
        //aileron = JoystickInputScript.aileron;

        //LeapMotion Joystick
        LeapMotionBehaviourScript.GetControlInputs(out aileron, out elevator, out rudder, out throttle);
        rudder = aileron * 0.25f; //coupled aileron rudder CAR
        //NOTE: throttle = 0..1
        if (!AltitudeHoldModeEnabled) ToggleAltHold(); //keep altitude hold mode on in LeapMotion joystick mode
        //if (throttle > 0) AltitudeHold += 0.1f; //throttle not really throttle - it's a delta height
        //if (throttle < 0) AltitudeHold -= 0.1f;
        float dy = (throttle-0.2f)*0.1f; //direct height (throttle) minus alt hold height to get delta height i.e. we want to go up or down an amount
        if (dy > 2) dy = 2;
        else if (dy < -2) dy = -2;
        AltitudeHold += dy; //take direct height from hand (throttle), subtract alt hold height and add a fraction on to alt hold - move up or down based on height difference
        if (AltitudeHold < 0) AltitudeHold = 0;
        if (AltitudeHold > 100) AltitudeHold = 100;
        //Debug.Log(AltitudeHold+" "+dy+" "+throttle);
        //throttle = throttle * 2.0f - 1.0f; //convert 0..1 to -1..1

        //Touch joystick for Android - NOTE, this is added to the scene as a JoystickGameObject with TXJoystick script attached, which is STATIC
        //One stick, coupled ailerons and rudder, fixed throttle
        //Vector2 v = TXJoystickScript.VJRvector;
        if (Application.platform == RuntimePlatform.Android) {
			Vector2 v = TXJoystickScript.VJRnormals;
			aileron = v.x / 4; //coupled rudder aileron
			//elevator = -v.y;
			rudder = v.x;
			if (elevator>0.1f) throttle=elevator*2.0f;
		}

        //apply exponential settings to transmitter controls
        //aileron = ApplyExpo(aileron, ExpoRoll); //Y=X^Expo, where Expo=1,2,3...
        //elevator = ApplyExpo(elevator, ExpoPitch); //Y=X^Expo, where Expo=1,2,3...
        //rudder = ApplyExpo(rudder, ExpoYaw); //Y=X^Expo, where Expo=1,2,3...

        //Altitude hold controller - override the throttle
        if (AltitudeHoldModeEnabled) {
			throttle = pidAltitude.process(rb.position.y,AltitudeHold,Time.deltaTime);
		}

		//Simulate gyro sensor on the aircraft by measuring current body frame angles
		Vector3 euler = rb.transform.localEulerAngles;
		//roll angle (degrees)
		gyroAngles.x = euler.z;
		if (gyroAngles.x > 180.0f)
			gyroAngles.x = gyroAngles.x - 360.0f;
		gyroAngles.x = -gyroAngles.x;
		//pitch angle (degrees)
		gyroAngles.y = euler.x;
		if (gyroAngles.y > 180.0f)
			gyroAngles.y = gyroAngles.y - 360.0f;
		gyroAngles.y = -gyroAngles.y;
		//note no absolute yaw angle

		//now body angle rates
		Vector3 relAV = transform.InverseTransformDirection(rb.angularVelocity); //need body rates, not world
        gyroRates.x = -relAV.z; //roll rate (degrees per sec)
        gyroRates.y = -relAV.x; //pitch rate
        gyroRates.z = relAV.y; //yaw rate

        float YawRateTarget = rudder * AxisRateYaw;
		float errorAngle;
		//roll axis - it's not entirely clear how you transform an angle error into a target roll rate and put it into the PID controller.
		//I'm using the P value for the controller here as it works well i.e. the linear bit of the controller.
		//TODO: check how the Cleanflight P8[PIDLEVEL] constant works, I've more or less copied that code.
		errorAngle = aileron * AxisRateRoll - gyroAngles.x;
		float RollRateTarget = errorAngle * PID_Aileron_P; //hacked 0.4 - P8[PIDLEVEL]?????
		//pitch axis
		errorAngle = elevator * AxisRatePitch - gyroAngles.y;
		float PitchRateTarget = errorAngle * PID_Elevator_P; //hacked 0.4f;

		//PID Calculations using (actual, desired) which the error is calculated from
		float P = pidAileron.process (gyroRates.x, RollRateTarget, Time.deltaTime);
		float Q = pidElevator.process (gyroRates.y, PitchRateTarget, Time.deltaTime);
		float R = pidRudder.process (gyroRates.z, YawRateTarget, Time.deltaTime);

        //*amount * Time.deltaTime???? in force mode

        //Work out thrust from stick position. We're using a two part throttle curve split on the hover point at the mid throttle position.
        //t=-1 => zero thrust, t=0 => HoverThrust, t=1 => MaxThrust
        if (throttle < 0) Thrust = throttle * ThrustConstantLow + HoverThrust;
        else Thrust = throttle * ThrustConstantHigh + HoverThrust;

        //these are force and torque direct from the PID without modelling the four props, motors and mixer
        rb.AddRelativeForce(new Vector3(0, Thrust, 0));
        rb.AddRelativeTorque(new Vector3(-Q, R, -P));

        //This is the mixer code for simulating the effect of the thrust from the four motors - removed in favour of just using the angles above.
        //I might put this back in as an experiment, but a simulator doesn't need to simulator the four motors to then calculate torques as we can
        //get the torques direct from the PID. It might be a more accurate simulation as you could model the motor delays and inertia as well as
        //prop effects, but it might not work that well with the flight dynamics.
        //PID then go into mixer to command motors
        //Need E0,E1,E2,E3
        // E1 E0
        // E2 E3
        //Clockwise: E1,E3 Anticlockwise: E0,E2
        //these then get me F0,F1,F2,F3 (forces) and M0,M1,M2,M3 (torques)
        //float Hover = 0.1f;
        //E0 = eazSpeedOutput+Hover; E1 = eazSpeedOutput+Hover; E2 = eazSpeedOutput+Hover; E3 = eazSpeedOutput+Hover;
        //E1 += pitchRate / 2; E0 += pitchRate / 2;
        //E2 -= pitchRate / 2; E3 -= pitchRate / 2;
        //
        //E1 += rollRate / 2; E0 -= rollRate / 2;
        //E2 += rollRate / 2; E3 -= rollRate / 2;
        //
        //E1 += yawRate / 2; E0 -= yawRate / 2;
        //E2 -= yawRate / 2; E3 += yawRate / 2;

        //clamping - normalisation
        //float EMax = Mathf.Max (E3, Mathf.Max (E2, Mathf.Max (E1, E0)));
        //if (EMax>1.0f)
        //{
        //	//or subtract the delta?
        //	E0/=EMax; E1/=EMax; E2/=EMax; E3/=EMax;
        //}
        //if (E0 < 0)
        //	E0 = 0;
        //if (E1 < 0)
        //	E1 = 0;
        //if (E2 < 0)
        //	E2 = 0;
        //if (E3 < 0)
        //	E3 = 0;

        //new forces and moments
        //calculate net force and torgue from all motors
        //float F0 = MaxThrust * E0, F1 = MaxThrust * E1, F2 = MaxThrust * E2, F3 = MaxThrust * E3;
        //rb.AddRelativeForce (0,F0 + F1 + F2 + F3,0);
        //rb.AddForce(new Vector3 (0, F0 + F1 + F2 + F3, 0));
        //float P = ((F1 + F2) - (F0 + F3)) * LArm; //it's not quite LArm as that's the hpot and we want the two other sides of the tri, but this will do
        //float Q = 0; //((F1 + F0) - (F2 + F3)) * LArm;
        //float R = 0; //((E0 + E2) - (E1 + E3)) * 0.1f; //TODO: need torque constant for revs E0..3
        //rb.AddRelativeTorque (new Vector3 (Q, R, -P));
    }

	//public void OnTriggerEnter(Collider other) {
	//	Debug.Log ("Quadcopter COLLIDE");
	//}

}
