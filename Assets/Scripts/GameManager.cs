using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Chef d'orchestre de la partie : démarrage, rythme (caméra, difficulté), score, pause,
/// mort, HUD. Le terrain est délégué à TerrainGenerator, les monstres à MonsterSpawner.
/// </summary>
public class GameManager : MonoBehaviour
{
		public GameObject BrickPrefab;
		public Transform Character;
		public AudioClip DieAudio;
		public GameObject BlackCache;
		public int Offset = 6;
		public GameObject TaupePrefab, RawPrefab;
		public Transform BlackHub;
		public Transform ScoreLine;
		public GameObject Dock;

		// Réglages de rythme
		private const float BaseCameraSpeed = 0.01f;
		private const float CameraSpeedPerDifficulty = 0.0015f;
		private const float CatchUpMultiplier = 5f;
		private const float CatchUpScreenFraction = 0.3f;
		private const float DifficultyStep = 0.07f;
		private const int DifficultyStepMeters = 15;
		private const float MonsterMinDifficulty = 2f;
		private const float SpawnIntervalBase = 3f;
		private const int StartDynamites = 5;
		private const int InitialRows = 20;

		private TerrainGenerator _terrain;
		private MonsterSpawner _spawner;
		private GameHud _hud;
		private Atmosphere _atmosphere;
		private CharacterControllerScript _characterController;
		private Camera _camera;
		private CameraManager _cameraManager;
		private Coroutine _monsterRoutine;

		private bool _gameStarted, _isPaused, _died, _deathShown, _scoreSaved;
		private float _difficulty = 1f;
		private int _meters, _finalScore, _yourBestScore, _nbDynamites;
		private float _yTopPosition;
		// Les paliers (messages, difficulté) ne se déclenchent qu'une fois par mètre parcouru
		private int _lastMilestoneMeters = -1;

    #region MONO BEHAVIOUR METHODS

		private void Awake ()
		{
				_characterController = Character.GetComponent<CharacterControllerScript> ();
				_camera = Camera.main;
				_cameraManager = _camera.GetComponent<CameraManager> ();

				Time.timeScale = 0;
				_nbDynamites = StartDynamites;
				_yourBestScore = PlayerPrefs.GetInt ("score");
				_yTopPosition = _camera.transform.position.y + Offset;

				// Nombre de briques par ligne : largeur de l'écran / largeur d'une brique, plus une marge
				var brickBounds = BrickPrefab.GetComponent<Renderer> ().bounds;
				Vector3 origin = _camera.WorldToScreenPoint (new Vector3 (brickBounds.min.x, brickBounds.max.y, 0f));
				Vector3 extent = _camera.WorldToScreenPoint (new Vector3 (brickBounds.max.x, brickBounds.min.y, 0f));
				int bricksPerRow = (int)(Screen.width / (extent.x - origin.x)) + 2;

				_terrain = new TerrainGenerator (RawPrefab, _camera, Offset, bricksPerRow);
				_spawner = new MonsterSpawner (TaupePrefab, _terrain, _characterController, Offset);

				if (ScoreLine != null) {
						ScoreLine.transform.position = -new Vector3 (0, (_camera.transform.position.y + _yourBestScore), 0);
				}

				_monsterRoutine = StartCoroutine (GenerateMonsters ());

				// En dernier : une UI ou un effet qui échoue ne doit pas empêcher la partie
				try {
						_atmosphere = Atmosphere.Create (_camera, Character);
				} catch (System.Exception e) {
						Debug.LogError ("[GameManager] Ambiance non créée : " + e.Message);
				}
				try {
						WireHud ();
				} catch (System.Exception e) {
						Debug.LogError ("[GameManager] HUD non créé : " + e.Message + "\nWindow > TextMeshPro > Import TMP Essential Resources ?");
				}
		}

		void Start ()
		{
				StartGame ();
		}

		public void StartGame ()
		{
				_gameStarted = true;

				// Les premières lignes suivent un motif fixe, le reste est aléatoire
				_terrain.EnqueuePatterns (new[] {
						"BBAAABAAABB",
						"BABBBBABABB",
						"BABAABABABB",
						"BABBABABABB",
						"BAAAABAAABB",
						"BBBBBBBBBBB"
				});
				for (int i = 0; i < InitialRows; i++) {
						_terrain.CreateRow ();
				}

				BlackCache.SetActive (false);
				Time.timeScale = 1;
				_cameraManager.enabled = true;
				this.GetComponent<AudioSource> ().Play ();

				StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["FirstGameMessage"], 5));
		}

		void Update ()
		{
				if (!_gameStarted) {
						return;
				}

				if (_died) {
						ShowDeathOnce ();
						return;
				}

				PushHud ();

				// Bouton retour Android / Échap
				if (!_isPaused && Input.GetKeyDown (KeyCode.Escape)) {
						Pause ();
						return;
				}

				if (_characterController != null && _characterController.IsDied) {
						_died = true;
						_cameraManager.enabled = false;
						if (!_scoreSaved) {
								SaveScore ();
						}
						return;
				}

				// La caméra accélère quand le mineur est dans le bas de l'écran
				float speed = BaseCameraSpeed + CameraSpeedPerDifficulty * _difficulty;
				bool playerLow = _camera.WorldToScreenPoint (Character.position).y <= Screen.width * CatchUpScreenFraction;
				_cameraManager.CameraSpeed = playerLow ? speed * CatchUpMultiplier : speed;

				_yTopPosition = _camera.transform.position.y + Offset;
				if (Character.position.y > _yTopPosition) {
						KillPlayerAndDestroyGround (_cameraManager);
						return;
				}

				_terrain.Recycle (_yTopPosition);

				_meters = Mathf.RoundToInt (_cameraManager.Distance);
				if (_meters == _lastMilestoneMeters) {
						return;
				}
				_lastMilestoneMeters = _meters;
				if (_atmosphere != null) {
						_atmosphere.SetDepth (_meters);
				}

				if (_yourBestScore > _meters && _meters == _yourBestScore - (_yourBestScore * 0.20f)) {
						StartCoroutine (DisplayMessage (string.Format (LocalizationStrings.Instance.Values ["BreakYourRecord"], _yourBestScore - _meters), 5));
				} else {
						switch (_meters) {
						case 10:
								StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["BeCareful"], 5));
								break;
						case 20:
								StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["GoodJobContinue"], 5));
								break;
						}
				}

				if (_meters > 0 && _meters % DifficultyStepMeters == 0) {
						_difficulty += DifficultyStep;
				}
		}

		// Appel, notification, changement d'app : on met en pause plutôt que de laisser mourir le mineur
		void OnApplicationPause (bool paused)
		{
				if (paused) {
						AutoPause ();
				}
		}

		void OnApplicationFocus (bool hasFocus)
		{
				if (!hasFocus) {
						AutoPause ();
				}
		}

		private void AutoPause ()
		{
				if (_gameStarted && !_isPaused && !_died) {
						Pause ();
				}
		}

    #endregion

    #region HUD

		IEnumerator DisplayMessage (string message, float time)
		{
				var animator = Dock.GetComponent<Animator> ();
				animator.SetBool ("Visible", true);
				var dockText = Dock.GetComponentInChildren<Text> ();
				dockText.text = message;
				yield return new WaitForSeconds (time + 1);
				animator.SetBool ("Visible", false);
		}

		private void WireHud ()
		{
				_hud = GameHud.Create ();
				_hud.OnPause = Pause;
				_hud.OnResume = Replay;
				_hud.OnRestart = () => { Replay (); SceneFader.LoadScene ("test"); };
				_hud.OnMenu = () => SceneFader.LoadScene ("menu");
				_hud.OnQuit = Application.Quit;
				_hud.OnDynamite = UseDynamite;
				_hud.OnSoundToggle = ToggleSound;
				_hud.OnHome = () => { Replay (); SceneFader.LoadScene ("menu"); };
				_hud.OnReplay = () => { Replay (); SceneFader.LoadScene ("test"); };
				_hud.SetDynamites (_nbDynamites);
		}

		private void PushHud ()
		{
				if (_hud == null) {
						return;
				}
				_hud.SetDistance (_meters);
				_hud.SetDynamites (_nbDynamites);
				if (_characterController != null) {
						_hud.SetCoins (_characterController.Coins);
						_hud.SetHealth (_characterController.Health, _characterController.MaxHealth);
				}
		}

		private void UseDynamite ()
		{
				if (_isPaused || _died) {
						return;
				}
				if (_nbDynamites > 0) {
						--_nbDynamites;
						if (_hud != null) {
								_hud.SetDynamites (_nbDynamites);
						}
						_characterController.ThrowDynamite ();
				} else {
						StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["NoDynamite"], 5));
				}
		}

		private void ToggleSound ()
		{
				AudioListener.pause = !AudioListener.pause;
				if (_hud != null) {
						_hud.SetSoundMuted (AudioListener.pause);
				}
		}

		private void ShowDeathOnce ()
		{
				if (_deathShown) {
						return;
				}
				_deathShown = true;
				Time.timeScale = 0;
				BlackHub.gameObject.SetActive (true);
				int coins = _characterController != null ? _characterController.Coins : 0;
				int taps = _characterController != null ? _characterController.NbAttack : 0;
				if (_hud != null) {
						_hud.ShowDeath (_meters, coins, taps, _finalScore);
				}
		}

    #endregion

    #region PAUSE / REPRISE

		public void Pause ()
		{
				Time.timeScale = 0;
				BlackHub.gameObject.SetActive (true);
				_isPaused = true;
				_characterController.IsActive = false;
				_cameraManager.enabled = false;
				if (_hud != null) {
						_hud.ShowPause (true, AudioListener.pause);
				}
		}

		public void Replay ()
		{
				Time.timeScale = 1;
				BlackHub.gameObject.SetActive (false);
				var buttons = GameObject.Find ("Buttons");
				if (buttons != null) {
						buttons.SetActive (true);
				}

				_isPaused = false;
				_characterController.IsActive = true;
				_died = false;
				_deathShown = false;
				_characterController.IsDied = false;
				_meters = 0;
				_cameraManager.enabled = true;
				if (_hud != null) {
						_hud.ShowPause (false, AudioListener.pause);
						_hud.HideDeath ();
				}

				// Une seule boucle de spawn : l'ancien code en empilait une par reprise de pause
				if (_monsterRoutine != null) {
						StopCoroutine (_monsterRoutine);
				}
				_monsterRoutine = StartCoroutine (GenerateMonsters ());
		}

    #endregion

    #region TERRAIN / MONSTRES

		private IEnumerator GenerateMonsters ()
		{
				while (!_died) {
						if (_difficulty > MonsterMinDifficulty) {
								_spawner.TrySpawnTaupe ();
						}
						yield return new WaitForSeconds (SpawnIntervalBase / _difficulty);
				}
		}

		/// <summary>Appelé par Dynamite quand la mèche est consumée.</summary>
		public void BangRepercution (GroundElement focusElement)
		{
				_terrain.Bang (focusElement);
				_cameraManager.Shake (0.22f, 0.5f);
				Tween.HitStop (0.05f, 0.1f);
				if (_atmosphere != null) {
						_atmosphere.Flash (0.75f, 0.6f); // la déflagration éclaire la mine
				}
		}

    #endregion

    #region FEEDBACK

		/// <summary>Le mineur prend un coup : secousse, ralenti bref, HUD qui tremble.</summary>
		public void NotifyDamage (bool fatal)
		{
				_cameraManager.Shake (fatal ? 0.28f : 0.12f, fatal ? 0.5f : 0.25f);
				if (!fatal) {
						Tween.HitStop (0.06f, 0.05f);
				}
				if (_hud != null) {
						_hud.OnDamage ();
				}
		}

		/// <summary>Début de l'animation de mort : secousse forte et zoom sur le mineur pendant le ralenti.</summary>
		public void OnPlayerDying ()
		{
				NotifyDamage (true);
				float from = _camera.orthographicSize;
				Tween.Value (from, from * 0.8f, 1.2f, size => { if (_camera != null) { _camera.orthographicSize = size; } });
		}

    #endregion

    #region SCORE

		public void KillPlayerAndDestroyGround (CameraManager cam)
		{
				CalculateFinalScore ();
				SaveScore ();

				var audio = Character.GetComponent<AudioSource> ();
				audio.clip = DieAudio;
				audio.Play ();
				Handheld.Vibrate ();
				cam.Shake (0.3f, 0.5f);

				_died = true;
				cam.enabled = false;
				_terrain.Clear ();
		}

		public void CalculateFinalScore ()
		{
				_finalScore = Mathf.RoundToInt (_meters + (_meters * _characterController.Coins / 100));
		}

		public void SaveScore ()
		{
				var db = FindAnyObjectByType<DBScript> ();
				if (db != null) {
						db.SaveScore (_finalScore);
				}
				_scoreSaved = true;

				if (_meters > _yourBestScore) {
						PlayerPrefs.SetInt ("score", _meters);
						PlayerPrefs.Save ();
						_yourBestScore = _meters;
				}
		}

    #endregion
}
