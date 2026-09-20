using System.Collections;
using UnityEngine;

/// <summary>
/// Bâton de dynamite posé par le mineur. Inerte tant qu'Arm() n'est pas appelé (le temps du
/// lancer), puis la mèche brûle : pulsation et clignotement qui s'accélèrent, étincelles au
/// bout de la mèche, et détonation via GameManager.BangRepercution.
/// </summary>
public class Dynamite : MonoBehaviour
{
		public GroundElement GroundElem;
		public float FuseTime = 1f;

		private static readonly Color FuseColor = new Color (1f, 0.45f, 0.35f);
		private static readonly Color SparkColor = new Color (1f, 0.78f, 0.3f);

		private GameManager _gameManager;
		private SpriteRenderer _renderer;
		private bool _armed;

		void Awake ()
		{
				_renderer = GetComponent<SpriteRenderer> ();
				_gameManager = FindAnyObjectByType<GameManager> ();
		}

		void Start ()
		{
				// Filet de sécurité : une dynamite jamais armée explose quand même
				Invoke ("Arm", 2f);
		}

		/// <summary>Allume la mèche. Appelé par le mineur une fois la dynamite posée.</summary>
		public void Arm ()
		{
				if (_armed) {
						return;
				}
				_armed = true;
				CancelInvoke ("Arm");
				StartCoroutine (Fuse ());
		}

		private IEnumerator Fuse ()
		{
				var baseScale = transform.localScale;
				float elapsed = 0f;
				float nextSpark = 0f;

				while (elapsed < FuseTime) {
						elapsed += Time.deltaTime;
						float k = Mathf.Clamp01 (elapsed / FuseTime);
						float frequency = Mathf.Lerp (6f, 24f, k * k);
						float wave = Mathf.Sin (elapsed * frequency);

						transform.localScale = baseScale * (1f + 0.1f * Mathf.Max (0f, wave) * (0.5f + k));
						if (_renderer != null) {
								_renderer.color = Color.Lerp (Color.white, FuseColor, wave > 0f ? k : 0f);
						}

						if (elapsed >= nextSpark) {
								nextSpark = elapsed + Mathf.Lerp (0.12f, 0.05f, k);
								Spark ();
						}
						yield return null;
				}

				Detonate ();
		}

		private void Spark ()
		{
				var tip = _renderer != null ? new Vector3 (transform.position.x, _renderer.bounds.max.y, 0f) : transform.position;
				int layer = _renderer != null ? _renderer.sortingLayerID : 0;
				int order = _renderer != null ? _renderer.sortingOrder + 1 : 1;
				Debris.Burst (tip, UiKit.WhiteSprite, 2, SparkColor, layer, order, speed: 1.3f, gravity: 0.6f, size: 0.06f, lifetime: 0.3f, spin: 0f);
		}

		private void Detonate ()
		{
				if (_gameManager != null && GroundElem != null) {
						_gameManager.BangRepercution (GroundElem);
				}
				Debris.Burst (transform.position, UiKit.WhiteSprite, 14, SparkColor, SortingLayer.NameToID ("Hub"), 2, speed: 4f, gravity: 1f, size: 0.1f, lifetime: 0.5f, spin: 0f);
				Destroy (gameObject);
		}
}
