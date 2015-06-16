using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{

		public Transform Character;
		public CharacterControllerScript CharController;
		public CameraManager CamManager;
		public int Offset = 6;
		private float _yTopPosition;
		private float _initialCameraSpeed;
		private float _virtualHeight = 1920f;
		private float _virtualWidth = 1080f;
		private Matrix4x4 _matrix;
		private bool _died = false, _isPaused = false, _scoreSaved = false, _win =false;

	public GUISkin Skin;

	private float _levelTime = 0;

		void Awake ()
		{
				_matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (Screen.width / _virtualWidth, Screen.height / _virtualHeight, 1.0f));
		}

		void Start ()
		{
				_yTopPosition = Camera.main.transform.position.y + Offset;
				_initialCameraSpeed = CamManager.CameraSpeed;
		}

		void OnGUI(){

		GUI.matrix = _matrix;
		GUI.skin = Skin;

			if(_win){
		
			GUI.Box (new Rect (0, 300, _virtualWidth, _virtualHeight * 0.7f), LocalizationStrings.Instance.Values ["YourScore"], "LevelBox");

			GUI.Label(new Rect(200,630, 400, 50), LocalizationStrings.Instance.Values ["CollectedCoins"]);
			GUI.Label(new Rect(600,600, 89, 122), CharController.Coins.ToString(), "DollarPic");

				if(GUI.Button(new Rect(700, 1500, 200, 200), string.Empty, "HomeButton")){
					Application.LoadLevel("menu");
				}
			}
		}

		void Update ()
		{

				if (_died) {
						return;
				}

				if (CharController != null && CharController.IsDied) {
						_died = true;
						Camera.main.GetComponent<CameraManager> ().enabled = false;
						if (!_scoreSaved) {
								SaveScore ();
						}
						return;
				}
		
				// Si le personnage se trouve dans les 30% du bas de l'écran on accélère la caméra
				if (Camera.main.WorldToScreenPoint (Character.position).y <= (Screen.width * 0.3)) {
						CamManager.CameraSpeed = _initialCameraSpeed * 5; 
				} else {
						CamManager.CameraSpeed = _initialCameraSpeed;
				}
		
				_yTopPosition = Camera.main.transform.position.y + Offset;
				if (Character.position.y > _yTopPosition) {
						KillPlayer (CamManager);
				} 

		}

		public void Win(){
			_levelTime = Time.timeSinceLevelLoad;
			_win = true;
			Time.timeScale = 0;
			// StartCoroutine(ChangeLevel());
		}

		public void KillPlayer (CameraManager cam)
		{
		_died = true;
		Character.GetComponent<AudioSource>().PlayOneShot (Resources.Load ("Music/scream") as AudioClip);
				SaveScore ();
				Handheld.Vibrate ();
				cam.enabled = false;
		}

		IEnumerator ChangeLevel(){
			yield return new WaitForSeconds(3);
			Application.LoadLevel("menu");
		}

		public void SaveScore ()
		{
				_scoreSaved = true;
				PlayerPrefs.SetInt (Application.loadedLevelName, 1);
				PlayerPrefs.Save ();
		}

}
