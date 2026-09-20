using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Stockage local du profil et du classement (PlayerPrefs).
/// Remplace l'ancien backend PHP (infinitedigger.azurewebsites.net), hors ligne depuis 2015.
/// La surface publique est conservée pour Menu et GameManager ; tout est désormais synchrone.
/// </summary>
public class DBScript : MonoBehaviour
{
		private const string UsernameKey = "Username";
		private const string HighscoresKey = "LocalHighscores";
		private const int MaxEntries = 10;
		private const char EntrySeparator = ';';
		private const char FieldSeparator = '|';

		public Player User = null;
		public List<Highscore> Highscore = null;
		private Menu _menu;

		void Awake ()
		{
				DontDestroyOnLoad (transform.gameObject);
				_menu = FindAnyObjectByType<Menu> ();
				LoadUser ();
		}

		void Start ()
		{
				if (_menu == null) {
						return;
				}

				_menu.ConnectionCallback (true);

				// Premier lancement : on demande un pseudo avant tout, comme avant
				if (string.IsNullOrEmpty (User.Username)) {
						_menu.CurrentMenuState = MenuState.OPTION;
				}
		}

		/// <summary>Le Menu est recréé à chaque chargement de la scène, DBScript non : on le rebranche.</summary>
		public void BindMenu (Menu menu)
		{
				_menu = menu;
		}

		public void SaveUsername (string username)
		{
				username = (username ?? string.Empty).Trim ();
				if (username.Length == 0) {
						if (_menu != null) {
								_menu.SaveUsernameCallback (0);
						}
						return;
				}

				PlayerPrefs.SetString (UsernameKey, username);
				PlayerPrefs.Save ();
				User.Username = username;

				foreach (var entry in Highscore) {
						entry.PlayerName = username;
				}

				if (_menu != null) {
						_menu.SaveUsernameCallback (3);
				}
		}

		public void GetHighscore ()
		{
				Highscore = LoadEntries ();
		}

		public void SaveScore (int score)
		{
				if (score <= 0) {
						return;
				}

				var entries = LoadEntries ();
				entries.Add (new Highscore () {
						Meters = score,
						PlayerName = User.Username,
						Date = DateTime.Now
				});

				entries.Sort ((a, b) => b.Meters.CompareTo (a.Meters));
				if (entries.Count > MaxEntries) {
						entries.RemoveRange (MaxEntries, entries.Count - MaxEntries);
				}
				for (int i = 0; i < entries.Count; i++) {
						entries [i].Rank = i + 1;
				}

				PlayerPrefs.SetString (HighscoresKey, Serialize (entries));
				PlayerPrefs.Save ();

				Highscore = entries;
				User.BestScore = entries [0].Meters;
		}

		private void LoadUser ()
		{
				User = new Player () {
						Username = PlayerPrefs.GetString (UsernameKey, string.Empty)
				};
				Highscore = LoadEntries ();
				User.BestScore = Highscore.Count > 0 ? Highscore [0].Meters : 0;
		}

		// Format : score|ticks;score|ticks;... trié par score décroissant
		private List<Highscore> LoadEntries ()
		{
				var entries = new List<Highscore> ();
				var raw = PlayerPrefs.GetString (HighscoresKey, string.Empty);
				if (string.IsNullOrEmpty (raw)) {
						return entries;
				}

				var playerName = User != null ? User.Username : string.Empty;
				foreach (var item in raw.Split (EntrySeparator)) {
						var fields = item.Split (FieldSeparator);
						int meters;
						long ticks;
						if (fields.Length < 2 || !int.TryParse (fields [0], out meters) || !long.TryParse (fields [1], out ticks)) {
								continue;
						}
						entries.Add (new Highscore () {
								Meters = meters,
								PlayerName = playerName,
								Date = new DateTime (ticks)
						});
				}

				entries.Sort ((a, b) => b.Meters.CompareTo (a.Meters));
				for (int i = 0; i < entries.Count; i++) {
						entries [i].Rank = i + 1;
				}
				return entries;
		}

		private static string Serialize (List<Highscore> entries)
		{
				var parts = new string[entries.Count];
				for (int i = 0; i < entries.Count; i++) {
						parts [i] = entries [i].Meters + FieldSeparator.ToString () + entries [i].Date.Ticks;
				}
				return string.Join (EntrySeparator.ToString (), parts);
		}
}

public class Highscore
{
		public int Rank { get; set; }

		public string PlayerName { get; set; }

		public int Meters { get; set; }

		public DateTime Date { get; set; }
}

public class Player
{
		public string Username { get; set; }

		public int BestScore { get; set; }

		public Player ()
		{
				Username = string.Empty;
				BestScore = 0;
		}
}
