using UnityEngine;
using System.Collections;

public class CameraCollector : MonoBehaviour {

	void Start () {
	
	}

	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D col){
		if(col.transform.tag == "Stone"){
			Destroy(col.transform.gameObject);
		}
	}

}
