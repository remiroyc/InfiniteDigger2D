using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	public Transform Target;
	public float Distance;

	void Start () {
	
	}

	void Update () {

		var z = Target.transform.position.z - Distance;
		this.transform.position = new Vector3(this.transform.position.x, Target.transform.position.y, z);

	}
}
