using UnityEngine;
using System.Collections;

public class EndCameraZone : MonoBehaviour {
	
	void OnTriggerEnter(Collider other) {
		if(other.tag == "MainCamera"){
			Debug.Break();
		}
	}

}
