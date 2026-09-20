using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ambiance de la mine : seul le cercle éclairé par la lampe frontale est visible. Un grand
/// voile noir suit le mineur, percé d'un trou radial ; un halo chaud additif colore
/// l'intérieur. Avec la profondeur, le noir devient total et la palette des briques glisse
/// de la terre chaude vers la roche bleutée (les briques figent leur teinte à la création,
/// la transition se fait ligne par ligne). Textures générées au lancement.
/// </summary>
public class Atmosphere : MonoBehaviour
{
		/// <summary>Teinte courante des briques selon la profondeur (blanc sans Atmosphere).</summary>
		public static Color BrickTint = Color.white;

		// Rayon éclairé autour du mineur, en unités monde (la caméra en voit 11 de haut)
		public float LightRadius = 3.2f;
		// Opacité du noir hors du cercle, en surface puis au fond
		private const float FogStart = 0.9f;
		private const float FogEnd = 1f;
		private const int FogDepth = 150;

		// Palette par profondeur : terre chaude -> pierre grise -> roche bleutée
		private static readonly int[] PaletteDepths = { 0, 45, 100, 160 };
		private static readonly Color[] PaletteTints = {
				Color.white,
				new Color (0.86f, 0.86f, 0.9f),
				new Color (0.62f, 0.66f, 0.85f),
				new Color (0.45f, 0.48f, 0.7f)
		};

		// Dans la texture du voile, le trou est transparent jusqu'à HoleInner et opaque à partir
		// de HoleOuter (rayons normalisés, 1 = milieu d'un bord). Le quad est dimensionné pour que
		// HoleOuter corresponde à LightRadius ; il couvre alors largement l'écran quel que soit
		// l'endroit où se trouve le mineur.
		private const float HoleInner = 0.075f;
		private const float HoleOuter = 0.11f;

		private Camera _camera;
		private Transform _miner;
		private SpriteRenderer _fog;
		private SpriteRenderer _lamp;
		private readonly List<SpriteRenderer> _backgrounds = new List<SpriteRenderer> ();
		private int _depth = -1;

		public static Atmosphere Create (Camera camera, Transform miner)
		{
				var go = new GameObject ("Atmosphere");
				var atmosphere = go.AddComponent<Atmosphere> ();
				atmosphere.Build (camera, miner);
				return atmosphere;
		}

		private void Build (Camera camera, Transform miner)
		{
				_camera = camera;
				_miner = miner;
				int hubLayer = SortingLayer.NameToID ("Hub");

				// Voile noir percé, au-dessus du décor et des personnages, sous les éléments Hub (+100, ligne de score)
				var fogGo = new GameObject ("Fog");
				fogGo.transform.SetParent (transform, false);
				_fog = fogGo.AddComponent<SpriteRenderer> ();
				_fog.sprite = RadialSprite (1024, HoleInner, HoleOuter, inner: 0f, outer: 1f, TextureFormat.Alpha8);
				_fog.color = new Color (0f, 0f, 0f, FogStart);
				_fog.sortingLayerID = hubLayer;
				_fog.sortingOrder = -10;
				float halfSize = _fog.sprite.bounds.extents.x;                 // 5,12 unités pour 1024 px à 100 ppu
				float scale = LightRadius / (HoleOuter * halfSize);
				fogGo.transform.localScale = new Vector3 (scale, scale, 1f);

				// Lampe frontale : halo chaud additif, sous le voile pour ne colorer que le cercle visible
				if (miner != null) {
						var lampGo = new GameObject ("HeadLamp");
						lampGo.transform.SetParent (miner, false);
						lampGo.transform.localPosition = new Vector3 (0f, 0.35f, 0f);
						_lamp = lampGo.AddComponent<SpriteRenderer> ();
						_lamp.sprite = RadialSprite (256, 0.0f, 0.95f, inner: 1f, outer: 0f, TextureFormat.RGBA32);
						_lamp.color = new Color (1f, 0.86f, 0.55f, 0.75f);
						_lamp.sortingLayerID = hubLayer;
						_lamp.sortingOrder = -11;
						float lampScale = LightRadius * 1.1f / _lamp.sprite.bounds.extents.x;
						lampGo.transform.localScale = Vector3.one * lampScale;
						var additive = Shader.Find ("Legacy Shaders/Particles/Additive");
						if (additive != null) {
								_lamp.sharedMaterial = new Material (additive);
						}
						Tween.Flicker (_lamp, 0.6f, 0.85f, 6f);
				}

				// Fonds du parallaxe : teintés avec la profondeur
				foreach (var parallax in FindObjectsByType<ParallaxBackground> (FindObjectsSortMode.None)) {
						_backgrounds.AddRange (parallax.GetComponentsInChildren<SpriteRenderer> ());
				}

				FollowMiner ();
				SetDepth (0);
		}

		void LateUpdate ()
		{
				FollowMiner ();
		}

		// Le voile suit le mineur ; s'il n'existe plus, il reste centré sur la caméra
		private void FollowMiner ()
		{
				if (_fog == null) {
						return;
				}
				var anchor = _miner != null ? _miner.position : (_camera != null ? _camera.transform.position : Vector3.zero);
				_fog.transform.position = new Vector3 (anchor.x, anchor.y + (_miner != null ? 0.35f : 0f), 0f);
		}

		/// <summary>À appeler quand la profondeur (en mètres) change.</summary>
		public void SetDepth (int meters)
		{
				if (meters == _depth) {
						return;
				}
				_depth = meters;

				float k = Mathf.Clamp01 ((float)meters / FogDepth);
				if (_fog != null) {
						_fog.color = new Color (0f, 0f, 0f, Mathf.Lerp (FogStart, FogEnd, k));
				}

				BrickTint = TintAt (meters);
				var backgroundTint = Color.Lerp (BrickTint, Color.black, k * 0.35f);
				foreach (var background in _backgrounds) {
						if (background != null) {
								background.color = backgroundTint;
						}
				}
		}

		/// <summary>Éclair : le voile s'ouvre d'un coup (intensity = part de noir retirée) puis se referme.</summary>
		public void Flash (float intensity, float duration)
		{
				if (_fog != null) {
						StartCoroutine (FlashRoutine (Mathf.Clamp01 (intensity), duration));
				}
		}

		private System.Collections.IEnumerator FlashRoutine (float intensity, float duration)
		{
				float target = Mathf.Lerp (FogStart, FogEnd, Mathf.Clamp01 ((float)Mathf.Max (_depth, 0) / FogDepth));
				float elapsed = 0f;
				while (elapsed < duration && _fog != null) {
						elapsed += Time.deltaTime;
						float k = Mathf.Clamp01 (elapsed / duration);
						float alpha = Mathf.Lerp (target * (1f - intensity), target, k * k);
						_fog.color = new Color (0f, 0f, 0f, alpha);
						yield return null;
				}
				if (_fog != null) {
						_fog.color = new Color (0f, 0f, 0f, target);
				}
		}

		private static Color TintAt (int meters)
		{
				for (int i = 1; i < PaletteDepths.Length; i++) {
						if (meters <= PaletteDepths [i]) {
								float k = Mathf.InverseLerp (PaletteDepths [i - 1], PaletteDepths [i], meters);
								return Color.Lerp (PaletteTints [i - 1], PaletteTints [i], k);
						}
				}
				return PaletteTints [PaletteTints.Length - 1];
		}

		/// <summary>
		/// Sprite radial : alpha = inner au centre, outer au-delà de r1, transition entre r0 et r1.
		/// Rayons normalisés : 1 = milieu d'un bord, 1,41 = coin (donc jamais de carré visible).
		/// Alpha8 suffit pour un voile noir (la couleur du renderer fournit le RGB).
		/// </summary>
		private static Sprite RadialSprite (int size, float r0, float r1, float inner, float outer, TextureFormat format)
		{
				var texture = new Texture2D (size, size, format, false);
				texture.wrapMode = TextureWrapMode.Clamp;
				texture.filterMode = FilterMode.Bilinear;
				var pixels = new Color32[size * size];
				float half = size * 0.5f;
				for (int y = 0; y < size; y++) {
						for (int x = 0; x < size; x++) {
								float dx = (x + 0.5f - half) / half;
								float dy = (y + 0.5f - half) / half;
								float r = Mathf.Sqrt (dx * dx + dy * dy);
								float k = Mathf.SmoothStep (0f, 1f, Mathf.InverseLerp (r0, r1, r));
								byte a = (byte)Mathf.RoundToInt (Mathf.Lerp (inner, outer, k) * 255f);
								pixels [y * size + x] = new Color32 (255, 255, 255, a);
						}
				}
				texture.SetPixels32 (pixels);
				texture.Apply (false, true);
				return Sprite.Create (texture, new Rect (0, 0, size, size), new Vector2 (0.5f, 0.5f), 100f);
		}
}
