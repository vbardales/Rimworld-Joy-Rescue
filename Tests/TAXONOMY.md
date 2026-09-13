# Common recreation fix set

## Scope

The optional preset contains 230 exact package/giver rules selected from the local
recreation inventory. These are compatibility identifiers and original correction
logic, not copies of the referenced mods. No inventory dumps, decompiled game code,
third-party DLLs or extracted assets belong in delivery commits.

This is a guarded building-activity implementation of the twelve-family design,
not a claim that every installed activity has been converted to exactly twelve types.
The existing vanilla categories remain, with three reserved custom types for video
games, creative recreation and sensory wellbeing. Ingestion aliases, direct C# gains,
unresolved interactions, unidentifiable legacy packages and unsupported drivers are
outside automatic correction. Missing optional mods are ignored.

Each rule checks the package, giver, old type, job identity and complete equipment
set. Only known vanilla giver/driver combinations and the audited Joy Preservation
mahjong visual adapter are supported. These guards detect structural changes, not
every possible future change inside another mod's code. The adapter was inspected;
its implementation is not redistributed.

Shared jobs and buildings are analyzed before any changes. If every consumer cannot
end on the same category, the plan is pruned to a fixed point. This preset does not
clone shared jobs or replace drivers. Job durations, skill XP, rewards, weights and
participant limits are retained. Manual giver/building assignments take priority.

## Settings contract

Default: off. Primary route: Mod options -> Joy Rescue. The existing hidden
MainButtons shortcut opens the same dialog; no additional access route is required.

Enabling queues corrections for the next full restart. Save settings first. The
existing pending-restart indicator covers both enabling and disabling. Log report
includes applied and skipped counts plus the skipped-rule reasons.

Three type identities are appended to the existing saved custom-kind list. Their
initial names are translated in the current UI language, then remain editable saved
names, like other custom kinds. All three require an actual object for automatic map
availability; they do not manufacture universally available recreation categories.

Once reserved types exist, deleting custom types is withheld and Reset retains the
whole custom-type list and its order, while clearing activity options and disabling
the preset. This prevents shifting those saved type indices. Turning the preset off
restores original XML assignments at the next startup, subject to explicit manual
assignments. It is not an exact rollback of already transferred tolerance.

## Save transfers

For correction pairs actually accepted during startup, load transfers the maximum
of source and target tolerance, retaining boredom without summing tolerances. A
snapshot prevents order-dependent cascading between pairs. Source/target markers are
saved per JoyToleranceSet, so each transfer occurs once. Splits copy the source to
each applicable target. New pawns saved under the preset record current pairs without
receiving a spurious legacy transfer on their first reload.

Original definitions and source tolerance values are retained. The local Scribe tests
exercise serialization and transfer logic, not a complete real colony or arbitrary
mod-order changes. Game-wide positional DefMap compatibility remains sensitive to
changes in the overall mod list; this feature does not solve that engine limitation.

## Automated evidence

`TaxonomyIntegration.cs` adds 23 cases to the existing 95-case runner: EN/FR defaults,
stable type ordering/reset retention, exact application and replay, all guards, shared
consumer rejection and agreement, tolerance merge/split/non-cascading/once behavior,
real Scribe settings and marker round trips, restart state, and first-save behavior.
The XML suite checks both translated resources and the unchanged native shortcut.

Commands from the repository root:

```powershell
dotnet build Tests/JoyRescue.Tests.csproj -c Release
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe
pwsh -NoProfile -File Tests/Test-Xml.ps1
```

## Remaining game acceptance

- F15: open the primary settings route in EN and FR; ensure the added controls fit,
  save the preset, restart and inspect the reported corrections on the installed pack.
- F16: exercise a changed table, musical/visual activity and wellbeing object; check
  actual job behavior, group participation, gains and logs for kind mismatches.
- F17: verify a deliberately conflicting shared building remains unmodified; verify
  manual assignments and source-provided special drivers retain priority.
- F18: load an existing colony, check tolerance/ennui, save and reload twice; repeat
  with a new colony and a newly recruited pawn. Then disable and restart, checking
  assignments and retained types without expecting tolerance rollback.
- F19: repeat existing shortcut reveal/open/hide checks with RIMMSQOL and verify both
  settings routes share the option. No live integration was certified offline.

These scenarios are unexecuted until recorded with actual in-game evidence.
