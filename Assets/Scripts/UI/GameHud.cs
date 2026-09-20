using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de la partie en uGUI + TextMeshPro : compteurs, barre de vie, boutons dynamite et
/// pause, écran de pause et écran de mort. Construit par code au lancement de la scène
/// par GameManager, qui pousse les valeurs et reçoit les clics via les callbacks.
/// Remplace l'ancien OnGUI (IMGUI) de GameManager.
/// </summary>
public class GameHud : MonoBehaviour
{
		// --- callbacks branchés par GameManager ---
		public Action OnPause, OnResume, OnRestart, OnMenu, OnQuit, OnDynamite, OnSoundToggle, OnHome, OnReplay;

		private TextMeshProUGUI _coins, _distance, _dynamites;
		private Image _healthFill;
		private RectTransform _hud, _pausePanel, _deathPanel;
		private Button _dynamiteButton, _pauseButton, _soundButton;
		private Image _soundImage;
		private Sprite _soundOn, _soundOff;
		private TextMeshProUGUI _deathDistance, _deathCoins, _deathTaps, _deathFinal;

		private int _lastCoins = int.MinValue, _lastDistance = int.MinValue, _lastDynamites = int.MinValue;
		private float _lastHealthRatio = -1f;
		private RectTransform _coinBox, _healthBar;
		private Coroutine _lowHealthBlink;
		private const float LowHealthRatio = 0.34f;

		private static string L (string key)
		{
				string value;
				return LocalizationStrings.Instance.Values.TryGetValue (key, out value) ? value : key;
		}

		public static GameHud Create ()
		{
				var canvas = UiKit.CreateCanvas ("GameHud", 5);
				var root = UiKit.Panel (canvas.transform, "SafeArea");
				root.gameObject.AddComponent<SafeAreaFitter> ();
				var hud = canvas.gameObject.AddComponent<GameHud> ();
				hud.Build (root);
				return hud;
		}

		private void Build (RectTransform root)
		{
				BuildHud (root);
				BuildPausePanel (root);
				BuildDeathPanel (root);
				_pausePanel.gameObject.SetActive (false);
				_deathPanel.gameObject.SetActive (false);
		}

		// ------------------------------------------------------------------ HUD

		private void BuildHud (RectTransform root)
		{
				_hud = UiKit.Panel (root, "Hud");
				var topLeft = new Vector2 (0f, 1f);
				var topRight = new Vector2 (1f, 1f);

				// Compteurs : la texture a l'icône à gauche et un cartouche à droite, le texte va dans le cartouche
				var coinBox = UiKit.Image (_hud, "CoinBox", UiKit.Sprite ("hub_coin"), topLeft, new Vector2 (32f, -32f), new Vector2 (216f, 84f));
				_coinBox = coinBox.rectTransform;
				_coins = UiKit.Text (coinBox.transform, "Value", "0", 46f, TextAlignmentOptions.Center, new Vector2 (0.5f, 0.5f), new Vector2 (36f, 0f), new Vector2 (120f, 84f));

				var distanceBox = UiKit.Image (_hud, "DistanceBox", UiKit.Sprite ("hub_time"), topLeft, new Vector2 (324f, -32f), new Vector2 (216f, 84f));
				_distance = UiKit.Text (distanceBox.transform, "Value", "0", 46f, TextAlignmentOptions.Center, new Vector2 (0.5f, 0.5f), new Vector2 (36f, 0f), new Vector2 (120f, 84f));

				// Barre de vie sous les compteurs
				var barBack = UiKit.Image (_hud, "HealthBar", UiKit.WhiteSprite, topLeft, new Vector2 (32f, -132f), new Vector2 (508f, 32f));
				barBack.color = new Color (0f, 0f, 0f, 0.6f);
				_healthBar = barBack.rectTransform;
				var fillRect = UiKit.Panel (barBack.transform, "Fill");
				fillRect.offsetMin = new Vector2 (4f, 4f);
				fillRect.offsetMax = new Vector2 (-4f, -4f);
				_healthFill = fillRect.gameObject.AddComponent<Image> ();
				_healthFill.sprite = UiKit.WhiteSprite;
				_healthFill.type = Image.Type.Filled;
				_healthFill.fillMethod = Image.FillMethod.Horizontal;
				_healthFill.fillOrigin = 0;
				_healthFill.raycastTarget = false;

				// Boutons en haut à droite : pause, puis dynamite avec son compteur dessous
				_pauseButton = UiKit.Button (_hud, "PauseButton", UiKit.Sprite ("pause_normal"), UiKit.Sprite ("pause_hover"), null, 0f, topRight, new Vector2 (-32f, -32f), new Vector2 (120f, 130f), () => Invoke (OnPause));
				_dynamiteButton = UiKit.Button (_hud, "DynamiteButton", UiKit.Sprite ("button_explosion_normal"), UiKit.Sprite ("button_explosion_hover"), null, 0f, topRight, new Vector2 (-192f, -32f), new Vector2 (120f, 130f), () => Invoke (OnDynamite));
				_dynamites = UiKit.Text (_hud, "DynamiteCount", "0", 44f, TextAlignmentOptions.Center, topRight, new Vector2 (-192f, -168f), new Vector2 (120f, 50f));

				// Bascule son : même emplacement que la dynamite, visible seulement en pause
				_soundOn = UiKit.Sprite ("sound");
				_soundOff = UiKit.Sprite ("sound_off");
				_soundButton = UiKit.Button (_hud, "SoundButton", _soundOn, UiKit.Sprite ("sound_hover"), null, 0f, topRight, new Vector2 (-192f, -32f), new Vector2 (120f, 130f), () => Invoke (OnSoundToggle));
				_soundImage = _soundButton.GetComponent<Image> ();
				_soundButton.gameObject.SetActive (false);
		}

		public void SetCoins (int coins)
		{
				if (coins == _lastCoins) {
						return;
				}
				bool increased = coins > _lastCoins && _lastCoins != int.MinValue;
				_lastCoins = coins;
				_coins.text = coins.ToString ();
				if (increased) {
						Tween.Punch (_coinBox, 1.12f, 0.16f, true);
				}
		}

		public void SetDistance (int meters)
		{
				if (meters == _lastDistance) {
						return;
				}
				_lastDistance = meters;
				_distance.text = (meters >= 1000) ? (meters / 1000) + "K" : meters.ToString ();
		}

		public void SetDynamites (int count)
		{
				if (count == _lastDynamites) {
						return;
				}
				bool changed = _lastDynamites != int.MinValue;
				_lastDynamites = count;
				_dynamites.text = count.ToString ();
				if (changed) {
						Tween.Punch (_dynamites.rectTransform, 1.4f, 0.18f, true);
				}
		}

		public void SetHealth (int health, int maxHealth)
		{
				float ratio = maxHealth > 0 ? Mathf.Clamp01 ((float)health / maxHealth) : 0f;
				if (Mathf.Approximately (ratio, _lastHealthRatio)) {
						return;
				}
				bool decreased = ratio < _lastHealthRatio;
				_lastHealthRatio = ratio;
				_healthFill.fillAmount = ratio;
				_healthFill.color = Color.Lerp (new Color (0.85f, 0.15f, 0.15f), new Color (0.25f, 0.8f, 0.3f), ratio);

				if (decreased) {
						Tween.ShakeRect (_healthBar, 6f, 0.3f);
				}

				bool low = ratio > 0f && ratio <= LowHealthRatio;
				if (low && _lowHealthBlink == null) {
						_lowHealthBlink = StartCoroutine (LowHealthBlink ());
				} else if (!low && _lowHealthBlink != null) {
						StopCoroutine (_lowHealthBlink);
						_lowHealthBlink = null;
						var c = _healthFill.color;
						c.a = 1f;
						_healthFill.color = c;
				}
		}

		/// <summary>Le HUD encaisse un coup : la barre de vie tremble.</summary>
		public void OnDamage ()
		{
				Tween.ShakeRect (_healthBar, 8f, 0.3f);
		}

		// Sous un tiers de vie, la barre pulse (temps réel : fonctionne aussi pendant le hit-stop)
		private System.Collections.IEnumerator LowHealthBlink ()
		{
				while (true) {
						var c = _healthFill.color;
						c.a = 0.55f + 0.45f * Mathf.Abs (Mathf.Sin (Time.unscaledTime * 5f));
						_healthFill.color = c;
						yield return null;
				}
		}

		// ------------------------------------------------------------------ Pause

		private void BuildPausePanel (RectTransform root)
		{
				_pausePanel = UiKit.Panel (root, "PausePanel");
				var center = new Vector2 (0.5f, 0.5f);

				// Texture menu_paused (412x659) : onglet vert 0-10 % de la hauteur, zone rouge 12-92 %.
				// Les positions désignent le bord haut des éléments (pivot = ancre).
				var top = new Vector2 (0.5f, 1f);
				var box = UiKit.Image (_pausePanel, "Box", UiKit.Sprite ("menu_paused"), center, new Vector2 (0f, 40f), new Vector2 (720f, 1150f));
				UiKit.Text (box.transform, "Title", L ("Pause"), 80f, TextAlignmentOptions.Center, top, new Vector2 (0f, -8f), new Vector2 (600f, 100f));

				var normal = UiKit.Sprite ("button_normal");
				var hover = UiKit.Sprite ("button_hover");
				var size = new Vector2 (440f, 150f);
				const float firstButtonTop = -238f; // 4 boutons + 3 espaces de 40, centrés dans la zone rouge
				const float step = 190f;
				UiKit.Button (box.transform, "Resume", normal, hover, L ("Resume"), 56f, top, new Vector2 (0f, firstButtonTop), size, () => Invoke (OnResume));
				UiKit.Button (box.transform, "Restart", normal, hover, L ("Restart"), 56f, top, new Vector2 (0f, firstButtonTop - step), size, () => Invoke (OnRestart));
				UiKit.Button (box.transform, "Menu", normal, hover, L ("Menu"), 56f, top, new Vector2 (0f, firstButtonTop - 2 * step), size, () => Invoke (OnMenu));
				UiKit.Button (box.transform, "Quit", normal, hover, L ("Quit"), 56f, top, new Vector2 (0f, firstButtonTop - 3 * step), size, () => Invoke (OnQuit));
		}

		public void ShowPause (bool visible, bool soundMuted)
		{
				_pausePanel.gameObject.SetActive (visible);
				if (visible) {
						UiKit.PopIn (this, _pausePanel);
				}
				_dynamiteButton.gameObject.SetActive (!visible);
				_dynamites.gameObject.SetActive (!visible);
				_pauseButton.gameObject.SetActive (!visible);
				_soundButton.gameObject.SetActive (visible);
				SetSoundMuted (soundMuted);
		}

		public void SetSoundMuted (bool muted)
		{
				_soundImage.sprite = muted ? _soundOff : _soundOn;
		}

		// ------------------------------------------------------------------ Mort

		private void BuildDeathPanel (RectTransform root)
		{
				_deathPanel = UiKit.Panel (root, "DeathPanel");
				var center = new Vector2 (0.5f, 0.5f);

				var message = UiKit.Text (_deathPanel, "Message", L ("YouAreDead"), 44f, TextAlignmentOptions.Center, new Vector2 (0.5f, 1f), new Vector2 (0f, -60f), new Vector2 (960f, 180f));
				message.enableAutoSizing = true;
				message.fontSizeMax = 44f;
				message.fontSizeMin = 28f;

				// Texture highscorebox (687x727) : en-tête vert 6-18 % de la hauteur, zone rouge 18-84 %.
				var top = new Vector2 (0.5f, 1f);
				var box = UiKit.Image (_deathPanel, "Box", UiKit.Sprite ("highscorebox"), center, new Vector2 (0f, -40f), new Vector2 (940f, 995f));
				UiKit.Text (box.transform, "Title", L ("YourScore"), 70f, TextAlignmentOptions.Center, top, new Vector2 (0f, -69f), new Vector2 (800f, 100f));

				float y = -230f;
				const float step = 90f;
				_deathDistance = StatLine (box.transform, "Distance", y);
				_deathCoins = StatLine (box.transform, "Coins", y - step);
				_deathTaps = StatLine (box.transform, "Taps", y - 2 * step);
				_deathFinal = StatLine (box.transform, "Final", y - 3 * step);
				_deathFinal.fontSize = 56f;

				var buttonSize = new Vector2 (170f, 184f);
				UiKit.Button (box.transform, "Home", UiKit.Sprite ("button_home_normal"), UiKit.Sprite ("button_home_hover"), null, 0f, top, new Vector2 (-130f, -620f), buttonSize, () => Invoke (OnHome));
				UiKit.Button (box.transform, "Replay", UiKit.Sprite ("button_replay_normal"), UiKit.Sprite ("button_replay_hover"), null, 0f, top, new Vector2 (130f, -620f), buttonSize, () => Invoke (OnReplay));
		}

		private static TextMeshProUGUI StatLine (Transform parent, string name, float y)
		{
				return UiKit.Text (parent, name, string.Empty, 46f, TextAlignmentOptions.Center, new Vector2 (0.5f, 1f), new Vector2 (0f, y), new Vector2 (800f, 80f));
		}

		public void ShowDeath (int meters, int coins, int taps, int finalScore)
		{
				_deathDistance.text = L ("Distance") + " " + meters + "m";
				_deathCoins.text = L ("CollectedCoins") + " " + coins;
				_deathTaps.text = L ("NbTap") + " " + taps;
				_deathFinal.text = L ("FinalScore") + " " + finalScore;
				_deathPanel.gameObject.SetActive (true);
				_hud.gameObject.SetActive (false);
				UiKit.PopIn (this, _deathPanel);
		}

		public void HideDeath ()
		{
				_deathPanel.gameObject.SetActive (false);
				_hud.gameObject.SetActive (true);
		}

		private static void Invoke (Action action)
		{
				if (action != null) {
						action ();
				}
		}
}
