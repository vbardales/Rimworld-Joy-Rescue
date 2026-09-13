# Joy Rescue automated tests

## Current commands

Prerequisites: .NET 8 SDK, installed RimWorld, and cached or accessible NuGet build
dependencies. Run from the repository root:

```powershell
dotnet build Tests/JoyRescue.Tests.csproj -c Release
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe
# Backward-compatible alias; regressions now run by default:
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe --regressions
pwsh -NoProfile -File Tests/Test-Xml.ps1
pwsh -NoProfile -File ../scripts/Check-DefInjected.ps1 -TransMod ./Mod
```

For a different game location, pass
`-p:RimWorldManagedDir=D:\Games\RimWorld\RimWorldWin64_Data\Managed` to the build.
The build compiles production net48 and the net8.0 runner. Exit 0 means success;
exit 1 means a failed assertion. `dotnet test` does not discover this custom runner.
Game runtime dependencies are copied only under ignored `.build/tests/`, never Mod/.

## Scope and limitations

Tests invoke the compiled production DLL and installed Verse/RimWorld types, without
loading or writing the player's preferences or saves. ThingDef fixtures use
`RuntimeHelpers.GetUninitializedObject` to avoid Unity shader initialization; they
supply only the relevant fields. This does not validate constructor defaults or
complete in-game definitions. .NET 8 can load the required netstandard 2.1 dependencies
but is not Unity/Mono. Private editor methods are invoked through reflection.

Baseline coverage: U01–U16, U20, U27 and ActivityName in U33. R01 is now mandatory.
An additional test binds the distributed shortcut definition to its compiled worker and
checks that it inherits native visibility. This is a contract test, not a reveal/hide
interaction test: calling Visible here initializes Unity save paths and cannot run in
this data-only environment. The attempted run is preserved in
`.build/fix-2026-09-13/visibility-runtime-attempt.txt`; use F13 for interaction.
There is no line/branch coverage measurement. SettingsIntegration.cs now executes full
Retarget transitions (including worker invalidation) in EN and FR, live weight restoration,
real generation and reassignment, editor helpers and Scribe serialization. The current
suite passes **95/95**; results are in `.build/settings-2026-09-13/results.txt`.
Manual acceptance is in MANUAL.md and remains unexecuted.

The fixtures initialize real Verse language objects from the distributed Keyed files,
capability DefOf bindings and the custom-type discovery cache. They use minimal definitions
and clear their databases between cases; globals they change are restored on disposal.
Unity profiling and unrelated date/colonist text decoration are disabled in the fixtures.
Generation/reassignment tests use Verse's Log.LockMessages to avoid the Unity log sink:
these tests check resulting definitions, not clean in-game logs. Scribe tests use the real
saver, extractor and loader through all loading phases, writing only `.build/` files.
No production settings, translation, serialization or generation method is mocked/patched.
The explicit Publicizer assembly attribute permits the unchanged private-member accesses
on .NET 8 as well as the intended Mono environment.

Pass --trace-exceptions for first-chance exception diagnostics when investigating a runner
initialization issue; the normal command does not enable this verbose diagnostic output.

The XML suite checks metadata, the exact final GitHub link, well-formed XML, nonempty
unique keys, EN/FR parity, format strings, literal translation calls and shortcut
resources. Static checks do not prove UI layout, actual game language loading or
customization-tool integration. DefInjected validation checks paths against the game
and target definitions, not exhaustive gameplay behavior.

## Historical results preserved

2026-09-12 baseline, reproduced by the 2026-09-13 pre-fix audit: **49/49 nominal,
49/51 with R01, 13/13 XML**. Undefined numeric modes `99` and `-1` were accepted
instead of Auto. These two failures were genuine reproductions, not ignored tests
or assertions changed to accept the defect. They were opt-in at that time and now
run by default. Initial outputs remain under `.build/tests/`; the pre-fix audit
outputs remain under `.build/audit-2026-09-13/`.

See STATUS.md for the fixed revision/worktree, current results and evidence paths.
A passing off-game suite never certifies the manual acceptance scenarios.