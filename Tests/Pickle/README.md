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
| Shared job A then B | `-DepMap wsl-deps.shared-job.map -Filter 03-shared-job-write-chess-then-ur.feature -Then 05-shared-job-read.feature` | `03`, `05` | The two loaded witnesses share a job before writing; after a real restart, both assigned kinds have independent jobs. |
| Shared job B then A | `-DepMap wsl-deps.shared-job.map -Filter 04-shared-job-write-ur-then-chess.feature -Then 05-shared-job-read.feature` | `04`, `05` | Same check in reverse insertion order. |

Queue every pass through the dispatcher, one ticket per pass. Never launch RimWorld directly, and
do not keep a `Run-PickleWsl.ps1` process of your own alive:

```powershell
powershell.exe -ExecutionPolicy Bypass -File Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod JoyRescue -Owner local_<id> -Label "minimal English" -Language English -EvidenceDir JoyRescue/Tests/Pickle/Evidence/<pass>
powershell.exe -ExecutionPolicy Bypass -File Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod JoyRescue -Owner local_<id> -Label "minimal French" -Language French -EvidenceDir JoyRescue/Tests/Pickle/Evidence/<pass>
powershell.exe -ExecutionPolicy Bypass -File Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod JoyRescue -Owner local_<id> -Label "RIMMSQOL shortcut" -DepMap wsl-deps.avec-rimmsqol.map -Language English -Filter 02-rimmsqol-shortcut.feature -EvidenceDir JoyRescue/Tests/Pickle/Evidence/<pass>
powershell.exe -ExecutionPolicy Bypass -File Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod JoyRescue -Owner local_<id> -Label "F14 chess then ur" -DepMap wsl-deps.shared-job.map -Language English -Filter 03-shared-job-write-chess-then-ur.feature -Then 05-shared-job-read.feature -EvidenceDir JoyRescue/Tests/Pickle/Evidence/<pass>
powershell.exe -ExecutionPolicy Bypass -File Rimworld-Ticket-Dispatcher/scripts/Submit-PickleRun.ps1 -Mod JoyRescue -Owner local_<id> -Label "F14 ur then chess" -DepMap wsl-deps.shared-job.map -Language English -Filter 04-shared-job-write-ur-then-chess.feature -Then 05-shared-job-read.feature -EvidenceDir JoyRescue/Tests/Pickle/Evidence/<pass>
```

Several small tickets rather than one large one. A ticket for an exploration or a fix runs as few
scenarios as possible: one feature file, or `-Filter '::<scenario name>'`. An initial or a final
ticket runs every scenario, with no `-Filter`, still one pass per ticket. A shared-job pair is the
smallest unit that means something: the writer and the reader must share one hold of the lock,
which is what `-Then` is for. Replace `<pass>` with a folder name that is new for the run.

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

Do not watch the queue from this session: no Monitor, heartbeat, cron or loop. The
TicketDispatcher, one session that reads the queue for every mod, wakes this session by
message: `START` when the ticket takes the lock, `END` when it gives it back (the lock, not the
verdict), `LOST` when a ticket vanished without ever holding it, and `RUN_DONE` for a request
dropped through `Submit-PickleRun.ps1`. It knows this session by `local_<id>`, taken from the
ticket label or from a first message whose first line is `REGISTER local_<id> JoyRescue`, so a
run is queued with `Submit-PickleRun.ps1 -Owner local_<id>`, which puts the id in the label.

A queued ticket can wait for hours behind other mods. Read the verdict in the run's own report
and log, never in a launcher's exit code: under `-Command` it is not handed on. A launcher keeps
running when its background task is reported stopped after a session restart, so check the
process before treating its ticket as lost.
