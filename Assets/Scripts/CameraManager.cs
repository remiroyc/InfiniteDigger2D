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
				Distance = Vector3.Distance (_initialPos, transform.position);
		}

}
