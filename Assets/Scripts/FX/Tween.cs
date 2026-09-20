using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tweens maison, sans dépendance : sursaut d'échelle, écrasement/étirement, flash de
/// couleur, ralenti, oscillation, scintillement, fondu. Les coroutines tournent sur un
/// runner caché qui survit aux changements de scène.
///
/// Les tweens monde suivent Time.timeScale (ils se figent en pause, ralentissent avec le
/// hit-stop) ; les tweens d'UI sont en temps non mis à l'échelle.
/// </summary>
public static class Tween
{
		private class Runner : MonoBehaviour
		{
		}

		private static Runner _runner;

		private static Runner RunnerInstance {
				get {
						if (_runner == null) {
								var go = new GameObject ("~Tween");
								go.hideFlags = HideFlags.HideAndDontSave;
								UnityEngine.Object.DontDestroyOnLoad (go);
								_runner = go.AddComponent<Runner> ();
						}
						return _runner;
				}
		}

		public static Coroutine Run (IEnumerator routine)
		{
				return RunnerInstance.StartCoroutine (routine);
		}

		public static void Stop (Coroutine coroutine)
		{
				if (coroutine != null && _runner != null) {
						_runner.StopCoroutine (coroutine);
				}
		}

		private static float Dt (bool unscaled)
		{
				return unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
		}

		private static float EaseOut (float t)
		{
				return 1f - (1f - t) * (1f - t);
		}

		// ------------------------------------------------------------------ Échelle

		/// <summary>Applique des multiplicateurs à l'échelle de base en conservant le signe (flip gauche/droite).</summary>
		private static void ApplyScale (Transform t, Vector3 baseScale, float mx, float my)
		{
				var current = t.localScale;
				float signX = current.x < 0f ? -1f : 1f;
				float signY = current.y < 0f ? -1f : 1f;
				t.localScale = new Vector3 (signX * baseScale.x * mx, signY * baseScale.y * my, baseScale.z);
		}

		private static Vector3 Abs (Vector3 v)
		{
				return new Vector3 (Mathf.Abs (v.x), Mathf.Abs (v.y), Mathf.Abs (v.z));
		}

		/// <summary>Sursaut d'échelle : monte à amount puis revient (courbe en cloche).</summary>
		public static Coroutine Punch (Transform t, float amount = 1.15f, float duration = 0.12f, bool unscaled = false)
		{
				return t == null ? null : Run (PunchRoutine (t, amount, duration, unscaled));
		}

		private static IEnumerator PunchRoutine (Transform t, float amount, float duration, bool unscaled)
		{
				var baseScale = Abs (t.localScale);
				float elapsed = 0f;
				while (elapsed < duration && t != null) {
						elapsed += Dt (unscaled);
						float k = Mathf.Clamp01 (elapsed / duration);
						float s = 1f + (amount - 1f) * Mathf.Sin (k * Mathf.PI);
						ApplyScale (t, baseScale, s, s);
						yield return null;
				}
				if (t != null) {
						ApplyScale (t, baseScale, 1f, 1f);
				}
		}

		/// <summary>Écrasement / étirement immédiat (multiplicateurs x, y) puis retour progressif à l'échelle normale.</summary>
		public static Coroutine Squash (Transform t, float sx, float sy, float duration = 0.14f)
		{
				return t == null ? null : Run (SquashRoutine (t, sx, sy, duration));
		}

		private static IEnumerator SquashRoutine (Transform t, float sx, float sy, float duration)
		{
				var baseScale = Abs (t.localScale);
				float elapsed = 0f;
				while (elapsed < duration && t != null) {
						elapsed += Time.deltaTime;
						float k = EaseOut (Mathf.Clamp01 (elapsed / duration));
						ApplyScale (t, baseScale, Mathf.Lerp (sx, 1f, k), Mathf.Lerp (sy, 1f, k));
						yield return null;
				}
				if (t != null) {
						ApplyScale (t, baseScale, 1f, 1f);
				}
		}

		// ------------------------------------------------------------------ Couleur

		/// <summary>Teinte un SpriteRenderer puis revient à la couleur donnée.</summary>
		public static Coroutine Flash (SpriteRenderer renderer, Color flash, Color back, float duration = 0.1f)
		{
				return renderer == null ? null : Run (FlashRoutine (renderer, flash, back, duration));
		}

		private static IEnumerator FlashRoutine (SpriteRenderer renderer, Color flash, Color back, float duration)
		{
				renderer.color = flash;
				float elapsed = 0f;
				while (elapsed < duration && renderer != null) {
						elapsed += Time.deltaTime;
						renderer.color = Color.Lerp (flash, back, Mathf.Clamp01 (elapsed / duration));
						yield return null;
				}
				if (renderer != null) {
						renderer.color = back;
				}
		}

		/// <summary>Scintillement continu de l'alpha (torche, halo). Renvoie la coroutine pour l'arrêter.</summary>
		public static Coroutine Flicker (SpriteRenderer renderer, float minAlpha, float maxAlpha, float speed)
		{
				return renderer == null ? null : Run (FlickerRoutine (renderer, minAlpha, maxAlpha, speed));
		}

		private static IEnumerator FlickerRoutine (SpriteRenderer renderer, float minAlpha, float maxAlpha, float speed)
		{
				float seed = UnityEngine.Random.value * 100f;
				while (renderer != null) {
						float n = Mathf.PerlinNoise (Time.time * speed + seed, seed);
						var c = renderer.color;
						c.a = Mathf.Lerp (minAlpha, maxAlpha, n);
						renderer.color = c;
						yield return null;
				}
		}

		// ------------------------------------------------------------------ Temps

		/// <summary>Ralenti bref pour rendre un impact lisible. Ignoré si le jeu n'est pas à vitesse normale (pause, mort).</summary>
		public static void HitStop (float duration = 0.06f, float scale = 0.05f)
		{
				if (Time.timeScale < 0.99f) {
						return;
				}
				Run (HitStopRoutine (duration, scale));
		}

		private static IEnumerator HitStopRoutine (float duration, float scale)
		{
				Time.timeScale = scale;
				float elapsed = 0f;
				while (elapsed < duration) {
						elapsed += Time.unscaledDeltaTime;
						yield return null;
				}
				// Ne restaure que si personne d'autre n'a touché au temps entre-temps (pause, mort)
				if (Mathf.Approximately (Time.timeScale, scale)) {
						Time.timeScale = 1f;
				}
		}

		// ------------------------------------------------------------------ UI

		/// <summary>Oscillation verticale continue d'un RectTransform (titre).</summary>
		public static Coroutine Bob (RectTransform rt, float amplitude, float period)
		{
				return rt == null ? null : Run (BobRoutine (rt, amplitude, period));
		}

		private static IEnumerator BobRoutine (RectTransform rt, float amplitude, float period)
		{
				var basePos = rt.anchoredPosition;
				float elapsed = 0f;
				while (rt != null) {
						elapsed += Time.unscaledDeltaTime;
						rt.anchoredPosition = basePos + Vector2.up * (Mathf.Sin (elapsed / period * Mathf.PI * 2f) * amplitude);
						yield return null;
				}
		}

		/// <summary>Tremblement d'un RectTransform autour de sa position, en temps non mis à l'échelle.</summary>
		public static Coroutine ShakeRect (RectTransform rt, float amplitude, float duration)
		{
				return rt == null ? null : Run (ShakeRectRoutine (rt, amplitude, duration));
		}

		private static IEnumerator ShakeRectRoutine (RectTransform rt, float amplitude, float duration)
		{
				var basePos = rt.anchoredPosition;
				float elapsed = 0f;
				while (elapsed < duration && rt != null) {
						elapsed += Time.unscaledDeltaTime;
						float k = 1f - Mathf.Clamp01 (elapsed / duration);
						rt.anchoredPosition = basePos + UnityEngine.Random.insideUnitCircle * amplitude * k;
						yield return null;
				}
				if (rt != null) {
						rt.anchoredPosition = basePos;
				}
		}

		/// <summary>Fondu d'un CanvasGroup, en temps non mis à l'échelle.</summary>
		public static Coroutine Fade (CanvasGroup group, float from, float to, float duration, Action onDone = null)
		{
				return group == null ? null : Run (FadeRoutine (group, from, to, duration, onDone));
		}

		private static IEnumerator FadeRoutine (CanvasGroup group, float from, float to, float duration, Action onDone)
		{
				float elapsed = 0f;
				group.alpha = from;
				while (elapsed < duration && group != null) {
						elapsed += Time.unscaledDeltaTime;
						group.alpha = Mathf.Lerp (from, to, Mathf.Clamp01 (elapsed / duration));
						yield return null;
				}
				if (group != null) {
						group.alpha = to;
				}
				if (onDone != null) {
						onDone ();
				}
		}

		/// <summary>Interpolation générique en temps non mis à l'échelle (zoom caméra...).</summary>
		public static Coroutine Value (float from, float to, float duration, Action<float> apply)
		{
				return Run (ValueRoutine (from, to, duration, apply));
		}

		private static IEnumerator ValueRoutine (float from, float to, float duration, Action<float> apply)
		{
				float elapsed = 0f;
				while (elapsed < duration) {
						elapsed += Time.unscaledDeltaTime;
						apply (Mathf.Lerp (from, to, EaseOut (Mathf.Clamp01 (elapsed / duration))));
						yield return null;
				}
				apply (to);
		}
}
