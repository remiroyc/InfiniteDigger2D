using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot helpers for the 5.0 -> 6000.6 migration. Safe to delete once the
/// project is settled on Unity 6.
/// </summary>
public static class MigrationTools
{
		// ProjectSettings/*.asset are not regular assets: ForceReserializeAssets ignores
		// them, and Unity only rewrites one when its settings window marks it dirty.
		// Loading + dirtying + saving upgrades them to the current object version.
		[MenuItem ("Tools/Migration/1. Upgrade ProjectSettings files")]
		public static void UpgradeProjectSettings ()
		{
				int count = 0;
				foreach (var path in Directory.GetFiles ("ProjectSettings", "*.asset")) {
						var p = path.Replace ('\\', '/');
						foreach (var obj in AssetDatabase.LoadAllAssetsAtPath (p)) {
								if (obj == null) {
										continue;
								}
								EditorUtility.SetDirty (obj);
								count++;
						}
				}
				AssetDatabase.SaveAssets ();
				Debug.Log ("[Migration] Dirtied and saved " + count + " ProjectSettings objects.");
		}

		// Rewrites every asset and .meta in the current editor's serialization format,
		// so the repo reflects Unity 6 rather than a mix of 5.6 YAML read on the fly.
		[MenuItem ("Tools/Migration/2. Reserialize all assets")]
		public static void ReserializeAllAssets ()
		{
				AssetDatabase.ForceReserializeAssets ();
				AssetDatabase.SaveAssets ();
				Debug.Log ("[Migration] ForceReserializeAssets done.");
		}
}
