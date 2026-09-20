using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{

		public float CameraSpeed;
		public int cameraTick = 0;
		private Vector3 _initialPos;
		public float Distance;
		public Transform EndZoneTransform = null;
		private bool _finished = false;

		// Murs latéraux : deux BoxCollider2D posés sur la caméra dans la scène. Leur offset
		// était figé pour un écran 9:16 ; on les recale sur la largeur réellement visible.
		private Camera _camera;
		private BoxCollider2D _leftWall, _rightWall;
		private float _fittedAspect = -1f;

		// Secousse : décalage appliqué par-dessus le défilement, retiré à la fin
		private Vector3 _shakeOffset;
		private Coroutine _shake;

		void Start ()
		{

				_initialPos = transform.position;
				Distance = 0;

				_camera = GetComponent<Camera> ();
				foreach (var wall in GetComponents<BoxCollider2D> ()) {
						if (wall.offset.x < 0f) {
								_leftWall = wall;
						} else if (wall.offset.x > 0f) {
								_rightWall = wall;
						}
				}
				FitSideWalls ();
		}

		void FixedUpdate ()
		{
				if (_camera != null && !Mathf.Approximately (_camera.aspect, _fittedAspect)) {
						FitSideWalls ();
				}

				if (!_finished) {

						if (EndZoneTransform != null) {
								var diff = transform.position - EndZoneTransform.position;
								if (diff.y <= 0) {
										_finished = true;
								} else {
										TickCamera ();
								}
						} else {
								TickCamera ();
						}
				}
		}

		/// <summary>
		/// Colle la face interne de chaque mur au bord visible de la caméra orthographique,
		/// quel que soit le ratio d'écran (9:16, 20:9, fenêtre d'éditeur...).
		/// </summary>
		private void FitSideWalls ()
		{
				if (_camera == null || !_camera.orthographic) {
						return;
				}
				_fittedAspect = _camera.aspect;

				float halfHeight = _camera.orthographicSize;
				float halfWidth = halfHeight * _camera.aspect;

				if (_leftWall != null) {
						_leftWall.size = new Vector2 (_leftWall.size.x, halfHeight * 2f);
						_leftWall.offset = new Vector2 (-(halfWidth + _leftWall.size.x * 0.5f), 0f);
				}
				if (_rightWall != null) {
						_rightWall.size = new Vector2 (_rightWall.size.x, halfHeight * 2f);
						_rightWall.offset = new Vector2 (halfWidth + _rightWall.size.x * 0.5f, 0f);
				}
		}

		public void TickCamera ()
		{
				// Ce script est posé sur la caméra principale : transform suffit.
				var newPosition = transform.position;
				newPosition.y -= CameraSpeed;
				transform.position = newPosition;
				++cameraTick;
				Distance = Vector3.Distance (_initialPos, transform.position - _shakeOffset);
		}

		/// <summary>Secousse décroissante (dynamite, coup, mort). Continue même si le composant est désactivé.</summary>
		public void Shake (float amplitude, float duration)
		{
				if (_shake != null) {
						StopCoroutine (_shake);
						transform.position -= _shakeOffset;
						_shakeOffset = Vector3.zero;
				}
				_shake = StartCoroutine (ShakeRoutine (amplitude, duration));
		}

		private IEnumerator ShakeRoutine (float amplitude, float duration)
		{
				float elapsed = 0f;
				while (elapsed < duration) {
						elapsed += Time.unscaledDeltaTime;
						float k = 1f - Mathf.Clamp01 (elapsed / duration);
						transform.position -= _shakeOffset;
						_shakeOffset = (Vector3)(Random.insideUnitCircle * amplitude * k);
						transform.position += _shakeOffset;
						yield return null;
				}
				transform.position -= _shakeOffset;
				_shakeOffset = Vector3.zero;
				_shake = null;
		}

}
