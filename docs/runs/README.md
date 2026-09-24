# Pickle runs

One line per run, newest last. A run's evidence stays on disk and out of git (see
`Tests/Pickle/README.md`, "Evidence to keep"); what is committed is this history, never folders.

| Date | Pass | Result | Evidence kept |
| --- | --- | --- | --- |
| 2026-09-22 23:14 | shared job, chess then ur (`03` then `05`), PID 22208 | infrastructure-error, code 137, 0 scenarios played, reader launch never ran | the queue stdout log, `Tests/Pickle/Evidence/F14-chess-then-ur-queue.stdout.log` |
