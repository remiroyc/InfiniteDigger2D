using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TutorialState
{

		LEARN_TO_MOVE,
		LEARN_TO_BREAK,
		LEARN_COINS,
		LEARN_ENEMIES

}

public class TutorialScript : MonoBehaviour
{
		public GUIStyle TitleStyle;
		public GUIStyle TextStyle;
		private float _virtualHeight = 1920f;
		private float _virtualWidth = 1080f;
		private Matrix4x4 _matrix;
		private GroundRaw _lastRawGround;
		public int Offset = 6;
		public GameObject RawPrefab;
		private int _nbBrick;
		public Transform BrickPrefab;
		private readonly Queue<GroundRaw> _groundRaws = new Queue<GroundRaw> ();
		public TutorialState CurrentState;
		public Transform Character, MoveObjective;
		private bool _learnToMoveRight = false, _learnToMoveLeft = false, _coroutineRunning = false;
		private CharacterControllerScript _characterControllerScript;
		private bool _destructed = false;

		void Awake ()
		{
				_characterControllerScript = Character.GetComponent<CharacterControllerScript> ();
		}

		void Start ()
		{
				_matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (Screen.width / _virtualWidth, Screen.height / _virtualHeight, 1.0f));				
				Vector3 origin = Camera.main.WorldToScreenPoint (new Vector3 (BrickPrefab.GetComponent<Renderer>().bounds.min.x, BrickPrefab.GetComponent<Renderer>().bounds.max.y, 0f));
				Vector3 extent = Camera.main.WorldToScreenPoint (new Vector3 (BrickPrefab.GetComponent<Renderer>().bounds.max.x, BrickPrefab.GetComponent<Renderer>().bounds.min.y, 0f));
				var brickDim = new Rect (origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
				_nbBrick = (int)(Screen.width / brickDim.width) + 2;

				for (int i = 0; i < 8; i++) {
						CreateRawGround ("AAAAAAAAAAAAAAAAAAAAAA");
				}


				for (int i = 0; i < 10; i++) {

						string line = "";

						for (int j = 0; j < 20; j++) {

								var randNb = Random.value;
								char c;

								if (randNb < 0.25f) {
										c = 'A';
								} else if (randNb < 0.5f) {
										c = 'B';
								} else if (randNb < 0.75f) {
										c = 'C';
								} else {
										c = 'D';
								}
								line += c;
						}

						CreateRawGround (line);
				}

				CurrentState = TutorialState.LEARN_TO_MOVE;
		}

		void Update ()
		{

				switch (CurrentState) {
				case TutorialState.LEARN_TO_MOVE:

						if (!_learnToMoveRight) {

								if (Character.position.x >= MoveObjective.position.x) {
										_learnToMoveRight = true;
										MoveObjective.position = new Vector3 (-2, MoveObjective.position.y, MoveObjective.position.z);
								}

						} else if (!_learnToMoveLeft) {

								if (Character.position.x <= MoveObjective.position.x) {
				
										_learnToMoveLeft = true;
										MoveObjective.gameObject.SetActive (false);
										if (!_coroutineRunning) {
												StartCoroutine (SetCurrentSetAfterSec (3, TutorialState.LEARN_TO_BREAK));
												GameObject.Find ("LearnToMoveItems").SetActive (false);
										}
								}

						}

						break;

				case TutorialState.LEARN_TO_BREAK:

						_characterControllerScript.CanAttack = true;
						if (_characterControllerScript.NbAttack >= 5) {
								_characterControllerScript.CanAttack = false;
								// _scrollCamDistance = 10f; 
								_characterControllerScript.NbAttack = 0;
								StartCoroutine (SetCurrentSetAfterSec (3, TutorialState.LEARN_ENEMIES));
						}

						break;


				case TutorialState.LEARN_ENEMIES:

						if (!_destructed) {
								for (int i = 0; i <  6; i++) {
										var line = _groundRaws.Dequeue ();
										Destroy (line.gameObject);
								}
							_destructed = true;
						}

						break;

				}



		}

		void OnGUI ()
		{

				GUI.matrix = _matrix;

				string title = string.Empty;

				switch (CurrentState) {
				case TutorialState.LEARN_TO_MOVE:

						title = "1 - Les déplacements";
						if (_learnToMoveLeft && _learnToMoveRight) {
								GUI.Label (new Rect ((_virtualWidth - (_virtualHeight * 0.5f)) / 2, _virtualHeight * 0.2f, _virtualHeight * 0.5f, 50), "Bravo, tu te débrouille bien. Prochaine étape : apprendre à creuser !", TextStyle);
						} else {
								GUI.Label (new Rect ((_virtualWidth - (_virtualHeight * 0.5f)) / 2, _virtualHeight * 0.2f, _virtualHeight * 0.5f, 50), "Pour se déplacer, rien de plus simple, utilisez l'accéléromètre de votre téléphone pour vous déplacer vers le point lumineux.", TextStyle);
						}

						break;
		
				case TutorialState.LEARN_TO_BREAK:
						title = "2 - Destruction";


						if (_characterControllerScript.NbAttack < 5) {
								GUI.Label (new Rect ((_virtualWidth - (_virtualHeight * 0.5f)) / 2, _virtualHeight * 0.3f, _virtualHeight * 0.5f, 50), "Pour creuser toujours plus profondément, brisez la pierre en gardant votre doigt appuyé sur l'écran. La pierre ciblée est indiquée par une teinte rouge. Par défault, c'est la pierre en dessous de vous qui est ciblé. Utilisez l'accéléromètre pour cibler d'autres pierres.\n\nEssaye de t'exercer sur quelques pierres...", TextStyle);
						} else {
								GUI.Label (new Rect ((_virtualWidth - (_virtualHeight * 0.5f)) / 2, _virtualHeight * 0.3f, _virtualHeight * 0.5f, 50), "Parfait, ca se voit que tu as un don. Toutefois prend garde, avant de creuser réfléchit bien car nombreux sont les dangers.", TextStyle);
						}		

						break;
		
				}

				GUI.Label (new Rect ((_virtualWidth - (_virtualHeight * 0.5f)) / 2, _virtualHeight * 0.1f, _virtualHeight * 0.5f, 50), title, TitleStyle);

		}

		public void FixedUpdate ()
		{
			/*
				if (_scrollCamDistance > 0) {
						_scrollCamDistance -= 0.1f;
						Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y - 0.05f, Camera.main.transform.position.z);
				}
				*/
		}

		IEnumerator SetCurrentSetAfterSec (float seconds, TutorialState state)
		{
				yield return new WaitForSeconds (seconds);
				CurrentState = state;
		}

		public void CreateRawGround (string line)
		{
		
				var initPos = Camera.main.ScreenToWorldPoint (Vector3.zero);
		
				Vector3 rawPosition;
				if (_lastRawGround == null) {
			
						rawPosition = new Vector3 (initPos.x, Camera.main.transform.position.y - Offset, 1);
			
				} else {
			
						var calculatedPosition = new Vector3 (initPos.x, _lastRawGround.gameObject.transform.position.y - 0.75f, 1);
						rawPosition = calculatedPosition;
				}
		
				var go = Instantiate (RawPrefab, rawPosition, Quaternion.identity) as GameObject;
				GroundRaw raw = go.GetComponent<GroundRaw> ();
				raw.InitialBrickVector = initPos;
				raw.NbElements = _nbBrick;
				raw.GenerateGroundElements (line.ToCharArray ());
				_groundRaws.Enqueue (raw);
				_lastRawGround = raw;
		}

}
