using UnityEngine;
using System.Collections;

public class CoinScript : MonoBehaviour
{

	// Appelé par un événement d'animation
	public void Delete(){
		Destroy(this.gameObject);
	}


}
