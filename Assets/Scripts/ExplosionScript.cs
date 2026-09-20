using UnityEngine;

/// <summary>
/// Effet éphémère (explosion). Détruit par l'événement d'animation Delete quand le clip en a
/// un, sinon par le filet de sécurité MaxLifetime : sans lui, un effet sans événement restait
/// en scène pour toujours (et bouclait si son clip était en boucle).
/// </summary>
public class ExplosionScript : MonoBehaviour
{
		public float MaxLifetime = 2f;

		void Start ()
		{
				Destroy (gameObject, MaxLifetime);
		}

		// Appelé par un événement d'animation en fin de clip
		public void Delete ()
		{
				Destroy (gameObject);
		}
}
