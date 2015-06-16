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
								var diff = Camera.main.transform.position - EndZoneTransform.position;
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
				var newPosition = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y - CameraSpeed, Camera.main.transform.position.z);
				Camera.main.transform.position = newPosition;
				++cameraTick;
				Distance = Vector3.Distance (_initialPos, transform.position);
		}

}
