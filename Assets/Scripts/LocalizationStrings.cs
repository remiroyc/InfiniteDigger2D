using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LocalizationStrings
{
		private static LocalizationStrings instance = null;

		public Dictionary<string,string> Values {
				get;
				set;
		}

		private LocalizationStrings ()
		{

				Values = new Dictionary<string, string> ();

				switch (Application.systemLanguage) {

				case SystemLanguage.French:

						Values.Add ("Loading", "Chargement...");
						Values.Add ("TryingToConnect", "Connexion en cours...");
						Values.Add ("YouAreConnected", "Joueur : {0}\nMeilleur score : {1}");
						Values.Add ("Play", "Jouer");
						Values.Add ("Tutorial", "Tutoriel");
						Values.Add ("Highscore", "Classement");
						Values.Add ("Options", "Options");
						Values.Add ("ChooseUsername", "Choisis ton pseudo :");
						Values.Add ("Save", "Sauvegarder");
						Values.Add ("FirstGameMessage", "Bienvenue. Ne perds pas de temps et creuse !");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Reprendre");
						Values.Add ("Restart", "Relancer");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Quitter");
						Values.Add ("NoDynamite", "Arf, tu n'as plus assez d'explosifs");
						Values.Add ("BreakYourRecord", "Plus que {0}m pour exploser ton record !");
						Values.Add ("BeCareful", "Sois prudent, il y a beaucoup de dangers ici !");
						Values.Add ("GoodJobContinue", "Tu fais du bon boulot, continue comme ça !");
						Values.Add ("YouAreDead", "Désolé, tu es mort. Analyse ta performance et retente vite ta chance pour exploser le record !");
						Values.Add ("YourScore", "Ton score");
						Values.Add ("CollectedCoins", "Pièces collectées :");
						Values.Add ("NbDestroyedObject", "Objets détruits :");
						Values.Add ("NbTap", "Nombre de coups de pioche :");
						Values.Add ("FinalScore", "Score final :");
						Values.Add ("Distance", "Distance :");
						Values.Add ("LastScore", "Dernier score");
						Values.Add ("ChooseUsernameDesc", "Avant tout, choisis le pseudo qui apparaîtra dans ton classement.");
						Values.Add ("UpdateWasNotSuccessful", "Le pseudo ne peut pas être vide.");
						Values.Add ("PwdAlreadyTaken", "Désolé, ce nom est déjà pris. Essaye d'en choisir un autre !");
						Values.Add ("LevelSelection", "Sélection du niveau");
						Values.Add ("ChallengeMode", "Mode record");
						Values.Add ("NoScoreYet", "Aucun score enregistré pour le moment. Lance une partie !");

						break;

				case SystemLanguage.Spanish:

						Values.Add ("Loading", "Cargando...");
						Values.Add ("TryingToConnect", "Intentando conectar...");
						Values.Add ("YouAreConnected", "Jugador: {0}\nMejor puntuación: {1}");
						Values.Add ("Play", "Jugar");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Clasificación");
						Values.Add ("Options", "Opciones");
						Values.Add ("ChooseUsername", "Elige tu apodo:");
						Values.Add ("Save", "Guardar");
						Values.Add ("FirstGameMessage", "Bienvenido. No hay tiempo que perder, ¡es hora de cavar!");
						Values.Add ("Pause", "Pausa");
						Values.Add ("Resume", "Reanudar");
						Values.Add ("Restart", "Reiniciar");
						Values.Add ("Menu", "Menú");
						Values.Add ("Quit", "Salir");
						Values.Add ("NoDynamite", "No tienes suficiente dinamita.");
						Values.Add ("BreakYourRecord", "¡{0} m para batir tu récord!");
						Values.Add ("BeCareful", "Ten cuidado, aquí caen muchas rocas");
						Values.Add ("GoodJobContinue", "¡Buen trabajo! Sigue cavando...");
						Values.Add ("YouAreDead", "Has muerto. ¡Mira tu puntuación y vuelve a intentarlo!");
						Values.Add ("YourScore", "Tu puntuación");
						Values.Add ("CollectedCoins", "Monedas conseguidas:");
						Values.Add ("NbDestroyedObject", "Objetos destruidos:");
						Values.Add ("NbTap", "Número de golpes:");
						Values.Add ("FinalScore", "Puntuación final:");
						Values.Add ("Distance", "Distancia:");
						Values.Add ("LastScore", "Última puntuación");
						Values.Add ("ChooseUsernameDesc", "Antes de empezar, elige el apodo que aparecerá en tu clasificación.");
						Values.Add ("UpdateWasNotSuccessful", "El apodo no puede estar vacío.");
						Values.Add ("PwdAlreadyTaken", "Este apodo ya está en uso. Elige otro.");
						Values.Add ("LevelSelection", "Selección de nivel");
						Values.Add ("ChallengeMode", "Modo récord");
						Values.Add ("NoScoreYet", "Todavía no hay puntuaciones. ¡Juega una partida!");
						break;

				// Le bloc historique était étiqueté Dutch mais contenait de l'allemand.
				case SystemLanguage.German:

						Values.Add ("Loading", "Laden...");
						Values.Add ("TryingToConnect", "Verbindung wird hergestellt...");
						Values.Add ("YouAreConnected", "Spieler: {0}\nBeste Punktzahl: {1}");
						Values.Add ("Play", "Spielen");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Rangliste");
						Values.Add ("Options", "Optionen");
						Values.Add ("ChooseUsername", "Wähle deinen Spitznamen:");
						Values.Add ("Save", "Speichern");
						Values.Add ("FirstGameMessage", "Willkommen. Keine Zeit zu verlieren, fang an zu graben!");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Weiter");
						Values.Add ("Restart", "Neustart");
						Values.Add ("Menu", "Menü");
						Values.Add ("Quit", "Beenden");
						Values.Add ("NoDynamite", "Du hast nicht genug Dynamit.");
						Values.Add ("BreakYourRecord", "Noch {0} m bis zu deinem Rekord!");
						Values.Add ("BeCareful", "Vorsicht, hier fallen viele Steine!");
						Values.Add ("GoodJobContinue", "Gut gemacht! Grab weiter...");
						Values.Add ("YouAreDead", "Du bist tot. Sieh dir deine Punktzahl an und versuch es gleich noch einmal!");
						Values.Add ("YourScore", "Dein Ergebnis");
						Values.Add ("CollectedCoins", "Gesammelte Münzen:");
						Values.Add ("NbDestroyedObject", "Zerstörte Objekte:");
						Values.Add ("NbTap", "Anzahl der Schläge:");
						Values.Add ("FinalScore", "Endstand:");
						Values.Add ("Distance", "Tiefe:");
						Values.Add ("LastScore", "Letzte Punktzahl");
						Values.Add ("ChooseUsernameDesc", "Wähle zuerst den Spitznamen, der in deiner Rangliste erscheint.");
						Values.Add ("UpdateWasNotSuccessful", "Der Spitzname darf nicht leer sein.");
						Values.Add ("PwdAlreadyTaken", "Dieser Spitzname ist bereits vergeben. Wähle einen anderen.");
						Values.Add ("LevelSelection", "Levelauswahl");
						Values.Add ("ChallengeMode", "Rekordmodus");
						Values.Add ("NoScoreYet", "Noch keine Punktzahl. Spiel eine Runde!");
						break;

				case SystemLanguage.Italian:

						Values.Add ("Loading", "Caricamento...");
						Values.Add ("TryingToConnect", "Connessione in corso...");
						Values.Add ("YouAreConnected", "Giocatore: {0}\nMiglior punteggio: {1}");
						Values.Add ("Play", "Gioca");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Classifica");
						Values.Add ("Options", "Opzioni");
						Values.Add ("ChooseUsername", "Scegli il tuo nickname:");
						Values.Add ("Save", "Salva");
						Values.Add ("FirstGameMessage", "Benvenuto. Non c'è tempo da perdere, è il momento di scavare!");
						Values.Add ("Pause", "Pausa");
						Values.Add ("Resume", "Riprendi");
						Values.Add ("Restart", "Ricomincia");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Esci");
						Values.Add ("NoDynamite", "Non hai abbastanza dinamite.");
						Values.Add ("BreakYourRecord", "Ancora {0} m per battere il tuo record!");
						Values.Add ("BeCareful", "Attenzione, qui cadono molti massi!");
						Values.Add ("GoodJobContinue", "Ottimo lavoro! Continua a scavare...");
						Values.Add ("YouAreDead", "Sei morto. Controlla il tuo punteggio e riprova subito!");
						Values.Add ("YourScore", "Il tuo punteggio");
						Values.Add ("CollectedCoins", "Monete raccolte:");
						Values.Add ("NbDestroyedObject", "Oggetti distrutti:");
						Values.Add ("NbTap", "Numero di colpi:");
						Values.Add ("FinalScore", "Punteggio finale:");
						Values.Add ("Distance", "Profondità:");
						Values.Add ("LastScore", "Ultimo punteggio");
						Values.Add ("ChooseUsernameDesc", "Prima di tutto, scegli il nickname che apparirà nella tua classifica.");
						Values.Add ("UpdateWasNotSuccessful", "Il nickname non può essere vuoto.");
						Values.Add ("PwdAlreadyTaken", "Questo nickname è già in uso. Scegline un altro.");
						Values.Add ("LevelSelection", "Selezione livello");
						Values.Add ("ChallengeMode", "Modalità record");
						Values.Add ("NoScoreYet", "Nessun punteggio ancora. Gioca una partita!");
						break;

				default:

						Values.Add ("Loading", "Loading...");
						Values.Add ("TryingToConnect", "Trying to connect...");
						Values.Add ("YouAreConnected", "Player: {0}\nBest score: {1}");
						Values.Add ("Play", "Play");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Highscore");
						Values.Add ("Options", "Options");
						Values.Add ("ChooseUsername", "Choose your nickname:");
						Values.Add ("Save", "Save");
						Values.Add ("FirstGameMessage", "Welcome. There ain't no time to lose, it's time to dig!");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Resume");
						Values.Add ("Restart", "Restart");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Quit");
						Values.Add ("NoDynamite", "You don't have enough dynamite.");
						Values.Add ("BreakYourRecord", "{0} m left to break your record!");
						Values.Add ("BeCareful", "Be careful, there's a lot of rockfall here!");
						Values.Add ("GoodJobContinue", "Good job! Keep digging...");
						Values.Add ("YouAreDead", "You are dead. Check your score and try again!");
						Values.Add ("YourScore", "Your score");
						Values.Add ("CollectedCoins", "Collected coins:");
						Values.Add ("NbDestroyedObject", "Destroyed objects:");
						Values.Add ("NbTap", "Number of hits:");
						Values.Add ("FinalScore", "Final score:");
						Values.Add ("Distance", "Distance:");
						Values.Add ("LastScore", "Last score");
						Values.Add ("ChooseUsernameDesc", "First, choose the nickname that will appear in your rankings.");
						Values.Add ("UpdateWasNotSuccessful", "The nickname can't be empty.");
						Values.Add ("PwdAlreadyTaken", "This nickname is already taken. Please choose another one.");
						Values.Add ("LevelSelection", "Level selection");
						Values.Add ("ChallengeMode", "Challenge mode");
						Values.Add ("NoScoreYet", "No score yet. Play a game!");
						break;

				}
		}

		public static LocalizationStrings Instance {
				get {
						if (instance == null) {
								instance = new LocalizationStrings ();
						}
						return instance;
				}
		}
}
