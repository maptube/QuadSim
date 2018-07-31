using UnityEngine;
using System;
using System.Collections;
//using System.IO.Ports;

/// <summary>
/// Serial joystick script.
/// Reads joystick information from the serial port.
/// Used with the Great Planes RealFlight controller and Arduino analogue to digital sketch.
/// </summary>
public class SerialJoystickScript : MonoBehaviour {

	public const string PortName = "COM3";

//	private static SerialPort sp=null;

	public static float Aileron = 0.0f;
	public static float Elevator = 0.0f;
	public static float Rudder = 0.0f;
	public static float Throttle = 0.0f;

	// Use this for initialization
	public void Start () {
		//string[] portNames = System.IO.Ports.SerialPort.GetPortNames ();
		//foreach (string name in portNames) print (name);

		//sp = new SerialPort (PortName);

		//sp.BaudRate = 9600;
		//sp.Parity = Parity.None;
		//sp.StopBits = StopBits.One;
		//sp.DataBits = 8;
		//sp.Handshake = Handshake.None;
		//sp.RtsEnable = true;
		
		////sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
		
		//sp.Open();
		//print ("Serial port open = "+sp.IsOpen);

	}

	//http://www.alanzucconi.com/2015/10/07/how-to-integrate-arduino-with-unity/
	public void Update()
	{
//		//StartCoroutine (ReadData());
//		if (sp!=null) {
//			//int size = sp.BytesToRead;
//			//print (size);
//			/*if (size > 0) {
//			char [] buf = new char[256];
//			int count = sp.Read(buf,0,256);
//			//string text = new string(buf);
//			print (count);
//		}*/
//			//string Text = sp.ReadExisting();
//			//print (Text);

//			/*sp.ReadTimeout=50;
//			try {
//				string Text = sp.ReadLine();
//				print (Text);
//				sp.BaseStream.Flush();
//			}
//			catch (UnityException ex) {} //TimeoutException
//*/
///*while(sp. BytesToRead>0) {
//				string Text = sp.ReadExisting();
//				print (Text);
//			}*/

//			//byte [] buf = new byte[256];
//			//int bytesread = sp.BaseStream.Read(buf,0,256);
//			//if (bytesread>0) {
//				//print ("read "+bytesread+" bytes");
//				//sp.BaseStream.Flush();
//			//}


//			StartCoroutine
//			(
//				AsynchronousReadFromArduino
//				(
//					//(string s) => Debug.Log(s),     // Callback
//				 	(string s) => ParseData (s), //callback
//					() => Debug.LogError("Error!"), // Error callback
//				 	10f                             // Timeout (seconds)
//				)
//			);

//		}
	}

	/*public void ReadData() {
		while (sp.BytesToRead>0) {
			string Text = sp.ReadExisting();
			print (Text);
		}
	}*/

	public void ParseData(string Text) {
		string [] Fields = Text.Split (new char[] {','});
		if (Fields.Length == 4) {
			float A = Convert.ToSingle (Fields[0]);
			float E = Convert.ToSingle (Fields[1]);
			float R = Convert.ToSingle (Fields[2]);
			float T = Convert.ToSingle (Fields[3]);
			//230 630 731
			if (A<630.0f) Aileron = (A-630.0f)/(512);
			else Aileron = (A-630.0f)/(128);
			//262 674 719
			if (E<674.0f) Elevator = (E-674.0f)/(512);
			else Elevator = (E-674.0f)/(96);
			//91 597 741
			if (R<597.0f) Rudder = (R-597.0f)/(597.0f-91.0f);
			else Rudder = (R-597.0f)/(741.0f-597.0f);
			//345 600 697
			if (T<600.0f) Throttle = (T-600.0f)/(600.0f-345.0f);
			else Throttle = (T-600.0f)/(697.0f-600.0f);
		}
	}

	//public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail, float timeout) {
		//DateTime initialTime = DateTime.Now;
		//DateTime nowTime;
		//TimeSpan diff = default(TimeSpan);

		//string dataString = null;

		//do {
		//	try {
		//		dataString = sp.ReadLine();
		//		sp.BaseStream.Flush();
		//	}
		//	catch (TimeoutException) {
		//		dataString = null;
		//	}
			
		//	if (dataString != null)
		//	{
		//		callback(dataString);
		//		yield return null;
		//	} else
		//		yield return new WaitForSeconds(0.05f);
			
		//	nowTime = DateTime.Now;
		//	diff = nowTime - initialTime;
			
		//} while (diff.Milliseconds < timeout);
		
		////if (fail != null)
		////	fail();
		//yield return null;
	//}

	public void OnApplicationQuit ()
	{
		//if (sp != null) {
		//	if (sp.IsOpen) {
		//		print ("closing serial port");
		//		sp.Close ();
		//	}
		//	sp = null;
		//}
	}
	

	/*private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
	{
		SerialPort sp = (SerialPort)sender;
		string indata = sp.ReadExisting();
		print("Data Received:");
		print(indata);
	}*/


}
