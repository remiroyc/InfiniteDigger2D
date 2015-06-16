using UnityEngine;
using System.Collections;

public class ScoreLineScript : MonoBehaviour
{

		public GUIStyle style;

		void Start ()
		{
		}

		void OnGUI ()
		{
				// GUI.matrix = _matrix;
				var screenPos = Camera.main.WorldToScreenPoint (this.transform.position);
				GUI.Label (new Rect (screenPos.x, Screen.height - screenPos.y, 150, 150), LocalizationStrings.Instance.Values["LastScore"], style);
		}

}
