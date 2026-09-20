using UnityEngine;
using UnityEngine.SceneManagement;

public enum MenuState
{
		MENU,
		HIGHSCORE,
		OPTION,
		LEVELS
}

/// <summary>
/// Contrôleur du menu principal : relie le stockage local (DBScript) à la vue uGUI (MenuView).
/// </summary>
public class Menu : MonoBehaviour
{
		private DBScript _dbScript;
		private MenuView _view;
		private MenuState _state;

		// DBScript change l'état au premier lancement (pas encore de pseudo -> OPTION)
		public MenuState CurrentMenuState {
				get { return _state; }
				set {
						_state = value;
						ApplyState ();
				}
		}

		void Start ()
		{
				var miner = GameObject.Find ("Miner");
				if (miner != null) {
						var animator = miner.GetComponent<Animator> ();
						if (animator != null) {
								animator.SetBool ("Grounded", true);
						}
				}
				Time.timeScale = 1;

				_dbScript = FindAnyObjectByType<DBScript> ();
				if (_dbScript == null) {
						GameObject go = new GameObject ("Database");
						_dbScript = go.AddComponent<DBScript> ();
				}
				// DBScript survit aux changements de scène : on lui redonne le Menu courant
				_dbScript.BindMenu (this);

				_view = MenuView.Create ();
				_view.OnPlay = () => SceneManager.LoadScene ("test");
				_view.OnHighscore = () => CurrentMenuState = MenuState.HIGHSCORE;
				_view.OnOptions = () => CurrentMenuState = MenuState.OPTION;
				_view.OnClose = () => CurrentMenuState = MenuState.MENU;
				_view.OnSaveUsername = name => _dbScript.SaveUsername (name);

				CurrentMenuState = MenuState.MENU;
		}

		void Update ()
		{
				// Bouton retour Android / Échap : retour à l'accueil
				if (_state != MenuState.MENU && Input.GetKeyDown (KeyCode.Escape)) {
						CurrentMenuState = MenuState.MENU;
				}
		}

		private void ApplyState ()
		{
				if (_view == null) {
						return;
				}

				switch (_state) {
				case MenuState.HIGHSCORE:
						_dbScript.GetHighscore ();
						_view.SetHighscores (_dbScript.Highscore);
						break;
				case MenuState.OPTION:
						_view.SetUsername (_dbScript.User != null ? _dbScript.User.Username : string.Empty);
						_view.SetOptionsMessage (string.Empty);
						break;
				default:
						_state = MenuState.MENU;
						RefreshPlayerLine ();
						break;
				}
				_view.Show (_state);
		}

		private void RefreshPlayerLine ()
		{
				var strings = LocalizationStrings.Instance.Values;
				if (_dbScript == null || _dbScript.User == null) {
						_view.SetPlayerLine (strings ["TryingToConnect"]);
				} else {
						_view.SetPlayerLine (string.Format (strings ["YouAreConnected"], _dbScript.User.Username, _dbScript.User.BestScore));
				}
		}

		public void ConnectionCallback (bool connected)
		{
				if (_state == MenuState.MENU) {
						RefreshPlayerLine ();
				}
		}

		public void SaveUsernameCallback (int returnCode)
		{
				var strings = LocalizationStrings.Instance.Values;
				switch (returnCode) {
				case 3: // Enregistré
						_view.SetOptionsMessage (string.Empty);
						CurrentMenuState = MenuState.MENU;
						break;
				case 2: // Pseudo déjà pris (plus possible en local, conservé par compatibilité)
						_view.SetOptionsMessage (strings ["PwdAlreadyTaken"]);
						break;
				default: // Pseudo vide
						_view.SetOptionsMessage (strings ["UpdateWasNotSuccessful"]);
						break;
				}
		}
}
