using System.Collections;
using UnityEngine;
using System.Linq;

public class CharacterControllerScript : MonoBehaviour
{
		public bool FacingRight = true, Grounded = false, IsActive = true, IsDied = false, Jumping = false, CanAttack;
		public float maxSpeed = 2f;
		public float groundRadius;
		public LayerMask TapLayer, CoinLayer, GUIButtonLayer, PlayerLayer;
		public GUISkin Skin;
		public float jumpForce = 150f;
		public AudioClip ScreamAudio;
		public AudioClip TapAudio;
		public AudioClip CoinAudio;
		public float MoveForce;
		public GameObject GroundElementTouched, FaceElementTouched;
		private Animator _characterAnimator;
		private bool _attacking = false;
		public int Coins = 0;
		private AudioSource _audioCharacter;
		private Collider2D _playerHit, _hit;
		private bool _touched = false, _rightButton = false, _leftButton = false;
		private float startTime;
		private bool couldBeSwipe;
		float comfortZone;
		private float maxSwipeTime = 1;
		private Vector2 startPos;
		public float Move;
		public int NbAttack = 0;
		public GameManager GameManager;
		public LayerMask GroundLayer;
		public Transform GroundChecker1, GroundChecker2, GroundChecker3, GroundChecker4;

    #region MONO BEHAVIOUR METHODS

		void OnGUI ()
		{

				// GUI.Box (new Rect (Screen.width / 2, Screen.height / 2 - 250, 200, 200), msg);
				// GUI.Box (new Rect (Screen.width / 2, Screen.height / 2, 200, 200), rigidbody2D.velocity.ToString ());
		}

		void OnCollisionEnter2D (Collision2D col)
		{
				if (col.gameObject.tag == "Stone") {
						Destroy (col.gameObject);
						StartCoroutine (Die ());
				}
		}
	
		void Start ()
		{
				_characterAnimator = this.GetComponent<Animator> ();
				_audioCharacter = this.GetComponent<AudioSource> ();
				GameManager = FindObjectOfType<GameManager> ();
		}
	
		void FixedUpdate ()
		{
				if (!IsActive || IsDied) {
						_characterAnimator.SetFloat ("Move", 0);
						return;
				}
	
				DetectCoin ();

				_characterAnimator.SetFloat ("Move", Mathf.Abs (Move));

	
				if (FaceElementTouched != null && !Grounded) {
						GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, GetComponent<Rigidbody2D> ().velocity.y); // On n'est soumis qu'à la gravité
				} else {

						if (Move * GetComponent<Rigidbody2D> ().velocity.x < maxSpeed) {
								GetComponent<Rigidbody2D> ().AddForce (Vector2.right * Move * MoveForce);
						}
				
						if (Mathf.Abs (GetComponent<Rigidbody2D> ().velocity.x) > maxSpeed) {
								GetComponent<Rigidbody2D> ().velocity = new Vector2 (Mathf.Sign (GetComponent<Rigidbody2D> ().velocity.x) * maxSpeed, GetComponent<Rigidbody2D> ().velocity.y);
						}
				}
			
				if (Move > 0 && !FacingRight) {
						Flip ();
				} else if (Move < 0 && FacingRight) {
						Flip ();
				}

				if (_touched && CanAttack) {

						var moveAbs = Mathf.Abs (Move);
						if (moveAbs == 0) {
								Tap (GroundElementTouched, true);
						} else if (moveAbs > 0) {
								Tap (FaceElementTouched, false);
						}
				}

		
		}

		public void ButtonTapDown ()
		{
				_touched = true;
		}

		public void ButtonTapUp ()
		{
				_touched = false;
		}

		public void ButtonRightDown ()
		{
				_rightButton = true;
		}

		public void ButtonRightUp ()
		{
				_rightButton = false;
			
		}

		public void ButtonLeftDown ()
		{
				_leftButton = true;
		}
	
		public void ButtonLeftUp ()
		{
				_leftButton = false;
		}

		void Update ()
		{
				if (IsDied) {
						return;
				}

				var faceHit = Physics2D.Linecast (GroundChecker3.position, GroundChecker4.position, GroundLayer);
				var newFaceElementTouched = (faceHit.transform != null) ? faceHit.transform.gameObject : null;
				
				if (newFaceElementTouched != null) {
					
						if (FaceElementTouched != newFaceElementTouched || FaceElementTouched == null) {

								var ge = newFaceElementTouched.GetComponent<GroundElement> ();
								if (ge.CurrentGroundType != GroundType.IndestructibleBrick) {
										newFaceElementTouched.GetComponent<SpriteRenderer> ().color = Color.red;
								}
					
								if (FaceElementTouched != null) {
										FaceElementTouched.GetComponent<SpriteRenderer> ().color = Color.white;
								}
								FaceElementTouched = newFaceElementTouched;
						}
				} else if (FaceElementTouched != null) {
						FaceElementTouched.GetComponent<SpriteRenderer> ().color = Color.white;
				}

				var groundHit = Physics2D.Linecast (GroundChecker1.position, GroundChecker2.position, GroundLayer);
				var newGroundElementTouched = (groundHit.transform != null) ? groundHit.transform.gameObject : null;

				if (newGroundElementTouched != null) {
		
						if (GroundElementTouched != newGroundElementTouched || GroundElementTouched == null) {

								var ge = newGroundElementTouched.GetComponent<GroundElement> ();
								if (ge.CurrentGroundType != GroundType.IndestructibleBrick) {
										newGroundElementTouched.GetComponent<SpriteRenderer> ().color = Color.red;
								}

								if (GroundElementTouched != null) {
										GroundElementTouched.GetComponent<SpriteRenderer> ().color = Color.white;
								}
								GroundElementTouched = newGroundElementTouched;
						}
				} else if (GroundElementTouched != null) {
						GroundElementTouched.GetComponent<SpriteRenderer> ().color = Color.white;
				}

				Grounded = GroundElementTouched != null;
				_characterAnimator.SetBool ("Grounded", Grounded);

				/*
				#if UNITY_EDITOR
				 Move = GetMoveWithInputPosition ();
				#else
					Move = GetMoveWithAccelerometer ();
				#endif
*/
				if (_rightButton) {
						Move = 0.8f;
				} else if (_leftButton) {
						Move = -0.8f;
				} else {
						Move = 0;		
				}


				// DetectSwipe ();

				/*
				if (!Jumping && !couldBeSwipe) {
					#if UNITY_EDITOR
						_touched = Input.GetMouseButtonDown(0);
					#else
						_touched = (Input.touchCount > 0) && (Input.GetTouch (0).position.y <= Screen.height * 0.9f);
					#endif
				}
				*/
		}
	
	#endregion
	
	#region OBJECTS MANAGEMENT
	
		IEnumerator CreateDynamite (Vector3 pos)
		{
				yield return new WaitForSeconds (0.6f);
				var dynamitePrefab = Resources.Load ("Dynamite") as GameObject;
				var dynamite = Instantiate (dynamitePrefab, pos, Quaternion.identity) as GameObject;
				dynamite.transform.parent = GroundElementTouched.transform;

				if (GroundElementTouched == null) {
						Debug.LogWarning ("Impossible de lancer une dynamite, car il n'y a pas de ground element");
				} else {

						var g = GroundElementTouched.GetComponent<GroundElement> ();
						dynamite.GetComponent<Dynamite> ().GroundElem = g;
						_attacking = false;
				}
		}

		public void DetectCoin ()
		{
				Vector2 point = new Vector2 (transform.position.x, transform.position.y);
				Collider2D[] CoinCollision = Physics2D.OverlapCircleAll (point, 0.5f, CoinLayer);

				if (CoinCollision != null) {
						
						AudioSource audioSource = null;
						GameObject gm = GameObject.Find ("GameManager");
						if (gm != null) {
								audioSource = gm.GetComponent<AudioSource> ();
						}

						foreach (var item in CoinCollision) {

								if (audioSource != null) {
										audioSource.clip = CoinAudio;
										audioSource.Play ();
								}

								++Coins;

								var screenPos = Camera.main.WorldToScreenPoint (item.transform.position);
								screenPos.y = Screen.height - screenPos.y;

			
								var coinScorePrefab = Resources.Load ("100") as GameObject;
								Instantiate (coinScorePrefab, item.transform.position, Quaternion.identity);
				            
								Destroy (item.gameObject);

						}
				}
		}

    #endregion

    #region CHARACTER ACTIONS

		public void ThrowDynamite ()
		{
				if (Grounded && !_attacking && CanAttack) {
						_attacking = true;
						_characterAnimator.Play ("Throw");
						var dynPos = new Vector3 (transform.position.x, transform.position.y - 0.5f, 0);
						StartCoroutine (CreateDynamite (dynPos));
				}
		}

		IEnumerator Jump ()
		{
				if (Grounded) {
						Grounded = false;
						GetComponent<Rigidbody2D> ().AddForce (new Vector2 (0, jumpForce));
						this._characterAnimator.Play ("Jump");
						yield return new WaitForSeconds (1);
						Jumping = false;
				}
		}

		IEnumerator Die ()
		{
				GameManager.CalculateFinalScore ();
				IsActive = false;
				_characterAnimator.Play ("Dead");
				yield return new WaitForSeconds (2);
				IsDied = true;
		}

		private void Flip ()
		{
				if (FaceElementTouched != null) {
						FaceElementTouched.GetComponent<SpriteRenderer> ().color = Color.white;
						FaceElementTouched = null;
				}

				FacingRight = !FacingRight;
				Vector3 theScale = transform.localScale;
				theScale.x *= -1;
				transform.localScale = theScale;
		}

		public void AttackMonster (GameObject go)
		{
				if (!_attacking && go != null) {
						_attacking = true;
						_characterAnimator.Play ("Tap");
						go.SendMessage ("TakeDamage");
						_attacking = false;
				}
		}

		public void Tap (GameObject go, bool bottomTap = true)
		{

			

				if (!_attacking && go != null) {

						GroundElement ge = go.GetComponent<GroundElement> ();
						if (ge.CurrentGroundType != GroundType.IndestructibleBrick) {

								GroundElement elementToDestroy = go.GetComponent<GroundElement> ();
								if (elementToDestroy != null && elementToDestroy.CurrentGroundType != GroundType.IndestructibleBrick) {

										_attacking = true;
										GetComponent<Rigidbody2D> ().velocity = new Vector2 (0f, GetComponent<Rigidbody2D> ().velocity.y);

										if (bottomTap) {
												_characterAnimator.Play ("CrouchTap");
										} else {
												_characterAnimator.Play ("Tap");
										}

										_audioCharacter.clip = TapAudio;
										_audioCharacter.Play ();

										StartCoroutine (WaitAndTap (elementToDestroy));

										++NbAttack;
				
								}
						}
				}
		}

		IEnumerator WaitAndTap (GroundElement elementManager)
		{
				yield return new WaitForSeconds (0.30f);
				elementManager.Tap ();
				_attacking = false;

				if (elementManager.CurrentGroundType == GroundType.Nitro) {
						StartCoroutine (Die ());
				}
		
		
		}

    #endregion

    #region GESTURES MANAGEMENT

		/*
		public void DetectSwipe ()
		{
		
				if (Input.touchCount > 0) {
			
						var touch = Input.GetTouch (0);
			
						switch (touch.phase) {
				
						case TouchPhase.Began:
				
								couldBeSwipe = true;
								startPos = touch.position;
								startTime = Time.time;
								break;
				
						case TouchPhase.Moved:
				
				

								//if (Mathf.Abs (touch.position.y - startPos.y) > comfortZone) {
										
								// }

								break;
				
						case TouchPhase.Stationary:
								couldBeSwipe = false;				
								break;
				
						case TouchPhase.Ended:
				
								var swipeTime = Time.time - startTime;
								if (couldBeSwipe && (swipeTime < maxSwipeTime)) {
										Jumping = Mathf.Sign (touch.position.y - startPos.y) == 1;
										if (Jumping) {
												StartCoroutine (Jump ());
										}
								}
								couldBeSwipe = false;
								break;
						}
				}
		}
*/

		public void JumpButtonClicked ()
		{
				StartCoroutine (Jump ());
		}

		public float GetMoveWithInputPosition ()
		{
				if (Input.GetMouseButton (0)) {

						Vector3 wantedPos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f));
						var v = transform.position - wantedPos;

						if (v.x > 0.1) {
								return -0.8f;
						} else if (v.x < -0.1) {
								return 0.8f;
						}

				}
				return 0;
		}

		public float GetMoveWithAccelerometer ()
		{
				float move = 0f;

				var accelerationVector = Vector3.zero;

				accelerationVector.x = -Input.acceleration.y;
				accelerationVector.z = Input.acceleration.x;


		
				if (accelerationVector.sqrMagnitude > 1) {
						accelerationVector.Normalize ();
				}

				accelerationVector *= Time.deltaTime;
				move = accelerationVector.z * 200f;

				if (Mathf.Abs (move) < 0.5f) {
						move = 0;
				}

				return move;

		}

    #endregion

}
