using UnityEngine;
using System.Collections;

public class TopCollector : MonoBehaviour {

	void Awake(){
	}

	void Start () {
		this.transform.position= Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height));	
	}

	void Update () {
	
	}

	void OnTriggerEnter(Collider col) {
		Debug.Log(col);
		if(col.tag == "Monster"){
		Destroy(col.gameObject);
		}
	}

}
