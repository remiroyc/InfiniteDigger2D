using UnityEngine;
using System.Collections;

public class RatScript : MonoBehaviour
{

		public LayerMask GroundLayer;
		public float MaxYPosition;
		private bool _grounded, _faceCollider, _backCollier, _facingRight = true;
		private Animator _animator;

		void Start ()
		{
				_animator = GetComponent<Animator> ();
		}

		void Update ()
		{
				if (transform.position.y >= MaxYPosition) {
						Destroy (this.gameObject);
						return;
				}
		
				_grounded = Physics2D.Raycast (new Vector2 (transform.position.x + 0.09f, transform.position.y), -Vector2.up, 0.5f, GroundLayer);
				_faceCollider = Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), 
		                                   _facingRight ? Vector2.right : -Vector2.right, 0.5f, GroundLayer);


				if (_faceCollider) {
						_backCollier = Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), 
			                                   _facingRight ? -Vector2.right : Vector2.right, 0.5f, GroundLayer);
				}

		}

		void FixedUpdate ()
		{

				if (_faceCollider && !_backCollier) {
						Flip ();
						return;
				} else if (!_faceCollider) {

						var move = 0f;

						if (_grounded) {
								move = 1f;
								if (!_facingRight) {
										move *= -1;
								}
						}

						GetComponent<Rigidbody2D>().velocity = (new Vector2 (move, GetComponent<Rigidbody2D>().velocity.y));
						_animator.SetFloat ("MoveSpeed", Mathf.Abs (move));
				} else {
						GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
						_animator.SetFloat ("MoveSpeed", 0);
				}
		}

		public void Flip ()
		{
				_facingRight = !_facingRight;
				Vector3 theScale = transform.localScale;
				theScale.x *= -1;
				transform.localScale = theScale;
		}

}
