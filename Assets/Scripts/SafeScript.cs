using UnityEngine;
using System.Collections;

public class SafeScript : MonoBehaviour {

	private bool _touched = false;

	void OnTriggerEnter2D(Collider2D other) {

		if(other.tag == "Player" && !_touched){
			_touched = true;
			this.GetComponent<Animator>().Play("Open");
			StartCoroutine(Win ());
		}
	}

	IEnumerator Win(){
		yield return new WaitForSeconds(1.5f);
		FindObjectOfType<LevelManager>().Win();
	}


}
