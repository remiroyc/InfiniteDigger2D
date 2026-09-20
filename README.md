# Infinite Digger 2D

Runner 2D mobile en portrait : un mineur creuse toujours plus profond, ramasse
des pièces, pose de la dynamite et évite les monstres. Projet Unity créé en
2015, migré vers **Unity 6 (6000.6.2f1)** en septembre 2026.

## Ouvrir le projet

- Unity **6000.6.2f1** (Unity Hub). Le projet est en sérialisation texte
  (`Force Text`) et `Visible Meta Files`.
- Le dossier doit être sur un système de fichiers **insensible à la casse**
  (NTFS, APFS). Unity refuse d'ouvrir un projet sur ext4 / WSL.
- Packages : générés dans `Packages/manifest.json` ; le seul ajout explicite
  est `com.unity.ugui` (le HUD utilise `UnityEngine.UI.Text`).
- Scènes dans le build : `menu` (démarrage) et `test` (mode record).
  `tutorial` et `Levels/Level1` sont conservées mais désactivées.

## Structure

| Dossier | Contenu |
|---|---|
| `Assets/Scripts` | Tout le code du jeu (≈ 30 scripts). Points d'entrée : `Menu`, `GameManager`, `CharacterControllerScript` |
| `Assets/Scripts/VirtualGui.cs` | Repère IMGUI partagé : 1080 de large, hauteur déduite de l'écran, calé sur `Screen.safeArea` |
| `Assets/Scripts/DBScript.cs` | Profil et classement **locaux** (`PlayerPrefs`) : pseudo + top 10 |
| `Assets/Scripts/LocalizationStrings.cs` | Textes FR / ES / DE / IT, anglais par défaut |
| `Assets/Resources` | Prefabs chargés par nom (`Dynamite`, `100`, `Golem`, `Taupe`, explosions) et musiques |
| `Assets/Sprites`, `Assets/Animations` | Sprites et clips Mecanim du mineur, des monstres et des effets |
| `Assets/GUI` | Textures et les deux `GUISkin` de l'interface IMGUI |

## État de la migration 5.0 → 6000.6

Tout le travail est sur la branche `unity6-migration`, un commit par phase :

1. **Purge** — SDKs disparus (GameAnalytics 0.6.9, AdBuddiz, UniRate, AdColony),
   plateformes retirées (Windows Phone, Web Player), backend PHP hors ligne
   remplacé par un stockage local.
2. **Unity 5.6** — passage en texte, nettoyage des références pendantes,
   `Application.LoadLevel` → `SceneManager`, composants `GUILayer` / `GUITexture`
   / `GUIText` supprimés.
3. **Unity 6** — reserialisation complète, `velocity` → `linearVelocity`,
   `FindObjectOfType` → `FindAnyObjectByType`, IL2CPP + ARM64, minSdk 26,
   iOS 15.
4. **Polish** — IMGUI sans déformation ni encoche, textes et traductions
   corrigés, code mort retiré.

## Ce qui reste à faire

- **Modules de build** : installer *Android Build Support* (SDK/NDK/OpenJDK)
  et, avec un Mac, *iOS Build Support* dans le Hub, puis premier build
  IL2CPP.
- **Keystore de release Android** : l'original
  (`C:/Users/Remi/Desktop/keystore.keystore`) n'est plus sur la machine.
  Sans lui ni Play App Signing, la fiche `com.aclick.infinitedigger`
  existante ne peut pas être mise à jour. Les builds utilisent le keystore de
  debug en attendant.
- **Icône** : aucune icône n'est configurée dans Player Settings ; la seule
  source (`Sprites/infinite_digger_140_140.png`) fait 144 px, trop petit
  pour les stores (Android 432 px adaptatif, iOS 1024 px).
- **Tutoriel et Level1** : ces deux scènes datent d'une version à
  accéléromètre. Le déplacement se fait maintenant par les boutons uGUI de
  `test.unity`, qui n'existent ni dans `tutorial` ni dans `Levels/Level1` :
  le mineur n'y bouge pas. À recâbler (copier le Canvas de `test`) ou à
  supprimer.
- **UI** : toujours en IMGUI. Une migration uGUI + TextMeshPro reste
  possible écran par écran.

## Licence

GNU GPL, voir `LICENCE.md`.
