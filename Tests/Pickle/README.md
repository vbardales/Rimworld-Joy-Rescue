# Joy Rescue in-game tests

This companion is development-only. `Mod/` is staged beside Joy Rescue by
`scripts/Run-PickleWsl.ps1`; it is never part of Joy Rescue's Workshop payload.

The executable and XML suites already cover generation, configuration validation,
serialization, localization resources, and MainButton definition binding. These features
keep only the evidence that requires RimWorld to run: the real settings window, the
hidden shortcut opening that window, and whether RIMMSQOL can reveal this mod's button.

| Pass | Command suffix | Features | Establishes |
| --- | --- | --- | --- |
| Minimal English | `-Language English` | `01-settings-and-shortcut.feature` | Settings window renders; the shortcut is hidden (not greyed); its worker opens the same dialog; a Boolean setting persists through close/reopen. |
| Minimal French | `-Language French` | `01-settings-and-shortcut.feature` | The same settings surface in the French startup language; review the screenshot for raw keys, fallback text and clipping. |
| RIMMSQOL | `-DepMap wsl-deps.avec-rimmsqol.map -Language English -Filter 02-rimmsqol-shortcut.feature` | `02-rimmsqol-shortcut.feature` | RIMMSQOL lists and reveals `JoyRescue_Settings`; the revealed button opens Joy Rescue settings. Hiding/forgetting only cleans up the test. Visibility persistence across a restart belongs to RIMMSQOL's own suite. |
| Shared job, both orders | `-DepMap wsl-deps.shared-job.map -Filter 03-shared-job-write-chess-then-ur.feature -Then 05-shared-job-read.feature,04-shared-job-write-ur-then-chess.feature,05-shared-job-read.feature`, under `-Command` (see below) | `03`, `05`, `04`, `05` | The two loaded witnesses share a job before writing; after a real restart, both assigned kinds have independent jobs (`03` then `05`), then the same check in reverse insertion order (`04` then `05`). Four launches under one queue ticket; `-EvidenceDir` keeps `seq1` to `seq4`, which spares a second wait behind the queue. |

Run only through the shared harness, never by launching RimWorld directly:

```powershell
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -Language English
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -Language French
powershell.exe -ExecutionPolicy Bypass -File scripts/Run-PickleWsl.ps1 -Mod JoyRescue -DepMap wsl-deps.avec-rimmsqol.map -Language English -Filter 02-rimmsqol-shortcut.feature
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& 'scripts/Run-PickleWsl.ps1' -Mod JoyRescue -DepMap wsl-deps.shared-job.map -Filter 03-shared-job-write-chess-then-ur.feature -Then 05-shared-job-read.feature,04-shared-job-write-ur-then-chess.feature,05-shared-job-read.feature -EvidenceDir JoyRescue/Tests/Pickle/Evidence/F14-<date> -MaxWaitMinutes 720"
```

The shared-job chain is the one command run with `-Command` instead of `-File`: under `-File`,
PowerShell 5.1 binds `-Then a,b,c` as one string, so only `-Command` splits it into three
filters. It hands back only 0 or 1: append `; exit $LASTEXITCODE` inside the quotes to keep the
launcher's own code. Replace `<date>` with the run's date; a name already taken gets a numbered
suffix.

Read `exitReason` before counts, compare discovered and played scenarios, inspect every
`@review` capture. `-EvidenceDir` copies each reached `-Then` report to this mod as
`seq1`, `seq2`, etc. before `UNLOCK`, including a failed launch that stops the chain.
If no fresh report exists, it keeps only the current `Player.log` and `no-report.txt`.
Retain only evidence needed for the verdict and remove superseded
copies after review. Do not rely on the rolling shared archive.

The F14 writer copies the existing Joy Rescue settings file to a named backup. The reader
restores it in `AfterScenario`, including after a failed assertion. If a run stops between
writer and reader, the next writer refuses to overwrite that backup: inspect the WSL Config
file and restore it before retrying. These checks assert loaded definition and job identities;
they do not prove a pawn used either table or earned recreation in a saved colony.

`Tests/MANUAL.md` F01-F12 and F14 remain required acceptance scenarios for actual orphan
buildings, seats, group reservations, third-party own-code detection, reassignment, tolerance
transfer, and new/existing saves. They require named witness buildings/mods and cannot be
truthfully replaced by a fabricated generic fixture. The saved-game handoff for F10-F12 and
F14 is [Fixtures/README.md](Fixtures/README.md). Record a missing witness as BLOCKED.

## Evidence to keep

The disk is short of space and a report about a superseded build proves nothing about the
current one, so a run's evidence is kept small and only while it still proves something (root
`AGENTS.md`, "Test evidence"). It lives on disk under `Tests/Pickle/Evidence/`, ignored by git,
with one line per run in `docs/runs/README.md`. Nothing from it is committed, and no `.dds` is
ever tracked.

**Keep, once a run has been read:**

- `summary.json` and `junit.xml` of the run: the raw result, tiny. Read `exitReason` first.
- The captures that show what a person validates, minified to jpeg: the rendered settings
  dialog in English and in French, the revealed RIMMSQOL button and the dialog it opens, and
  the two job identities of the shared-job reader (`05`).
- `Player.log` only when it holds an error or warning that the verdict depends on, or when a
  run failed before any report existed (then it and `no-report.txt` are the whole evidence).

**Delete:**

- Any report of a superseded build: as soon as a newer run of the same scenario exists.
- The full-size captures once their jpeg exists, every duplicate frame, the NDJSON log and the
  full report (they weigh tens of megabytes).
- The rolling shared archive copies of this mod: select what is needed, then remove that
  archive. Leave the archives of other mods alone.
- Offline suite outputs under `.build/` of an older build: keep only the latest
  `taxonomy-*.txt` set, which matches the delivered DLL.

Never delete a report a `STATUS.md` field still points to: repoint it first. List what goes
and what stays before deleting.

## Waiting on a queued ticket

A queued ticket can wait for hours behind other mods and can vanish. Observe it with a
background watcher (Claude's Monitor tool, the counterpart of a Codex heartbeat) and report
only on a meaningful change: the lock taken, the run finished, a failure, or a needed human
action. Watching does not reserve the machine and does not replace the run itself.

Use one watcher for all of the mod's tickets, not one per ticket. It finds them by the mod name
in the ticket file rather than by a label, so a ticket queued later is picked up without a new
watcher, and it follows every `*queue.stdout.log` kept next to the evidence. A watcher lasts at
most 30 minutes: re-arm it when it expires, and read the log it names on a ticket that vanishes.
A launcher keeps running when its background task is reported stopped after a session restart,
so check the process before treating its ticket as lost. A launcher stops touching its ticket
once it holds the lock, so a stale ticket is a warning only while the run is still queued.
