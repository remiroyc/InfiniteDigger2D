using UnityEngine;

/// <summary>
/// Cale un RectTransform étiré sur Screen.safeArea (encoche, barre de gestes).
/// À poser sur le conteneur racine de l'UI, enfant direct du Canvas.
/// </summary>
[RequireComponent (typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
		private RectTransform _rect;
		private Rect _applied;
		private Vector2Int _appliedScreen;

		void Awake ()
		{
				_rect = GetComponent<RectTransform> ();
				Apply ();
		}

		void Update ()
		{
				if (Screen.safeArea != _applied || Screen.width != _appliedScreen.x || Screen.height != _appliedScreen.y) {
						Apply ();
				}
		}

		private void Apply ()
		{
				var safe = Screen.safeArea;
				_applied = safe;
				_appliedScreen = new Vector2Int (Screen.width, Screen.height);
				if (Screen.width <= 0 || Screen.height <= 0) {
						return;
				}

				var min = new Vector2 (safe.xMin / Screen.width, safe.yMin / Screen.height);
				var max = new Vector2 (safe.xMax / Screen.width, safe.yMax / Screen.height);
				_rect.anchorMin = min;
				_rect.anchorMax = max;
				_rect.offsetMin = Vector2.zero;
				_rect.offsetMax = Vector2.zero;
		}
}
