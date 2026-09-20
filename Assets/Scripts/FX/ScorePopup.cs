using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Texte flottant dans le monde ("+100" à la collecte d'une pièce) : apparaît d'un coup,
/// monte, s'estompe, disparaît. Généré en TextMeshPro avec la police du jeu, rendu sur la
/// couche Hub pour rester lisible même dans le noir du brouillard.
/// </summary>
public static class ScorePopup
{
		public static readonly Color Gold = new Color (1f, 0.84f, 0.3f);

		private const float Duration = 0.8f;
		private const float Rise = 1.1f;
		private const float FontSize = 5f;

		public static void Show (Vector3 position, string text, Color color)
		{
				var go = new GameObject ("ScorePopup");
				go.transform.position = position + new Vector3 (Random.Range (-0.15f, 0.15f), 0.3f, 0f);

				var label = go.AddComponent<TextMeshPro> ();
				label.font = UiKit.Font;
				label.text = text;
				label.fontSize = FontSize;
				label.fontStyle = FontStyles.Bold;
				label.alignment = TextAlignmentOptions.Center;
				label.color = color;
				label.outlineWidth = 0.25f;
				label.outlineColor = UiKit.OutlineColor;
				label.rectTransform.sizeDelta = new Vector2 (4f, 1.5f);

				var renderer = go.GetComponent<MeshRenderer> ();
				renderer.sortingLayerName = "Hub";
				renderer.sortingOrder = 5;

				Tween.Run (Animate (go.transform, label));
		}

		/// <summary>Collecte de pièce : "+valeur" doré et quelques étincelles.</summary>
		public static void Coin (Vector3 position, int value, int sortingLayerID)
		{
				Show (position, "+" + value, Gold);
				Debris.Burst (position, UiKit.WhiteSprite, 8, Gold, sortingLayerID, 4, speed: 2.2f, gravity: 0.7f, size: 0.09f, lifetime: 0.45f, spin: 0f);
		}

		private static IEnumerator Animate (Transform t, TextMeshPro label)
		{
				var start = t.position;
				float elapsed = 0f;
				while (elapsed < Duration && t != null) {
						elapsed += Time.deltaTime;
						float k = Mathf.Clamp01 (elapsed / Duration);

						// Pop d'échelle sur les premiers 15 %, puis stable
						float pop = k < 0.15f ? Mathf.Lerp (0.4f, 1.25f, k / 0.15f) : Mathf.Lerp (1.25f, 1f, Mathf.Clamp01 ((k - 0.15f) / 0.25f));
						t.localScale = Vector3.one * pop;

						// Montée décélérée
						float rise = 1f - (1f - k) * (1f - k);
						t.position = start + Vector3.up * (Rise * rise);

						// Fondu sur la seconde moitié
						float alpha = k < 0.5f ? 1f : 1f - (k - 0.5f) / 0.5f;
						var c = label.color;
						c.a = alpha;
						label.color = c;
						yield return null;
				}
				if (t != null) {
						Object.Destroy (t.gameObject);
				}
		}
}
