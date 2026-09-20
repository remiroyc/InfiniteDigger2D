using UnityEngine;

/// <summary>
/// Repère virtuel partagé par tous les OnGUI du jeu.
///
/// Le jeu a été dessiné pour un écran 1080x1920 (9:16). L'ancien code étirait
/// séparément X et Y vers la résolution réelle, ce qui déformait tout sur les
/// écrans 20:9 et ignorait l'encoche. Ici :
///  - l'échelle est uniforme, calée sur la largeur de la zone sûre ;
///  - la hauteur virtuelle est déduite de la hauteur réelle : les éléments
///    ancrés sur Height restent collés en bas quelle que soit la hauteur ;
///  - la matrice est décalée à l'origine de Screen.safeArea, donc rien ne
///    passe sous l'encoche ni la barre de gestes.
///
/// Usage, en tête de chaque OnGUI :
///     GUI.matrix = VirtualGui.Matrix;
///     float h = VirtualGui.Height;   // à la place de la constante 1920
/// </summary>
public static class VirtualGui
{
		public const float Width = 1080f;

		public static float Height { get; private set; }

		public static float Scale { get; private set; }

		public static Matrix4x4 Matrix {
				get {
						Refresh ();
						return _matrix;
				}
		}

		private static Matrix4x4 _matrix = Matrix4x4.identity;
		private static int _lastWidth, _lastHeight;
		private static Rect _lastSafeArea;

		private static void Refresh ()
		{
				var safe = Screen.safeArea;
				if (Screen.width == _lastWidth && Screen.height == _lastHeight && safe == _lastSafeArea) {
						return;
				}
				_lastWidth = Screen.width;
				_lastHeight = Screen.height;
				_lastSafeArea = safe;

				// safeArea est exprimé avec l'origine en bas à gauche ; IMGUI a l'origine en haut à gauche.
				float scale = safe.width / Width;
				float topInset = Screen.height - (safe.y + safe.height);

				Scale = scale;
				Height = safe.height / scale;
				_matrix = Matrix4x4.TRS (new Vector3 (safe.x, topInset, 0f), Quaternion.identity, new Vector3 (scale, scale, 1f));
		}
}
