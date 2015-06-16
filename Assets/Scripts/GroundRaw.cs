using System;
using UnityEngine;
using System.Linq;

public class GroundRaw : MonoBehaviour
{

		public int NbElements;
		public GroundElement[] GroundElements;
		public Vector3 InitialBrickVector;
		public GameObject CoinPrefab, BrickPrefab;

		/*
		private void GenerateBrick (int index, Vector3 rawPosition)
		{
				var elemPosition = new Vector3 (InitialBrickVector.x + (0.75f * index), rawPosition.y, rawPosition.z);
				GameObject newBrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);
				var groundElem = newBrick.AddComponent<GroundElement> ();
				groundElem.Explosions = Explosions;

				groundElem.ExplosionAudio = ExplosionAudio;
				groundElem.TapAudio = TapAudio;
				groundElem.ElementIndex = index;


		groundElem.CurrentGroundType = GroundType.Brick;


				if (UnityEngine.Random.Range (0, 2) == 1) {
						groundElem.CurrentGroundType = GroundType.Brick;
				} else {
						if (UnityEngine.Random.Range (0, 2) == 1) {
								groundElem.CurrentGroundType = GroundType.SolidBrick;
						} else {
								groundElem.CurrentGroundType = GroundType.IndestructibleBrick;
						}
				}

				groundElem.Sprites = BrickSprites;
				GroundElements [index] = groundElem;
				newBrick.transform.parent = this.transform;

		}
*/

		/*
		private void GenerateObject (int index, Vector3 raw)
		{

				// Si le nombre de brique ajoutée est >= au nombre de brique calculée
				if (GroundElements.Count (g => g != null
						&& g.CurrentGroundType == GroundType.SolidBrick
						&& g.CurrentGroundType == GroundType.Brick
						&& g.CurrentGroundType == GroundType.IndestructibleBrick) >= _nbGroundElemMax) {

						// Case vide

				} else {

						if (UnityEngine.Random.value <= 0.5f) {

								GenerateBrick (index, raw);

						} else {
								if (GroundElements.Count (g => g != null
										&& g.CurrentGroundType != GroundType.SolidBrick
										&& g.CurrentGroundType != GroundType.Brick
										&& g.CurrentGroundType != GroundType.IndestructibleBrick) >= NbElements - _nbGroundElemMax) {

										GenerateBrick (index, raw);

								} else {

					var elemPosition = new Vector3 (InitialBrickVector.x + (0.75f * index), raw.y, raw.z);
					GameObject go = new GameObject();
					go.transform.parent = this.transform;
					var groundElem = go.AddComponent<GroundElement>() as GroundElement;
					groundElem.IsEmpty = true;

					Debug.Log(groundElem);

					GroundElements [index] =  groundElem;

								}
						}

				}

		}
*/


		public void GenerateGroundElements (char[] elems)
		{

				GroundElements = new GroundElement[NbElements];
				for (int i = 0; i < NbElements; i++) {

						if (elems.ElementAtOrDefault (i) != null) {

								char charElem = 'A';


								if (NbElements <= elems.Length) {
										charElem = elems [i];
								}

								if (!char.IsWhiteSpace (charElem)) {
				
				
										Vector3 elemPosition = new Vector3 (InitialBrickVector.x + (0.75f * i), transform.position.y, 0);

										switch (charElem) {
										case 'A': 

												GameObject ABrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);



												GroundElement AGroundElem = ABrick.GetComponent<GroundElement> ();
												AGroundElem.ElementIndex = i;
												GroundElements [i] = AGroundElem;
												AGroundElem.CurrentGroundType = GroundType.Brick;
												ABrick.transform.parent = this.transform;

												break;

										case 'B':

												GameObject BBrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);


												GroundElement BGroundElem = BBrick.GetComponent<GroundElement> ();
												BGroundElem.ElementIndex = i;
												GroundElements [i] = BGroundElem;
												BGroundElem.CurrentGroundType = GroundType.SolidBrick;
												BBrick.transform.parent = this.transform;

												break;


										case 'C':
						
												GameObject CBrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);
												GroundElement CGroundElem = CBrick.GetComponent<GroundElement> ();
												CGroundElem.ElementIndex = i;
												GroundElements [i] = CGroundElem;
												CGroundElem.CurrentGroundType = GroundType.IndestructibleBrick;
												CBrick.transform.parent = this.transform;
						
												break;


										case 'D':
						
												GameObject DBrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);
												GroundElement DGroundElem = DBrick.GetComponent<GroundElement> ();
												DGroundElem.ElementIndex = i;
												GroundElements [i] = DGroundElem;
												DGroundElem.CurrentGroundType = GroundType.Nitro;
												DBrick.transform.parent = this.transform;
						
												break;

										default:
												break;

										}


								}
						}
				}
		}

		public void GenerateGroundElements ()
		{
				GroundElements = new GroundElement[NbElements];
				for (int i = 0; i < NbElements; i++) {
						
						Vector3 elemPosition = new Vector3 (InitialBrickVector.x + (0.75f * i), transform.position.y, 0);
						GenerateRandomObject (i, elemPosition);
				}
		}

		private void GenerateRandomObject (int index, Vector3 elemPosition)
		{
				var randomValue = UnityEngine.Random.value;
				if (randomValue <= 0.1f) {

						GameObject coinGameObject = (GameObject)Instantiate (CoinPrefab, elemPosition, Quaternion.identity);
						coinGameObject.transform.parent = this.transform;

				} else if (randomValue >= 0.4f) {

						GenerateRandomBrick (index, elemPosition);

				} 
				/*
		else {
		
						
			GameObject go = new GameObject();
			go.transform.position = elemPosition;
			go.transform.parent = this.transform;
			var groundElem = go.AddComponent<GroundElement>() as GroundElement;
			groundElem.IsEmpty = true;
			GroundElements [index] =  groundElem;

				}*/


		}

		private void GenerateRandomBrick (int index, Vector3 elemPosition)
		{
				GameObject newBrick = (GameObject)Instantiate (BrickPrefab, elemPosition, Quaternion.identity);
				newBrick.transform.parent = this.transform;

				var groundElem = newBrick.GetComponent<GroundElement> ();
				groundElem.ElementIndex = index;
				

				var random = UnityEngine.Random.value;

				if (random < 0.5) {
						groundElem.CurrentGroundType = GroundType.Brick;
				} else if (random < 0.85) {
						groundElem.CurrentGroundType = GroundType.SolidBrick;
				} else if (random < 0.95) {
						groundElem.CurrentGroundType = GroundType.IndestructibleBrick;
				} else {
						groundElem.CurrentGroundType = GroundType.Nitro;
				}

				GroundElements [index] = groundElem;

		}
}

