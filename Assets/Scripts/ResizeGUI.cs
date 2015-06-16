using UnityEngine;
using System.Collections;

public class ResizeGUI : MonoBehaviour
{

		private GUITexture myGUITexture;
	
		void Awake ()
		{
				myGUITexture = this.gameObject.GetComponent<GUITexture> ();
		}

		void Start ()
		{
				// Position the billboard in the center, 
				// but respect the picture aspect ratio
				int textureHeight = GetComponent<GUITexture>().texture.height;
				int textureWidth = GetComponent<GUITexture>().texture.width;
				int screenHeight = Screen.height;
				int screenWidth = Screen.width;
		
				int screenAspectRatio = (screenWidth / screenHeight);
				int textureAspectRatio = (textureWidth / textureHeight);
		
				int scaledHeight;
				int scaledWidth;
				if (textureAspectRatio <= screenAspectRatio) {
						// The scaled size is based on the height
						scaledHeight = screenHeight;
						scaledWidth = (screenHeight * textureAspectRatio);
				} else {
						// The scaled size is based on the width
						scaledWidth = screenWidth;
						scaledHeight = (scaledWidth / textureAspectRatio);
				}
				float xPosition = screenWidth / 2 - (scaledWidth / 2);
				myGUITexture.pixelInset = 
			new Rect (xPosition, Screen.height - scaledHeight, scaledWidth, scaledHeight);
		}
}
