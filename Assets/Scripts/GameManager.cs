using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
		private Matrix4x4 _matrix;
		private int _nbBrick;
		public GUISkin _skin;
		public Transform ScoreLine;
		private float _virtualHeight = 1920f;
		private float _virtualWidth = 1080f;
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

    #region MONO BEHAVIOUR METHODS

		private void Awake ()
		{
				_currentModelRaw = new Queue<char[]> ();
				_characterController = Character.GetComponent<CharacterControllerScript> ();

				Time.timeScale = 0;
				_yourBestScore = PlayerPrefs.GetInt ("score");
				_yTopPosition = Camera.main.transform.position.y + Offset;
		
				StartCoroutine (GenerateMonsters ());
		
				_matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (Screen.width / _virtualWidth, Screen.height / _virtualHeight, 1.0f));
		
		
				Vector3 origin = Camera.main.WorldToScreenPoint (new Vector3 (BrickPrefab.GetComponent<Renderer>().bounds.min.x, BrickPrefab.GetComponent<Renderer>().bounds.max.y, 0f));
				Vector3 extent = Camera.main.WorldToScreenPoint (new Vector3 (BrickPrefab.GetComponent<Renderer>().bounds.max.x, BrickPrefab.GetComponent<Renderer>().bounds.min.y, 0f));
				var brickDim = new Rect (origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
		
				_nbBrick = (int)(Screen.width / brickDim.width) + 2;
				_nbDynamites = 5;

				if (ScoreLine != null) {
						ScoreLine.transform.position = -new Vector3 (0, (Camera.main.transform.position.y + _yourBestScore), 0);
				}

		}
	
		void Start ()
		{
				var adManager = FindObjectOfType<AdManager> ();
				if (adManager != null) {
						adManager.DisplayAd ();
				}
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

				Camera.main.GetComponent<CameraManager> ().enabled = true;
				this.GetComponent<AudioSource> ().Play ();

				StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["FirstGameMessage"], 5));
		}

		void OnGUI ()
		{

				if (!_gameStarted) {
						return;
				}

				GUI.matrix = _matrix;
				GUI.skin = _skin;

				if (_died) {

						Time.timeScale = 0;
						DisplayScore ();

						
				} else {

			/*
						if (!string.IsNullOrEmpty (_messageToDisplay)) {
								GUI.Label (new Rect (_virtualWidth * 0.1f, _virtualHeight - 100, _virtualWidth * 0.8f, 80), _messageToDisplay, "BlackMessage");
						}
*/
						string strMeters = (_meters >= 1000) ? (_meters / 1000) + "K" : _meters.ToString ();

						GUI.Box (new Rect (_virtualWidth * 0.3f, _virtualWidth * 0.03f, _virtualWidth * 0.2f, _virtualWidth * 0.077f), strMeters, "DistanceHub");
						GUI.Box (new Rect (_virtualWidth * 0.03f, _virtualWidth * 0.03f, _virtualWidth * 0.2f, _virtualWidth * 0.077f), _characterController.Coins.ToString (), "CoinHub");

						// GUI.Label (new Rect (5, 35, 150, 25), "Votre meilleur score : " + _yourBestScore);

						if (_isPaused) {

								GUI.Box (new Rect ((_virtualWidth - (_virtualWidth * 0.8f)) / 2, _virtualHeight * 0.2f, _virtualWidth * 0.8f, _virtualWidth), LocalizationStrings.Instance.Values ["Pause"], "LayoutOption");
					

								if (!AudioListener.pause) {

										if (GUI.Button (new Rect (_virtualWidth - (_virtualWidth * 0.30f), (_virtualWidth * 0.03f), (_virtualWidth * 0.1f), (_virtualWidth * 0.1f)), string.Empty, "SoundButton")) {
												AudioListener.pause = !AudioListener.pause;
										}

								} else {
				
										if (GUI.Button (new Rect (_virtualWidth - (_virtualWidth * 0.30f), (_virtualWidth * 0.03f), (_virtualWidth * 0.1f), (_virtualWidth * 0.1f)), string.Empty, "SoundButton2")) {
												AudioListener.pause = !AudioListener.pause;
										}

								}
				                       
				                       
								if (GUI.Button (new Rect ((_virtualWidth - (_virtualWidth * 0.4f)) / 2, _virtualHeight * 0.3f, (_virtualWidth * 0.4f), (_virtualWidth * 0.13f)), LocalizationStrings.Instance.Values ["Resume"])) {
										Replay ();
								}

								if (GUI.Button (new Rect ((_virtualWidth - (_virtualWidth * 0.4f)) / 2, _virtualHeight * 0.4f, (_virtualWidth * 0.4f), (_virtualWidth * 0.13f)), LocalizationStrings.Instance.Values ["Restart"])) {
										Replay ();
										Application.LoadLevel ("test");
								}

								if (GUI.Button (new Rect ((_virtualWidth - (_virtualWidth * 0.4f)) / 2, _virtualHeight * 0.5f, (_virtualWidth * 0.4f), (_virtualWidth * 0.13f)), LocalizationStrings.Instance.Values ["Menu"])) {
										Application.LoadLevel ("Menu");
								}
								

								if (GUI.Button (new Rect ((_virtualWidth - (_virtualWidth * 0.4f)) / 2, _virtualHeight * 0.6f, (_virtualWidth * 0.4f), (_virtualWidth * 0.13f)), LocalizationStrings.Instance.Values ["Quit"])) {
										Application.Quit ();
								}

						} else {

								if (GUI.Button (new Rect (_virtualWidth - (_virtualWidth * 0.30f), (_virtualWidth * 0.03f), (_virtualWidth * 0.1f), (_virtualWidth * 0.1f)), _nbDynamites.ToString (), "ExplosionButton")) {

										if (_nbDynamites > 0) {
												--_nbDynamites;
												_characterController.ThrowDynamite ();
										} else {
												StartCoroutine (DisplayMessage (LocalizationStrings.Instance.Values ["NoDynamite"], 5));
										}
								}
								
								// GUI.Box (new Rect (0, _virtualHeight - 35, _virtualWidth, 35), string.Empty, "BottomLayout");

								/*
								if (GUI.Button (new Rect (virtualWidth - 80, virtualHeight - 80, 55, 70), "JUMP", "JumpButton")) {
										_characterController.Jump ();
								}
*/

								if (GUI.Button (new Rect (_virtualWidth - (_virtualWidth * 0.13f), (_virtualWidth * 0.03f), (_virtualWidth * 0.1f), (_virtualWidth * 0.1f)), LocalizationStrings.Instance.Values ["Pause"], "PauseButton")) {
										Pause ();
								}

								if (Input.GetKey (KeyCode.Escape)) {
										Pause ();
										return;
								}

						}
				}
		}
	
		void Update ()
		{

				if (!_gameStarted) {
						return;
				}

				var camManager = Camera.main.GetComponent<CameraManager> ();

				if (_characterController != null && _characterController.IsDied) {
						_died = true;
						Camera.main.GetComponent<CameraManager> ().enabled = false;

						if (!_scoreSaved) {
								SaveScore ();
						}
						return;
				}

				


				// Si le personnage se trouve dans les 30% du bas de l'écran on accélère la caméra
				if (Camera.main.WorldToScreenPoint (Character.position).y <= (Screen.width * 0.3)) {
					camManager.CameraSpeed = (0.01f + (0.0015f * _difficulty)) * 5; 
				} else {
					camManager.CameraSpeed = 0.01f + (0.0015f * _difficulty);
				}
		
				_yTopPosition = Camera.main.transform.position.y + Offset;
				if (Character.position.y > _yTopPosition) {
					KillPlayerAndDestroyGround (camManager);
				} 

				if (_groundRaws != null && _groundRaws.Any ()) {
					
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
			case 25:
				// StartCoroutine (DisplayMessage ("Boss incoming !!!", 5));
				break;
			}
		}
		
		/*
				if (_meters == 30 && !_spawningBoss) {
						StartCoroutine (CreateBoss ());
				}
		        */
		
		
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

		public void DisplayScore ()
		{
				BlackHub.gameObject.SetActive (true);


				GUI.Label (new Rect ((_virtualWidth - (_virtualWidth * 0.9f)), 100, _virtualWidth * 0.9f, 100), LocalizationStrings.Instance.Values ["YouAreDead"]);

				GUI.Box (new Rect ((_virtualWidth - (_virtualWidth * 0.9f)) / 3, _virtualHeight * 0.2f, _virtualWidth * 0.9f, _virtualWidth * 0.84f), LocalizationStrings.Instance.Values ["YourScore"], "HighScoreBox");
				// GUI.Box (new Rect ((_virtualWidth - (_virtualWidth * 0.8f)) / 2, 20, _virtualWidth * 0.8f, _virtualWidth * 0.30f), string.Empty, "HighScore");

				GUI.Label (new Rect (_virtualWidth * 0.3f, _virtualHeight * 0.3f, _virtualWidth * 0.4f, 100f), LocalizationStrings.Instance.Values ["Distance"] + " " + _meters + "m");
				
				if (_characterController != null) {
						GUI.Label (new Rect (_virtualWidth * 0.3f, _virtualHeight * 0.35f, _virtualWidth * 0.4f, 100f), LocalizationStrings.Instance.Values ["CollectedCoins"] + " " + _characterController.Coins);
						GUI.Label (new Rect (_virtualWidth * 0.3f, _virtualHeight * 0.4f, _virtualWidth * 0.4f, 100f), LocalizationStrings.Instance.Values ["NbTap"] + " " + _characterController.NbAttack);
						// GUI.Label (new Rect (_virtualWidth * 0.4f, _virtualHeight * 0.4f, _virtualWidth * 0.3f, 100f), LocalizationStrings.Instance.Values ["NbDestroyedObject"]);
				}

				GUI.Label (new Rect (_virtualWidth * 0.3f, _virtualHeight * 0.45f, _virtualWidth * 0.3f, 100f), LocalizationStrings.Instance.Values ["FinalScore"] + " " + _finalScore);

				// GUI.Label (new Rect (_virtualWidth * 0.2f, _virtualHeight * 0.35f, _virtualWidth * 0.3f, 100f), "Username : ");
				// _username = GUI.TextField (new Rect (_virtualWidth * 0.4f, _virtualHeight * 0.35f, _virtualWidth * 0.4f, 100f), _username);

				// GUI.Label (new Rect (150, 30, 220, 84), "Reminouche"); 
				// GUI.Label (new Rect (120, 200, 300, 25), "Vous etes mort");

				/*
				if (GUI.Button (new Rect (_virtualWidth * 0.4f, _virtualHeight * 0.45f, (_virtualWidth * 0.4f), (_virtualWidth * 0.13f)), "Save")) {
						SaveScore ();
				}
*/

				if (GUI.Button (new Rect (_virtualWidth * 0.50f, _virtualHeight * 0.58f, _virtualWidth * 0.15f, _virtualWidth * 0.15f), string.Empty, "HomeButton")) {
						Replay ();
						Application.LoadLevel ("menu");
				}

				if (GUI.Button (new Rect (_virtualWidth * 0.70f, _virtualHeight * 0.58f, _virtualWidth * 0.15f, _virtualWidth * 0.15f), string.Empty, "ReplayButton")) {
						Replay ();
						Application.LoadLevel ("test");
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
				Camera.main.GetComponent<CameraManager> ().enabled = false;

				// StopCoroutine ("GenerateInfiniteGround");

		}

		public void Replay ()
		{

				Time.timeScale = 1;

				BlackHub.gameObject.SetActive (false);
				GameObject.Find ("Buttons").SetActive (true);

				_isPaused = false;
				_characterController.IsActive = true;
				_died = false;
				_characterController.IsDied = false;
				_meters = 0;
				Camera.main.GetComponent<CameraManager> ().enabled = true;
				StartCoroutine (GenerateMonsters ());
				
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
				while (_groundRaws.Any()) {
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

				FindObjectOfType<DBScript> ().StartCoroutine ("SaveScore", _finalScore);
				_scoreSaved = true;

				if (_meters > _yourBestScore) {
						PlayerPrefs.SetInt ("score", _meters);
						PlayerPrefs.Save ();
						_yourBestScore = _meters;
				}
		}

}