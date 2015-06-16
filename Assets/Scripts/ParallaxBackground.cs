using UnityEngine;
using System.Collections;

public class ParallaxBackground : MonoBehaviour {

	public float InitialSpeed;

	void Start () {
	
	}

	void Update () {

		foreach (Transform child in transform) {


			float coef = 1;
			switch(child.tag){
			case "parallax1":
				coef = 1;
				break;
			case "parallax2":
				coef = 3f;
					break;
			default:
				coef = 1;
				break;

			}

			child.position += new Vector3(0, InitialSpeed * coef, 0);

					if(child.position.y >= 15){
						Destroy(child.gameObject);
					}

	}

	}
}
