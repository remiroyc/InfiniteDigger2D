using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DBScript : MonoBehaviour
{

		public const string HASH = "284edb632dfffec02a4a72e736ed80f4";
		public const string BASE_URL = "http://infinitedigger.azurewebsites.net/";
		public Player User = null;
		public List<Highscore> Highscore = null;
		private bool _coroutine = false;
		private Menu _menu;

		void Awake ()
		{
				DontDestroyOnLoad (transform.gameObject);
				_menu = FindObjectOfType<Menu> ();
		}

		IEnumerator Start ()
		{
				string url = BASE_URL + "add_player.php?name=" + WWW.EscapeURL (SystemInfo.deviceUniqueIdentifier) + "&hash=" + WWW.EscapeURL (HASH);
				Debug.Log (url);
				WWW result = new WWW (url);
				yield return result;

				if (result.error == null) {

						if (User == null) {
								User = new Player ();
						}
						GetPlayer (result.text);

						if (_menu != null) {
								_menu.ConnectionCallback (true);
								// On test si l'utilisateur a déja au moins édité son pseudo
								if (string.IsNullOrEmpty (PlayerPrefs.GetString ("Username"))) {
										_menu.CurrentMenuState = MenuState.OPTION;
								}
						}

				} else {
						_menu.ConnectionCallback (false);
				}
		}

		IEnumerator SaveUsername (string username)
		{
				PlayerPrefs.SetString ("Username", username);
				string url = BASE_URL + "save_username.php?id=" + User.Id + "&password=" + WWW.EscapeURL (User.Password) + "&username=" + WWW.EscapeURL (username);
				WWW res = new WWW (url);
				Debug.Log (url);
				yield return res;
		
				try {

						if (string.IsNullOrEmpty (res.error)) {
								User.Username = username;
						}

						int result = 0;
						int.TryParse (res.text, out result);
						_menu.SaveUsernameCallback (result);
		
				} catch {
						_menu.CurrentMenuState = MenuState.MENU;
				}
		}

		IEnumerator GetHighscore ()
		{
				string url = BASE_URL + "get_highscore.php";
				WWW res = new WWW (url);
				yield return res;
				
				var tab = res.text.Split ('%');
				Highscore = new List<Highscore> ();


				for (int i = 1; i < tab.Length; i++) {
					
						var item = tab [i - 1];
						var itemTab = item.Split ('|');

						var score = itemTab [0];
						var username = itemTab [1];
						var date = itemTab [2];


						DateTime dtt;
						DateTime.TryParse (date, out dtt);

						Highscore.Add (new Highscore (){
							Meters = int.Parse(score),
							PlayerName = username,
							Rank = i,
							Date = dtt
						}); 

				}

		}

		IEnumerator SaveScore (int score)
		{
				if (!_coroutine) {
						_coroutine = true;
						if (User != null && User.Id != 0 && score > 0) {
								string url = BASE_URL + "add_score.php?id=" + User.Id + "&score=" + score + "&platform=" + WWW.EscapeURL (Application.platform.ToString ()) 
										+ "&password=" + WWW.EscapeURL (User.Password);
								Debug.Log (url);
								WWW res = new WWW (url);
								yield return res;
						}
						_coroutine = false;
				}
		}

		public void GetPlayer (string str)
		{
				Debug.Log (str);
				var tab = str.Split ('|');

				int id;
				if (!string.IsNullOrEmpty (tab [0])) {
						if (int.TryParse (tab [0], out id)) {
								User.Id = id;
						}
				}

				if (tab.Length > 2 && !string.IsNullOrEmpty (tab [1])) {
						User.Username = tab [1];
				} else {
						User.Username = "Player" + User.Id;
				}

				if (!string.IsNullOrEmpty (tab [2])) {
						User.Password = tab [2];
				}

				if (!string.IsNullOrEmpty (tab [3])) {
					int bestScore = 0;
					if(int.TryParse(tab [3], out bestScore)){
						User.BestScore = bestScore;
					}
				}

		}

}

public class Highscore
{

		public int Rank {
				get;
				set;
		}

		public string PlayerName {
				get;
				set;
		}

		public int Meters {
				get;
				set;
		}

		public DateTime Date {
				get;
				set;
		}

}

public class Player
{
		public string Username {
				get;
				set;
		}

		public int Id {
				get;
				set;
		}

		public int BestScore {
				get;
				set;
		}
		
		public string Password { get; set; }

		public Player ()
		{
				Username = string.Empty;
				Id = 0;
				BestScore = 0;
				Password = string.Empty;
		}

}