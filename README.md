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
- Deux scènes : `menu` (démarrage) et `test` (mode record). Les anciennes
  scènes `tutorial` et `Level1`, injouables depuis le passage aux boutons,
  ont été supprimées (récupérables dans l'historique git).

## Structure

| Dossier | Contenu |
|---|---|
| `Assets/Scripts` | Code du jeu (≈ 25 scripts). Points d'entrée : `Menu` (contrôleur du menu), `GameManager` (rythme, score, pause), `CharacterControllerScript` |
| `Assets/Scripts/TerrainGenerator.cs`, `MonsterSpawner.cs` | Génération / recyclage des lignes de briques ; apparition des monstres |
| `Assets/Scripts/UI` | UI uGUI + TextMeshPro construite par code : `UiKit` (fabrique, police TMP dynamique), `GameHud`, `MenuView`, `SafeAreaFitter` |
| `Assets/Resources/UI` | Sprites du HUD et du menu, police `Carton_Six.ttf` |
| `Assets/Scripts/DBScript.cs` | Profil et classement **locaux** (`PlayerPrefs`) : pseudo + top 10 |
| `Assets/Scripts/LocalizationStrings.cs` | Textes FR / ES / DE / IT, anglais par défaut |
| `Assets/Resources` | Prefabs chargés par nom (`Dynamite`, `100`, `Golem`, `Taupe`, explosions) et musiques |
| `Assets/Sprites`, `Assets/Animations` | Sprites et clips Mecanim du mineur, des monstres et des effets |
| `Assets/GUI` | Trois textures encore utilisées par le Canvas de `test` (boutons de déplacement, dock) |

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
4. **Polish** — textes et traductions corrigés, code mort retiré, audit de
   performance (allocations par frame, atlas de sprites, audio, mipmaps).
5. **UI** — HUD, pause, mort et menu réécrits en uGUI + TextMeshPro ; plus
   aucun IMGUI. Barre de vie, pause automatique en arrière-plan.

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
- **Tutoriel** : à refaire sous forme de bulles contextuelles dans la
  première partie (le Dock uGUI existe déjà) plutôt qu'en scène séparée.
- **Réglages de jeu** : vitesse caméra, courbe de difficulté et cadence
  de spawn sont des constantes de `GameManager` ; à sortir dans un
  `ScriptableObject`. Golem et Rat (prefabs prêts, tag `Monster`) ne sont
  pas encore instanciés par `MonsterSpawner`.

## Licence

GNU GPL, voir `LICENCE.md`.
