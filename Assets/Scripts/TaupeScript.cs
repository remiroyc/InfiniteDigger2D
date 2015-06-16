using UnityEngine;
using System.Collections;

public class TaupeScript : MonoBehaviour {

	public bool FacingRight = true;
	private bool _attacking = false, _died = false;
	private GameObject _rock;
	public float stoneSpeed = 2f;
	private Animator _animator;
	public LayerMask PlayerMask;
	public Transform Player;
	public float Offset;
	private bool _detectPlayer = false;

	void Awake(){
		_animator = GetComponent<Animator> ();
		_rock = Resources.Load ("StonePrefab") as GameObject;
		Player = GameObject.Find("MinerCharacter").transform;
	}

	void Start () {
		
	}

	void Update () {

		if (!_attacking) {

			_detectPlayer = Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), 
			                                       FacingRight ? Vector2.right : -Vector2.right, 10, PlayerMask);
		}

	}

	void FixedUpdate(){

		var yTopPosition = Camera.main.transform.position.y + Offset;
		if (transform.position.y >= yTopPosition) {
			Destroy (this.gameObject);
			return;
		}

		var dir = transform.position - Player.position;
		if (dir.x < 0 && !FacingRight) {
			Flip ();
		}else if(dir.x > 0 && FacingRight){
			Flip();
		}

		if (_detectPlayer && !_attacking) {
			if (Mathf.Abs(dir.x) > 1) {
				StartCoroutine (ThrowRock ());
			}
		}
	}

	IEnumerator ThrowRock ()
	{
		if (_rock != null) {

			_attacking = true;
			_animator.Play ("Appear");
			
			yield return new WaitForSeconds (0.5f);

			_animator.Play ("Throw");

			yield return new WaitForSeconds(0.7f);

			if (!_died) {
				
				var offset = FacingRight ? new Vector3 (0.5f, 0, 0) : new Vector3 (-0.5f, 0, 0);
				GameObject stone = Instantiate (_rock, this.transform.position + offset, Quaternion.identity) as GameObject;
				
				if (FacingRight) {
					stone.GetComponent<Rigidbody2D>().velocity = new Vector2 (stoneSpeed, 0);
				} else {
					stone.GetComponent<Rigidbody2D>().velocity = new Vector2 (-stoneSpeed, 0);
				}

				yield return new WaitForSeconds (1.5f);
				_animator.Play ("Desappear");

			}

		}
	}

	void DestroyTaupe(){
		Destroy(this.gameObject);
	}

	public void Flip ()
	{
		FacingRight = !FacingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

}
