# Protocol documents read

The rules this mod is worked under live outside this repository. A rule that has moved since it
was read is only visible if the version read is written down, so this file records it. Read the
documents again at the start of a session and after every compaction of the context, then compare
with the table below: if a document's version has not changed, skip it; if it has, read what
changed. **Documents marked "not useful" are not to be read again unless their trigger happens.**

A version is the last commit that touched the file, `git log -1 --format='%h %ad' -- <file>`.
The protocol documents have two histories: the protocols repository (`vbardales/Rimworld-protocols`,
git directory `C:\Users\nelim\Documents\rimworld-protocols.git`, work tree the monorepo), which is
the reference, and the monorepo. The blob is the hash of the content actually read
(`git hash-object`), which settles a doubt between the two histories. A working copy is clean
unless `git status --short -- <file>` says otherwise.

## Read on 2026-09-25, full text, line by line

Heads at the time: protocols `8066e0c`, monorepo `9afdc758`, PickleTools `6c1d976`,
Release-Admin `d403592`, Ticket-Dispatcher `79668cc`, this repository `371ff8e`.

### Useful

| File | Version read | Monorepo | Blob | Working copy | What I took from it |
| --- | --- | --- | --- | --- | --- |
| `AGENTS.md` | `3a1d2cb` (24/09 12:08) | `90d51374` | `bb4c08c1e4` | clean | The three ordered gates, the evidence policy (keep one report per scenario and revision, history as one line per run in `docs/runs/`, never delete what STATUS still points to), and the CI publication rules |
| `AUDIT.md` | `49cd841` (25/09 17:09) | `90d51374` | `4db3571e83` | clean | The chain of stages and their criteria, the three gates for `done -> tested`, the Pickle rules, the session title, the `0.1.0` changelog convention, the fail fast policy. Read again on any change to a stage |
| `PUBLISHING.md` | `04aa365` (25/09 15:10) | `90d51374` | `b9d6db1c07` | clean | Description sections, `PUBLICATION.md`, the changelog read by the CI, rollback, the git pitfalls of a shared repository. Only the publication parts apply to this mod now |
| `TRANSLATIONS.md` | `b83933b` (23/09 20:46) | `90d51374` | `8970fe6c4a` | clean | A change to player-facing text, UI code or language resources sends the translation fields back to `unchecked` until revalidated; in-game checks are tracked separately as `unverified` |
| `Rimworld-Ticket-Dispatcher/docs/WELCOME.md` | `79668cc` (25/09 17:16) | own repository | `eb72657fe0` | clean | Filter terms, `-DepMap`, one pass is one request, a request carries no SHA, the dispatcher wakes the session so none keeps a watcher |
| `Rimworld-Ticket-Dispatcher/docs/SUBMIT.md` | `79668cc` (25/09 17:16) | own repository | `a86821acda` | clean | Every option of `Submit-PickleRun.ps1`, the exit codes, the `-Label` with the SHA, examples |
| `PickleTools/README.md` | `2b7b6d0` (25/09 17:22) | own repository | `5b617e4030` | clean | The catalogue of shared steps. Two matter here: `HoverSteps` (hover a tooltip and assert it) and `ScreenshotStudio` (the `nelim-zen-meadow-studio` fixture) |

### Partly useful

| File | Version read | Blob | Working copy | Read it again for |
| --- | --- | --- | --- | --- |
| `PickleTools/Headless/README.md` | `b2712fc` (25/09 15:03) | `9e4bf0ba90` | clean | Filter terms, `-Then`, what happens to a report, the traps (the game logs in UTC, a language is never switched during a run). The lock, reservation and staging sections belong to the dispatcher |
| `PickleTools/Docs/steps.md` (`docs/steps.md` in that repository) | `7268217` (25/09 17:22) | `a79d939093` | clean | Only the tables of `HoverSteps`, `KeyedClick`, `RimmsqolSteps` and `ScreenshotMode`, when writing a step. It is generated: never edit it |

### Not useful for this mod: do not read again unless the trigger happens

| File | Version read | Blob | Trigger |
| --- | --- | --- | --- |
| `STYLE_RIMWORLD.md` | `7311308` (25/09 15:50) | `83c8a1412d` | The Preview or the ModIcon is redone, or a Preview overlay is engraved. The showcase is complete and the icon is the owner's alone to generate |
| `scripts/SEARCHING.md` | `372c447` (23/09 21:01) | `93d971dc6a` | Searching the mod corpus for a defName or a class, for example to find which mod ships its own recreation code |
| `Rimworld-Release-Admin/docs/OPERATIONS.md` | `d403592` (25/09 16:33) | `2bb7a32d8b` | Publishing: dry run, `publish`, rollback, the workflow generator. Not before. Its "Machine coordination" section is the dispatcher's business |

## This repository's own documents

Written or maintained by this session. Read them again only if `git log -1` differs from the
commit below, which would mean someone else changed them.

| File | Version | Notes |
| --- | --- | --- |
| `STATUS.md` | `371ff8e` (25/09 14:01) | The source of truth for the stage; the session reads it first |
| `README.md`, `CHANGELOG.md` | `1e71d2b` (25/09 11:27) | |
| `ATTRIBUTION.md`, `LICENSE`, `Mod/About/About.xml` | `6e6c48c` (20/09 11:14) | `ATTRIBUTION.md` and the About say "generated with AI" without naming a tool, which `PUBLISHING.md` asks for: see the defect in STATUS |
| `docs/runs/README.md` | `371ff8e` (25/09 14:01) | One line per run |
| `Tests/Pickle/README.md` | `2babd5b` (25/09 11:25) | |
| `Tests/Pickle/Fixtures/README.md` | `6fef4fe` (22/09 22:25) | The plan of the saved-game fixtures for F10 to F14, which is the route to automating the manual tests |

## Named in the list, absent from this repository

`PUBLICATION.md` (a defect in STATUS, required before `prepublished`), `BACKLOG.md`, `NOTES.md`
and `BUGS.md` do not exist here, and the monorepo's `BACKLOG.md` is not this mod's. `TESTING.md`
did not exist and was written on the day of this reading. `docs/PROTOCOLS-READ.md` is this file.

## Pointed to by these documents, not on the list, not read

- `MOD_SETTINGS.md`: the settings gate is `complete`. Read it if the settings change again.
- `PickleTools/Authoring/README.md`: **read it before writing the next Pickle scenario**, the
  documents above send every new suite there.
- `PickleTools/HoverSteps/README.md`: before using it.
- `WORKSHOP_COMMENTS.md` and `EXTERNAL_TOOLS.md`: at publication.

## What the reading changed in this mod

- **A request carries no SHA** (`WELCOME.md`, `SUBMIT.md`). The tree is staged when the ticket is
  played, hours after the request. Two requests of 2026-09-25 were played on a tree that had moved
  since they were filed, which the documents ask not to do. From now on: the SHA goes in `-Label`,
  the tree is left alone until `RUN_DONE`, and `docs/runs/README.md` names the revision tested.
- **`TESTING.md` is required** and declares the passes (`AUDIT.md`). Written.
- **Translations.** The View button, the renamed option and its rewritten tooltip changed
  player-facing text. The resource checks pass and the English pass showed them, but the French
  rendering has not been seen in game. Tracked as `unverified` in STATUS.
- **Fail fast** (`AUDIT.md`, `PUBLISHING.md`, 2026-09-25) is written as in force for the `1.0.0`
  of an item created by a `0.1.0` prepublication. STATUS said it was not yet in force, after the
  owner's words. Left open in STATUS as a question for the owner.
- **The game logs in UTC** (`Headless/README.md`): the clock mismatch seen on 2026-09-24 was that.
- **Gallery captures** can also come from a dedicated Pickle scenario in the
  `nelim-zen-meadow-studio` fixture (`AUDIT.md`, `PickleTools/README.md`), not only by hand. The
  captures of the test passes stay validation evidence.
