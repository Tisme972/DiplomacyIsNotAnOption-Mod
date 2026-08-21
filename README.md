# DNO Stats Config Mod 2.0.0 — version *standalone* (sans BepInEx) pour **v1.0.99_r**

Cette version supprime **toute** dépendance à BepInEx : plus besoin d'installer BepInEx 5
ni de déposer un plugin dans `BepInEx/plugins`. Le système du mod
(`DnoStatsConfigSystem`) a été **recompilé directement contre les DLLs de TA version
(1.0.99_r)** et le binding de config BepInEx a été remplacé par un simple fichier `.ini`.

Le jeu tourne sur **Unity DOTS / ECS**. Le système du mod porte les attributs
`[UpdateInGroup(GameplayInitializationSystemsGroup)]` + `[UpdateBefore(GameStateUpdateHandler)]` :
dès que l'assembly qui le contient est chargée, **DOTS l'instancie tout seul** (le jeu n'a pas
de `ICustomBootstrap`, donc l'auto-création des systèmes est active). Il suffit donc que le code
soit présent dans une assembly chargée — c'est tout l'objet des deux méthodes ci-dessous.

---

## ⚠️ À lire avant de commencer

- **Sauvegarde obligatoire.** Fais une copie de `DNO.Main.dll` avant toute manip. En cas de
  problème, Steam → clic droit sur le jeu → *Propriétés* → *Fichiers installés* →
  *Vérifier l'intégrité des fichiers* restaure les fichiers d'origine.
- **Verrouillé sur 1.0.99_r.** Ce build est spécifique à ta version. Une mise à jour du jeu qui
  modifie l'API interne nécessitera une recompilation (voir `build.sh`) et/ou écrasera le DLL.
- Le fichier `.log` du jeu (Player.log) contiendra des lignes préfixées `[DNOStats]`.

---

## Méthode A — Fichier unique (recommandée)

Un seul fichier à remplacer. Le patch `DNO.Main.PATCHED.dll` **est** ton `DNO.Main.dll`
d'origine + les types du mod fusionnés dedans (même nom d'assembly « DNO.Main », donc Unity le
charge exactement pareil).

1. Va dans le dossier du jeu, sous‑dossier `*_Data/Managed/`
   (typiquement `…/steamapps/common/Diplomacy is Not an Option/DNO_Data/Managed/`).
2. **Sauvegarde** : copie `DNO.Main.dll` → `DNO.Main.dll.bak`.
3. Copie `DNO.Main.PATCHED.dll` dans ce dossier et **renomme‑le** en `DNO.Main.dll`
   (remplace l'existant).
4. Copie `dno.statsconfig.ini` **à côté de l'exécutable du jeu** (le `.exe`, à la racine du jeu,
   PAS dans Managed). S'il est absent, le mod en génère un par défaut au premier lancement.
5. Lance le jeu, charge une partie. Édite le `.ini`, relance/recharge pour appliquer.

Désinstallation : remets `DNO.Main.dll.bak` → `DNO.Main.dll` (ou vérifie l'intégrité Steam).

---

## Méthode B — Sans toucher à `DNO.Main.dll` (alternative, la plus réversible)

Ici on ne modifie jamais le gros assembly du jeu : on ajoute un petit DLL à côté et on demande
à Unity de le charger au démarrage. DOTS le scanne alors et crée le système.

1. Copie `DnoStatsStandalone.dll` dans `*_Data/Managed/`.
2. Ouvre `*_Data/ScriptingAssemblies.json`. Il contient deux tableaux **parallèles**
   `"names"` et `"types"` (même longueur).
   - Ajoute `"DnoStatsStandalone.dll"` à la fin de `"names"`.
   - Ajoute **une** valeur à la fin de `"types"` pour garder la même longueur
     (mets `16`, la valeur « user assembly » ; en cas de doute recopie la valeur du dernier
     élément de la liste). ⚠️ Si les deux tableaux n'ont pas la même taille, le jeu peut
     refuser de démarrer — d'où la sauvegarde du `.json`.
3. Copie `dno.statsconfig.ini` à côté du `.exe` (comme méthode A).
4. Lance le jeu.

Désinstallation : supprime le DLL + retire les deux entrées ajoutées au `.json`.

> Si tu préfères le zéro‑risque total sur le cœur du jeu, c'est la méthode B — au prix d'une
> édition JSON un peu délicate. Sinon la méthode A est plus simple (un fichier).

---

## Configuration (`dno.statsconfig.ini`)

Fichier texte, sections `[…]`, une clé par ligne. Emplacement : **à côté de l'exécutable**.
Les nombres utilisent le point décimal (`1.5`, pas `1,5`). Booléens : `true` / `false`.

Aperçu des sections : `General`, `Multipliers` (PV/dégâts/vitesse armée, PV bâtiments),
`Speed Multipliers` (recherche, entraînement, construction, ouvriers),
`Range Multipliers` (portée d'attaque / vision), `Resources` (sources infinies, minimums de
stock), `Resource Gain Multipliers` (multiplicateurs de gains), `Capacities` (grenier, stockage,
maisons), `Advanced` (`EnforceHealthEveryTick`), puis une section `Troop - X` par unité
(multiplicateurs spécifiques ; `0` = utilise la valeur globale d'armée).

### Note de version 1.0.99 vs 1.0.144 (le mod visait 1.0.144)
- Les **valeurs numériques** des unités coïncident entre les deux versions ; le mapping reste
  correct. Certains **noms** différaient (ex. « Footman » = *Swordsman*, « Healer » = *Doctor*,
  « Banner Bearer » = *BannerPeasant*, « Torchbearer » = *Brander*), mais ce ne sont que des
  libellés de section — sans impact.
- Deux unités du mod **n'existent pas** en 1.0.99 : `Troop - Mounted Crossbowman` et
  `Troop - Peasant Ballista`. Leurs sections sont **inertes** (elles ne ciblent aucune unité
  réelle) — tu peux les ignorer ou les laisser telles quelles.

---

## Recompiler pour une future version du jeu

Si tu mets le jeu à jour, l'API interne peut changer. Recompile contre le nouveau `Managed` :

```bash
./build.sh "/chemin/vers/DNO_Data/Managed"
```

Le script régénère `DnoStatsStandalone.dll`. Pour refaire le fichier unique (méthode A),
fusionne‑le ensuite avec ILRepack (commande affichée en fin de script).

Sources fournies dans `src/` (`standalone_core.cs` = le mod décompilé, dé‑BepInEx‑isé et adapté ;
`shim.cs` = le remplacement de `ConfigEntry<T>` + logging). Aucune logique de gameplay n'a été
modifiée : seuls le *chargement* (BepInEx → INI) et les adaptations d'API 1.0.99 l'ont été.

---

## Contenu du dossier

| Fichier | Rôle |
|---|---|
| `DNO.Main.PATCHED.dll` | **Méthode A** : ton `DNO.Main.dll` + mod fusionné (à renommer en `DNO.Main.dll`). |
| `DnoStatsStandalone.dll` | **Méthode B** : petit DLL à charger via `ScriptingAssemblies.json`. |
| `dno.statsconfig.ini` | Config par défaut (toutes valeurs neutres = 1 / 0 / false). |
| `build.sh` | Recompilation contre une autre version du jeu. |
| `src/standalone_core.cs`, `src/shim.cs` | Sources pour reproduire / recompiler. |

---

## Correctif v2 (build du jour)

Correction d'un **bug de boxing de struct** qui rendait inopérantes toutes les options de
*montant* de ressources : `MinimumFood/Money/Wood/Stone/Iron/Souls` **et** les
`Resource Gain Multipliers`. Le mod castait une struct vers l'interface `IUserUIResource`
(→ copie boxée), modifiait la copie, puis réécrivait la struct d'origine restée inchangée.
Désormais la valeur modifiée est bien recopiée (`box → IncreaseAmount → unbox`).

Rappel de comportement : `MinimumMoney = 500` **complète** l'argent jusqu'à 500 **uniquement
quand il est en dessous** ; si tu as déjà plus de 500, rien ne bouge (c'est voulu). Le contrôle
est réappliqué à chaque tick, donc l'argent ne redescend plus durablement sous le seuil.
Les autres fonctions (PV/dégâts/vitesse/portée unités & bâtiments, sources infinies, capacités)
n'étaient pas concernées par ce bug.
