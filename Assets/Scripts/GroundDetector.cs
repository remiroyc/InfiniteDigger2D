using UnityEngine;
using System.Collections;

public class GroundDetector : MonoBehaviour
{

		public CharacterControllerScript CharController;
		private GameObject _groundElementSelected = null;

		void OnTriggerEnter2D (Collider2D other)
		{
				if (other.transform.tag == "Ground") {

						if (_groundElementSelected != null && _groundElementSelected != other.transform.gameObject) {
								_groundElementSelected.transform.GetComponent<SpriteRenderer> ().color = Color.white;
						}

						GroundElement ge = other.transform.GetComponent<GroundElement> ();
						if (ge.CurrentGroundType != GroundType.IndestructibleBrick) {
								other.transform.GetComponent<SpriteRenderer> ().color = Color.red;
						}
			_groundElementSelected = other.transform.gameObject;
						CharController.GroundElementTouched = _groundElementSelected;

				}
		}

		void OnTriggerExit2D (Collider2D other)
		{
				if (other.transform.tag == "Ground") {
						other.transform.GetComponent<SpriteRenderer> ().color = Color.white;

						if (CharController.Grounded) {
								CharController.Grounded = false;
								if (CharController.GroundElementTouched == other.gameObject) {
										CharController.GroundElementTouched = null;
								}
						}
				}
		}
	
}
