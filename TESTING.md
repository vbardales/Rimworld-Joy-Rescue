# Testing Joy Rescue

Two layers. What can be proved outside the game is proved outside it, in seconds. What only a
running game can show goes through Pickle, in the headless WSL install, in small tickets.
`Tests/README.md` describes the offline suites, `Tests/Pickle/README.md` the in-game features and
the evidence to keep, `docs/runs/README.md` the runs so far, `STATUS.md` where it all stands.

## Offline

```powershell
dotnet build Tests/JoyRescue.Tests.csproj -c Release
& ./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Test-Xml.ps1
```

The executable runner (127 cases) runs the compiled mod and real game types with no game start:
settings, the choice of mode, generation, reassignment, tolerance transfer, the order of the
settings list (`L01` to `L09`), real Scribe round trips, and EN/FR resources. The XML suite
(20 checks) covers resources, translation call arguments, the shortcut definition and the
distributed documents. Exit 0 is a pass. Neither proves a pawn used a building.

## In game: the passes

A mod is not validated by one run. Each pass below is **one request** to the dispatcher
(`Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1`, see `Tests/Pickle/README.md`), with
the SHA of the tree in `-Label`.

| Pass | `-DepMap` and arguments | Features played | What it establishes |
| --- | --- | --- | --- |
| Minimal English | none, `-Language English` | `01` (5 scenarios); `02` to `05` skip, their mods are not staged | The settings window renders, the shortcut is hidden and not greyed, it opens the same window, a setting persists, the list can be arranged buildings first. The only pass where a capture is clean |
| Minimal French | none, `-Language French` | `01` | The same, in French: raw keys, fallback text, clipping |
| With RIMMSQOL | `wsl-deps.avec-rimmsqol.map`, English | `01` and `02` (8 scenarios) | RIMMSQOL lists the hidden shortcut, reveals it, the revealed button opens the window, hiding restores the hidden state |
| Shared job, chess then Ur | `wsl-deps.shared-job.map`, `-Filter 03-... -Then 05-...` | `03`, `05` | Two activities sharing a job keep their own kinds after a real restart |
| Shared job, Ur then chess | `wsl-deps.shared-job.map`, `-Filter 04-... -Then 05-...` | `04`, `05` | The same, in the reverse insertion order |

The writer and its reader of the shared-job pairs must be two game processes, which is what
`-Then` gives under one hold of the lock. That is why those two passes cannot run without a
filter, and why each pair is its own request.

**No optional mod is declared.** `loadAfter` names Harmony and the five DLC only, and detection
happens at runtime, so load order changes nothing. RIMMSQOL is a test-only integration of the
shortcut and the shared-job witness is a test-only mod: neither is a dependency of the mod.
**No incompatibility is declared** (`incompatibleWith` is absent), so no pass exists to check one.
If either changes, this table changes with it.

## Which run for which work

A ticket for a fix or an exploration plays as few scenarios as possible: one feature, or
`-Filter '::<scenario name>'`. An initial or a final ticket plays everything, a pass per ticket.
Filing many small tickets is preferred to one large one.

## The manual acceptance cases

`Tests/MANUAL.md` (F01 to F14) and `Tests/TAXONOMY.md` (F15 to F19) are not executed. `AUDIT.md`
asks that each be either automated and green or listed as not applicable with its reason before
`tested`. Where they stand:

| Case | In game so far | What is missing |
| --- | --- | --- |
| F09 settings, translations, persistence | `01` in English and French, with captures | A restart, the sort order, a custom type name |
| F13, F19 shortcut and RIMMSQOL | `02`, with captures | Hiding then restarting, the RIMMSQOL version |
| F14 shared activities | `03`, `04`, `05`: loaded definitions and jobs in both orders | Whether a pawn credits the kind, which needs the two tables in a colony |
| F15 fix set, controls | The controls fit in the two languages | Saving it, restarting, reading the corrections |
| F01, F02, F03, F04, F05, F06, F07, F08, F10, F11, F16, F17, F18 | Nothing | Named witness buildings and mods, or saved-game fixtures: `Tests/Pickle/Fixtures/README.md` |
| F12 mod list | Nothing | A capture of the mod list |

Nothing here is a pass until it has run. A skipped scenario, a `@review` capture nobody opened and
a green run that shows nothing are not passes.
