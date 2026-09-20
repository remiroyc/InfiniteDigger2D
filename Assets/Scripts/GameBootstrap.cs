using UnityEngine;

/// <summary>
/// Réglages runtime appliqués avant le chargement de la première scène.
/// </summary>
public static class GameBootstrap
{
		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize ()
		{
				// Sans cible explicite, Android et iOS plafonnent à 30 fps : le jeu est un runner
				// physique, 60 fps change la sensation de contrôle. vSync est à 0 dans QualitySettings.
				Application.targetFrameRate = 60;
		}
}
