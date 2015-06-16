using UnityEngine;
using System.Collections;

public class TapControllerScript : MonoBehaviour {

	private bool coroutine;

	void Start () {
	
	}

	void Update () {
	
	}

	void FixedUpdate(){

		if(!coroutine){
			StartCoroutine(StartAnim());
		}

	}

	IEnumerator StartAnim(){
		coroutine = true;
		this.GetComponent<Animator>().Play("touch");
		yield return new WaitForSeconds(2f);
		coroutine = false;
	}


}
