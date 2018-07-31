using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;

//This is coming from the hello world example in the API documentation.
//NOTE: for Unity 5.5.1, I'm using LeapMotion Core Assets 4.1.6 unity package. Later versions of unity (5.6/2018) used the newer 4.3.4.
//https://developer.leapmotion.com/documentation/csharp/devguide/Sample_Tutorial.html#id5
//The file Sample.cs in the SDK contains the exmaple code.

/// <summary>
/// Subscriber to LeapMotion events
/// </summary>
public class SampleListener
{

    //hand information to feed back - this gets turned into control inputs later
    /// <summary>
    /// Hand confidence of highest hand
    /// </summary>
    float _handConfidence;
    public float handConfidence
    {
        get { return _handConfidence;  }
    }

    int _numFingers;
    public int numFingers
    {
        get { return _numFingers; }
    }

    //ALL IN RADIANS
    float _handRoll;
    public float handRoll {
        get { return _handRoll; }
    }

    float _handPitch;
    public float handPitch
    {
        get { return _handPitch; }
    }

    float _handYaw;
    public float handYaw {
        get { return _handYaw; }
    }

    Leap.Vector _palmPosition;
    public Leap.Vector palmPosition
    {
        get { return _palmPosition; }
    }

    Leap.Vector _palmVelocity;
    public Leap.Vector palmVelocity
    {
        get { return _palmVelocity; }
    }

    public void OnServiceConnect(object sender, ConnectionEventArgs args)
    {
        //Console.WriteLine("Service Connected");
    }

    public void OnConnect(object sender, DeviceEventArgs args)
    {
        //Console.WriteLine("Connected");
    }

    public void OnFrame(object sender, FrameEventArgs args)
    {
        //Console.WriteLine("Frame Available.");

        // Get the most recent frame and report some basic information
        Frame frame = args.frame;

        //debug info from the hello world example in the sdk
        //Debug.Log(string.Format("LeapMotion: Frame id: {0}, timestamp: {1}, hands: {2}",frame.Id, frame.Timestamp, frame.Hands.Count));
        //foreach (Hand hand in frame.Hands)
        //{
        //    Debug.Log(string.Format("LeapMotion: Hand id: {0}, palm position: {1}, fingers: {2}",hand.Id, hand.PalmPosition, hand.Fingers.Count));
        //    // Get the hand's normal vector and direction
        //    Vector normal = hand.PalmNormal;
        //    Vector direction = hand.Direction;

        //    // Calculate the hand's pitch, roll, and yaw angles
        //    Debug.Log(
        //        string.Format("LeapMotion: Hand pitch: {0} degrees, roll: {1} degrees, yaw: {2} degrees",
        //            direction.Pitch * 180.0f / Mathf.PI,
        //            normal.Roll * 180.0f / Mathf.PI,
        //            direction.Yaw * 180.0f / Mathf.PI
        //        )
        //    );
        //}

        //real code
        //which hand???? how is this going to work?
        //todo: should track the hand id between successive frames
        Hand hand = null;
        _handConfidence = 0;
        foreach (Hand h in frame.Hands) //find hand with the best confidence - we don't care which one
        {
            if (h.Confidence > _handConfidence)
            {
                hand = h;
                _handConfidence = h.Confidence;
            }
        }
        if (hand!=null)
        {
            _numFingers = hand.Fingers.Count;
            Vector normal = hand.PalmNormal;
            Vector direction = hand.Direction;
            _handPitch = direction.Pitch; // * 180.0f / Mathf.PI;
            _handRoll = normal.Roll; // * 180.0f / Mathf.PI; NOTE: 0.35=20 degrees, 1.22=70 degrees
            _handYaw = direction.Yaw; // * 180.0f / Mathf.PI;
            _palmPosition = hand.PalmPosition;
            _palmVelocity = hand.PalmVelocity; //NOTE: this is a Leap.Vector not a Unity one
        }

    }

}

/// <summary>
/// Main class which listens for leap motion events (via the listener above) and translates them into Drone control inputs.
/// These control inputs are then read off of this class and applied to the simulator's aileron, elevator, rudder and throttle.
/// NOTE: might have to add some cleverness with auto throttle control and target translation and angles - leap isn't as sensitive as a radio tx.
/// </summary>
public class LeapMotionBehaviourScript : MonoBehaviour {

    /// <summary>
    /// Hand confidence thresholds below this are discarded and result in null control inputs (a=0,e=0,r=0)
    /// </summary>
    public const float confidenceThreshold = 0.9f;
    public static float LastAileron, LastElevator, LastRudder, LastThrottle;

    public static Controller leapController;
    public static SampleListener leapListener;

    #region UnityMethods

    // Use this for initialization
    void Start () {
        leapController = new Controller();
        //add subscriber for LeapMotion frame events
        leapListener = new SampleListener();
        leapController.Connect += leapListener.OnServiceConnect;
        leapController.Device += leapListener.OnConnect;
        leapController.FrameReady += leapListener.OnFrame;
        
    }

    void OnDestroy()
    {
        //cleanup code - you need to do this, otherwise it locks up Unity conpletely on disconnect
        //leapController.RemoveListener(leapListener); //this was in the api instructions, but it's been removed
        leapController.StopConnection();
        leapController.Dispose();
    }


    // Update is called once per frame
    void Update () {
		
	}

    #endregion UnityMethods

    #region Methods

    /// <summary>
    /// Poll function for current LeapMontion controller status.
    /// Return control inputs (aileron, elevator, rudder, throttle) based on latest hand position from the LeapMotion controller.
    /// This procedure contains all the logic to map the hand(s?) position to the control inputs.
    /// </summary>
    /// <param name="Aileron"></param>
    /// <param name="Elevator"></param>
    /// <param name="Rudder"></param>
    /// <param name="Throttle"></param>
    public static void GetControlInputs(out float Aileron, out float Elevator, out float Rudder, out float Throttle)
    {
        //NOTE: aileron, elevator, rudder are -1..+1, while throttle is 0..1
        Aileron = 0.9f*LastAileron; //this is designed to back off the controls softly on loss of LeapMotion tracking
        Elevator = 0.9f*LastElevator;
        Rudder = 0.9f*LastRudder;
        Throttle = 0.9f*LastThrottle;

        //use the values last seen by the leap listener event handler object
        //Debug.Log("confidence: " + leapListener.handConfidence);
        if (leapListener.handConfidence > confidenceThreshold)
        {
            if (leapListener.numFingers > 2) //if we're seeing less than two fingers, then assume the hand is a fist and don't read it
            {
                Aileron = -leapListener.handRoll;
                Elevator = leapListener.handPitch;
                //if (leapListener.palmVelocity.y>10)
                //{
                //    Throttle = 10;
                //}
                //else if (leapListener.palmVelocity.y<10)
                //{
                //    Throttle = -10;
                //}

                //palm height seems to vary between 70 and 500 (mm from sensor?)
                Throttle = (leapListener.palmPosition.y-70)/500;
                if (Throttle < 0) Throttle = 0;
                else if (Throttle > 1) Throttle = 1;
            }
        }
        //remember last positions in case we lose tracking
        LastAileron = Aileron;
        LastElevator = Elevator;
        LastRudder = Rudder;
        LastThrottle = Throttle;
    }

    #endregion Methods


}
