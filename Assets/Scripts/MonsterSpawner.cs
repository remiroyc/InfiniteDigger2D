using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Fait apparaître les monstres sur le terrain. Pour l'instant : la taupe, qui surgit
/// d'une brique de la ligne où se tient le joueur après avoir fait sauter la brique du
/// dessus. Golem et Rat (prefabs prêts) viendront s'ajouter ici.
/// </summary>
public class MonsterSpawner
{
		private readonly GameObject _taupePrefab;
		private readonly TerrainGenerator _terrain;
		private readonly CharacterControllerScript _player;
		private readonly int _offset;
		private GameObject _burstPrefab;
		private float _lastSpawnTime;

		public float Cooldown = 5f;

		public MonsterSpawner (GameObject taupePrefab, TerrainGenerator terrain, CharacterControllerScript player, int offset)
		{
				_taupePrefab = taupePrefab;
				_terrain = terrain;
				_player = player;
				_offset = offset;
		}

		public void TrySpawnTaupe ()
		{
				if (_taupePrefab == null || _player == null || Time.time - _lastSpawnTime < Cooldown) {
						return;
				}
				var row = _player.GroundElementTouched != null ? _terrain.RowOf (_player.GroundElementTouched.GetComponent<GroundElement> ()) : null;
				if (row == null || row.GroundElements == null) {
						return; // en l'air : on réessaiera au prochain tick
				}

				var candidates = row.GroundElements.Where (g => g != null).ToArray ();
				if (candidates.Length == 0) {
						return;
				}
				var anchor = candidates [UnityEngine.Random.Range (0, candidates.Length)];
				int column = Array.IndexOf (row.GroundElements, anchor);

				var above = _terrain.RowAbove (row);
				if (above == null || above.GroundElements == null || column >= above.GroundElements.Length) {
						return;
				}
				var toBlast = above.GroundElements [column];
				if (toBlast == null) {
						return;
				}

				if (_burstPrefab == null) {
						_burstPrefab = Resources.Load<GameObject> ("BottomExplosion");
				}
				if (_burstPrefab != null) {
						UnityEngine.Object.Instantiate (_burstPrefab, toBlast.transform.position, Quaternion.identity);
				}
				UnityEngine.Object.Destroy (toBlast.gameObject);

				var taupe = UnityEngine.Object.Instantiate (_taupePrefab, anchor.transform.position + new Vector3 (0f, 0.75f, 0f), Quaternion.identity);
				taupe.transform.parent = anchor.transform;
				var script = taupe.GetComponent<TaupeScript> ();
				if (script != null) {
						script.Offset = _offset;
				}
				_lastSpawnTime = Time.time;
		}
}
