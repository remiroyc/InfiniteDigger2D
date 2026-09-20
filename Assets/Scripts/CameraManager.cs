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

		void Start ()
		{

				_initialPos = transform.position;
				Distance = 0;

		}

		void FixedUpdate ()
		{
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
