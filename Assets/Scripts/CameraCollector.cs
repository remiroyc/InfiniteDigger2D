using UnityEngine;
using System.Collections;

public class CameraCollector : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D col){
		if(col.transform.CompareTag ("Stone")){
			Destroy(col.transform.gameObject);
		}
	}

}
