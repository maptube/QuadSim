using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class MainMenu : MonoBehaviour {
	//UI code for main menu to detect when X button is pressed and trigger opening the main menu

	GameObject _goMainMenu;
	GameObject _goPIDMenu;

	// Use this for initialization
	void Start () {
		//wire up the start game button - need to do this first as we're about to take the menus out of the game hierarchy!
		GameObject goStartGameButton = GameObject.Find("StartGameButton");
		Button StartGameButton = goStartGameButton.GetComponent<Button> ();
		StartGameButton.onClick.AddListener(StartGame);

		//remove UI panels from game hierarchy until we make a request for them
		_goMainMenu = GameObject.Find("MainMenuPanel");
		_goMainMenu.SetActive (false);
		_goPIDMenu = GameObject.Find ("PIDPanel");
		_goPIDMenu.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("SpeedlinkButtonX")) {
			//Debug.Log ("BUTTON X");
			GameObject goSCM = GameObject.Find("ScreenManager");
			ScreenManager scm = (ScreenManager)goSCM.GetComponent(typeof(ScreenManager));
			if (scm.GetCurrentlyOpen()!=null) {
				scm.CloseCurrent();
			}
			else {
				_goMainMenu.SetActive(true);
				GameObject goMainMenu = GameObject.Find("MainMenuPanel");
				//goMainMenu.SetActive (true); //animation turns it off when it closes!!!!

				//Panel MainMenuPanel = MainMenu.GetComponentInChildren<Panel>();
				//ScreenManager scm = (ScreenManager)goSCM.GetComponent(typeof(Animator));
				Animator MainMenuAnim = goMainMenu.GetComponent<Animator>();
				scm.OpenPanel(MainMenuAnim);
				//and focus a button, otherwise the UI doesn't work with the game controller
				//GameObject go2 = GameObject.Find ("MMPIDValuesButton");
				//Button but = go2.GetComponent<Button>();
				//GUI.FocusControl("MMPIDValuesButton");
				GUI.FocusControl("MainMenuPanel");
				//EventSystemManager esm = goEventSystem.GetComponent<EventSystemManager>();
				//EventSystemManager.currentSystem.SetSelectedGameObject (goMainMenu);
			}
		}
	
	}

	//void OnGUI() {
	//}

	//Start the game over again by reloading the level and resetting the timer to zero
	public void StartGame() {
		Application.LoadLevel(Application.loadedLevel); // reload the same level
		GameObject goMainMenuCanvas = GameObject.Find("MainMenuCanvas");
		//Script RaceTimerScript = goMainMenuCanvas.GetComponent<Script> ();
		//RaceTimerScript script = (RaceTimerScript)goMainMenuCanvas.GetComponent (typeof(RaceTimerScript));
		RaceTimerScript script = goMainMenuCanvas.GetComponent<RaceTimerScript> ();
		script.ResetTimer ();
	}
}
