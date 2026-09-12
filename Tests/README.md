# Tests automatisés JoyRescue

Première tranche exécutée le 12 septembre 2026 : **49/49 cas nominaux passent**.
La commande de régression exécute aussi deux cas R01 : **49/51 passent**, les
valeurs numériques `99` et `-1` sont acceptées au lieu de revenir à `Auto`.
Le défaut est reproduit, pas corrigé dans cette tranche.

## Exécution sous Windows

Prérequis : SDK .NET 8, RimWorld installé, dépendances NuGet de compilation du mod
accessibles ou déjà en cache. Depuis la racine du dépôt :

```powershell
dotnet build Tests/JoyRescue.Tests.csproj -c Release
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe
# Inclut les deux assertions de régression actuellement en échec :
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe --regressions
```

Pour une installation du jeu ailleurs :

```powershell
dotnet build Tests/JoyRescue.Tests.csproj -c Release '-p:RimWorldManagedDir=D:\Games\RimWorld\RimWorldWin64_Data\Managed'
```

La compilation reconstruit le mod net48, puis le programme de tests net8.0.
Le runner affiche chaque cas et retourne **0 en réussite, 1 en échec**. Il s'agit
d'un petit exécutable sans framework de test supplémentaire : `dotnet test` ne
découvre pas ces cas. Les dépendances réelles du jeu sont copiées uniquement sous
`.build/tests/`, ignoré par Git et extérieur au dossier publié `Mod/`.

## Ce que ces résultats prouvent

`Program.cs` teste directement les classes de la DLL JoyRescue compilée, sans
recopier les règles de production. Les classes Verse/RimWorld sont celles de
l'installation locale. Les paramètres sont des instances neuves par cas ; aucune
partie ni préférence du joueur n'est chargée ou modifiée.

Le constructeur de ThingDef charge des shaders Unity : les fixtures utilisent
`RuntimeHelpers.GetUninitializedObject` et renseignent explicitement les seuls
champs nécessaires. Cela ne vérifie ni les valeurs par défaut du constructeur,
ni la validité d'une def complète dans le jeu. Le runtime .NET 8 permet de charger
les dépendances netstandard 2.1, mais ne remplace pas Unity/Mono.

Couverture initiale : U01–U16, U20, U27 et la partie ActivityName de U33.
Les méthodes privées de l'éditeur sont appelées par réflexion, sans modifier leur visibilité.
Pas encore de mesure de couverture de lignes.
Les transitions complètes Retarget, `workerInt`, `baseChance`, génération,
sérialisation et autres règles d'éditeur restent à automatiser avec leur environnement
initialisé. Voir `SCENARIOS.md` pour la revue et les nouveaux cas identifiés.

Les cas R01 sont volontairement séparés pour conserver une commande nominale
utilisable tout en fournissant une reproduction en échec. Ils ne sont ni ignorés
silencieusement, ni transformés en assertions acceptant le défaut.
