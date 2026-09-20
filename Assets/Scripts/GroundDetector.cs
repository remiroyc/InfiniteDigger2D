using UnityEngine;

/// <summary>Trigger sous le mineur : brique visée pour le coup de pioche vers le bas.</summary>
public class GroundDetector : MonoBehaviour
{

		public CharacterControllerScript CharController;
		private GameObject _groundElementSelected = null;

		void OnTriggerEnter2D (Collider2D other)
		{
				if (other.CompareTag ("Ground")) {

						if (_groundElementSelected != null && _groundElementSelected != other.gameObject) {
								Highlight (_groundElementSelected, false);
						}

						Highlight (other.gameObject, true);
						_groundElementSelected = other.gameObject;
						CharController.GroundElementTouched = _groundElementSelected;
				}
		}

		void OnTriggerExit2D (Collider2D other)
		{
				if (other.CompareTag ("Ground")) {
						Highlight (other.gameObject, false);

						if (CharController.Grounded) {
								CharController.Grounded = false;
								if (CharController.GroundElementTouched == other.gameObject) {
										CharController.GroundElementTouched = null;
								}
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
