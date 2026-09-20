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

				_highscoreEmpty = UiKit.Text (board.transform, "Empty", L ("NoScoreYet"), 44f, TextAlignmentOptions.Center, new Vector2 (0.5f, 0.5f), Vector2.zero, new Vector2 (640f, 400f));
				_highscoreEmpty.enableAutoSizing = true;
				_highscoreEmpty.fontSizeMax = 44f;
				_highscoreEmpty.fontSizeMin = 30f;

				float y = -190f;
				for (int i = 0; i < MaxRows; i++) {
						var row = UiKit.Text (board.transform, "Row" + i, string.Empty, 42f, TextAlignmentOptions.Left, new Vector2 (0.5f, 1f), new Vector2 (0f, y - i * 62f), new Vector2 (640f, 60f));
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

				var desc = UiKit.Text (board.transform, "Description", L ("ChooseUsernameDesc"), 38f, TextAlignmentOptions.Center, top, new Vector2 (0f, -170f), new Vector2 (660f, 220f));
				desc.enableAutoSizing = true;
				desc.fontSizeMax = 38f;
				desc.fontSizeMin = 26f;

				UiKit.Text (board.transform, "Label", L ("ChooseUsername"), 44f, TextAlignmentOptions.Center, top, new Vector2 (0f, -400f), new Vector2 (660f, 60f));

				_username = InputField (board.transform, top, new Vector2 (0f, -470f), new Vector2 (620f, 100f));
				_username.onSubmit.AddListener (_ => Save ());

				UiKit.Button (board.transform, "Save", UiKit.Sprite ("button_normal"), UiKit.Sprite ("button_hover"), L ("Save"), 54f, top, new Vector2 (0f, -640f), new Vector2 (440f, 140f), Save);

				_optionsMessage = UiKit.Text (board.transform, "Message", string.Empty, 34f, TextAlignmentOptions.Center, top, new Vector2 (0f, -800f), new Vector2 (660f, 120f));
				_optionsMessage.color = new Color (1f, 0.6f, 0.5f);
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

		/// <summary>Panneau central sur le tableau en bois, avec titre et bouton fermer.</summary>
		private Image Board (RectTransform panel, string title)
		{
				var center = new Vector2 (0.5f, 0.5f);
				var board = UiKit.Image (panel, "Board", UiKit.Sprite ("highscore_board"), center, new Vector2 (0f, 40f), new Vector2 (820f, 920f));
				UiKit.Text (board.transform, "Title", title, 70f, TextAlignmentOptions.Center, new Vector2 (0.5f, 1f), new Vector2 (0f, -60f), new Vector2 (600f, 100f));
				UiKit.Button (board.transform, "Close", UiKit.Sprite ("close_button"), UiKit.Sprite ("close_button_hover"), null, 0f, new Vector2 (1f, 1f), new Vector2 (40f, 40f), new Vector2 (120f, 130f), () => Invoke (OnClose));
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
		}

		private static void Invoke (Action action)
		{
				if (action != null) {
						action ();
				}
		}
}
