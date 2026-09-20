using System.Collections;
using UnityEngine;
using System.Linq;

public class CharacterControllerScript : MonoBehaviour
{
		public bool FacingRight = true, Grounded = false, IsActive = true, IsDied = false, Jumping = false, CanAttack;
		public float maxSpeed = 2f;
		public float groundRadius;
		public LayerMask TapLayer, CoinLayer, GUIButtonLayer, PlayerLayer;
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
		public float Move;
		public int NbAttack = 0;
		public GameManager GameManager;
		public LayerMask GroundLayer;
		public Transform GroundChecker1, GroundChecker2, GroundChecker3, GroundChecker4;

		// Points de vie : la nitro et les pierres retirent des PV au lieu de tuer sur le coup
		public int MaxHealth = 3;
		public int NitroDamage = 1;
		public int StoneDamage = 1;
		public float InvulnerabilityDuration = 0.8f;
		public int Health { get; private set; }
		private bool _invulnerable;
		private SpriteRenderer _spriteRenderer;

		// Saut : le linecast des pieds touche encore le sol pendant l'envol et touche les murs
		// latéraux en l'air, d'où des sauts infinis. On exige un vrai contact par en dessous,
		// une vitesse verticale nulle, et un verrou court après chaque saut.
		public float JumpLockout = 0.25f;
		private ContactFilter2D _groundContactFilter;
		private float _jumpLockUntil;

		// Atterrissage : écrasement + poussière quand on retrouve le sol après un vrai temps en l'air
		private bool _wasGrounded;
		private float _airTime;

		// Chute : étirement vertical proportionnel à la vitesse, traînée au-delà de FallTrailSpeed
		public float FallStretchSpeed = 10f;
		public float FallTrailSpeed = 3.5f;
		private Vector3 _baseScale;
		private bool _stretched;
		private TrailRenderer _fallTrail;

		private void ApplyBaseScale (float mx, float my)
		{
				var current = transform.localScale;
				float signX = current.x < 0f ? -1f : 1f;
				transform.localScale = new Vector3 (signX * _baseScale.x * mx, _baseScale.y * my, _baseScale.z);
		}

		private static void SetHighlight (GameObject brick, bool on)
		{
				var element = brick != null ? brick.GetComponent<GroundElement> () : null;
				if (element != null) {
						element.SetHighlight (on);
				}
		}

    #region MONO BEHAVIOUR METHODS

		void OnCollisionEnter2D (Collision2D col)
		{
				if (col.gameObject.CompareTag ("Stone")) {
						Destroy (col.gameObject);
						TakeDamage (StoneDamage);
				}
		}

		void Start ()
		{
				_characterAnimator = this.GetComponent<Animator> ();
				_audioCharacter = this.GetComponent<AudioSource> ();
				_rigidbody = this.GetComponent<Rigidbody2D> ();
				_spriteRenderer = this.GetComponent<SpriteRenderer> ();
				Health = MaxHealth;
				GameManager = FindAnyObjectByType<GameManager> ();

				// Même sémantique que l'ancien OverlapCircleAll (layer + triggers), sans allocation
				_coinFilter = new ContactFilter2D ();
				_coinFilter.SetLayerMask (CoinLayer);
				_coinFilter.useTriggers = true;

				// Contact avec le sol : collider du sol (pas trigger) dont la normale pointe vers le haut
				_groundContactFilter = new ContactFilter2D ();
				_groundContactFilter.SetLayerMask (GroundLayer);
				_groundContactFilter.useTriggers = false;
				_groundContactFilter.SetNormalAngle (80f, 100f);

				var scale = transform.localScale;
				_baseScale = new Vector3 (Mathf.Abs (scale.x), Mathf.Abs (scale.y), scale.z);

				// Traînée de chute rapide, derrière le mineur
				var trailGo = new GameObject ("FallTrail");
				trailGo.transform.SetParent (transform, false);
				_fallTrail = trailGo.AddComponent<TrailRenderer> ();
				_fallTrail.time = 0.18f;
				_fallTrail.startWidth = 0.45f;
				_fallTrail.endWidth = 0.05f;
				_fallTrail.minVertexDistance = 0.05f;
				_fallTrail.numCapVertices = 4;
				_fallTrail.sharedMaterial = new Material (Shader.Find ("Sprites/Default"));
				_fallTrail.startColor = new Color (1f, 1f, 1f, 0.3f);
				_fallTrail.endColor = new Color (1f, 1f, 1f, 0f);
				if (_spriteRenderer != null) {
						_fallTrail.sortingLayerID = _spriteRenderer.sortingLayerID;
						_fallTrail.sortingOrder = _spriteRenderer.sortingOrder - 1;
				}
				_fallTrail.emitting = false;
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
										SetHighlight (newFaceElementTouched, true);
								}
					
								if (FaceElementTouched != null) {
										SetHighlight (FaceElementTouched, false);
								}
								FaceElementTouched = newFaceElementTouched;
						}
				} else if (FaceElementTouched != null) {
						SetHighlight (FaceElementTouched, false);
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
										SetHighlight (newGroundElementTouched, true);
								}

								if (GroundElementTouched != null) {
										SetHighlight (GroundElementTouched, false);
								}
								GroundElementTouched = newGroundElementTouched;
						}
				} else if (GroundElementTouched != null) {
						SetHighlight (GroundElementTouched, false);
				}

				Grounded = GroundElementTouched != null;
				_characterAnimator.SetBool ("Grounded", Grounded);

				// Chute : le mineur s'étire avec la vitesse et laisse une traînée quand ça va vite.
				// Remis à l'échelle de base avant l'écrasement d'atterrissage, qui part de là.
				float verticalSpeed = _rigidbody != null ? _rigidbody.linearVelocity.y : 0f;
				bool falling = !Grounded && verticalSpeed < -1f && _airTime > 0.1f;
				if (falling) {
						float stretch = Mathf.Clamp01 (-verticalSpeed / FallStretchSpeed) * 0.22f;
						ApplyBaseScale (1f - stretch * 0.55f, 1f + stretch);
						_stretched = true;
				} else if (_stretched) {
						ApplyBaseScale (1f, 1f);
						_stretched = false;
				}
				if (_fallTrail != null) {
						_fallTrail.emitting = !Grounded && verticalSpeed < -FallTrailSpeed;
				}

				if (Grounded && !_wasGrounded && _airTime > 0.15f) {
						Tween.Squash (transform, 1.2f, 0.8f, 0.14f);
						if (_spriteRenderer != null) {
								Debris.DustPuff (transform.position + Vector3.down * 0.45f, _spriteRenderer.sortingLayerID, _spriteRenderer.sortingOrder - 1);
						}
				}
				_airTime = Grounded ? 0f : _airTime + Time.deltaTime;
				_wasGrounded = Grounded;

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
	
		private static GameObject _dynamitePrefab;

		/// <summary>
		/// Pose de dynamite : le bâton part de la main quand le bras est tendu, décrit un petit
		/// arc en tournant, s'écrase sur la brique visée, puis la mèche s'allume.
		/// </summary>
		IEnumerator PlantDynamite (GameObject targetBrick)
		{
				yield return new WaitForSeconds (0.25f); // le bras de l'animation Throw est tendu

				if (_dynamitePrefab == null) {
						_dynamitePrefab = Resources.Load<GameObject> ("Dynamite");
				}
				var start = transform.position + new Vector3 (FacingRight ? 0.25f : -0.25f, 0.1f, 0f);
				var end = targetBrick != null
						? targetBrick.transform.position + Vector3.up * 0.45f
						: new Vector3 (transform.position.x, transform.position.y - 0.5f, 0f);
				var dynamite = Instantiate (_dynamitePrefab, start, Quaternion.identity);

				const float flight = 0.3f;
				float spin = FacingRight ? -360f : 360f;
				float elapsed = 0f;
				while (elapsed < flight && dynamite != null) {
						elapsed += Time.deltaTime;
						float k = Mathf.Clamp01 (elapsed / flight);
						dynamite.transform.position = Vector3.Lerp (start, end, k) + Vector3.up * (Mathf.Sin (k * Mathf.PI) * 0.5f);
						dynamite.transform.rotation = Quaternion.Euler (0f, 0f, spin * k);
						yield return null;
				}
				if (dynamite == null) {
						_attacking = false;
						yield break;
				}

				// Atterrissage
				dynamite.transform.position = end;
				dynamite.transform.rotation = Quaternion.identity;
				Tween.Squash (dynamite.transform, 1.25f, 0.75f, 0.12f);
				if (_spriteRenderer != null) {
						Debris.DustPuff (end + Vector3.down * 0.15f, _spriteRenderer.sortingLayerID, _spriteRenderer.sortingOrder);
				}

				var script = dynamite.GetComponent<Dynamite> ();
				if (targetBrick != null) {
						dynamite.transform.SetParent (targetBrick.transform, true);
						script.GroundElem = targetBrick.GetComponent<GroundElement> ();
				} else {
						Debug.LogWarning ("Dynamite posée sans brique cible : elle n'explosera rien");
				}
				script.Arm ();

				Tween.Squash (transform, 1.08f, 0.92f, 0.12f); // léger recul du mineur
				_attacking = false;
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

						ScorePopup.Coin (item.transform.position, 100, _spriteRenderer != null ? _spriteRenderer.sortingLayerID : 0);
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
						StartCoroutine (PlantDynamite (GroundElementTouched)); // cible figée au moment du geste
				}
		}

		private bool CanJump ()
		{
				return Grounded
				&& Time.time >= _jumpLockUntil
				&& Mathf.Abs (_rigidbody.linearVelocity.y) < 0.5f
				&& _rigidbody.IsTouching (_groundContactFilter);
		}

		IEnumerator Jump ()
		{
				if (!CanJump ()) {
						yield break;
				}
				Grounded = false;
				Jumping = true;
				_jumpLockUntil = Time.time + JumpLockout;
				_rigidbody.AddForce (new Vector2 (0, jumpForce));
				Tween.Squash (transform, 0.82f, 1.22f, 0.14f);
				this._characterAnimator.Play ("Jump");
				yield return new WaitForSeconds (1);
				Jumping = false;
		}

		/// <summary>
		/// Retire des PV. Ignoré pendant la fenêtre d'invulnérabilité qui suit un coup,
		/// pour qu'une même pierre ou explosion ne compte pas deux fois.
		/// </summary>
		public void TakeDamage (int amount)
		{
				if (IsDied || _invulnerable || amount <= 0) {
						return;
				}

				Health = Mathf.Max (0, Health - amount);

				if (Health == 0) {
						StartCoroutine (Die ());
				} else {
						StartCoroutine (HitFeedback ());
						if (GameManager != null) {
								GameManager.NotifyDamage (false);
						}
				}
		}

		IEnumerator HitFeedback ()
		{
				_invulnerable = true;

				if (ScreamAudio != null) {
						_audioCharacter.PlayOneShot (ScreamAudio);
				}
				Handheld.Vibrate ();

				// Clignotement rouge pendant l'invulnérabilité
				float elapsed = 0f;
				bool red = false;
				while (elapsed < InvulnerabilityDuration) {
						red = !red;
						if (_spriteRenderer != null) {
								_spriteRenderer.color = red ? new Color (1f, 0.4f, 0.4f) : Color.white;
						}
						yield return new WaitForSeconds (0.1f);
						elapsed += 0.1f;
				}
				if (_spriteRenderer != null) {
						_spriteRenderer.color = Color.white;
				}

				_invulnerable = false;
		}

		IEnumerator Die ()
		{
				GameManager.CalculateFinalScore ();
				GameManager.OnPlayerDying ();
				IsActive = false;
				_characterAnimator.Play ("Dead");

				// Ralenti dramatique pendant l'animation de mort, en temps réel pour ne pas durer 6 s
				Time.timeScale = 0.35f;
				yield return new WaitForSecondsRealtime (1.4f);
				if (Mathf.Approximately (Time.timeScale, 0.35f)) {
						Time.timeScale = 1f;
				}
				IsDied = true;
		}

		private void Flip ()
		{
				if (FaceElementTouched != null) {
						SetHighlight (FaceElementTouched, false);
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

		// Clips miner_attack / miner_attack_crouch : 6 frames à 20 i/s = 0,30 s ; la pioche
		// touche sur la 4e frame. On attend la fin du clip avant d'autoriser le coup suivant,
		// sinon l'animation est relancée à mi-course et paraît saccadée.
		private const float TapImpactTime = 0.15f;
		private const float TapClipLength = 0.30f;

		IEnumerator WaitAndTap (GroundElement elementManager)
		{
				yield return new WaitForSeconds (TapImpactTime);
				if (elementManager != null) {
						bool nitro = elementManager.CurrentGroundType == GroundType.Nitro;
						elementManager.Hit (transform.position);
						elementManager.Tap (); // peut détruire la brique (nitro, dernière vie)
						if (nitro) {
								TakeDamage (NitroDamage); // au moment de l'explosion, pas à la fin du geste
						}
				}
				yield return new WaitForSeconds (TapClipLength - TapImpactTime);
				_attacking = false;
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
