# Pickle runs

One line per run, newest last. A run's evidence stays on disk and out of git (see
`Tests/Pickle/README.md`, "Evidence to keep"); what is committed is this history, never folders.

| Date | Pass | Result | Evidence kept |
| --- | --- | --- | --- |
| 2026-09-22 23:14 | shared job, chess then ur (`03` then `05`), PID 22208 | infrastructure-error, code 137, 0 scenarios played, reader launch never ran | the queue stdout log, `Tests/Pickle/Evidence/F14-chess-then-ur-queue.stdout.log` |
| 2026-09-24 about 12:55 | shared job, four launches on one ticket (`03`, `05`, `04`, `05`), PID 52588 | launch 1 (`03`) passed, 1 of 1, game exit 137 after a complete report, kept. Launch 2 (`05`) failed, 0 of 1: `JoyRescueWitness_PlayChess kind is Gaming_Cerebral, expected Artistic`. The chain stopped, `04` and the second `05` did not play. Cause: a test defect, the fixture assigned `Artistic`, which is not a Core recreation kind, and the mod logged that it skipped it. Fixed in `SharedJobSteps.cs` (now `Social`, checked to exist by the writer). | deleted once the passing rerun below replaced it |
| 2026-09-24, Player.log 16:40 (WSL clock, UTC) | F14 chess then ur, two launches (`03` then `05`), request `20260924-163956-958-a63d` through the dispatcher's worker | passed both: `03` 1 of 1 (the writer) and `05` 1 of 1 (the reader after a real restart), `exitReason: passed` each, worker exit 0, one attempt. The reader found chess on `Social` and Ur on `Gaming_Dexterity`, with independent jobs crediting their kinds, and the log holds no skipped reassignment. The launcher also left a third folder, an exact copy of the second, deleted. | `Tests/Pickle/Evidence/F14-20260924-chess-then-ur/seq1` and `seq2`: `summary.json`, `summary.md`, `junit.xml`, `Player.log` |
