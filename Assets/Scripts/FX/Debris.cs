using UnityEngine;

/// <summary>
/// Éclats de particules construits à la volée : débris de brique (avec le sprite de la
/// brique elle-même), poussière d'atterrissage. Aucun asset à créer.
/// </summary>
public static class Debris
{
		private static Material _material;

		private static Material SpriteMaterial {
				get {
						if (_material == null) {
								_material = new Material (Shader.Find ("Sprites/Default"));
						}
						return _material;
				}
		}

		/// <summary>Fragments d'une brique qui casse : morceaux du sprite, gravité, rotation.</summary>
		public static void BrickBurst (Vector3 position, Sprite sprite, Color tint, int sortingLayerID, int sortingOrder)
		{
				Burst (position, sprite, 8, tint, sortingLayerID, sortingOrder, speed: 3.2f, gravity: 1.6f, size: 0.2f, lifetime: 0.7f, spin: 8f);
		}

		/// <summary>Nuage de poussière qui s'élève doucement (atterrissage).</summary>
		public static void DustPuff (Vector3 position, int sortingLayerID, int sortingOrder)
		{
				Burst (position, UiKit.WhiteSprite, 6, new Color (0.75f, 0.65f, 0.5f, 0.55f), sortingLayerID, sortingOrder, speed: 1.1f, gravity: -0.08f, size: 0.13f, lifetime: 0.45f, spin: 0f);
		}

		public static void Burst (Vector3 position, Sprite sprite, int count, Color tint, int sortingLayerID, int sortingOrder, float speed, float gravity, float size, float lifetime, float spin)
		{
				if (sprite == null || count <= 0) {
						return;
				}

				var go = new GameObject ("Debris");
				go.transform.position = position;
				var ps = go.AddComponent<ParticleSystem> ();
				ps.Stop (true, ParticleSystemStopBehavior.StopEmittingAndClear);

				var main = ps.main;
				main.playOnAwake = false;
				main.loop = false;
				main.duration = lifetime;
				main.startLifetime = new ParticleSystem.MinMaxCurve (lifetime * 0.6f, lifetime);
				main.startSpeed = new ParticleSystem.MinMaxCurve (speed * 0.4f, speed);
				main.startSize = new ParticleSystem.MinMaxCurve (size * 0.6f, size);
				main.startRotation = new ParticleSystem.MinMaxCurve (0f, Mathf.PI * 2f);
				main.startColor = tint;
				main.gravityModifier = gravity;
				main.simulationSpace = ParticleSystemSimulationSpace.World;
				main.maxParticles = Mathf.Max (count, 8);

				var emission = ps.emission;
				emission.enabled = false;

				var shape = ps.shape;
				shape.enabled = true;
				shape.shapeType = ParticleSystemShapeType.Sphere;
				shape.radius = 0.15f;

				if (spin > 0f) {
						var rotation = ps.rotationOverLifetime;
						rotation.enabled = true;
						rotation.z = new ParticleSystem.MinMaxCurve (-spin, spin);
				}

				var color = ps.colorOverLifetime;
				color.enabled = true;
				var gradient = new Gradient ();
				gradient.SetKeys (
						new[] { new GradientColorKey (Color.white, 0f), new GradientColorKey (Color.white, 1f) },
						new[] { new GradientAlphaKey (1f, 0f), new GradientAlphaKey (1f, 0.55f), new GradientAlphaKey (0f, 1f) });
				color.color = gradient;

				var sheet = ps.textureSheetAnimation;
				sheet.enabled = true;
				sheet.mode = ParticleSystemAnimationMode.Sprites;
				sheet.AddSprite (sprite);

				var renderer = go.GetComponent<ParticleSystemRenderer> ();
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.sharedMaterial = SpriteMaterial;
				renderer.sortingLayerID = sortingLayerID;
				renderer.sortingOrder = sortingOrder;

				ps.Play ();
				ps.Emit (count);
				Object.Destroy (go, lifetime + 0.3f);
		}
}
