using UnityEngine;
using System.Collections;

public class FaceDetector : MonoBehaviour
{

		public CharacterControllerScript CharController;
		private GameObject _faceElementSelected = null;

		void OnTriggerEnter2D (Collider2D other)
		{
				if (other.transform.tag == "Ground") {

						if (_faceElementSelected != null && _faceElementSelected != other.transform.gameObject) {
								_faceElementSelected.transform.GetComponent<SpriteRenderer> ().color = Color.white;
						}

						GroundElement ge = other.transform.GetComponent<GroundElement> ();
						if (ge.CurrentGroundType != GroundType.IndestructibleBrick) {
								other.transform.GetComponent<SpriteRenderer> ().color = Color.red;
						}
						_faceElementSelected = other.transform.gameObject;
						CharController.FaceElementTouched = _faceElementSelected;

				}
		}
	
		void OnTriggerExit2D (Collider2D other)
		{
				if (other.transform.tag == "Ground") {
						other.transform.GetComponent<SpriteRenderer> ().color = Color.white;
						if (CharController.FaceElementTouched == other.transform.gameObject) {
								CharController.FaceElementTouched = null;
						}
				}
		}

}
