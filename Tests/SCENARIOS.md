# JoyRescue — scénarios de tests

Statut : première suite automatisée disponible ; voir `Tests/README.md` pour les
résultats et le périmètre exact. Les autres scénarios restent à implémenter.
Scénarios établis à partir du code présent dans `Source/`.

Chaque ligne décrit les données initiales, l'action et les assertions attendues.
P0 : protection du fonctionnement et des données ; P1 : comportement courant ;
P2 : présentation et diagnostic. Les cas paramétrés doivent être exécutés séparément.

## Organisation de la future suite

- **Unitaires** : règles de paramètres, heuristique, configuration des jobs et logique
  de l'éditeur. Employer des définitions minimales sans carte ni colon.
- **Intégration des définitions** : génération, réaffectations, réflexion et sauvegarde.
  Ces tests utilisent les services statiques de Verse ; ils ne sont pas des tests
  unitaires isolés, même s'ils peuvent s'exécuter sans partie jouable.
- **En jeu** : chargement, réservation, déplacement et satisfaction du loisir.

Pour les tests touchant les statiques, réinitialiser les paramètres, `Entries`,
`AllEntries`, `OriginalChances`, les compteurs et `HasRun` entre les cas. Isoler les
`DefDatabase<T>` et restaurer l'état dans le nettoyage, même après une assertion échouée.
Ne pas exécuter ces cas en parallèle. Utiliser des packs distincts pour JoyRescue et
le mod tiers : deux `modContentPack` nuls seraient considérés comme égaux par
`ApplyDisabledKinds` et masqueraient le comportement à vérifier.

Les méthodes privées peuvent être couvertes par leurs points d'entrée publics ;
les règles privées de l'éditeur nécessiteront une extraction testable ou un accès
de test. Fixer la langue et initialiser les DefOf nécessaires pour les cas utilisant
les traductions, capacités et compétences. Les assemblages de référence du projet
ne suffisent pas à prouver la disponibilité d'un runtime Verse pour les tests.

## Tests unitaires — paramètres et identité

Sources : `JoyRescueSettings.cs`, `CustomJoyKind.cs`, `Runtime/RescueEntry.cs`.

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| U01 | P1 | Des paramètres neufs | Lire leurs valeurs | `rescueModsWithOwnCode=false`, `requireChairForWatching=true`, collections vides, tri à 0 et prochain identifiant à 1. |
| U02 | P0 | Entrée avec/sans code de loisir dans son mod ; option globale vraie/fausse | Appeler `DefaultEnabled` pour les quatre combinaisons | Seule l'entrée avec code propre et option globale fausse est désactivée. |
| U03 | P0 | Une surcharge explicite vraie puis fausse, opposée au défaut | Appeler `IsEnabled` | La décision explicite prime sur le défaut dans les deux sens. |
| U04 | P1 | Une entrée avec une surcharge existante | Appeler `SetEnabled` avec la valeur par défaut | La clé est supprimée ; `IsEnabled` revient au défaut. |
| U05 | P1 | Deux entrées distinctes | Changer une entrée avec `SetEnabled` vers une valeur différente du défaut | Seule sa clé est enregistrée et l'autre entrée reste inchangée. |
| U06 | P0 | Deux entrées avec code propre, l'une sans surcharge, l'autre explicitement désactivée | Activer `rescueModsWithOwnCode` | La première devient active ; la seconde reste désactivée. |
| U07 | P1 | Mode absent, `Auto`, texte invalide ou vide | Lire `RawMode`, puis `ModeFor` | `RawMode` retourne `Auto` ; `ModeFor` applique l'heuristique. |
| U08 | P1 | Chacun des trois modes explicites valides | Appeler `SetMode`, `RawMode` et `ModeFor` | Le mode est conservé et prime sur l'heuristique. |
| U09 | P1 | Un mode explicite enregistré | Appeler `SetMode(..., Auto)` | La surcharge est supprimée et le mode redevient automatique. |
| U10 | P1 | Toutes les options modifiées et toutes les collections remplies | Appeler `Reset` | Toutes les valeurs initiales de U01 sont restaurées, sans ancienne réaffectation ni type personnalisé. |
| U11 | P2 | Bâtiment de `defName=A` et label renseigné, vide ou nul | Lire `Key` et `BuildingLabel` | La clé vaut toujours `A` ; le label renseigné est utilisé, sinon `A`. |
| U12 | P1 | `CustomJoyKind("7", "Musique")` | Lire ses propriétés | `DefName=JoyRescue_Kind_7`, label conservé et `needsThing=true` ; renommer le label ne change pas l'identité. |

## Tests unitaires — sélection et changement de mode

Source : `Runtime/JoyRescueGenerator.cs`, méthodes `Heuristic` et `Retarget`.

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| U13 | P0 | Cellule d'interaction déclarée ; type `Television` ou autre | Appeler `Heuristic` | `InteractionCell` dans les deux cas : la cellule prime sur le type. |
| U14 | P1 | Sans cellule d'interaction, type `Television` | Appeler `Heuristic` | `Watch`. |
| U15 | P1 | Sans cellule, type différent, type nul ou propriétés `building` nulles | Appeler `Heuristic` sur un `ThingDef` non nul | `SitAdjacent` pour chaque cas. |
| U16 | P1 | Deux définitions de même structure avec des noms évoquant des activités différentes | Appeler `Heuristic` | Même résultat : ni le label ni le nom du bâtiment ne décident du mode. |
| U17 | P0 | Une entrée avec job et giver | Appliquer chaque mode | Toutes les valeurs de la matrice ci-dessous sont respectées et `resolvedMode` correspond au mode demandé. |
| U18 | P0 | Entrée déjà configurée dans un mode et worker déjà en cache | Effectuer chacune des six transitions entre modes distincts | Classes, capacités, nombre de participants, options de siège/lit et rapport correspondent au mode cible ; `workerInt` est nul. |
| U19 | P0 | Entrée `Watch`, option de siège vraie puis fausse | Réappliquer `Watch` | `desireSit` suit l'option ; huit participants et usage depuis un lit restent autorisés ; worker invalidé. |
| U20 | P1 | Job nul, giver nul, puis les deux nuls | Appeler `Retarget` | Aucune exception ni modification partielle de `resolvedMode`. |
| U21 | P1 | Entrée déjà configurée | Réappliquer le même mode | Configuration identique, mêmes objets job/giver ; worker invalidé pour permettre sa recréation. |

Matrice attendue pour les entrées générées (`requireChair=false` à la création) :

| Champ | InteractionCell | SitAdjacent | Watch |
|---|---|---|---|
| `giverClass` | `JoyGiver_InteractBuildingInteractionCell` | `JoyGiver_InteractBuildingSitAdjacent` | `JoyGiver_WatchBuilding` |
| `driverClass` | `JobDriver_WatchBuilding` | `JobDriver_SitFacingBuilding` | `JobDriver_WatchBuilding` |
| `joyMaxParticipants` | 1 | 2 | 8 |
| `canDoWhileInBed` | false | false | true |
| `desireSit` | false | false | `requireChairForWatching` |
| `requiredCapacities` | Sight, Manipulation | Sight, Manipulation | Sight |
| Clé du rapport traduit | `JoyRescue.Report.Using` | `JoyRescue.Report.Playing` | `JoyRescue.Report.Watching` |

Vérifier également que chaque rapport inclut le label du bâtiment.

## Tests d'intégration des définitions — détection et génération

Source : `Runtime/JoyRescueGenerator.cs`, entrée publique `Generate`.

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| D01 | P1 | Bases de définitions vides | Générer | Listes et compteurs vides, `HasRun=true`, rapport « Nothing to rescue ». |
| D02 | P0 | Bâtiment avec type de loisir, sans giver | Générer | Une entrée dans chaque liste, un job et un giver liés au bâtiment ; compteur vu à 1 et couvert à 0. |
| D03 | P0 | Bâtiment déjà présent dans `thingDefs` d'un giver | Générer | Présent seulement dans `AllEntries`, `covered=true`, compteur couvert à 1 ; aucun job/giver supplémentaire pour lui. |
| D04 | P1 | Plusieurs givers couvrant le même bâtiment, dont un à poids nul | Générer | Une seule entrée couverte, aucun doublon ; le poids ne modifie pas la détection de couverture. |
| D05 | P0 | Cas séparés : pas de propriétés building, pas de joyKind, mauvaise catégorie, blueprint, frame, `entityDefToBuild` non nul, siège | Générer | Chaque définition exclue est absente des listes et compteurs ; un bâtiment valide témoin reste détecté. |
| D06 | P1 | Giver avec `thingDefs=null`, puis liste contenant une valeur nulle | Générer | Pas d'exception ; les références valides sont prises en compte. |
| D07 | P0 | Un orphelin `A` | Générer | `JoyRescue_A` et `JoyRescue_Giver_A` existent une fois ; même joyKind sur bâtiment, job et giver ; `thingDefs` contient uniquement A ; durée 4000, poids 2 si actif, `requireChair=false`. |
| D08 | P1 | Orphelins de types `Gaming_Cerebral`, `Telescope`, `Gaming_Dexterity`, `HighCulture`, autre | Générer | Compétences respectives Intellectual, Intellectual, Shooting, Artistic, aucune ; XP/tick de 0.002 avec compétence, sinon 0. |
| D09 | P0 | Mod source contenant un sous-type de `JoyGiver`, puis de `JobDriver` | Générer | `sourceShipsJoyCode=true`, entrée réparable listée, giver créé mais poids nul par défaut. Une activation explicite lui donne le poids 2. |
| D10 | P1 | Mod sans code concerné, pack nul, ou code présent uniquement dans un pack de dépendance | Générer | Aucune alerte de code propre ; réparation active par défaut ; source `?` si pack nul. |
| D11 | P1 | Assemblage dont `GetTypes` lève `ReflectionTypeLoadException` avec types partiellement disponibles | Inspecter pendant la génération | Les types non nuls sont examinés ; un sous-type concerné reste détecté. |
| D12 | P1 | Inspection du pack levant une autre exception | Générer | Avertissement avec nom du pack ; scan poursuivi sans exception propagée. |
| D13 | P1 | Plusieurs bâtiments du même pack | Générer avec inspection instrumentée | Une seule inspection du pack par génération, résultat partagé par ses entrées orphelines. |

## Tests d'intégration des définitions — activation à chaud

Entrée publique : `ApplySettings`. Capturer avant chaque action les références,
effectifs et indices des définitions pour vérifier qu'aucune suppression ni création
n'a lieu pendant l'application des paramètres.

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| D14 | P0 | Réparation active | Désactiver puis réactiver son entrée | Poids 2 → 0 → 2 ; mêmes defs et indices. |
| D15 | P0 | Type partagé par un giver JoyRescue, vanilla et tiers avec poids 2, 3 et 4 | Désactiver le type | Tous passent à 0 ; les autres types restent inchangés. |
| D16 | P0 | Type désactivé avec poids originaux 3, 4 et 0 sur des givers externes | Réactiver le type | Poids restaurés à 3, 4 et 0 ; aucune activation artificielle d'un giver initialement nul. |
| D17 | P0 | Entrée désactivée individuellement, puis son type désactivé | Réactiver le type | L'entrée reste à 0 ; les autres givers autorisés sont restaurés. |
| D18 | P0 | Type encore désactivé | Activer individuellement une de ses entrées | Son poids reste à 0 : la désactivation du type prime. |
| D19 | P0 | Poids externes mémorisés | Appliquer plusieurs fois les paramètres pendant la désactivation, puis réactiver | Les valeurs originales ne sont pas remplacées par 0 et sont correctement restaurées. |
| D20 | P1 | Type inconnu dans `disabledKinds`, giver sans joyKind, entrée sans giver | Appliquer les paramètres | Aucune exception ; les givers valides non concernés conservent leur comportement. |

## Tests d'intégration des définitions — types personnalisés et réaffectations

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| D21 | P0 | Type personnalisé avec identifiant et label, `needsThing` vrai puis faux | Générer | Une def `JoyRescue_Kind_<id>` avec les valeurs prévues ; définitions préexistantes conservées. |
| D22 | P1 | Élément personnalisé nul, identifiant nul/vide, label nul/vide, identifiant déjà existant | Générer chaque cas | Élément/identifiant invalide ignoré ; label manquant remplacé par le defName ; def existante non dupliquée ni écrasée. |
| D23 | P0 | Nouveau type et réaffectation vers lui enregistrés avant démarrage | Générer | Type créé avant la réaffectation ; bâtiment, nouveau job et giver portent ce type en un seul passage. |
| D24 | P0 | Un giver partagé par A et B sur type X ; A réaffecté vers Y | Générer | A retiré des givers qui le listaient et réparé sur Y ; B, le giver partagé et son job restent sur X. |
| D25 | P1 | Réaffectation d'un bâtiment vers son type actuel | Générer | Aucun détachement ni nouveau giver pour un bâtiment déjà couvert. |
| D26 | P1 | Bâtiment absent ou sans propriétés building ; puis cible de type absente | Générer | Source invalide ignorée ; cible absente signalée avec identifiants dans un avertissement, bâtiment et couverture inchangés. |
| D27 | P0 | Activité avec job et plusieurs bâtiments sur X | Réaffecter l'activité à Y puis générer | Giver, job et bâtiments concernés passent à Y, sans nouveau giver pour les bâtiments couverts. |
| D28 | P1 | Activité sans job, liste `thingDefs` nulle, ou liste avec éléments nuls/sans joyKind | Réaffecter puis générer | Pas d'exception ; giver et seuls bâtiments admissibles sont modifiés. |
| D29 | P1 | Activité inexistante, puis type cible inexistant | Générer | Activité absente ignorée ; cible absente avertie et defs inchangées. |
| D30 | P0 | Activité réaffectée vers Y et l'un de ses bâtiments spécifiquement vers Z | Générer | L'activité et ses autres bâtiments restent sur Y ; bâtiment particulier détaché et réparé sur Z. |

## Tests unitaires de la logique de l'éditeur

Source : `JoyRescueMod.cs`. Tester les règles indépendamment du dessin Unity.

| ID | Priorité | Étant donné | Quand | Alors |
|---|---|---|---|---|
| U22 | P1 | Réaffectations de bâtiments et d'activités vers deux types | Purger les références à un type | Seules ses références disparaissent ; nombre retourné égal au total supprimé dans les deux dictionnaires. |
| U23 | P1 | Types existants et personnalisés en attente | Construire `KindChoices` | Tous sont proposés, triés par label ; seuls les types non matérialisés portent `pending=true`, aucun doublon avec une def existante. |
| U24 | P2 | Type existant, personnalisé en attente, label vide, identifiant inconnu | Résoudre le nom | Label disponible utilisé ; sinon defName, y compris avant redémarrage. |
| U25 | P1 | Aucun changement, nouveau type, réaffectation différente, ou réaffectation déjà appliquée | Calculer `PendingRestart` séparément | Faux, vrai, vrai, faux respectivement ; couvrir bâtiments et activités. |
| U26 | P1 | Réaffectation dont le bâtiment ou l'activité source a disparu | Calculer `PendingRestart` sans autre changement | Faux : cette source absente est ignorée. |
| U27 | P2 | Chaque mode courant | Demander `NextMode` | Cycle Auto → InteractionCell → SitAdjacent → Watch → Auto. |
| U28 | P2 | Plusieurs entrées couvertes et orphelines de plusieurs types | Grouper les entrées | Un groupe par type, orphelins avant couverts puis tri par label. |
| U29 | P2 | Types avec différents nombres de bâtiments, dont des égalités | Trier par nombre puis par nom | Nombre décroissant avec nom en départage ; mode alphabétique uniquement par nom. |
| U30 | P2 | Types sans giver actif, avec orphelin, uniquement couverts, actifs sans bâtiment | Trier par état | Cet ordre de catégories est respecté, avec tri par nom dans chaque catégorie. |

## Diagnostic et persistance

| ID | Niveau | Priorité | Étant donné / action | Résultat attendu |
|---|---|---|---|---|
| D31 | Intégration | P1 | Sauvegarder puis recharger des paramètres non triviaux et un type personnalisé | Options, dictionnaires, types, `needsThing`, tri et compteur d'identifiants préservés. |
| D32 | Intégration | P1 | Charger des paramètres anciens sans les champs ajoutés | Valeurs par défaut restaurées, toutes les collections non nulles et utilisables. |
| U31 | Unitaire | P2 | Produire `Report` avec zéro orphelin, puis plusieurs sources et états | Compteurs exacts ; tri sans code propre avant avec code propre, puis source et label ; identifiant, type, mode, état individuel et avertissement présents. |
| D33 | Intégration | P0 | Provoquer une exception contrôlée dans `Generate`, appeler le postfix | Exception non propagée et journal d'erreur avec préfixe Joy Rescue et exception d'origine. Ce cas ne prouve pas un rollback des mutations déjà faites. |

## Régressions à préciser avant implémentation

Ces cas expriment des garanties souhaitables et peuvent échouer avec le code actuel.
Ne pas les transformer en tests validant automatiquement le comportement existant.

| ID | Priorité | Scénario | Attendu souhaité et point à vérifier |
|---|---|---|---|
| R01 | P1 | Charger `modeOverrides[A]="99"` | Revenir au mode automatique pour une valeur hors enum. `Enum.TryParse` accepte actuellement les valeurs numériques non définies ; `resolvedMode` peut donc devenir 99. |
| R02 | P0 | Réaffecter un giver dont le job est partagé avec un autre giver | Aucun désaccord entre type choisi et type crédité pour les activités affectées. Le code modifie le job partagé mais seulement le giver ciblé et ses bâtiments ; les autres reçoivent un message sans être synchronisés. Décider entre propagation cohérente et séparation du job. |
| R03 | P0 | Appeler `Generate` deux fois sur la même base de defs | Conserver le suivi des réparations et la capacité de les désactiver après le second passage. Les givers créés au premier passage peuvent rendre les bâtiments « déjà couverts » au second et vider `Entries`. |
| R04 | P1 | Désactiver tous les givers d'un type ayant des bâtiments couverts, puis compter les types utilisables | Ne plus compter ce type comme utilisable. `UsableKindCount` se fonde actuellement sur la couverture et l'activation individuelle, sans consulter les poids effectifs ni `disabledKinds`. |

## Vérifications en jeu complémentaires

Ces vérifications ne remplacent pas les tests précédents et ne sont pas unitaires.

1. Charger avec un orphelin de chaque mode : génération au PreResolve, références,
   indices et short hashes valides, puis recherche de loisir sans erreur de DefMap.
2. Observer un colon utiliser réellement chaque bâtiment et recevoir le bon type de
   loisir ; vérifier positions, capacités requises et absence d'erreurs répétées.
3. Tester Watch avec sièges, lit et sans siège selon l'option ; tester une activité
   de groupe avec assez de cellules disponibles, puis avec trop peu de places.
4. Changer le mode et désactiver une réparation en cours de partie : les nouvelles
   sélections suivent les paramètres. Ne pas exiger l'arrêt immédiat d'un job en cours.
5. Créer un type, l'affecter, redémarrer une fois et vérifier sa disponibilité ainsi
   que la conservation des tolérances des types déjà présents dans la sauvegarde.
6. Ouvrir l'éditeur en français et en anglais : libellés, états en attente,
   avertissements, tri et réinitialisation cohérents après les changements.

## Compléments issus de la revue d'exhaustivité du 12 septembre 2026

La première version couvrait les principales règles du générateur, mais pas toutes
les règles de l'éditeur ni plusieurs interactions entre changements successifs.
Cette revue est une couverture fonctionnelle par inspection, pas une mesure de
couverture de lignes/branches et pas une garantie d'exhaustivité de tous les mods tiers.

| ID | Niveau | Priorité | Données et action | Attendu |
|---|---|---|---|---|
| U32 | Unitaire | P2 | Givers de même type à poids positif, nul, négatif, et giver d'un autre type ; calculer compteurs | `ActiveGiverCount` compte uniquement les poids strictement positifs du type ; `TotalGiverCount` les compte tous. |
| U33 | Unitaire | P2 | Rapports avec TargetA/B/C, ponctuation finale, doublons, rapport nul et job absent ; calculer Activities/ActivityName | Jetons retirés sans supprimer un mot comme TargetAlpha ; rapports nettoyés et dédupliqués dans Activities ; repli sur defName dans ActivityName si nécessaire. |
| U34 | Unitaire | P2 | Listes de 0, 1, 8 et 9 éléments ; appeler Join | Jusqu'à 8 éléments affichés ; au-delà, 8 éléments et nombre exact d'éléments supplémentaires. |
| U35 | Unitaire | P2 | Givers de plusieurs types, dont un sans type ; construire GiversByKind | Giver sans type ignoré, autres regroupés par type et triés par nom d'activité. |
| U36 | Unitaire | P1 | Caches remplis ; modifier état, nom ou affectation sans changer le nombre d'entrées ; invalider et relire | Groupes, ordre et infobulles reflètent les nouvelles données ; pas de valeur obsolète conservée par un cache. |
| U37 | Unitaire | P1 | Plusieurs réparations, dont une avec code propre ; SetAll(false), puis SetAll(true) | Choix de toutes les réparations mis à jour, poids effectifs appliqués, couverture préexistante inchangée ; un type désactivé reste à poids nul. |
| U38 | Unitaire | P2 | Plusieurs bâtiments du même type et réparations désactivées ; compter les éléments | EnabledCount compte les choix individuels, UsableKindCount compte les types distincts ; compléter par R04 pour les poids effectifs. |
| D34 | Intégration/UI | P1 | Créer, supprimer un type en attente, puis en créer un autre | Identifiant non réutilisé ; réaffectations de l'ancien purgées, nouvelle identité distincte ; compteur préservé après sauvegarde. |
| D35 | Intégration/UI | P1 | Sélectionner une réaffectation, choisir le type courant, puis utiliser Revert ; cas bâtiment et activité | Dictionnaire enregistré puis supprimé selon l'action ; defs non mutées à chaud ; caches invalidés et affichage pending cohérent. |
| D36 | Intégration/UI | P0 | Supprimer un type personnalisé déjà matérialisé | Def et indices conservés durant la session ; références aux réaffectations purgées. Vérifier séparément au rechargement l'impact de sa disparition sur les tolérances sauvegardées. |
| D37 | Intégration/UI | P1 | Renommer un type déjà matérialisé puis sauvegarder/redémarrer | Vérifier le nom affiché avant et après redémarrage ; le code actuel ignore les types déjà présents pendant CreateCustomKinds. Préciser le contrat de renommage à chaud avant une assertion définitive. |
| D38 | Intégration/UI | P1 | Sauvegarder par bouton et par fermeture des paramètres | Persistance et ApplySettings déclenchés ; à la réouverture, choix conservés. |
| D39 | Intégration | P0 | Deux activités partageant un job, réaffectées vers des types contradictoires | Résultat cohérent quel que soit l'ordre d'insertion des paramètres ; préciser stratégie de conflit avec R02. |
| D40 | Intégration | P0 | Même bâtiment présent deux fois dans une liste thingDefs ; le réaffecter | Toutes ses occurrences doivent être détachées pour permettre sa réparation sur le nouveau type. `List.Remove` n'en supprime actuellement qu'une : cas de robustesse à confirmer. |
| D41 | Intégration | P0 | Génération réussie, puis nouvelle génération échouant après une mutation | Aucun faux signal de scan réussi ni rapport affirmant un rollback inexistant ; vérifier `HasRun` et état partiel. |
| D42 | Intégration | P1 | Collision d'un nom de def générée avec une def existante | Pas de doublon ; vérifier explicitement l'impact du remplacement actuel par AddDef sur les références. Ce remplacement ne doit jamais être utilisé après allocation des DefMaps. |
| D43 | Intégration | P1 | Retirer une surcharge déjà appliquée puis redémarrer sur les defs XML d'origine | Type d'origine restauré ; comparaison avec simple ApplySettings, qui ne rejoue pas les réaffectations. |
| D44 | Intégration | P1 | Option mode modifiée pour une entrée désactivée ; appliquer, puis réactiver | Mode cible déjà cohérent et worker invalidé ; poids nul jusqu'à réactivation. |
| D45 | Intégration | P0 | Désactiver un type, ajouter un nouveau giver de ce type avant réapplication | Nouveau giver neutralisé ; valeur initiale mémorisée et restaurée à la réactivation. |

### Matrice de couverture actuelle

| Zone | Scénarios | Automatisation initiale |
|---|---|---|
| Paramètres et identité | U01–U12 | Oui, 26 cas paramétrés |
| Heuristique | U13–U16 | Oui, 9 cas |
| Reconfiguration | U17–U21, D44 | U20 seulement, 3 cas ; moteur/traductions à initialiser pour les autres |
| Valeurs de paramètres invalides | R01 | 2 cas qui échouent avec le code actuel, commande dédiée |
| Reset répété | U10 | Inclus dans son cas |
| Détection et génération | D01–D13, D42 | À implémenter |
| Activation à chaud | D14–D20, D45 | À implémenter |
| Types et réaffectations | D21–D30, D34–D37, D39–D40, D43 | À implémenter |
| Éditeur, compteurs, caches | U22–U30, U32–U38, D35, D38 | U27 : 4 cas ; U33 : 7 cas ActivityName ; reste à implémenter |
| Rapport, persistance, erreurs | U31, D31–D33, D41 | À implémenter |
| Rejeu, jobs partagés, types utilisables | R02–R04 | Soupçons issus du code, non reproduits automatiquement |
| Cycle réel RimWorld et sauvegardes | Vérifications en jeu 1–6, D36 | À effectuer en jeu |

Le total de la suite nominale est de 49 cas : 26 pour U01–U12, 9 pour
l'heuristique, 3 pour U20 et 11 pour l'éditeur. Une ligne de scénario peut
correspondre à plusieurs cas.
