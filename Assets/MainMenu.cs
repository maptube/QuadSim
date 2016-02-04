using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour {
	//UI code for main menu to detect when X button is pressed and trigger opening the main menu

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("SpeedlinkButtonX")) {
			//Debug.Log ("BUTTON X");
			GameObject goSCM = GameObject.Find("ScreenManager");
			ScreenManager scm = (ScreenManager)goSCM.GetComponent(typeof(ScreenManager));
			GameObject goMainMenu = GameObject.Find("MainMenuPanel");
			//Panel MainMenuPanel = MainMenu.GetComponentInChildren<Panel>();
			//ScreenManager scm = (ScreenManager)goSCM.GetComponent(typeof(Animator));
			Animator MainMenuAnim = goMainMenu.GetComponent<Animator>();
			scm.OpenPanel(MainMenuAnim);
		}
	
	}

	//void OnGUI() {
	//}
}
