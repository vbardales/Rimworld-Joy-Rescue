# Joy Rescue

Makes usable again the recreation buildings their own mod left inert.

## The problem

In RimWorld, `<building><joyKind>` on a `ThingDef` declares its recreation type and contributes to map availability; it does not schedule an activity by itself. What makes
a building actually usable is a `JoyGiverDef` whose `<thingDefs>` list contains that building,
plus the `JobDef` that goes with it. Plenty of mods write the label and stop there: the building
gets built, looks the part, and nobody ever uses it.

This matters more than it looks. **Recreation tolerance is tracked per type**
(`Need_Joy.tolerances`), and expectations ask for up to **6 different types**
(`ExpectationDef.joyKindsNeeded`, from 2 to 6). The installed 1.6 Core declares 10 types; equipment, activities and optional DLC
provide different access routes. A recreation type no giver can produce is a type the colony simply does not have.

## What the mod does

At load time it compares every `ThingDef` carrying a `joyKind` against every
`JoyGiverDef.thingDefs`, and builds the missing `JobDef` and `JoyGiverDef` for the orphans. Each
mode is a giver and driver pair **the base game already uses as it stands**:

| Mode | Giver | Driver | Vanilla model |
|---|---|---|---|
| Interaction cell | `JoyGiver_InteractBuildingInteractionCell` | `JobDriver_WatchBuilding` | telescope, instruments |
| Play beside | `JoyGiver_InteractBuildingSitAdjacent` | `JobDriver_SitFacingBuilding` | chess, game of Ur, poker |
| Watch | `JoyGiver_WatchBuilding` | `JobDriver_WatchBuilding` | televisions |

The choice is made on the **shape of the def**, never on its name: a declared interaction cell is
an explicit intention of the author, and it is honoured; a `Television` `joyKind` is watched;
everything else is played from an adjacent cell. Every line of the settings can force the mode by
hand.

## The technical point not to undo

Generation happens in a **postfix of `DefGenerator.GenerateImpliedDefs_PreResolve`**. It is the
only window where both conditions hold:

1. `DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences` has already run, so `building.joyKind`
   is a real object and not a string still waiting;
2. `DefDatabase<T>.ResolveAllReferences()` has **not** run yet. `JobGiver_GetJoy.ResolveReferences`
   allocates there a `DefMap<JoyGiverDef, float>` sized on the number of existing `JoyGiverDef` and
   indexed by `def.index`. Adding a giver afterwards would push the indexing out of that array
   **on every recreation-seeking tick**.

Short hashes are handed out further down the load ("Short hash giving"), so the generated defs
receive one without any effort.

A corollary: a def is **never** removed from the database. Disabling an entry sets its `baseChance`
to 0, which gives it a zero draw weight in `JobGiver_GetJoy.TryGiveJob`
(`TryRandomElementByWeight`): the giver is never picked again. That is what makes the settings
applicable to a running game.

## False positives

A mod shipping its own `JoyGiver` or `JobDriver` can serve its building in code without ever
listing it in a `thingDefs`. Repairing it would create two competing jobs on the same piece of
furniture. Those buildings are therefore **listed but disabled by default**, with a warning;
detection is done by reflection over the assemblies of the originating `ModContentPack` alone
(those of its dependencies are not in there).

## Settings

Open **Options → Mod options → Joy Rescue**. Repairs and mode changes apply to new
activity selections immediately. Creating recreation types and reassigning buildings
or activities require saving settings and restarting RimWorld. Settings are global,
not per save; existing custom names are preserved when changing the interface language.

An optional `JoyRescue_Settings` MainButtons entry opens this same settings dialog.
It is hidden by default, and its standard `buttonVisible` field can be exposed by
MainButtons customization tools. Such tools are not required for the primary settings
entry. Actual RIMMSQOL integration is pending in-game acceptance; see `Tests/MANUAL.md`.

## Common recreation fix set

In the window this option reads "Move other mods' activities to better recreation types".
An optional preset in **Mod options -> Joy Rescue** applies exact, guarded corrections
at the next restart. It is off by default. Save settings before restarting. It
reclassifies compatible activities together with their jobs and equipment, preserving
drivers, durations and rewards. Manual assignments win; changed or conflicting shared
activities are skipped and reported through the existing Log report button.

The preset includes three saved types: video games and simulations, creative recreation,
and sensory wellbeing. Their initial names are localized; subsequent edits remain user
names. Once used, custom types and their order are retained, including on Reset, to
avoid shifting save indices. Turning the preset off restores original assignments after
restart, subject to manual choices, while retaining types and transferred tolerance.

This is a guarded correction set, not a universal remapping of every mod or ingestion
system. See [scope, save behavior and acceptance scenarios](Tests/TAXONOMY.md).

## Building

```
dotnet build Source/JoyRescue.csproj -c Release
```

An NTFS junction links `RimWorld\Mods\JoyRescue` to this folder, so the DLL reaches the game
without being copied.

## Saves

The common preset records per-pawn tolerance-transfer markers when saving. Each accepted
source/target transfer uses the maximum rather than a sum and runs once. Newly saved
pawns do not receive a legacy transfer on their first reload. This has automated Scribe
evidence; complete existing-save/gameplay validation remains pending.

Removing Joy Rescue can lose a generated job currently being performed and removes
its custom type definitions. Do not interpret offline tests as certification for every
mod-list change or saved colony. See Tests/TAXONOMY.md for the precise scope.

## Licence

MIT. This is original work: it owes nothing to another mod, not a name, not an asset, not an idea
traceable to one.
