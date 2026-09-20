using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu principal en uGUI + TextMeshPro : écran d'accueil, classement local, options
/// (pseudo). Construit par code par Menu, qui pousse les données et reçoit les clics.
/// </summary>
public class MenuView : MonoBehaviour
{
		public Action OnPlay, OnHighscore, OnOptions, OnClose;
		public Action<string> OnSaveUsername;

		private RectTransform _main, _highscore, _options;
		private Image _overlay;
		private TextMeshProUGUI _playerLine, _highscoreEmpty, _optionsMessage;
		private readonly List<TextMeshProUGUI> _rows = new List<TextMeshProUGUI> ();
		private TMP_InputField _username;

		private const int MaxRows = 10;

		private static string L (string key)
		{
				string value;
				return LocalizationStrings.Instance.Values.TryGetValue (key, out value) ? value : key;
		}

		public static MenuView Create ()
		{
				var canvas = UiKit.CreateCanvas ("MenuUi", 5);
				var root = UiKit.Panel (canvas.transform, "SafeArea");
				root.gameObject.AddComponent<SafeAreaFitter> ();
				var view = canvas.gameObject.AddComponent<MenuView> ();
				view.Build (root);
				return view;
		}

		private void Build (RectTransform root)
		{
				BuildMain (root);

				// Voile sombre derrière les panneaux, qui bloque aussi les clics vers l'accueil
				_overlay = UiKit.Fill (root, "Overlay", new Color (0f, 0f, 0f, 0.55f));
				_overlay.raycastTarget = true;

				BuildHighscore (root);
				BuildOptions (root);
				Show (MenuState.MENU);
		}

		// ------------------------------------------------------------------ Accueil

		private void BuildMain (RectTransform root)
		{
				_main = UiKit.Panel (root, "Main");
				var top = new Vector2 (0.5f, 1f);

				var title = UiKit.Text (_main, "Title", "INFINITE DIGGER", 130f, TextAlignmentOptions.Center, top, new Vector2 (0f, -70f), new Vector2 (1000f, 200f));
				title.enableAutoSizing = true;
				title.fontSizeMax = 130f;
				title.fontSizeMin = 70f;
				title.outlineWidth = 0.25f;
				Tween.Bob (title.rectTransform, 10f, 2.6f);

				var normal = UiKit.Sprite ("button_normal");
				var hover = UiKit.Sprite ("button_hover");
				var size = new Vector2 (720f, 150f);
				UiKit.Button (_main, "Play", normal, hover, L ("ChallengeMode"), 60f, top, new Vector2 (0f, -620f), size, () => Invoke (OnPlay));
				UiKit.Button (_main, "Highscore", normal, hover, L ("Highscore"), 60f, top, new Vector2 (0f, -800f), size, () => Invoke (OnHighscore));
				UiKit.Button (_main, "Options", normal, hover, L ("Options"), 60f, top, new Vector2 (0f, -980f), size, () => Invoke (OnOptions));

				_playerLine = UiKit.Text (_main, "PlayerLine", string.Empty, 40f, TextAlignmentOptions.Center, new Vector2 (0.5f, 0f), new Vector2 (0f, 30f), new Vector2 (1000f, 130f));
		}

		public void SetPlayerLine (string text)
		{
				_playerLine.text = text;
		}

		// ------------------------------------------------------------------ Classement

		private void BuildHighscore (RectTransform root)
		{
				_highscore = UiKit.Panel (root, "Highscore");
				var board = Board (_highscore, L ("Highscore"));

				// Zone de contenu du tableau : 24-90 % de la hauteur (voir Board)
				_highscoreEmpty = UiKit.Text (board.transform, "Empty", L ("NoScoreYet"), 44f, TextAlignmentOptions.Center, new Vector2 (0.5f, 0.5f), new Vector2 (0f, -64f), new Vector2 (600f, 400f));
				_highscoreEmpty.enableAutoSizing = true;
				_highscoreEmpty.fontSizeMax = 44f;
				_highscoreEmpty.fontSizeMin = 30f;

				const float firstRowTop = -235f;
				const float rowStep = 58f;
				for (int i = 0; i < MaxRows; i++) {
						var row = UiKit.Text (board.transform, "Row" + i, string.Empty, 40f, TextAlignmentOptions.Left, new Vector2 (0.5f, 1f), new Vector2 (0f, firstRowTop - i * rowStep), new Vector2 (600f, 56f));
						_rows.Add (row);
				}
		}

		public void SetHighscores (List<Highscore> entries)
		{
				bool empty = entries == null || entries.Count == 0;
				_highscoreEmpty.gameObject.SetActive (empty);
				for (int i = 0; i < _rows.Count; i++) {
						bool visible = !empty && i < entries.Count;
						_rows [i].gameObject.SetActive (visible);
						if (visible) {
								var e = entries [i];
								_rows [i].text = string.Format ("{0}.  {1}  —  {2} m", e.Rank, e.PlayerName, e.Meters);
						}
				}
		}

		// ------------------------------------------------------------------ Options

		private void BuildOptions (RectTransform root)
		{
				_options = UiKit.Panel (root, "Options");
				var board = Board (_options, L ("Options"));
				var top = new Vector2 (0.5f, 1f);

				// Zone de contenu du tableau : 24-90 % de la hauteur (-221 à -828)
				var desc = UiKit.Text (board.transform, "Description", L ("ChooseUsernameDesc"), 36f, TextAlignmentOptions.Center, top, new Vector2 (0f, -235f), new Vector2 (600f, 160f));
				desc.enableAutoSizing = true;
				desc.fontSizeMax = 36f;
				desc.fontSizeMin = 24f;

				UiKit.Text (board.transform, "Label", L ("ChooseUsername"), 42f, TextAlignmentOptions.Center, top, new Vector2 (0f, -405f), new Vector2 (600f, 55f));

				_username = InputField (board.transform, top, new Vector2 (0f, -470f), new Vector2 (600f, 100f));
				_username.onSubmit.AddListener (_ => Save ());

				_optionsMessage = UiKit.Text (board.transform, "Message", string.Empty, 30f, TextAlignmentOptions.Center, top, new Vector2 (0f, -580f), new Vector2 (600f, 60f));
				_optionsMessage.color = new Color (1f, 0.6f, 0.5f);

				UiKit.Button (board.transform, "Save", UiKit.Sprite ("button_normal"), UiKit.Sprite ("button_hover"), L ("Save"), 54f, top, new Vector2 (0f, -655f), new Vector2 (440f, 140f), Save);
		}

		private void Save ()
		{
				if (OnSaveUsername != null) {
						OnSaveUsername (_username.text);
				}
		}

		public void SetUsername (string value)
		{
				_username.SetTextWithoutNotify (value ?? string.Empty);
		}

		public void SetOptionsMessage (string message)
		{
				_optionsMessage.text = message ?? string.Empty;
		}

		// ------------------------------------------------------------------ Communs

		/// <summary>
		/// Panneau central sur le tableau en bois, avec titre et bouton fermer.
		/// Texture highscore_board (604x678) : marge transparente 0-4 % de la hauteur, en-tête
		/// vert 4-16 %, contenu 24-90 %. Les positions désignent le bord haut (pivot = ancre).
		/// </summary>
		private Image Board (RectTransform panel, string title)
		{
				var center = new Vector2 (0.5f, 0.5f);
				var board = UiKit.Image (panel, "Board", UiKit.Sprite ("highscore_board"), center, new Vector2 (0f, 40f), new Vector2 (820f, 920f));
				UiKit.Text (board.transform, "Title", title, 66f, TextAlignmentOptions.Center, new Vector2 (0.5f, 1f), new Vector2 (0f, -42f), new Vector2 (560f, 100f));
				UiKit.Button (board.transform, "Close", UiKit.Sprite ("close_button"), UiKit.Sprite ("close_button_hover"), null, 0f, new Vector2 (1f, 1f), new Vector2 (30f, 20f), new Vector2 (120f, 130f), () => Invoke (OnClose));
				return board;
		}

		/// <summary>Champ de saisie TMP sur le fond input.png : zone de texte masquée + texte + placeholder.</summary>
		private static TMP_InputField InputField (Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
		{
				var go = new GameObject ("UsernameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
				var rt = go.GetComponent<RectTransform> ();
				rt.SetParent (parent, false);
				UiKit.Place (rt, anchor, position, size);
				go.GetComponent<Image> ().sprite = UiKit.Sprite ("input");

				var area = UiKit.Panel (go.transform, "TextArea");
				area.offsetMin = new Vector2 (24f, 8f);
				area.offsetMax = new Vector2 (-24f, -8f);
				area.gameObject.AddComponent<RectMask2D> ();

				var dark = new Color (0.16f, 0.09f, 0.03f);
				var text = UiKit.Text (area, "Text", string.Empty, 48f, TextAlignmentOptions.MidlineLeft, new Vector2 (0.5f, 0.5f), Vector2.zero, Vector2.zero, false);
				UiKit.Stretch (text.rectTransform);
				text.color = dark;
				text.textWrappingMode = TextWrappingModes.NoWrap;

				var placeholder = UiKit.Text (area, "Placeholder", "...", 48f, TextAlignmentOptions.MidlineLeft, new Vector2 (0.5f, 0.5f), Vector2.zero, Vector2.zero, false);
				UiKit.Stretch (placeholder.rectTransform);
				placeholder.color = new Color (dark.r, dark.g, dark.b, 0.4f);
				placeholder.fontStyle = FontStyles.Italic;

				var field = go.GetComponent<TMP_InputField> ();
				field.textViewport = area;
				field.textComponent = text;
				field.placeholder = placeholder;
				field.characterLimit = 20;
				field.contentType = TMP_InputField.ContentType.Name;
				return field;
		}

		public void Show (MenuState state)
		{
				bool main = state == MenuState.MENU;
				_main.gameObject.SetActive (main);
				_overlay.gameObject.SetActive (!main);
				_highscore.gameObject.SetActive (state == MenuState.HIGHSCORE);
				_options.gameObject.SetActive (state == MenuState.OPTION);

				if (!main) {
						UiKit.PopIn (this, _overlay.rectTransform, false);
						UiKit.PopIn (this, state == MenuState.HIGHSCORE ? _highscore : _options);
				}
		}

		private static void Invoke (Action action)
		{
				if (action != null) {
						action ();
				}
		}
}
