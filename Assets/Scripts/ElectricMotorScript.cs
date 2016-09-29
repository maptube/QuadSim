using UnityEngine;
using System.Collections;

public class ElectricMotorScript {
		
	public float StaticRPM = -1.0f;
	public float Rbatt = 0.0f; //resistance of pack (series resistance of every cell)
	public float Rwire = 0.0f; //resistance of motor and ESC wiring
	public float Rs = 0.012f; //series resistance of motor from motor data
	public float I0 = 1.2f; //Motor idle current from motor data
	public float Kv = 1300.0f; //Kv constant of motor in RPM/Volt from motor data
	public float Vbatt = 11.1f; //Nominal battery voltage (all cells)
		
	// Use this for initialization
	//void Start () {
	//	
	//}
		
	// Update is called once per frame
	//void Update () {
	//	
	//}

	public ElectricMotorScript(float Kv,float I0,float Rs,float VBatt)
	{
		this.Kv = Kv;
		this.I0 = I0;
		this.Rs = Rs;
		this.Vbatt = VBatt;
	}
		
	public float SolveStaticRPM(float Diameter,float Pitch)
	{
		//solve static RPM for this motor, prop and pack
		//pitch and diameter in inches
		float R=this.Rbatt+this.Rwire+this.Rs;
		float a=Pitch*Mathf.Pow(Diameter,4.0f)*5.33e-15f;
		float b=1/(this.Kv*this.Kv*R);
		float c=(this.I0*R-this.Vbatt)/(this.Kv*R);
			
		float rpm1=(-b+Mathf.Sqrt(Mathf.Pow(b,2.0f)-4.0f*a*c))/(2*a);
		//double rpm2=(-b-Math.sqrt(Math.pow(b,2)-4*a*c))/(2*a);
		//System.out.println("RPM1="+rpm1+" RPM2="+rpm2);
			
		//Calculate total current by putting RPM back into formula used when solving above
		//double Itot=(Vbatt*k-rpm1)/(k*R);
			
		//alternative current calculation
		//float Vbemf=((float)rpm1)/k; //CHECK THIS!
		//double Itot2=(Vbatt-Vbemf)/(Rbatt+Rwire+Rs);
			
		//approx thrust equation - this could be completely wrong!
		//double thrust=pitch*Math.pow(diameter,3)*Math.pow(rpm1,2)*28.349e-10;
		//thrust=thrust/1000.0*9.81; //grams to Newtons
			
		return rpm1;
	}
		
	public float SolveAirspeedRPM(float Vas,float Diameter)
	{
		//Vas is the airspeed in ms-1
		//Calculate the motor RPM at this airspeed using the pitch reduction
		//method.
		//NOTE: diameter and pitch are in inches
		//Motor equation is the same as in solveStaticRPM2 except that the
		//pitch and RPM are constrained as follows:
		//P x RPM x 1.1 x 0.00152=Airspeed in KPH (from EFI)
		//P x RPM x 2.54/(100*60) = Airspeed in ms-1 (pitch in inches) (from Simons)
		//See Simons, P.210. EFI uses an additional 1.1 factor - why?
			
		//Step 1: calculate the new RPM given the pitch*RPM constraint
		float C3=Vas/(2.54f/(100.0f*60.0f))*5.33e-15f; //pitch speed rpm constant
		//Step 2: use modified equation from solveStaticRPM2 to get airspeed RPM
		float R=this.Rbatt+this.Rwire+this.Rs;
		float rpm=this.Kv*(this.Vbatt-this.I0*R)
				/(C3*(Mathf.Pow(Diameter,4.0f)*Mathf.Pow(this.Kv,2.0f))*R+1.0f);
		float redPitch=Vas/(rpm*2.54f/(100.0f*60.0f));
			
		//check this - not sure it's right
		float Itot=(this.Vbatt*this.Kv-rpm)/(this.Kv*R);
			//System::Diagnostics::Debug::WriteLine("Itot="+Itot+" RPM="+rpm);
			
		return rpm;
	}
		
	public float CalcPitchSpeed(float Pitch,float rpm)
	{
		//calculate speed for a given propellor pitch and rpm
		//pitch is in inches, but the result is in ms-1
		return Pitch*rpm*2.54f/(100.0f*60.0f);
	}
		
	public float CalcThrust(float Diameter,float Pitch,float rpm)
	{
		//Work out the thrust for a propellor of given pitch, diameter and rpm
		//Pitch and Diameter are in inches. The result is in Newtons.
		float thrust=Pitch*(Mathf.Pow(Diameter,3.0f)*Mathf.Pow(rpm,2.0f))*28.349e-10f;
		thrust=thrust/1000.0f*9.81f; //grams to Newtons
		return thrust;
	}
		
	public float SolveThrustAtAirspeed(float Vas,float Diameter,float Pitch)
	{
		//Use pitch reduction method to calculate the thrust at airspeed Vas
		//Returned result is in Newtons
		//Vas is airspeed in ms-1 diameter and pitch are in inches
		//Rbatt is battery internal resistance, Rwire is wire resistance and Rs is
		//the motor series resistance
		//K is the motor's Kv constant, Vbatt is the battery voltage and I0 is the
		//motor's idle current constant
		//NOTE: it might be useful to return the airspeed RPM in addition to the thrust
			
		//Step 1: calculate the static RPM for this prop and motor
		if (this.StaticRPM<=0) this.StaticRPM=this.SolveStaticRPM(Diameter,Pitch); //save this as it never changes
		float staticPitchSpeed=this.CalcPitchSpeed(Pitch,this.StaticRPM);
			
		//Step 2: find the RPM of the prop with reduced pitch that would travel
		//at the model's airspeed
		float airspeedRPM=this.SolveAirspeedRPM(staticPitchSpeed-Vas,Diameter);
		//above should really return the pitch and RPM, but we can calculate pitch easily
		float reducedPitch=(staticPitchSpeed-Vas)/(airspeedRPM*2.54f/(100.0f*60.0f));
			
		//now calculate the thrust for the reducedPitch propellor at the airspeedRPM
		float thrust=this.CalcThrust(Diameter,reducedPitch,airspeedRPM);
		return thrust;
	}
}
