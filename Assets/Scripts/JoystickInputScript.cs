using UnityEngine;
using System;
using System.Collections;
//using SharpDX.DirectInput;
using XInputDotNetPure; //it's a Unity Package: https://github.com/speps/XInputDotNet/releases

public class JoystickInputScript : MonoBehaviour {

	//https://msdn.microsoft.com/en-us/library/windows/desktop/bb153252(v=vs.85).aspx

	//private static DirectInput directInput;
	//private static Joystick joystick;
	//private static JoystickState joystickState;
	private static GamePadState gamepadState;
	private static PlayerIndex playerIndex;

	public static float aileron=0;
	public static float elevator =0;
	public static float rudder=0;
	public static float throttle=0;


	// Use this for initialization
	void Start () {
		print ("Joystick start");
		for (int i = 0; i < 4; ++i)
		{
			PlayerIndex testPlayerIndex = PlayerIndex.One; // (PlayerIndex)i;
			GamePadState testState = GamePad.GetState(testPlayerIndex);
			print (testState);
			if (testState.IsConnected)
			{
				print ("joystick found");
				Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
				playerIndex = testPlayerIndex;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		gamepadState = GamePad.GetState (PlayerIndex.One/*playerIndex*/);
		aileron = gamepadState.ThumbSticks.Right.X;
		elevator = gamepadState.ThumbSticks.Right.Y;
		rudder = gamepadState.ThumbSticks.Left.X;
		throttle = gamepadState.ThumbSticks.Left.Y;
		//print ("Joystick: " + aileron + " " + elevator + " " + rudder+" "+throttle);

	}
}
