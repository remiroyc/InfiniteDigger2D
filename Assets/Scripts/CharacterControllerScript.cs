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
		private Rigidbody2D _rigidbody;
		private ContactFilter2D _coinFilter;
		private readonly Collider2D[] _coinBuffer = new Collider2D[8];
		private GameObject _coinScorePrefab;
		public float Move;
		public int NbAttack = 0;
		public GameManager GameManager;
		public LayerMask GroundLayer;
		public Transform GroundChecker1, GroundChecker2, GroundChecker3, GroundChecker4;

    #region MONO BEHAVIOUR METHODS

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
				_rigidbody = this.GetComponent<Rigidbody2D> ();
				GameManager = FindAnyObjectByType<GameManager> ();

				// Même sémantique que l'ancien OverlapCircleAll (layer + triggers), sans allocation
				_coinFilter = new ContactFilter2D ();
				_coinFilter.SetLayerMask (CoinLayer);
				_coinFilter.useTriggers = true;
				_coinScorePrefab = Resources.Load ("100") as GameObject;
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
						_rigidbody.linearVelocity = new Vector2 (0, _rigidbody.linearVelocity.y); // On n'est soumis qu'à la gravité
				} else {

						if (Move * _rigidbody.linearVelocity.x < maxSpeed) {
								_rigidbody.AddForce (Vector2.right * Move * MoveForce);
						}

						if (Mathf.Abs (_rigidbody.linearVelocity.x) > maxSpeed) {
								_rigidbody.linearVelocity = new Vector2 (Mathf.Sign (_rigidbody.linearVelocity.x) * maxSpeed, _rigidbody.linearVelocity.y);
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

				// Les checkers ne sont câblés que dans la scène test : sans eux, on ne détecte rien
				// plutôt que de lever une UnassignedReferenceException à chaque frame.
				GameObject newFaceElementTouched = null;
				if (GroundChecker3 != null && GroundChecker4 != null) {
						var faceHit = Physics2D.Linecast (GroundChecker3.position, GroundChecker4.position, GroundLayer);
						newFaceElementTouched = (faceHit.transform != null) ? faceHit.transform.gameObject : null;
				}
				
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

				GameObject newGroundElementTouched = null;
				if (GroundChecker1 != null && GroundChecker2 != null) {
						var groundHit = Physics2D.Linecast (GroundChecker1.position, GroundChecker2.position, GroundLayer);
						newGroundElementTouched = (groundHit.transform != null) ? groundHit.transform.gameObject : null;
				}

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

				/*
				if (!Jumping) {
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
				// Appelé à chaque FixedUpdate : pas d'allocation, pas de GameObject.Find.
				int count = Physics2D.OverlapCircle (transform.position, 0.5f, _coinFilter, _coinBuffer);
				if (count == 0) {
						return;
				}

				for (int i = 0; i < count; i++) {
						var item = _coinBuffer [i];
						_coinBuffer [i] = null;
						if (item == null) {
								continue;
						}

						// PlayOneShot sur la source du personnage : l'ancien code écrasait le clip de
						// l'AudioSource du GameManager, c'est-à-dire la musique, dès la première pièce.
						if (CoinAudio != null) {
								_audioCharacter.PlayOneShot (CoinAudio);
						}

						++Coins;

						if (_coinScorePrefab != null) {
								Instantiate (_coinScorePrefab, item.transform.position, Quaternion.identity);
						}
						Destroy (item.gameObject);
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
						_rigidbody.AddForce (new Vector2 (0, jumpForce));
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
										_rigidbody.linearVelocity = new Vector2 (0f, _rigidbody.linearVelocity.y);

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
