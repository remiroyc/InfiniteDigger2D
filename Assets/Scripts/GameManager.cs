using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

		public GameObject BrickPrefab;
		public Transform Character;
		public GameObject CoinPrefab;
		public AudioClip DieAudio;
		// public GameObject[] Explosions;
		public GameObject BlackCache;
		public int Offset = 6;
		public GameObject TaupePrefab, RawPrefab;
		public Transform BlackHub;
		private readonly Queue<GroundRaw> _groundRaws = new Queue<GroundRaw> ();
		private bool _died, _isPaused = false;
		private float _difficulty = 1;
		private GroundRaw _lastRawGround;
		private int _meters, _finalScore;
		private int _nbRaws;
		private int _nbBrick;
		public Transform ScoreLine;
		private CharacterControllerScript _characterController;
		private float _yTopPosition;
		private int _yourBestScore;
		private Queue<char[]> _currentModelRaw = null;
		private float createTaupeTimer = 0;
		// private string _messageToDisplay = string.Empty;
		public GameObject Dock;
		private int _nbDynamites;
		private bool _spawningBoss = false;
		private bool _gameStarted = false;
		private bool _scoreSaved = false;
		private Camera _camera;
		private CameraManager _cameraManager;
		private Coroutine _monsterRoutine;
		// Les paliers (messages, difficulté) ne doivent se déclencher qu'une fois par mètre parcouru
		private int _lastMilestoneMeters = -1;
		// HUD uGUI construit par code (compteurs, vie, boutons, pause, mort)
		private GameHud _hud;
		private bool _deathShown;

    #region MONO BEHAVIOUR METHODS

		private void Awake ()
		{
				_currentModelRaw = new Queue<char[]> ();
				_characterController = Character.GetComponent<CharacterControllerScript> ();
				_camera = Camera.main;
				_cameraManager = _camera.GetComponent<CameraManager> ();

				Time.timeScale = 0;
				_nbDynamites = 5;
				_yourBestScore = PlayerPrefs.GetInt ("score");
				_yTopPosition = _camera.transform.position.y + Offset;

				_monsterRoutine = StartCoroutine (GenerateMonsters ());

				var brickBounds = BrickPrefab.GetComponent<Renderer> ().bounds;
				Vector3 origin = _camera.WorldToScreenPoint (new Vector3 (brickBounds.min.x, brickBounds.max.y, 0f));
				Vector3 extent = _camera.WorldToScreenPoint (new Vector3 (brickBounds.max.x, brickBounds.min.y, 0f));
				var brickDim = new Rect (origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
		
				_nbBrick = (int)(Screen.width / brickDim.width) + 2;

				if (ScoreLine != null) {
						ScoreLine.transform.position = -new Vector3 (0, (_camera.transform.position.y + _yourBestScore), 0);
				}

				// En dernier : une UI qui échoue (ex. ressources TMP absentes) ne doit pas empêcher la partie
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

				_currentModelRaw.Enqueue ("BBAAABAAABB".ToArray ());
				_currentModelRaw.Enqueue ("BABBBBABABB".ToArray ());
				_currentModelRaw.Enqueue ("BABAABABABB".ToArray ());
				_currentModelRaw.Enqueue ("BABBABABABB".ToArray ());
				_currentModelRaw.Enqueue ("BAAAABAAABB".ToArray ());
				_currentModelRaw.Enqueue ("BBBBBBBBBBB".ToArray ());

				for (int i = 0; i < 20; i++) {
						CreateRawGround ();
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

				var camManager = _cameraManager;

				if (_characterController != null && _characterController.IsDied) {
						_died = true;
						_cameraManager.enabled = false;

						if (!_scoreSaved) {
								SaveScore ();
						}
						return;
				}

				// Si le personnage se trouve dans les 30% du bas de l'écran on accélère la caméra
				if (_camera.WorldToScreenPoint (Character.position).y <= (Screen.width * 0.3)) {
					camManager.CameraSpeed = (0.01f + (0.0015f * _difficulty)) * 5;
				} else {
					camManager.CameraSpeed = 0.01f + (0.0015f * _difficulty);
				}

				_yTopPosition = _camera.transform.position.y + Offset;
				if (Character.position.y > _yTopPosition) {
					KillPlayerAndDestroyGround (camManager);
				}

				if (_groundRaws.Count > 0) {
					
					GroundRaw ground = _groundRaws.Peek ();
					
					if (ground != null) {
						var goGround = ground.gameObject;
						
						if (goGround.transform.position.y >= _yTopPosition) {
							_groundRaws.Dequeue ();
							Destroy (goGround);
							
							
							if (!_spawningBoss) {
								CreateRawGround ();
							}
							
						}
					}
				}

				_meters = Mathf.RoundToInt (camManager.Distance);

				// Un mètre dure une centaine de frames : sans ce garde, le message "BeCareful" partait
				// ~100 fois à 10 m (autant de coroutines) et la difficulté gagnait +0.07 par frame
				// tant que _meters restait multiple de 15, soit +7 d'un coup au lieu de +0.07.
				if (_meters == _lastMilestoneMeters) {
						return;
				}
				_lastMilestoneMeters = _meters;

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

				if (_meters > 0 && _meters % 15 == 0) {
						_difficulty += 0.07f;
				}
		}

    #endregion

	#region GUI MANAGEMENT

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
				_hud.OnRestart = () => { Replay (); SceneManager.LoadScene ("test"); };
				_hud.OnMenu = () => SceneManager.LoadScene ("menu");
				_hud.OnQuit = Application.Quit;
				_hud.OnDynamite = UseDynamite;
				_hud.OnSoundToggle = ToggleSound;
				_hud.OnHome = () => { Replay (); SceneManager.LoadScene ("menu"); };
				_hud.OnReplay = () => { Replay (); SceneManager.LoadScene ("test"); };
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

    #region TERRAIN GENERATION

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
				GameObject.Find ("Buttons").SetActive (true);

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

		private IEnumerator GenerateMonsters ()
		{
				while (_died == false) {
						if (_difficulty > 2) {
								CreateMonsters ();
						}
						yield return new WaitForSeconds ((3 / _difficulty));	
				}
		}

		IEnumerator CreateBoss ()
		{
				_spawningBoss = true;
				var initPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 0));
				var lastPosition = _lastRawGround.gameObject.transform.position.y - 0.75f;

				while (_groundRaws.Any()) {
			
						var item = _groundRaws.Dequeue ();
						Destroy (item.gameObject);
				}


				var rawPosition = new Vector3 (initPos.x, lastPosition, 1);
				var go = Instantiate (RawPrefab, rawPosition, Quaternion.identity) as GameObject;
				GroundRaw raw = go.GetComponent<GroundRaw> ();
				raw.InitialBrickVector = initPos;
				raw.NbElements = _nbBrick;
				raw.GenerateGroundElements ("CCCCCCCCCCCCCC".ToCharArray ());
				_groundRaws.Enqueue (raw);
				_lastRawGround = raw;

				for (int i = 0; i < 20; i++) {

						CreateRawGround ();

				}

				yield return new WaitForSeconds (1f);

				while (!_characterController.Grounded) {
						yield return new WaitForSeconds (1f);
				}

				Camera.main.GetComponent<CameraManager> ().enabled = false;


				yield return new WaitForSeconds (2f);
				var golem = Instantiate (Resources.Load ("Golem"), _groundRaws.Peek ().transform.position + new Vector3 (0, 1.75f, 0), Quaternion.identity) as GameObject;


				yield return new WaitForSeconds (8f);

				_spawningBoss = false;
				Camera.main.GetComponent<CameraManager> ().enabled = true;
				Destroy (golem);
				Destroy (_groundRaws.Dequeue ().gameObject);

		}
	
		public void CreateRawGround ()
		{

				var initPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 0));

				Vector3 rawPosition;
				if (_lastRawGround == null) {

						rawPosition = new Vector3 (initPos.x, Camera.main.transform.position.y - Offset, 1);
				
				} else {

						var calculatedPosition = new Vector3 (initPos.x, _lastRawGround.gameObject.transform.position.y - 0.75f, 1);

						/*
						if ((calculatedPosition.y - Camera.main.transform.position.y) < -10) {
								return;
						}
						*/

						rawPosition = calculatedPosition;
				}

				var go = Instantiate (RawPrefab, rawPosition, Quaternion.identity) as GameObject;
			

				GroundRaw raw = go.GetComponent<GroundRaw> ();
				raw.InitialBrickVector = initPos;
				raw.NbElements = _nbBrick;

				if (_currentModelRaw == null || !_currentModelRaw.Any ()) {

						raw.GenerateGroundElements ();

						// 2% de chance de créer un modèle prédéfini
						if (UnityEngine.Random.value <= 0.02f) {
						
								foreach (var item in TerrainFactory.GetTerrain()) {

										if (item != null) {
												_currentModelRaw.Enqueue (item.ToCharArray ());
										}
								}
						}


				} else {

						char[] rawModel = _currentModelRaw.Dequeue ();
						raw.GenerateGroundElements (rawModel);

				}


				// GroundRaw raw = new GroundRaw(BrickTransform, 15);


				_groundRaws.Enqueue (raw);

				++_nbRaws;
				_lastRawGround = raw;
		}
		
		public void BangRepercution (GroundElement focusElement)
		{

				// Camera.main.transform.parent.GetComponent<Animation> ().Play ();

				GroundRaw selectedRaw = _groundRaws.FirstOrDefault (g => g.GroundElements.Contains (focusElement));
				
				foreach (GroundElement elem in selectedRaw.GroundElements) {
						if (elem != null) {
								elem.Explosion ();
						}
				}

				int nbDeleted = 0;
				foreach (GroundRaw raw in _groundRaws) {

						if (nbDeleted >= 10) {
								break;
						}

						if (raw != selectedRaw) {
								if (raw.GroundElements != null && raw.GroundElements.Length > focusElement.ElementIndex) {
										GroundElement item = raw.GroundElements [focusElement.ElementIndex];
										if (item != null) {
												item.Explosion ();
												++nbDeleted;
										}
								}
						}
				}
		}


		/// <summary>
		/// Retourne la ligne ou le joueur se trouve
		/// </summary>
		public GroundRaw GetRawWithCharacter ()
		{
				if (_characterController.GroundElementTouched != null) {
						var groundElem = _characterController.GroundElementTouched.GetComponent<GroundElement> ();
						var raw = _groundRaws.FirstOrDefault (g => g.GroundElements != null && g.GroundElements.Contains (groundElem));
						return raw;
				}
				return null;
		}

    #endregion

    #region CHARACTER MANAGEMENT

		public void KillPlayerAndDestroyGround (CameraManager cam)
		{
				CalculateFinalScore ();
				SaveScore ();

				Character.GetComponent<AudioSource>().clip = DieAudio;
				Character.GetComponent<AudioSource>().Play ();

				Handheld.Vibrate ();

				_died = true;
				cam.enabled = false;
				while (_groundRaws.Count > 0) {
						GroundRaw go = _groundRaws.Dequeue ();
						Destroy (go.gameObject);
				}
		}

		public void CalculateFinalScore ()
		{
				_finalScore = Mathf.RoundToInt (_meters + (_meters * _characterController.Coins / 100));
				Debug.Log ("CalculateFinalScore() = " + _finalScore);
		}

    #endregion

	#region MONSTERS MANAGEMENT

		/// <summary>
		/// Calcul la chance qu'un monstre apparaisse (générée toutes les 5 secondes)
		/// </summary>
		/// <returns>The monster.</returns>
		public void CreateMonsters ()
		{

				CreateRandomTaupe ();

				/*
				if (Random.value < 0.1 && !_monsterInstanciated) {

						var golem = Resources.Load ("Golem") as GameObject;
						var pos = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width / 2, Screen.height, 0));

						var monsterObj = Instantiate (golem, new Vector3 (pos.x, pos.y, 0), Quaternion.identity) as GameObject;
						monsterObj.GetComponent<GolemScript> ().MaxYPosition = _yTopPosition;
						_monsterInstanciated = true;

				}
				*/

		}

		/*
		[System.Obsolete()]
		public void CreateTaupe ()
		{
				float diff = Time.time - createTaupeTimer;
				if (diff >= 5) {
			
			
						var groundRawTab = _groundRaws.Where (g => g.GroundElements != null && g.GroundElements.Any (e => e != null && e.IsEmpty == true)).ToArray ();
			
						if (groundRawTab.Any ()) {
				
								var randVal = Random.Range (0, groundRawTab.Count ());
								var taupe = Resources.Load ("Taupe") as GameObject;
				
								GroundRaw selectedRaw = groundRawTab [randVal];
				
				
				
								var groundElementsTab = selectedRaw.GroundElements.Where (g => g != null && g.IsEmpty).ToArray ();
				
								var randVal2 = Random.Range (0, groundElementsTab.Count ());
				
								//								Debug.Log (groundElementsTab.Count () + " / " + randVal2);
								GroundElement selectedGroundElem = groundElementsTab [randVal2];
				
								int groundIndex = groundElementsTab.ToList ().IndexOf (selectedGroundElem);
				
								int index = _groundRaws.ToList ().IndexOf (selectedRaw);
								var botRaw = _groundRaws.ElementAtOrDefault (index + 1);
								if (botRaw != null) {
										var botItem = botRaw.GroundElements [groundIndex];
					
										if (selectedGroundElem != null && botItem != null && !botItem.IsEmpty) {
						
						
						
												var taupeGO = Instantiate (taupe, selectedGroundElem.transform.position, Quaternion.identity) as GameObject;
												createTaupeTimer = Time.time;
												taupeGO.GetComponent<TaupeScript> ().Offset = Offset;
						
										}
								}
						}
				}
		}
*/

		public void CreateRandomTaupe ()
		{

				float diff = Time.time - createTaupeTimer;
				if (diff >= 5) {

						var raw = GetRawWithCharacter ();

						if (raw == null) {
								Debug.LogError ("Impossible de récupérer la ligne");
						} else {

								var tabTemp = raw.GroundElements.Where (g => g != null).ToArray ();
								var rand = UnityEngine.Random.Range (0, tabTemp.Count ());

								var groundElem = tabTemp [rand];
	
								int realIndex = raw.GroundElements.ToList ().IndexOf (groundElem);

								Vector3 taupePosition = tabTemp [rand].transform.position;

								var liste = _groundRaws.ToList ();
								int index = liste.IndexOf (raw);
								var previousRaw = _groundRaws.ElementAtOrDefault (index - 1);

								if (previousRaw != null) {
				
										var elemToDestroy = previousRaw.GroundElements [realIndex];
										if (elemToDestroy != null) {


												Instantiate (Resources.Load ("BottomExplosion"), elemToDestroy.transform.position, Quaternion.identity);
		
												Destroy (elemToDestroy.gameObject);

												taupePosition += new Vector3 (0, 0.75f, 0);
												var taupeGO = Instantiate (TaupePrefab, taupePosition, Quaternion.identity) as GameObject;
												taupeGO.transform.parent = groundElem.transform;
												taupeGO.GetComponent<TaupeScript> ().Offset = Offset;
												createTaupeTimer = Time.time;

										}
								}
						}
				}
		}

	#endregion

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

}
