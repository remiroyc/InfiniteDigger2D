using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Fabrique d'éléments uGUI construits par code. Toutes les tailles sont exprimées dans
/// le repère de référence 1080 x 1920 du CanvasScaler ; uGUI se charge de l'échelle et
/// rend les textes à la taille écran réelle (net à toute résolution, contrairement à IMGUI).
///
/// Les sprites et la police sont chargés depuis Resources/UI.
/// </summary>
public static class UiKit
{
		public const float ReferenceWidth = 1080f;
		public const float ReferenceHeight = 1920f;

		private static TMP_FontAsset _font;
		private static Sprite _whiteSprite;

		public static readonly Color TextColor = Color.white;
		public static readonly Color OutlineColor = new Color (0.16f, 0.09f, 0.03f);

		/// <summary>Police du jeu (Carton Six) en TMP, générée dynamiquement au premier usage.</summary>
		public static TMP_FontAsset Font {
				get {
						if (_font != null) {
								return _font;
						}
						var ttf = Resources.Load<Font> ("UI/Carton_Six");
						if (ttf != null) {
								try {
										_font = TMP_FontAsset.CreateFontAsset (ttf, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
								} catch (Exception e) {
										Debug.LogWarning ("[UiKit] Police TMP non générée, repli sur la police TMP par défaut : " + e.Message);
								}
						}
						if (_font == null) {
								_font = TMP_Settings.defaultFontAsset;
						}
						return _font;
				}
		}

		public static Sprite WhiteSprite {
				get {
						if (_whiteSprite == null) {
								_whiteSprite = UnityEngine.Sprite.Create (Texture2D.whiteTexture, new Rect (0, 0, 4, 4), new Vector2 (0.5f, 0.5f), 100f);
						}
						return _whiteSprite;
				}
		}

		public static Sprite Sprite (string name)
		{
				var sprite = Resources.Load<Sprite> ("UI/" + name);
				if (sprite == null) {
						Debug.LogWarning ("[UiKit] Sprite introuvable : Resources/UI/" + name);
				}
				return sprite;
		}

		/// <summary>Canvas plein écran en overlay, mis à l'échelle sur la largeur de référence.</summary>
		public static Canvas CreateCanvas (string name, int sortingOrder)
		{
				EnsureEventSystem ();

				var go = new GameObject (name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
				var canvas = go.GetComponent<Canvas> ();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = sortingOrder;

				var scaler = go.GetComponent<CanvasScaler> ();
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.referenceResolution = new Vector2 (ReferenceWidth, ReferenceHeight);
				scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				scaler.matchWidthOrHeight = 0f; // largeur : même logique que VirtualGui
				return canvas;
		}

		/// <summary>
		/// uGUI ne reçoit aucun clic sans EventSystem. La scène test en a un (boutons de
		/// déplacement), la scène menu non : on le crée à la demande.
		/// </summary>
		private static void EnsureEventSystem ()
		{
				if (UnityEngine.Object.FindAnyObjectByType<EventSystem> () != null) {
						return;
				}
				new GameObject ("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
		}

		/// <summary>RectTransform vide, étiré sur son parent par défaut.</summary>
		public static RectTransform Panel (Transform parent, string name)
		{
				var go = new GameObject (name, typeof(RectTransform));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				Stretch (rt);
				return rt;
		}

		public static void Stretch (RectTransform rt)
		{
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.offsetMin = Vector2.zero;
				rt.offsetMax = Vector2.zero;
		}

		/// <summary>Ancre ponctuelle : même point d'ancrage et pivot, position et taille en unités de référence.</summary>
		public static void Place (RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
		{
				rt.anchorMin = anchor;
				rt.anchorMax = anchor;
				rt.pivot = anchor;
				rt.anchoredPosition = position;
				rt.sizeDelta = size;
		}

		public static Image Image (Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size)
		{
				var go = new GameObject (name, typeof(RectTransform), typeof(Image));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				Place (rt, anchor, position, size);
				var image = go.GetComponent<Image> ();
				image.sprite = sprite;
				image.raycastTarget = false;
				return image;
		}

		public static Image Fill (Transform parent, string name, Color color)
		{
				var go = new GameObject (name, typeof(RectTransform), typeof(Image));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				Stretch (rt);
				var image = go.GetComponent<Image> ();
				image.sprite = WhiteSprite;
				image.color = color;
				image.raycastTarget = false;
				return image;
		}

		public static TextMeshProUGUI Text (Transform parent, string name, string text, float size, TextAlignmentOptions alignment, Vector2 anchor, Vector2 position, Vector2 boxSize, bool outline = true)
		{
				var go = new GameObject (name, typeof(RectTransform), typeof(TextMeshProUGUI));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				Place (rt, anchor, position, boxSize);

				var tmp = go.GetComponent<TextMeshProUGUI> ();
				tmp.font = Font;
				tmp.text = text;
				tmp.fontSize = size;
				tmp.color = TextColor;
				tmp.alignment = alignment;
				tmp.raycastTarget = false;
				tmp.textWrappingMode = TextWrappingModes.Normal;
				tmp.overflowMode = TextOverflowModes.Overflow;
				if (outline) {
						tmp.outlineWidth = 0.2f;
						tmp.outlineColor = OutlineColor;
				}
				return tmp;
		}

		/// <summary>Bouton image avec sprite survolé/pressé optionnel et libellé TMP optionnel.</summary>
		public static Button Button (Transform parent, string name, Sprite normal, Sprite pressed, string label, float labelSize, Vector2 anchor, Vector2 position, Vector2 size, Action onClick)
		{
				var go = new GameObject (name, typeof(RectTransform), typeof(Image), typeof(Button));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				Place (rt, anchor, position, size);

				var image = go.GetComponent<Image> ();
				image.sprite = normal;
				image.type = UnityEngine.UI.Image.Type.Simple;
				image.preserveAspect = false;

				var button = go.GetComponent<Button> ();
				button.targetGraphic = image;
				if (pressed != null) {
						button.transition = Selectable.Transition.SpriteSwap;
						var state = button.spriteState;
						state.highlightedSprite = pressed;
						state.pressedSprite = pressed;
						state.selectedSprite = normal;
						button.spriteState = state;
				} else {
						button.transition = Selectable.Transition.ColorTint;
				}
				if (onClick != null) {
						button.onClick.AddListener (() => onClick ());
				}

				if (!string.IsNullOrEmpty (label)) {
						var text = Text (go.transform, "Label", label, labelSize, TextAlignmentOptions.Center, new Vector2 (0.5f, 0.5f), Vector2.zero, size * 0.9f);
						text.enableAutoSizing = true;
						text.fontSizeMax = labelSize;
						text.fontSizeMin = labelSize * 0.5f;
				}
				return button;
		}
}
