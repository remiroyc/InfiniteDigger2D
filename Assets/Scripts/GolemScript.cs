using UnityEngine;
using System.Collections;

public class GolemScript : MonoBehaviour
{
		public float MaxYPosition;
		public bool facingRight = true;
		public LayerMask GroundLayer, FlipLayer, PlayerLayer;
		private Transform _aGroundChecker, _bGroundChecker;
		// private Animator _animator;

		private bool grounded, faceCollider;
		private Collider2D nextGround;
		private Animator _animator;
		private bool _attacking = false, _died = false;
		private GameObject _rock;
		public float stoneSpeed = 1;

		void Awake ()
		{
				_animator = GetComponent<Animator> ();

				_aGroundChecker = transform.FindChild ("AGroundChecker");
				_bGroundChecker = transform.FindChild ("BGroundChecker");

				_rock = Resources.Load ("StonePrefab") as GameObject;
		}

		void Start ()
		{
		}

		void Update ()
		{

				if (transform.position.y >= MaxYPosition) {
						Destroy (this.gameObject);
						return;
				}

				if (_died) {
						if (_attacking) {
								StopCoroutine ("ThrowRock");
						}
						return;
				}

				grounded = Physics2D.Linecast (_aGroundChecker.position, _bGroundChecker.position, GroundLayer);
				
				if (grounded && !_attacking) {
						var detectPlayer = Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), 
		                                     facingRight ? Vector2.right : -Vector2.right, 10, PlayerLayer);
						
						// nextGround = Physics2D.OverlapPoint (new Vector2 (_groundChecker.position.x, _groundChecker.position.y), GroundLayer);
						

						if (detectPlayer) {
						
								StartCoroutine (ThrowRock ());
								

						} else {

								faceCollider = Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), 
				                                  facingRight ? Vector2.right : -Vector2.right, 0.5f, FlipLayer);

								// var nextBackGround = Physics2D.OverlapPoint (new Vector2 (-_groundChecker.position.x, _groundChecker.position.y), GroundLayer);

								if (faceCollider) {
										Flip ();
										return;
								}
				

								//if (nextGround != null) {
					
								var move = 0.01f;
					
								if (!facingRight) {
										move *= -1;
								}

								GetComponent<Rigidbody2D>().velocity = (new Vector2 (move * 100, GetComponent<Rigidbody2D>().velocity.y));
								_animator.SetFloat ("MoveSpeed", Mathf.Abs (move));
								/*
				} else if(nextBackGround != null) {
										Flip ();
								}
								*/

						}
						
				} else {
						_animator.SetFloat ("MoveSpeed", 0);
				}
		}

		IEnumerator ThrowRock ()
		{
				if (_rock != null) {
						_attacking = true;
						_animator.SetFloat ("MoveSpeed", 0);
						_animator.Play ("Throw");

						yield return new WaitForSeconds (1f);

						if (!_died) {

								var offset = facingRight ? new Vector3 (0.5f, 0, 0) : new Vector3 (-0.5f, 0, 0);
								GameObject stone = Instantiate (_rock, this.transform.position + offset, Quaternion.identity) as GameObject;

								if (facingRight) {
										stone.GetComponent<Rigidbody2D>().velocity = new Vector2 (stoneSpeed, 0);
								} else {
										stone.GetComponent<Rigidbody2D>().velocity = new Vector2 (-stoneSpeed, 0);
								}
								yield return new WaitForSeconds (3);
						}
						_attacking = false;

				}
		}

		public void Flip ()
		{
				facingRight = !facingRight;
				Vector3 theScale = transform.localScale;
				theScale.x *= -1;
				transform.localScale = theScale;
		}

		public void TakeDamage ()
		{
				StartCoroutine (Die ());
		}

		IEnumerator Die ()
		{
				_died = true;
				_animator.Play ("Dead");
				yield return new WaitForSeconds (3);
				Destroy (this.gameObject);
		}


}
