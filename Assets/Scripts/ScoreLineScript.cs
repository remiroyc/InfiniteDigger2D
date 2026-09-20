using TMPro;
using UnityEngine;

/// <summary>
/// Étiquette "Dernier score" affichée dans le monde, sur la ligne qui marque le meilleur
/// score (prefab ScoreLine, positionné par GameManager). Texte TMP en espace monde :
/// remplace l'ancien OnGUI, le dernier IMGUI des scènes du build.
/// </summary>
public class ScoreLineScript : MonoBehaviour
{
		public float FontSize = 5f;
		public Vector3 LabelOffset = new Vector3 (0f, 0.4f, 0f);
		public string SortingLayer = "Hub";

		void Start ()
		{
				var go = new GameObject ("Label");
				go.transform.SetParent (transform, false);
				go.transform.localPosition = LabelOffset;

				// Le trait est un sprite étiré (échelle 10 x 1) : on annule cette échelle pour le texte
				var parentScale = transform.lossyScale;
				go.transform.localScale = new Vector3 (
						Mathf.Approximately (parentScale.x, 0f) ? 1f : 1f / parentScale.x,
						Mathf.Approximately (parentScale.y, 0f) ? 1f : 1f / parentScale.y,
						1f);

				var label = go.AddComponent<TextMeshPro> ();
				label.font = UiKit.Font;
				label.text = LocalizationStrings.Instance.Values ["LastScore"];
				label.fontSize = FontSize;
				label.alignment = TextAlignmentOptions.Center;
				label.color = UiKit.TextColor;
				label.outlineWidth = 0.2f;
				label.outlineColor = UiKit.OutlineColor;
				label.rectTransform.sizeDelta = new Vector2 (12f, 2f); // en unités monde, après annulation de l'échelle

				var renderer = go.GetComponent<MeshRenderer> ();
				renderer.sortingLayerName = SortingLayer;
				renderer.sortingOrder = 10;
		}
}
