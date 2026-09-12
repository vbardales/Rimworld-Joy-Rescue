# Joy Rescue

Rend utilisables les bâtiments de loisir que leur mod d'origine a laissés inertes.

## Le problème

Dans RimWorld, `<building><joyKind>` sur un `ThingDef` n'est **qu'une étiquette d'affichage**.
Ce qui rend un bâtiment réellement utilisable, c'est un `JoyGiverDef` dont la liste `<thingDefs>`
contient ce bâtiment, plus le `JobDef` associé. Beaucoup de mods écrivent l'étiquette et s'arrêtent
là : le bâtiment se construit, ressemble à ce qu'il devrait être, et personne ne s'en sert jamais.

Ça compte plus qu'il n'y paraît. La **tolérance au loisir se compte par type**
(`Need_Joy.tolerances`), et les attentes réclament jusqu'à **6 types différents**
(`ExpectationDef.joyKindsNeeded`, de 2 à 6). Le jeu de base n'en offre que 8, dont 4 seulement
viennent de bâtiments. Un type de loisir qu'aucun fournisseur ne produit est un type que la
colonie n'a tout simplement pas.

## Ce que fait le mod

Au chargement, il compare tous les `ThingDef` portant un `joyKind` à tous les `JoyGiverDef.thingDefs`,
et fabrique le `JobDef` et le `JoyGiverDef` manquants pour les orphelins. Chaque mode est un couple
giver + driver **déjà employé tel quel par le jeu de base** :

| Mode | Giver | Driver | Modèle vanilla |
|---|---|---|---|
| Case d'interaction | `JoyGiver_InteractBuildingInteractionCell` | `JobDriver_WatchBuilding` | télescope, instruments |
| Jouer à côté | `JoyGiver_InteractBuildingSitAdjacent` | `JobDriver_SitFacingBuilding` | échecs, jeu d'Ur, poker |
| Regarder | `JoyGiver_WatchBuilding` | `JobDriver_WatchBuilding` | téléviseurs |

Le choix se fait sur la **forme de la def**, jamais sur son nom : une case d'interaction déclarée
est une intention explicite de l'auteur, on la respecte ; un `joyKind` `Television` se regarde ;
tout le reste se joue depuis une case adjacente. Chaque ligne des réglages permet de forcer le mode
à la main.

## Le point technique à ne pas défaire

La génération se fait dans un **postfix de `DefGenerator.GenerateImpliedDefs_PreResolve`**. C'est la
seule fenêtre où les deux conditions sont réunies :

1. `DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences` est déjà passé, donc
   `building.joyKind` est un vrai objet et non une chaîne en attente ;
2. `DefDatabase<T>.ResolveAllReferences()` n'a **pas encore** tourné. Or
   `JobGiver_GetJoy.ResolveReferences` y alloue un `DefMap<JoyGiverDef, float>` dimensionné sur le
   nombre de `JoyGiverDef` existants et indexé par `def.index`. Ajouter un giver après coup ferait
   sortir l'indexation du tableau **à chaque tick de recherche de loisir**.

Les hash courts sont distribués plus loin dans le chargement (« Short hash giving »), donc les defs
générées en reçoivent un sans rien faire.

Corollaire : on n'enlève **jamais** une def de la base. Désactiver une entrée met son
`baseChance` à 0, ce qui donne un poids de tirage nul dans
`JobGiver_GetJoy.TryGiveJob` (`TryRandomElementByWeight`) : le giver n'est plus jamais choisi.
C'est ce qui permet d'appliquer les réglages à chaud.

## Faux positifs

Un mod qui embarque ses propres `JoyGiver` ou `JobDriver` peut desservir son bâtiment par code,
sans jamais le lister dans un `thingDefs`. Le réparer créerait deux tâches concurrentes sur le même
meuble. Ces bâtiments sont donc **listés mais désactivés par défaut**, avec un avertissement ;
la détection se fait par réflexion sur les seules assemblies du `ModContentPack` d'origine
(celles de ses dépendances n'y sont pas).

## Construction

```
dotnet build JoyRescue/Source/JoyRescue.csproj -c Release
```

Une jonction NTFS relie `RimWorld\Mods\JoyRescue` à ce dossier : la DLL est propagée sans copie.

## Sauvegardes

Aucune donnée ajoutée. Le mod peut être ajouté ou retiré d'une partie en cours. Seule réserve :
retirer le mod alors qu'un colon exécute une tâche générée fait perdre cette tâche au chargement,
ce que RimWorld gère comme n'importe quelle def manquante.
