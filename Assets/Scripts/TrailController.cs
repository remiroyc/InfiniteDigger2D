using UnityEngine;
using System.Collections;

public class TrailController : MonoBehaviour
{

	#region Members

		public GameObject trailPrefab;
		private GameObject _trailRef;
		bool buttonDown;

	#endregion

	#region Functions

		void Start ()
		{
				buttonDown = false;
				_trailRef = Instantiate (trailPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				_trailRef.GetComponent<TrailRenderer> ().sortingLayerName = "Hub";
		}

		void Update ()
		{
				if (Input.GetMouseButtonDown (0)) {
						buttonDown = true;
				}

				if (Input.GetMouseButtonUp (0)) {
						buttonDown = false;
				}
		}
	
		void FixedUpdate ()
		{

				if (buttonDown) {
						Vector3 newPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
						_trailRef.transform.position = new Vector3 (newPos.x, newPos.y, 0);
				}
		}
	
	#endregion
	
	
	
}