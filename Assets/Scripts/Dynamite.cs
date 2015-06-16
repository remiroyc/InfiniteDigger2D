using UnityEngine;
using System.Collections;

public class Dynamite : MonoBehaviour
{

		private GameManager _gameManager;
		public GroundElement GroundElem;

		void Start ()
		{
				_gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
				StartCoroutine (Bang ());
		}

		void Update ()
		{
	
		}

		IEnumerator Bang ()
		{

				if (GroundElem != null) {
						yield return new WaitForSeconds (1);
						_gameManager.BangRepercution (GroundElem);
						Destroy (this.gameObject);
				}

		}


}
