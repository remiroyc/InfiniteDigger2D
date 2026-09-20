using UnityEngine;

/// <summary>Trigger devant le mineur : brique visée pour le coup de pioche latéral.</summary>
public class FaceDetector : MonoBehaviour
{

		public CharacterControllerScript CharController;
		private GameObject _faceElementSelected = null;

		void OnTriggerEnter2D (Collider2D other)
		{
				if (other.CompareTag ("Ground")) {

						if (_faceElementSelected != null && _faceElementSelected != other.gameObject) {
								Highlight (_faceElementSelected, false);
						}

						Highlight (other.gameObject, true);
						_faceElementSelected = other.gameObject;
						CharController.FaceElementTouched = _faceElementSelected;
				}
		}

		void OnTriggerExit2D (Collider2D other)
		{
				if (other.CompareTag ("Ground")) {
						Highlight (other.gameObject, false);
						if (CharController.FaceElementTouched == other.gameObject) {
								CharController.FaceElementTouched = null;
						}
				}
		}

		private static void Highlight (GameObject go, bool on)
		{
				var element = go != null ? go.GetComponent<GroundElement> () : null;
				if (element != null) {
						element.SetHighlight (on);
				}
		}

}
