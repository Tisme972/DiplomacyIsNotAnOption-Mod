# DNO Stats Config Mod 2.0.0 — version standalone (sans BepInEx) pour v1.0.99_r

J'ai viré la dépendance à BepInEx : plus besoin de l'installer ni de mettre quoi que ce soit dans `BepInEx/plugins`. Le système du mod (`DnoStatsConfigSystem`) a été recompilé directement contre les DLLs de la version 1.0.99_r, et le binding de config BepInEx a été remplacé par un simple fichier `.ini`.

Le jeu tourne sur Unity DOTS/ECS. Le système du mod porte les attributs `[UpdateInGroup(GameplayInitializationSystemsGroup)]` et `[UpdateBefore(GameStateUpdateHandler)]`, donc dès que l'assembly qui le contient est chargée, DOTS l'instancie automatiquement (le jeu n'a pas de `ICustomBootstrap`, l'auto-création des systèmes est donc active). Il suffit que le code soit présent dans une assembly chargée — c'est tout l'objet des deux méthodes ci-dessous.

---

## À lire avant de commencer

Fais une copie de `DNO.Main.dll` avant toute manip, sérieusement. En cas de souci, Steam → clic droit sur le jeu → Propriétés → Fichiers installés → Vérifier l'intégrité des fichiers te remet les fichiers d'origine.

Ce build est verrouillé sur la 1.0.99_r. Une mise à jour du jeu qui change l'API interne demandera une recompilation (voir `build.sh`) et écrasera probablement le DLL.

Le fichier Player.log du jeu contiendra des lignes préfixées `[DNOStats]`.

---

## Méthode A — fichier unique (recommandée)

Un seul fichier à remplacer. `DNO.Main.PATCHED.dll`, c'est ton `DNO.Main.dll` d'origine avec les types du mod fusionnés dedans (même nom d'assembly « DNO.Main », donc Unity le charge pareil).

1. Va dans le dossier du jeu, sous-dossier `*_Data/Managed/` (typiquement `…/steamapps/common/Diplomacy is Not an Option/DNO_Data/Managed/`).
2. Sauvegarde `DNO.Main.dll` en `DNO.Main.dll.bak`.
3. Copie `DNO.Main.PATCHED.dll` dans ce dossier et renomme-le en `DNO.Main.dll` (ça remplace l'existant).
4. Copie `dno.statsconfig.ini` à côté de l'exécutable du jeu (le .exe, à la racine, pas dans Managed). S'il est absent, le mod en génère un par défaut au premier lancement.
5. Lance le jeu, charge une partie. Tu peux éditer le .ini et relancer/recharger pour appliquer les changements.

Pour désinstaller, remets `DNO.Main.dll.bak` à la place de `DNO.Main.dll` (ou vérifie l'intégrité Steam).

---

## Méthode B — sans toucher à DNO.Main.dll

Celle-là ne touche jamais au gros assembly du jeu : on ajoute un petit DLL à côté et on demande à Unity de le charger au démarrage. DOTS le scanne et crée le système tout seul.

1. Copie `DnoStatsStandalone.dll` dans `*_Data/Managed/`.
2. Ouvre `*_Data/ScriptingAssemblies.json`. Il y a deux tableaux parallèles, `"names"` et `"types"`, de même longueur.
   - Ajoute `"DnoStatsStandalone.dll"` à la fin de `"names"`.
   - Ajoute une valeur à la fin de `"types"` pour garder la même longueur (mets `16`, c'est la valeur "user assembly" ; si tu doutes, recopie la valeur du dernier élément). Si les deux tableaux n'ont pas la même taille, le jeu peut refuser de démarrer — d'où l'intérêt de sauvegarder le json avant.
3. Copie `dno.statsconfig.ini` à côté du .exe, comme pour la méthode A.
4. Lance le jeu.

Pour désinstaller, supprime le DLL et retire les deux entrées ajoutées dans le json.

Si tu veux le zéro-risque total sur le cœur du jeu, c'est la méthode B, au prix d'une édition JSON un peu chiante. Sinon la méthode A est plus simple, un seul fichier à remplacer.

---

## Configuration (dno.statsconfig.ini)

Fichier texte classique, sections entre crochets, une clé par ligne, à côté de l'exécutable. Les nombres utilisent le point décimal (1.5, pas 1,5). Booléens : true/false.

Les sections : `General`, `Multipliers` (PV/dégâts/vitesse armée, PV bâtiments), `Speed Multipliers` (recherche, entraînement, construction, ouvriers), `Range Multipliers` (portée d'attaque/vision), `Resources` (sources infinies, minimums de stock), `Resource Gain Multipliers`, `Capacities` (grenier, stockage, maisons), `Advanced` (EnforceHealthEveryTick), puis une section `Troop - X` par unité (0 = utilise la valeur globale d'armée).

Petite note sur la 1.0.99 vs la 1.0.144 (le mod visait à l'origine la 1.0.144) : les valeurs numériques des unités coïncident entre les deux versions donc le mapping reste bon. Quelques noms diffèrent (« Footman » = Swordsman, « Healer » = Doctor, « Banner Bearer » = BannerPeasant, « Torchbearer » = Brander), mais c'est juste cosmétique, ça n'a pas d'impact. Deux unités du mod n'existent pas en 1.0.99 (`Troop - Mounted Crossbowman` et `Troop - Peasant Ballista`) : leurs sections sont inertes, tu peux les ignorer.

---

## Recompiler pour une future version du jeu

Si tu mets le jeu à jour, l'API interne peut bouger. Recompile contre le nouveau dossier Managed :

```bash
./build.sh "/chemin/vers/DNO_Data/Managed"
```

Ça régénère `DnoStatsStandalone.dll`. Pour refaire le fichier unique de la méthode A, fusionne-le ensuite avec ILRepack (la commande s'affiche à la fin du script).

Les sources sont dans `src/` : `standalone_core.cs` c'est le mod décompilé, débarrassé de BepInEx et adapté ; `shim.cs` remplace `ConfigEntry<T>` et gère le logging. Je n'ai touché à aucune logique de gameplay, juste au chargement (BepInEx → INI) et aux adaptations d'API pour la 1.0.99.

---

## Contenu du dossier

- `DNO.Main.PATCHED.dll` — méthode A, ton `DNO.Main.dll` + mod fusionné (à renommer en `DNO.Main.dll`)
- `DnoStatsStandalone.dll` — méthode B, petit DLL à charger via `ScriptingAssemblies.json`
- `dno.statsconfig.ini` — config par défaut, toutes les valeurs neutres (1 / 0 / false)
- `build.sh` — pour recompiler contre une autre version du jeu
- `src/standalone_core.cs`, `src/shim.cs` — sources pour reproduire ou recompiler
