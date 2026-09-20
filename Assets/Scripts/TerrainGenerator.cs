using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Génère et recycle les lignes de terrain (GroundRaw) sous la caméra. Les lignes sont
/// empilées de haut en bas : la première de la file est la plus haute, la dernière créée
/// est la plus profonde. Sans état Unity propre, possédé par GameManager.
/// </summary>
public class TerrainGenerator
{
		private const float RowSpacing = 0.75f;
		private const float PatternChance = 0.02f;

		private readonly GameObject _rowPrefab;
		private readonly Camera _camera;
		private readonly int _offset;
		private readonly int _bricksPerRow;
		private readonly Queue<GroundRaw> _rows = new Queue<GroundRaw> ();
		private readonly Queue<char[]> _pendingPatterns = new Queue<char[]> ();
		private GroundRaw _lastRow;

		public TerrainGenerator (GameObject rowPrefab, Camera camera, int offset, int bricksPerRow)
		{
				_rowPrefab = rowPrefab;
				_camera = camera;
				_offset = offset;
				_bricksPerRow = bricksPerRow;
		}

		public int RowCount {
				get { return _rows.Count; }
		}

		/// <summary>Lignes à générer telles quelles avant de repasser en aléatoire ('A' brique, 'B' solide, 'C' indestructible, 'D' nitro, ' ' vide).</summary>
		public void EnqueuePatterns (IEnumerable<string> lines)
		{
				foreach (var line in lines) {
						if (line != null) {
								_pendingPatterns.Enqueue (line.ToCharArray ());
						}
				}
		}

		public void CreateRow ()
		{
				var leftEdge = _camera.ScreenToWorldPoint (Vector3.zero);
				float y = _lastRow != null ? _lastRow.transform.position.y - RowSpacing : _camera.transform.position.y - _offset;

				var go = Object.Instantiate (_rowPrefab, new Vector3 (leftEdge.x, y, 1f), Quaternion.identity);
				var row = go.GetComponent<GroundRaw> ();
				row.InitialBrickVector = leftEdge;
				row.NbElements = _bricksPerRow;

				if (_pendingPatterns.Count > 0) {
						row.GenerateGroundElements (_pendingPatterns.Dequeue ());
				} else {
						row.GenerateGroundElements ();
						if (Random.value <= PatternChance) {
								EnqueuePatterns (TerrainFactory.GetTerrain ());
						}
				}

				_rows.Enqueue (row);
				_lastRow = row;
		}

		/// <summary>Détruit la ligne du haut si elle est sortie de l'écran et en crée une en bas.</summary>
		public void Recycle (float yTop)
		{
				if (_rows.Count == 0) {
						return;
				}
				var top = _rows.Peek ();
				if (top == null) {
						_rows.Dequeue ();
						return;
				}
				if (top.transform.position.y >= yTop) {
						_rows.Dequeue ();
						Object.Destroy (top.gameObject);
						CreateRow ();
				}
		}

		public void Clear ()
		{
				while (_rows.Count > 0) {
						var row = _rows.Dequeue ();
						if (row != null) {
								Object.Destroy (row.gameObject);
						}
				}
				_lastRow = null;
		}

		public GroundRaw RowOf (GroundElement element)
		{
				if (element == null) {
						return null;
				}
				return _rows.FirstOrDefault (r => r != null && r.GroundElements != null && r.GroundElements.Contains (element));
		}

		/// <summary>La ligne juste au-dessus (créée juste avant) de celle donnée.</summary>
		public GroundRaw RowAbove (GroundRaw row)
		{
				GroundRaw previous = null;
				foreach (var r in _rows) {
						if (r == row) {
								return previous;
						}
						previous = r;
				}
				return null;
		}

		/// <summary>
		/// Explosion de dynamite : toute la ligne de l'élément visé, plus la même colonne
		/// sur les dix lignes suivantes.
		/// </summary>
		public void Bang (GroundElement focus)
		{
				var selected = RowOf (focus);
				if (selected == null) {
						return;
				}

				foreach (var element in selected.GroundElements) {
						if (element != null) {
								element.Explosion ();
						}
				}

				int destroyed = 0;
				foreach (var row in _rows) {
						if (destroyed >= 10) {
								break;
						}
						if (row == selected || row == null || row.GroundElements == null || row.GroundElements.Length <= focus.ElementIndex) {
								continue;
						}
						var element = row.GroundElements [focus.ElementIndex];
						if (element != null) {
								element.Explosion ();
								++destroyed;
						}
				}
		}
}
