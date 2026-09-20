using UnityEngine;
using System.Collections;

public class ParallaxBackground : MonoBehaviour {

	public float InitialSpeed;

	void Update () {

		foreach (Transform child in transform) {

			// CompareTag évite l'allocation d'une string par enfant et par frame que provoque child.tag
			float coef = child.CompareTag ("parallax2") ? 3f : 1f;

			child.position += new Vector3(0, InitialSpeed * coef, 0);

					if(child.position.y >= 15){
						Destroy(child.gameObject);
					}

	}

	}
}
