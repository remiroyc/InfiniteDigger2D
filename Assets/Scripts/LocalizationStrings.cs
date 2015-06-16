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
						Values.Add ("YouAreConnected", "Tu es connecté en tant que : {0}.\nTon meilleur score : {1}");
						Values.Add ("Play", "Jouer");
						Values.Add ("Tutorial", "Tutoriel");
						Values.Add ("Highscore", "Classement");
						Values.Add ("Options", "Options");
						Values.Add ("ChooseUsername", "Choisis ton pseudo :");
						Values.Add ("Save", "Sauvegarder");
						Values.Add ("FirstGameMessage", "Bienvenue. Ne perd pas de temps et creuse !");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Reprendre");
						Values.Add ("Restart", "Relancer");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Quitter");
						Values.Add ("NoDynamite", "Arf, tu n'as plus assez d'explosifs");
						Values.Add ("BreakYourRecord", "Plus que {0}m pour exploser ton record!");
						Values.Add ("BeCareful", "Sois prudent il y a beaucoup de dangers ici!");
						Values.Add ("GoodJobContinue", "Tu fais du bon boulot, continue comme ça !");
						Values.Add ("YouAreDead", "Désole, tu es mort. Analyse ta performance et retente vite ta chance pour exploser le record !");
						Values.Add ("YourScore", "Ton score");
						Values.Add ("CollectedCoins", "Pièces collectées :");
						Values.Add ("NbDestroyedObject", "Number of destroyed objects :");
						Values.Add ("NbTap", "Nombre de coups de pioche :");
						Values.Add ("FinalScore", "Score final :");
						Values.Add ("Distance", "Distance :");
						Values.Add ("LastScore", "Dernier score");
						Values.Add ("ChooseUsernameDesc", "Avant tout, choisis un pseudo qui sera visible dans le classement mondial.");
						Values.Add ("UpdateWasNotSuccessful", "La mise à jour ne fait pas effectuée correctement. Vérifie ta connexion internet et réassaye.");
						Values.Add ("PwdAlreadyTaken", "Désolé tu arrives trop tard, ce nom est déja pris. Essaye d'en choisir un autre !");
						Values.Add ("LevelSelection", "Sélection du niveau");
						Values.Add ("ChallengeMode", "Mode record");

						break;
		
				case SystemLanguage.Spanish:
				
						Values.Add ("Loading", "Loading...");
						Values.Add ("TryingToConnect", "Intentando conectar...");
						Values.Add ("YouAreConnected", "You're logged : {0}.\nYour best score : {1}");
						Values.Add ("Play", "Juego");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Máxima puntuación");
						Values.Add ("Options", "Opciones");
						Values.Add ("ChooseUsername", "Elija su nombre de usuario :");
						Values.Add ("Save", "Guardar");
						Values.Add ("FirstGameMessage", "Bienvenido. No hay tiempo que perder, es hora de cavar!");
						Values.Add ("Pause", "Pausa");
						Values.Add ("Resume", "Resume");
						Values.Add ("Restart", "Reanudar");
						Values.Add ("Menu", "Menú");
						Values.Add ("Quit", "Quit");
						Values.Add ("NoDynamite", "Usted no tiene suficiente dinamita.");
						Values.Add ("BreakYourRecord", "{0} metros para romper su récord personal.");
						Values.Add ("BeCareful", "Ten cuidado, hay una gran cantidad de caída de rocas");
						Values.Add ("GoodJobContinue", "Has hecho un buen trabajo! Continuar a profundizar ...");
						Values.Add ("YouAreDead", "Usted está muerto, pero comprueba su puntuación y la repetición!");
						Values.Add ("YourScore", "Su puntuación");
						Values.Add ("CollectedCoins", "Monedas conseguidas :");
						Values.Add ("NbDestroyedObject", "Número de objetos destruidos :");
						Values.Add ("NbTap", "Número de ataques :");
						Values.Add ("FinalScore", "Puntuación final :");
						Values.Add ("Distance", "Distancia :");
						Values.Add ("LastScore", "Última puntuación");
						Values.Add ("ChooseUsernameDesc", "Elija un apodo que será visible en el ranking mundial.");
						Values.Add ("UpdateWasNotSuccessful", "Esta actualización no se realizó correctamente. Compruebe su conexión a Internet y vuelva a intentarlo.");
						Values.Add ("PwdAlreadyTaken", "Siento que es demasiado tarde, este nombre de usuario ya está en uso. Por favor, inténtelo de nuevo.");
						Values.Add ("LevelSelection", "Level selection");
						Values.Add ("ChallengeMode", "Challenge mode");
						break;
		
				case SystemLanguage.Dutch:
		
						Values.Add ("Loading", "Verladung...");
						Values.Add ("TryingToConnect", "Trying to connect...");
						Values.Add ("YouAreConnected", "Sie sind eingeloggt : {0}.\nIhre beste Punktzahl : {1}");
						Values.Add ("Play", "Wiedergabe");
						Values.Add ("Tutorial", "Lernprogramm");
						Values.Add ("Highscore", "Highscore");
						Values.Add ("Options", "Optionen");
						Values.Add ("ChooseUsername", "Choose your username :");
						Values.Add ("Save", "Sparen");
						Values.Add ("FirstGameMessage", "Herzlich Willkommen. There ain't no time to lose, it's time to dig !");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Resume");
						Values.Add ("Restart", "Restart");
						Values.Add ("Menu", "Menü");
						Values.Add ("Quit", "Verlassen");
						Values.Add ("NoDynamite", "Sie haben nicht genug Dynamit haben.");
						Values.Add ("BreakYourRecord", "{0} meters for break your personal record.");
						Values.Add ("BeCareful", "Seien Sie vorsichtig, es gibt eine Menge von Steinschlag");
						Values.Add ("GoodJobContinue", "Sie hat einen guten Job! Weiter, tiefer zu graben ...");
						Values.Add ("YouAreDead", "Sie sind tot, aber überprüfen Sie Ihre Punktzahl und Wiedergabe!");
						Values.Add ("YourScore", "Ihr Ergebnis");
						Values.Add ("CollectedCoins", "Gesammelte Münzen :");
						Values.Add ("NbDestroyedObject", "Anzahl der Objekte zerstört :");
						Values.Add ("NbTap", "Anzahl der Angriffe :");
						Values.Add ("FinalScore", "Endstand :");
						Values.Add ("Distance", "Abstand :");
						Values.Add ("LastScore", "Letzte Punktzahl");
						Values.Add ("ChooseUsernameDesc", "Wählen Sie einen Spitznamen, der in der Weltrangliste sichtbar sein wird.");
						Values.Add ("UpdateWasNotSuccessful", "Dieses Update war nicht erfolgreich. Überprüfen Sie Ihre Internetverbindung und versuchen Sie es erneut.");
						Values.Add ("PwdAlreadyTaken", "Leider haben Sie zu spät sind, ist dieser Benutzername bereits vergeben. Bitte versuchen Sie es erneut.");
						Values.Add ("LevelSelection", "Level selection");
						Values.Add ("ChallengeMode", "Challenge mode");
						break;
		
				case SystemLanguage.Italian:
		
						Values.Add ("Loading", "Caricamento...");
						Values.Add ("TryingToConnect", "Trying to connect...");
						Values.Add ("YouAreConnected", "Lei è registrato : {0}.\nIl tuo miglior punteggio : {1}");
						Values.Add ("Play", "Ascolta");
						Values.Add ("Tutorial", "Lezione");
						Values.Add ("Highscore", "Classifica");
						Values.Add ("Options", "Opzioni");
						Values.Add ("ChooseUsername", "Scegli il tuo username :");
						Values.Add ("Save", "Salvare");
						Values.Add ("FirstGameMessage", "Benvenuti. Non c'è tempo da perdere, è il momento di scavare!");
						Values.Add ("Pause", "Pausa");
						Values.Add ("Resume", "Resume");
						Values.Add ("Restart", "Restart");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Quit");
						Values.Add ("NoDynamite", "Non hai abbastanza dinamite.");
						Values.Add ("BreakYourRecord", "{0} meters for break your personal record.");
						Values.Add ("BeCareful", "Attenzione, ci sono un sacco di caduta massi");
						Values.Add ("GoodJobContinue", "Hai fatto un buon lavoro! Continuare a scavare più a fondo ...");
						Values.Add ("YouAreDead", "Voi siete morti, ma controllare il tuo punteggio e riproduzione!");
						Values.Add ("YourScore", "Il tuo punteggio");
						Values.Add ("CollectedCoins", "Collected coins :");
						Values.Add ("NbDestroyedObject", "Numero di oggetti distrutti :");
						Values.Add ("NbTap", "Numero di attacchi :");
						Values.Add ("FinalScore", "Punteggio finale :");
						Values.Add ("Distance", "Distanza :");
						Values.Add ("LastScore", "Ultimo punteggio");
						Values.Add ("ChooseUsernameDesc", "Scegli un nickname che sarà visibile nella classifica mondiale.");
						Values.Add ("UpdateWasNotSuccessful", "Questo aggiornamento non è riuscita. Controlla la tua connessione a Internet e riprova.");
						Values.Add ("PwdAlreadyTaken", "Spiacenti siete troppo tardi, questo il nome utente è già stato preso. Riprova.");
						Values.Add ("LevelSelection", "Level selection");
						Values.Add ("ChallengeMode", "Challenge mode");
						break;
		
				default:

						Values.Add ("Loading", "Loading...");
						Values.Add ("TryingToConnect", "Trying to connect...");
						Values.Add ("YouAreConnected", "You're logged : {0}.\nYour best score : {1}");
						Values.Add ("Play", "Play");
						Values.Add ("Tutorial", "Tutorial");
						Values.Add ("Highscore", "Highscore");
						Values.Add ("Options", "Options");
						Values.Add ("ChooseUsername", "Choose your username :");
						Values.Add ("Save", "Save");
						Values.Add ("FirstGameMessage", "Welcome. There ain't no time to lose, it's time to dig !");
						Values.Add ("Pause", "Pause");
						Values.Add ("Resume", "Resume");
						Values.Add ("Restart", "Restart");
						Values.Add ("Menu", "Menu");
						Values.Add ("Quit", "Quit");
						Values.Add ("NoDynamite", "You don't have enough dynamite.");
						Values.Add ("BreakYourRecord", "{0} meters for break your personal record.");
						Values.Add ("BeCareful", "Be careful, there are a lot of rockfall");
						Values.Add ("GoodJobContinue", "You did a good job ! Continue to dig deeper...");
						Values.Add ("YouAreDead", "You are dead, but check your score and replay !");
						Values.Add ("YourScore", "Your score");
						Values.Add ("CollectedCoins", "Collected coins :");
						Values.Add ("NbDestroyedObject", "Number of destroyed objects :");
						Values.Add ("NbTap", "Number of attacks :");
						Values.Add ("FinalScore", "Final score :");
						Values.Add ("Distance", "Distance :");
						Values.Add ("LastScore", "Last score");
						Values.Add ("ChooseUsernameDesc", "Choose a nickname that will be visible in the world rankings.");
						Values.Add ("UpdateWasNotSuccessful", "This update wasn't successful. Check your internet connection and try again.");
						Values.Add ("PwdAlreadyTaken", "Sorry you're too late, this username is already taken. Please try again.");
						Values.Add ("LevelSelection", "Level selection");
						Values.Add ("ChallengeMode", "Challenge mode");
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