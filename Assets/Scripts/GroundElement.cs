using System;
using System.Collections;
using UnityEngine;

public enum GroundType
{
		Undefined,
		Brick,
		SolidBrick,
		IndestructibleBrick,
		Nitro
}

public class GroundElement : MonoBehaviour
{

		public int ElementIndex;
		public GroundType CurrentGroundType = GroundType.Undefined;
		public int Life = 0;
		public Sprite[] Sprites;
		public GameObject ExplosionPrefab;
		private SpriteRenderer _spriteManager;
		private static GameObject _mortalExplosionPrefab;

		// Teinte de base (palette de profondeur, figée à la création) et surbrillance de ciblage
		private Color _baseTint = Color.white;
		private bool _highlighted;
		private static readonly Color HighlightColor = new Color (1f, 0.45f, 0.4f);
		private static readonly Color NitroPulseColor = new Color (1f, 0.75f, 0.35f);

    #region MONO BEHAVIOUR METHODS

		void Awake ()
		{
				_spriteManager = this.GetComponent<SpriteRenderer> ();
				_baseTint = Atmosphere.BrickTint;
		}

		void Start ()
		{
				switch (CurrentGroundType) {
				case GroundType.Brick:
						Life = 1;
						_spriteManager.sprite = Sprites [0];
						break;
				case GroundType.SolidBrick:
						Life = 2;
						_spriteManager.sprite = Sprites [1];
						break;
				case GroundType.Nitro:
						Life = 1;
						_spriteManager.sprite = Sprites [4];
						StartCoroutine (NitroPulse ());
						break;
				case GroundType.IndestructibleBrick:
						_spriteManager.sprite = Sprites [2];
						break;
				}
				RefreshColor ();
		}

    #endregion

		/// <summary>Surbrillance de la brique visée par le mineur. Les indestructibles ne s'allument pas.</summary>
		public void SetHighlight (bool on)
		{
				_highlighted = on && CurrentGroundType != GroundType.IndestructibleBrick;
				RefreshColor ();
		}

		private void RefreshColor ()
		{
				if (_spriteManager != null) {
						_spriteManager.color = _highlighted ? HighlightColor : _baseTint;
				}
		}

		// La nitro respire en orange pour prévenir du danger
		private IEnumerator NitroPulse ()
		{
				float seed = UnityEngine.Random.value * 10f;
				while (_spriteManager != null) {
						if (!_highlighted) {
								float k = 0.5f + 0.5f * Mathf.Sin ((Time.time + seed) * 5f);
								_spriteManager.color = Color.Lerp (_baseTint, NitroPulseColor, k * 0.7f);
						}
						yield return null;
				}
		}

		void UpdateSprite (int? spriteSelection = null)
		{
				if (Sprites != null && _spriteManager != null) {
						if (spriteSelection != null) {
								_spriteManager.sprite = Sprites [(int)spriteSelection];
						} else {
								if (Life == 1) {
										_spriteManager.sprite = Sprites [3];
								} else if (Life == 2) {
										_spriteManager.sprite = Sprites [1];
								}
						}
				}
		}

		public void Tap ()
		{
				if (CurrentGroundType == GroundType.IndestructibleBrick) {
						return;
				}

				if (CurrentGroundType == GroundType.Nitro) {
						MortalExplosion ();
						return;
				}

				--Life;

				if (Life <= 0) {
						Explosion ();
				} else {
						UpdateSprite ();
				}
		}

		/// <summary>
		/// Impact de la pioche venant de `from` : éclats de la brique et étincelles au point
		/// de contact, flash et sursaut. À appeler avant Tap(), qui peut détruire la brique.
		/// </summary>
		public void Hit (Vector3 from)
		{
				if (_spriteManager == null) {
						return;
				}
				var impact = Vector3.Lerp (transform.position, from, 0.35f);
				int layer = _spriteManager.sortingLayerID;
				int order = _spriteManager.sortingOrder;

				if (CurrentGroundType != GroundType.IndestructibleBrick && _spriteManager.sprite != null) {
						Debris.Burst (impact, _spriteManager.sprite, 4, _baseTint, layer, order + 1, speed: 2.4f, gravity: 1.4f, size: 0.11f, lifetime: 0.45f, spin: 8f);
				}
				Debris.Burst (impact, UiKit.WhiteSprite, 3, new Color (1f, 0.9f, 0.6f), layer, order + 2, speed: 2.2f, gravity: 0.8f, size: 0.05f, lifetime: 0.25f, spin: 0f);

				Tween.Flash (_spriteManager, new Color (1f, 0.96f, 0.85f), _highlighted ? HighlightColor : _baseTint, 0.1f);
				Tween.Punch (transform, 1.12f, 0.12f);
		}

		public void MortalExplosion ()
		{
				if (_mortalExplosionPrefab == null) {
						_mortalExplosionPrefab = Resources.Load ("ExplosionPrefab") as GameObject;
				}
				Instantiate (_mortalExplosionPrefab, this.transform.position, Quaternion.identity);
				Burst ();
				Destroy (this.gameObject);
		}

		public void Explosion ()
		{
				Instantiate (ExplosionPrefab, this.transform.position, Quaternion.identity);
				Burst ();
				Destroy (this.gameObject);
		}

		private void Burst ()
		{
				if (_spriteManager != null && _spriteManager.sprite != null) {
						Debris.BrickBurst (transform.position, _spriteManager.sprite, _baseTint, _spriteManager.sortingLayerID, _spriteManager.sortingOrder + 1);
				}
		}

}
