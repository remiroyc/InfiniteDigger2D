using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fondu au noir entre les scènes. Le voile est un Canvas persistant au-dessus de tout.
/// </summary>
public static class SceneFader
{
		private const float FadeOut = 0.25f;
		private const float FadeIn = 0.3f;

		private static CanvasGroup _group;
		private static bool _busy;

		public static void LoadScene (string sceneName)
		{
				if (_busy) {
						return;
				}
				Tween.Run (LoadRoutine (sceneName));
		}

		private static IEnumerator LoadRoutine (string sceneName)
		{
				_busy = true;
				var group = Group ();
				group.blocksRaycasts = true;
				yield return Tween.Fade (group, 0f, 1f, FadeOut);

				Time.timeScale = 1f;
				SceneManager.LoadScene (sceneName);
				yield return null;

				yield return Tween.Fade (group, 1f, 0f, FadeIn);
				group.blocksRaycasts = false;
				_busy = false;
		}

		private static CanvasGroup Group ()
		{
				if (_group != null) {
						return _group;
				}
				var canvas = UiKit.CreateCanvas ("~SceneFader", 100);
				Object.DontDestroyOnLoad (canvas.gameObject);
				var veil = UiKit.Fill (canvas.transform, "Veil", Color.black);
				veil.raycastTarget = true;
				_group = canvas.gameObject.AddComponent<CanvasGroup> ();
				_group.alpha = 0f;
				_group.blocksRaycasts = false;
				return _group;
		}
}
