using UnityEngine;
using System.Collections;

public class GUIMenuScript : MonoBehaviour
{

		private float guiRatio;
		private float sWidth;
		private Vector3 GUIsF;
		public GUISkin Skin;

		void Awake ()
		{

		}

		void Start ()
		{

		}

		void Update ()
		{
		sWidth = Screen.width;  
		guiRatio = sWidth / 320; 
		Debug.Log(sWidth + " " + guiRatio);
		GUIsF = new Vector3 (guiRatio, guiRatio, 1);  
		}

		void OnGUI ()
		{
				GUI.skin = Skin;
				GUI.matrix = Matrix4x4.TRS(new Vector3(GUIsF.x,GUIsF.y,0), Quaternion.identity,GUIsF);  

				int buttonWidth = 50;
				int buttonHeight = 25;
				GUI.Button (new Rect (
							Screen.width / 2 - (buttonWidth / 2), 
		                    Screen.height - (buttonHeight * 2), 
		                    buttonWidth, buttonHeight), "Play");
		}
}
