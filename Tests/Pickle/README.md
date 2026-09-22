# Joy Rescue in-game tests

This companion is development-only. `Mod/` is staged beside Joy Rescue by
`scripts/Run-PickleWsl.ps1`; it is never part of Joy Rescue's Workshop payload.

The executable and XML suites already cover generation, configuration validation,
serialization, localization resources, and MainButton definition binding. These features
keep only the evidence that requires RimWorld to run: the real settings window, the
hidden shortcut opening that window, and RIMMSQOL's reveal/hide lifecycle.

| Pass | Command suffix | Features | Establishes |
| --- | --- | --- | --- |
| Minimal English | `-Language English` | `01-settings-and-shortcut.feature` | Settings window renders; the shortcut is hidden (not greyed); its worker opens the same dialog; a Boolean setting persists through close/reopen. |
| Minimal French | `-Language French` | `01-settings-and-shortcut.feature` | The same settings surface in the French startup language; review the screenshot for raw keys, fallback text and clipping. |
| RIMMSQOL | `-DepMap wsl-deps.avec-rimmsqol.map -Language English -Filter 02-rimmsqol-shortcut.feature` | `02-rimmsqol-shortcut.feature` | RIMMSQOL lists, reveals, hides and forgets `JoyRescue_Settings`; the revealed button opens Joy Rescue settings. |

Run only through the shared harness, never by launching RimWorld directly:

```powershell
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -Language English
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -Language French
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -DepMap wsl-deps.avec-rimmsqol.map -Language English -Filter 02-rimmsqol-shortcut.feature
```

Read `exitReason` before counts, compare discovered and played scenarios, inspect every
`@review` capture, and copy reports out of the shared Pickle report directory before another
run replaces them.

`Tests/MANUAL.md` F01-F12 and F14 remain required acceptance scenarios for actual orphan
buildings, seats, group reservations, third-party own-code detection, reassignment, tolerance
transfer, and new/existing saves. They require named witness buildings/mods and cannot be
truthfully replaced by a fabricated generic fixture. The saved-game handoff for F10-F12 and
F14 is [Fixtures/README.md](Fixtures/README.md). Record a missing witness as BLOCKED.
