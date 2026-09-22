# Runtime fixture handoff

These are **saved-game fixtures to generate in RimWorld**, not XML data to fabricate. The
generator/postfix, job reservations, tolerance arrays and removal behavior are engine state;
a hand-written `.rws` would not be evidence of them. Generate them only through the shared WSL
Pickle harness after it has a ticket and the machine is free. Do not use the Windows game.

Saved-game fixtures start from Pickle's `test-colony` and are copied here only after their
generating run has a terminal report. Record the source revision, staged mod set, RimWorld
version, command, report path and SHA-256 beside every saved file. Joy Rescue settings are
**global**: they live in the game's Config file, not in `.rws`. A fixture that depends on
settings must pair the save with the exact settings file, or use a `-Filter ... -Then ...`
restart chain that preserves that file under one lock.

## F10 — `Mod_JoyRescue_JoyRescueMod.xml` settings seed

Create a non-reserved custom type, one building override, one activity override, one disabled
type, one individually-disabled repair, `requireChairForWatching=false`, and a non-default sort
order. Save through the mod button and capture the populated settings page. Preserve the
resulting **Config XML**, not a colony save. The consumer cancels once (all values remain),
then confirms, saves, restarts and checks defaults plus empty overrides. Keep the writer and
reader in one `-Then` chain so staging cannot replace the XML in between.

## F11 — `joyrescue-removal-job.rws`

Stage a named orphan-building witness, build it, lower a capable pawn's recreation need and wait
until the pawn has a `JoyRescue_...` job actually assigned. Record pawn, building defName, job
defName and relevant logs in `joyrescue-removal-job.md`, then save immediately. The consumer uses
a *copy* with Joy Rescue disabled and records load/log/save usability and the documented loss of
an active generated job. It never overwrites this original or a player save.

## F12 — no save required

F12 is metadata presentation. Use a clean startup and capture the Mod list / metadata page that
shows the title, 1.6 support, Harmony dependency and GitHub link. Store the reviewed capture as
evidence; do not invent an `.rws` for this scenario.

## F14 — paired colony and settings for each assignment order

The staged `wsl-deps.shared-job.map` witness mod defines
`JoyRescueWitness_PlayChess`/`ChessTable` and `JoyRescueWitness_PlayUr`/`GameOfUrBoard`, both
using vanilla `Play_Chess`. First inspect the **loaded** definitions to confirm those cross
references and record a capable pawn plus both built, reachable tables. Generate two copies with
opposite insertion order:

- `joyrescue-shared-activities-a-then-b.rws` plus its Joy Rescue Config XML
- `joyrescue-shared-activities-b-then-a.rws` plus its Joy Rescue Config XML

For each, save selected kinds, restart under the same staged set **without restaging**, and
observe both activities. A save without its matching settings XML cannot establish F14.
The `03`/`04` writer features and `05` reader feature now automate the loaded-definition
half of that contract under `-Then`. They do not create the `.rws` files or observe a pawn;
that acceptance evidence still requires the two reachable tables to be placed in a colony.
Each must credit its chosen kind; neither may change the other or emit a repeated mismatch error.
Separate runs are required because dictionary insertion order is part of D39/F14.

## Ticket and evidence rule

Before generation, run `scripts/Pickle-Status.ps1`. A busy lock or another session's game means
wait: never stage, reserve, kill or relaunch. Only a future authorized launcher invocation may
take a ticket, stage witnesses and generate these files. Afterwards read `exitReason`, compare
discovered/played scenarios, inspect `@review` captures, and copy report/log/capture evidence
into this repository before the shared report directory is overwritten.
