using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum MenuState
{
		MENU,
		HIGHSCORE,
		OPTION,
		LEVELS
}

public class Menu : MonoBehaviour
{

		public GUISkin Skin;
		private float virtualHeight = 1920f;
		private float virtualWidth = 1080f;
		private DBScript _dbScript;
		public MenuState CurrentMenuState;
		public Vector2 scrollPosition = Vector2.zero;
		private string _username = string.Empty;
		private string _message = string.Empty;
		private string _connectionMessage, _connectionMessageUsername;
		private int _connectionMessageBest = -1;

		void Start ()
		{
				CurrentMenuState = MenuState.MENU;

				var animator = GameObject.Find ("Miner").GetComponent<Animator> ();
				animator.SetBool ("Grounded", true);
				Time.timeScale = 1;

				_dbScript = FindAnyObjectByType<DBScript> ();
				if (_dbScript == null) {
						GameObject go = new GameObject ("Database");
						_dbScript = go.AddComponent<DBScript> ();
				}
		}

		void Update ()
		{
				if (CurrentMenuState != MenuState.MENU && Input.GetKey (KeyCode.Escape)) {
						CurrentMenuState = MenuState.MENU;
				}
		}

		void OnGUI ()
		{
				GUI.skin = Skin;
				GUI.matrix = VirtualGui.Matrix;
				virtualHeight = VirtualGui.Height;

				switch (CurrentMenuState) {

				case MenuState.MENU:
						DisplayMainMenu ();
						break;
				case MenuState.HIGHSCORE:
						DisplayHighScoreMenu ();
						break;
				case MenuState.OPTION:
						DisplayOptions ();
						break;
				case MenuState.LEVELS:
						DisplayLevels ();
						break;

				}
		}

		public void DisplayLevels ()
		{
				GUI.Box (new Rect (0, virtualHeight * 0.05f, virtualWidth * 0.99f, virtualWidth * 1.4f), LocalizationStrings.Instance.Values ["LevelSelection"], "LevelBox");

				if (GUI.Button (new Rect (150, 500, 200, 200), "Level 1", "LevelButton")) {
						SceneManager.LoadScene ("Level1");
				}

				GUI.Button (new Rect (450, 500, 200, 200), "Level 2", "LockButton");
				GUI.Button (new Rect (750, 500, 200, 200), "Level 3", "LockButton");

				GUI.Button (new Rect (150, 800, 200, 200), "Level 4", "LockButton");
				GUI.Button (new Rect (450, 800, 200, 200), "Level 5", "LockButton");
				GUI.Button (new Rect (750, 800, 200, 200), "Level 6", "LockButton");

				GUI.Button (new Rect (150, 1100, 200, 200), "Level 7", "LockButton");
		}

		public void DisplayOptions ()
		{
				GUI.Label (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.15f, virtualWidth * 0.8f, 100), LocalizationStrings.Instance.Values ["ChooseUsernameDesc"]);

				GUI.Label (new Rect (0, virtualHeight * 0.26f, virtualWidth, 50), LocalizationStrings.Instance.Values ["ChooseUsername"]);
				_username = GUI.TextField (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.3f, virtualWidth * 0.8f, 150), _username, 50);

				if (GUI.Button (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.5f, virtualWidth * 0.8f, 150), LocalizationStrings.Instance.Values ["Save"])) {
						_dbScript.SaveUsername (_username);
				}

				if (!string.IsNullOrEmpty (_message)) {
						GUI.Label (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.6f, virtualWidth * 0.8f, 150), _message);
				}
		}

		public void DisplayHighScoreMenu ()
		{
				GUI.Box (new Rect (0, virtualHeight * 0.05f, virtualWidth * 0.99f, virtualWidth * 1.4f), LocalizationStrings.Instance.Values ["Highscore"], "HighScoreBox");
				if (GUI.Button (new Rect (900, 200, 150, 150), string.Empty, "CloseButton")) {
						CurrentMenuState = MenuState.MENU;
				}

				if (_dbScript.Highscore == null || _dbScript.Highscore.Count == 0) {

						GUI.Label (new Rect (200, 480, 750, 900), LocalizationStrings.Instance.Values ["NoScoreYet"]);

				} else {

						_username = _dbScript.User.Username;

						int rowHeight = 80;
						var totalScroll = _dbScript.Highscore.Count * rowHeight;

						scrollPosition = GUI.BeginScrollView (new Rect (0, 480, 750, 900), scrollPosition, new Rect (0, 0, 400, totalScroll));

						for (int i = 0; i < _dbScript.Highscore.Count; i++) {

								float top = i * rowHeight;
								GUI.Label (new Rect (0, top, 500, 60), _dbScript.Highscore [i].Rank.ToString (), "RankLabel");
								GUI.Label (new Rect (200, top, 500, 60), _dbScript.Highscore [i].PlayerName + " - " + _dbScript.Highscore [i].Meters);
						}

						GUI.EndScrollView ();

				}
		}

		public void DisplayMainMenu ()
		{
				// OnGUI tourne plusieurs fois par frame : on ne reformate que si les données changent
				if (_dbScript.User == null) {
						_connectionMessage = LocalizationStrings.Instance.Values ["TryingToConnect"];
				} else if (_connectionMessage == null || _connectionMessageUsername != _dbScript.User.Username || _connectionMessageBest != _dbScript.User.BestScore) {
						_connectionMessageUsername = _dbScript.User.Username;
						_connectionMessageBest = _dbScript.User.BestScore;
						_connectionMessage = string.Format (LocalizationStrings.Instance.Values ["YouAreConnected"], _connectionMessageUsername, _connectionMessageBest);
				}

				GUI.Label (new Rect (0, virtualHeight - (virtualHeight * 0.085f), virtualWidth, virtualHeight * 0.085f), _connectionMessage);

				GUI.Box (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.05f, virtualWidth * 0.8f, virtualWidth * 0.1f), "Infinite Digger", "Title");

				/*
				if (GUI.Button (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.2f, virtualWidth * 0.8f, virtualWidth * 0.1248f), LocalizationStrings.Instance.Values ["Play"])) {

						CurrentMenuState = MenuState.LEVELS;
						// SceneManager.LoadScene ("test");
				}
*/

				if (GUI.Button (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.3f, virtualWidth * 0.8f, virtualWidth * 0.1248f), LocalizationStrings.Instance.Values ["ChallengeMode"])) {
						SceneManager.LoadScene ("test");
				}

				if (_dbScript.User != null) {

						if (GUI.Button (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.4f, virtualWidth * 0.8f, virtualWidth * 0.1248f), LocalizationStrings.Instance.Values ["Highscore"])) {
								CurrentMenuState = MenuState.HIGHSCORE;
								_dbScript.GetHighscore ();
						}

						if (GUI.Button (new Rect ((virtualWidth - (virtualWidth * 0.8f)) / 2, virtualHeight * 0.5f, virtualWidth * 0.8f, virtualWidth * 0.1248f), LocalizationStrings.Instance.Values ["Options"])) {
								CurrentMenuState = MenuState.OPTION;
						}

				}
		}

		public void DisplayOptionMessages ()
		{

		}

		public void ConnectionCallback (bool connected)
		{
				_username = _dbScript.User.Username;
		}

		public void SaveUsernameCallback (int returnCode)
		{
				Debug.Log (returnCode);
				switch (returnCode) {
				case 0: // Impossible de mettre à jour
						_message = LocalizationStrings.Instance.Values ["UpdateWasNotSuccessful"];
						break;
				case 1: // Mot de passe ou id incorrect
						_message = LocalizationStrings.Instance.Values ["UpdateWasNotSuccessful"];
						break;
				case 2: // Le pseudo est déja pris
						_message = LocalizationStrings.Instance.Values ["PwdAlreadyTaken"];
						break;
				case 3: // Mise à jour effectuée
						_message = string.Empty;
						CurrentMenuState = MenuState.MENU;
						break;
				}
		}

}
