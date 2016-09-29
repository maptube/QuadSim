using UnityEngine;
using System.Collections;

//http://forum.unity3d.com/threads/vjr-virtual-joystick-region-sample.116076/
public class TXJoystickScript : MonoBehaviour {
	
	public static Vector2 VJRvector;    // Joystick's controls in Screen-Space.
	public static Vector2 VJRnormals;   // Joystick's normalized controls.
	public static bool VJRdoubleTap;    // Player double tapped this Joystick.
	public Color activeColor;           // Joystick's color when active.
	public Color inactiveColor;         // Joystick's color when inactive.
	public Texture2D joystick2D;        // Joystick's Image.
	public Texture2D background2D;      // Background's Image.
	public bool useAbsoluteScales;      // If true, graphics wont auto-scale.
	public int[] joystickScale;         // Size of joystick in pixels, if using absolute scales.
	public int[] backgroundScale;       // Size of background in pixels, if using absolute scales.
	private GameObject backOBJ;         // Background's Object.
	private GUITexture joystick;        // Joystick's GUI.
	private GUITexture background;      // Background's GUI.
	private Vector2 origin;             // Touch's Origin in Screen-Space.
	private Vector2 position;           // Pixel Position in Screen-Space.
	private int size;                   // Screen's smaller side.
	private float length;               // The maximum distance the Joystick can be pushed.
	private bool gotPosition;           // Joystick has a position.
	private int fingerID;               // ID of finger touching this Joystick.
	private int lastID;                 // ID of last finger touching this Joystick.
	private float tapTimer;             // Double-tap's timer.
	private bool enable;                // VJR external control.

	
	public void DisableJoystick() {enable = false; ResetJoystick();}
	public void EnableJoystick() {enable = true; ResetJoystick();}

	
	private void ResetJoystick() {
		VJRvector = new Vector2(0,0); VJRnormals = VJRvector;
		lastID = fingerID; fingerID = -1; tapTimer = 0.150f;
		joystick.color = inactiveColor; position = origin; gotPosition = false;
		
	}

	private Vector2 GetRadius(Vector2 midPoint, Vector2 endPoint, float maxDistance) {
		Vector2 distance = endPoint;
		if (Vector2.Distance(midPoint,endPoint) > maxDistance) {
			distance = endPoint-midPoint; distance.Normalize();
			return (distance*maxDistance)+midPoint;
		}
		return distance;
	}

	
	private void GetPosition() {
		foreach (Touch touch in Input.touches) {
			fingerID = touch.fingerId;
			if (fingerID >= 0 &&  fingerID < Input.touchCount) {
				if(Input.GetTouch(fingerID).position.x < Screen.width/3 &&  Input.GetTouch(fingerID).position.y < Screen.height/3 && Input.GetTouch(fingerID).phase == TouchPhase.Began) {
					position = Input.GetTouch(fingerID).position; origin = position;
					joystick.texture = joystick2D; joystick.color = activeColor;
					background.texture = background2D; background.color = activeColor;
					if (fingerID == lastID && tapTimer > 0) {VJRdoubleTap = true;} gotPosition = true;
				}
			}
		}
	}

	
	private void GetConstraints() {
		if (origin.x < (background.pixelInset.width/2)+25) {origin.x = (background.pixelInset.width/2)+25;}
		if (origin.y < (background.pixelInset.height/2)+25) {origin.y = (background.pixelInset.height/2)+25;}
		if (origin.x > Screen.width/3) {origin.x = Screen.width/3;}
		if (origin.y > Screen.height/3) {origin.y = Screen.height/3;}
	}

	private Vector2 GetControls(Vector2 pos, Vector2 ori) {
		Vector2 vector = new Vector2();
		if (Vector2.Distance(pos,ori) > 0) {vector = new Vector2(pos.x-ori.x,pos.y-ori.y);}
		return vector;
	}

	
	private void Awake() {
		gameObject.transform.localScale = new Vector3(0,0,0);
		gameObject.transform.position = new Vector3(0,0,999);
		if (Screen.width > Screen.height) {size = Screen.height;} else {size = Screen.width;} VJRvector = new Vector2(0,0);
		joystick = gameObject.AddComponent<GUITexture>() as GUITexture;
		joystick.texture = joystick2D; joystick.color = inactiveColor;
		backOBJ = new GameObject("VJR-Joystick Back");
		backOBJ.transform.localScale = new Vector3(0,0,0);
		background = backOBJ.AddComponent<GUITexture>() as GUITexture;
		background.texture = background2D; background.color = inactiveColor;
		fingerID = -1; lastID = -1; VJRdoubleTap = false; tapTimer = 0; length = 50;
		position = new Vector2((Screen.width/3)/2,(Screen.height/3)/2); origin = position;
		gotPosition = false; EnableJoystick(); enable = true;
	}





	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (tapTimer > 0) {tapTimer -= Time.deltaTime;}
		if (fingerID > -1 &&  fingerID >= Input.touchCount) {ResetJoystick();}
		if (enable == true) {
			if (Input.touchCount > 0 &&  gotPosition == false) {GetPosition(); GetConstraints();}
			if (Input.touchCount > 0 && fingerID > -1 &&  fingerID < Input.touchCount &&  gotPosition == true) {
				foreach (Touch touch in Input.touches) {
					if (touch.fingerId == fingerID) {
						position = touch.position; position = GetRadius(origin,position,length);
						VJRvector = GetControls(position,origin); VJRnormals = new Vector2(VJRvector.x/length,VJRvector.y/length);
						if (Input.GetTouch(fingerID).position.x > (Screen.width/3)+background.pixelInset.width
						                                || Input.GetTouch(fingerID).position.y > (Screen.height/3)+background.pixelInset.height) {ResetJoystick();}
						//
						Debug.Log("Joystick Axis:: "+VJRnormals); //<-- Delete this line | (X,Y), from -1.0 to +1.0 | Use this value "VJRnormals" in your scripts.
					}
				}
			}
			if (gotPosition == true &&  Input.touchCount > 0 &&  fingerID > -1 &&  fingerID < Input.touchCount) {
				if (Input.GetTouch(fingerID).phase == TouchPhase.Ended || Input.GetTouch(fingerID).phase == TouchPhase.Canceled) {ResetJoystick();}
			}
			
			if (gotPosition == false &&  fingerID == -1 &&  tapTimer <= 0) {if (background.color != inactiveColor) {background.color = inactiveColor;}}
			
			//
				
			if (useAbsoluteScales) {
				if (joystickScale.Length < 2) {Debug.LogError(this.name+"(JVR): "+"Did you setup 'X,Y' absolute scales for the joystick? Set 'Size' = 2..."); return;}
				if (backgroundScale.Length < 2) {Debug.LogError(this.name+"(JVR): "+"Did you setup 'X,Y' absolute scales for the background? Set 'Size' = 2..."); return;}
				background.pixelInset = new Rect(origin.x-(background.pixelInset.width/2),origin.y-(background.pixelInset.height/2),backgroundScale[0],backgroundScale[1]);
				joystick.pixelInset = new Rect(position.x-(joystick.pixelInset.width/2),position.y-(joystick.pixelInset.height/2),joystickScale[0],joystickScale[1]);
			}
			else {
				background.pixelInset = new Rect(origin.x-(background.pixelInset.width/2),origin.y-(background.pixelInset.height/2),size/5,size/5);
				joystick.pixelInset = new Rect(position.x-(joystick.pixelInset.width/2),position.y-(joystick.pixelInset.height/2),size/11,size/11);
			}
		}
		else if (background.pixelInset.width > 0) {background.pixelInset = new Rect(0,0,0,0); joystick.pixelInset = new Rect(0,0,0,0);}	
	}
	
}
