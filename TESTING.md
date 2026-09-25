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
(21 checks) covers resources, translation call arguments, the shortcut definition and the
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
| Tooltips | `wsl-deps.hover.map`, English then French | `06` | The two long tooltips (the fix set option, the View button) show, in both languages |
| Orphans | `wsl-deps.orphans.map`, one ticket per feature | `07` to `12` | Scan and counters, real use in the three modes, seats and group, live changes, own code, a disabled type |
| Settings across restarts | `wsl-deps.settings-restart.map`, a chain per ticket | `13`+`14`, `15`+`16`+`17`, `18`+`19`+`20`, `21`+`22` (English and French), `29`+`30`+`31`, and `23` alone | Create and reassign types, delete one, the reset button with real clicks, persistence and translations, the fix set across restarts, the mod list |
| RIMMSQOL, restart | `wsl-deps.avec-rimmsqol.map`, a chain | `24`+`25`+`26` | The revealed shortcut survives a restart, and the exact RIMMSQOL version is on record |
| Mod removal and addition | `wsl-deps.removal.map`, two tickets | `27` + `removal-check` without Joy Rescue, then `28` | A colony saved mid-job without the mod loads with it, and the reverse |
| Fix set guards | `wsl-deps.rules.map` | `32` | Every guard of the fix set on witness mods |
| Fix set on real mods | `wsl-deps.real-mods.map`, a chain | `33`+`34` | Five Workshop mods corrected and used |

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

`Tests/MANUAL.md` (F01 to F14) and `Tests/TAXONOMY.md` (F15 to F19) are the acceptance cases.
`AUDIT.md` asks that each be automated and green, or listed as not applicable with its reason,
before `tested`: no manual test may be left. All nineteen are now written as Pickle scenarios
(witness mods under `Tests/Pickle/`, steps in `Tests/Pickle/Source/`). Where they stand:

| Case | Feature | Result |
| --- | --- | --- |
| F01 load, counters | `07` | passed, run of 2026-09-25 21:07, tree `fed2bb2` |
| F02 use in the three modes | `08` | passed, run of 2026-09-25 21:09, tree `fed2bb2` |
| F03 seats and group | `09` | filed at `07065ce`, not yet run. The part with a third-party group-reservation mod is untestable for now, with the owner's agreement (2026-09-25): no such mod is known or in the test set. The vanilla groups (chess and Ur for 2, poker for 4) and the seat and cell counts of a witness building are tested. To be played the day a mod with its own group reservation is found |
| F04 live changes | `10` | filed at `07065ce`, not yet run |
| F05 own code | `11` | filed at `07065ce`, not yet run |
| F06 disabled type | `12` | filed at `07065ce`, not yet run |
| F07 create, reassign | `13` then `14` | filed at `07065ce`, not yet run |
| F08 delete, revert | `15` then `16`, `17` | filed at `07065ce`, not yet run. The tolerance map is indexed by position, so a failure here can be a real defect of the mod |
| F09 persistence, translations | `21` then `22`, English and French; `01` | `01` passed; the chain filed at `07065ce`, not yet run |
| F10 reset | `18` then `19`, `20` | filed at `07065ce`, not yet run |
| F11 removal, addition | `27` + `removal-check`, then `28` | filed at `07065ce`, not yet run |
| F12 presentation | `23` | filed at `07065ce`, not yet run. Not applicable: the click on the source link, which hands the address to the operating system's browser; the address and the BBCode are asserted instead |
| F13, F19 shortcut, RIMMSQOL | `24` then `25`, `26`; `02` | `02` passed. The chain passed, three launches, run of 2026-09-25 22:42, tree `07065ce`. RIMMSQOL is Workshop item 1084452457; its About.xml carries no version number, so the build is identified by `1.6/Assemblies/RIMMSqol.dll`, 1,822,720 bytes, SHA-256 starting `1152b0c198d34d4b` |
| F14 shared activities | `03`, `04`, `05` | definitions and jobs passed in both orders; the two colonist scenarios added to `05` filed at `07065ce`, not yet run |
| F15, F18 fix set, save and restart | `29` then `30`, `31` | filed at `07065ce`, not yet run |
| F16 fix set on real mods | `33` then `34` | filed at `07065ce`, not yet run |
| F17 guards | `32` | filed at `07065ce`, not yet run |
| Tooltips (not a numbered case) | `06` | English passed at `fed2bb2`; French filed at `07065ce`, not yet run |

Nothing here is a pass until it has run. A skipped scenario, a `@review` capture nobody opened and
a green run that shows nothing are not passes.
