---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Joy Rescue
packageId:    nelim.joyrescue
repo:         Rimworld-Joy-Rescue
visibility:   public
detached:     yes
workflow_stage: playTests[1.0.0]
licence:      original
licence_at:   original work, MIT
upstream_mod_remotes: N/A
license_spdx: MIT
license_files: LICENSE, Mod/LICENSE
dependencies: declared
showcase:     complete
settings_audit: complete
automated_tests: passed
xml_tests: passed
tested_on:    2026-09-29
unit_tested_on: 2026-11-10 (133/133 offline, Test-Xml 8/8, tree be8faf5, code last changed at e87e873)
workshop:     3806137974 (Joy Rescue 0.1.0, prepublished by the maintainer; public/subscription check unverified)
remaining:
  - done 2026-11-10 (4.e, 4.g): `Mod/About/OptionalCompat.xml` created (85 packageIds of `Source/Runtime/TaxonomyRules.cs`, all `kind` synergy, workshopId from the registry); `Check-OptionalCompat.ps1`: 85 patched, 85 declared. Replay of 2026-11-10 then moved the stage back to `playTests`.
  - open (replay-audit 2026-11-10, stage playTests): 8.a and 8.l `tested_on` is stale (2026-09-29, no sha; last green run 2026-10-15 at `e87e873`); 8.c features 02-05, 07-12 and 24-34 were last
      played at `0f7a731`, before the plural (`7c6fbfe`) and ellipsis (`e87e873`) code changes: replay tickets to file (8.f, 8.g, 8.j follow); 8.d, 8.h, 8.i the F10 headers capture;
      8.m code review (`code_review_sha` absent). Ledger `../.replays/JoyRescue-20261110.tsv`.
  - ticket `449f` (2026-11-10, `be8faf5`) failed: `-Then '19,20'` ran 19 and 20 in one launch, so 20 found no earlier writer process (same slip as `21bd`); 18 and 19 passed. Capture of 19 not conclusive: tooltip still on screen after the mouse park. Fix `757dddb`: 5 frames after the park in `19-f10-mid.feature`. Folder deleted.
  - ticket `8d0a` (2026-11-10, `757dddb`) GREEN: F10 chain 18, 19, 20 in three launches (`83e0` and `449f` failed because `powershell -File` joins `-Then a,b`: use `-Command` for a multi `-Then`). Folder deleted after reading. Headers capture of 8.d/8.h/8.i still not conclusive: a tooltip over the list rows survives `I move the mouse to` (pointer rests at screen centre, Pickle 6.0.0 moves no real pointer) and hides the right end of the long headers; the headers that fit read whole. To fix: capture with the window scrolled or moved so no row sits under the centre. The 8.c replay of the stale features follows in small tickets.
  - code review 8.m done 2026-11-10 (Opus sub-agent, whole `Source/` since the first commit, `code_review_sha` set; 10 findings, none high, read by the reviewer, not yet reproduced in game). Defects, to fix after the F10 ticket `449f` (a `Source/` commit reopens 8.m and replays):
      - defect (med) `Runtime/JoyRescueGenerator.cs:371`: a rescued activity moved to another type: `giverKindOverrides` skipped at startup (giver not built yet), "restart pending" shown forever.
      - defect (med) `Runtime/JoyRescueGenerator.cs:409`: `ApplyGiverKindOverrides` leaves shared buildings on other givers of the old type: "Joy source joyKind and jobDef.joyKind are not the same" logged.
      - defect (med) `Runtime/JoyRescueGenerator.cs:116`: a reassigned building of a mod with its own joy code ends disabled by default (orphan, `DefaultEnabled`, `JoyRescueSettings.cs:80`).
      - defect (med) `JoyRescueMod.cs:796` (also 782, 907, 921): picking the type a restart already applied removes the override, the building reverts at the next start, no restart warning.
      - defect (med) `JoyRescueSettings.cs:135`: `nextCustomKindId` resets to 1 while live custom types exist (reserved taxonomy type present): duplicate `JoyRescue_Kind_1`.
      - defect (med, perf) `JoyRescueMod.cs:260`, `:1031`: `ListLayout.Build`, regex in `ActivityName`, `UsableKindCount` run every GUI event with no cache and no row culling.
      - defect (low) `JoyRescueMod.cs:991`: `NextMode` offers `InteractionCell` on a building without interaction cell. (low) `JoyRescueMod.cs:208`: Remove hidden for every custom type once the preset exists, type list not scrollable. (low) `Patches/Patch_TaxonomyTolerance.cs:18`: no try/catch around the postfix.
  - resolved 2026-10-12: French and English review by Virginie, recorded in Translation audit below (revision bd5c2a2, CHANGELOG at dc67e6b).
  - unverified (2026-10-15): F10 French group headers ("Consommation de drogues", "Gourmandise"): the old captures showed them cut under the
      "Éteindre" button; the replay at `e87e873` (ellipsis in `JoyRescueMod.cs`) has a tooltip over their end. Close once a capture without a
      tooltip shows them cut with an ellipsis, or whole. The mode button ellipsis and the F09 test step (`AssertEveryTextTranslated`) are confirmed fixed.
  - 2026-11-10 (read of ticket `21bd`, ended 2026-11-03): chain broken. `seq1` has no report (launcher exit 1073807364, `no-report.txt`); `seq1-1` (feature `18`) 1/1 passed;
      `seq2` 1/2: feature `19` passed, feature `20` failed ("the writer did not run in an earlier game process of this chain", chain broken by the seq1 loss). The `19` captures
      (reset confirmation, after the reset) do not show the F10 group headers cut or whole: the confirmation dialog covers them, and the list after the reset is scrolled to another group.
      F10 line above stays open: needs a capture of the headers ("Consommation de drogues", "Gourmandise") with no dialog and no tooltip. Inbox folder deleted after read (not a validated proof).
      Next: refile a small F10 ticket (French) when the owner of the queue is free; no run in progress.
  - open: 8.m code review (`code_review_sha` absent), `tested_1_7` absent (1.7 pass follows a green 1.6 `playTests`).
  - done 2026-10-15: settings replay in game on the new DLL, all green, run lines in `docs/runs/README.md` (the plural change made the settings scenarios new again:
      Filed 2026-10-13 at a0674d8 (12 tickets, by this session): 06 EN 20261013-172627-144-2d3d, 06 FR 20261013-172729-322-7ec2; chains 13+14, 15-17, 18-20, 21+22 and 23, EN 20261013-172735-899-c348, 172740-530-8b98, 172745-035-9894, 172749-157-8617, 172753-193-147f, FR 20261013-172757-860-c139, 172803-750-ef6c, 172808-986-6e65, 172814-908-50d6, 172820-184-9d74.
      `01`, `06`, `13` to `23`, English and French). Tickets 20261011-234938-670-dfd0 (English, `01`) and
      20261011-234939-528-0a77 (French, `01`) are in the queue; the rest follows at `playTests`.
  - unverified: Workshop self-subscription and public visibility of item 3806137974, reported by the maintainer, not
      observed. 0.1.0 has no Git tag and no GitHub release; the CI creates `v1.0.0` after the upload.
  - unverified: engine-specific mod-pack detection, full load/short hashes, save tolerance migration and log rendering
      are not certified by the isolated definition runner.
  - feature: Workshop gallery images (PUBLICATION.md, AUDIT.md 9): taken from the nelimZen colony or a dedicated Pickle
      scenario, never from test passes. Belongs to `shootGallery`.
session:      local_54512579-d805-4ab9-b6a3-59507d509b27
updated:      2026-11-10 code review 8.m done (10 findings, none high), F10 ticket 449f filed
code_review_sha: abfb9bd8a7d721a7bad03efba7e7afb08e2bebba
protocols_read_sha: a24bdc17d165fab9b12a0dffa2df5859c5a28e10
---

# Joy Rescue — status

The journal before 2026-10-11 (audits of 2026-09-13, 09-22, 09-29, 10-02, the Pickle runs, the taxonomy and settings
validations) moved whole to `docs/runs/status-journal-2026-10-11.md`; `docs/runs/README.md` keeps the run lines.

## Audit — 2026-10-11 (tree after commit `7c6fbfe`, plus the CHANGELOG entry and the rebuilt DLL)

Old `workflow_stage: l10n` (old vocabulary, `stage: showcase`, plural defect of 2026-10-02) -> retained
`writeTests[1.0.0]`. The first transition whose exit criteria are not established is `writeTests -> playTests`, on
7.g: the texts changed on 2026-10-11 await Virginie (TRANSLATIONS.md, section 4). Everything before it holds.

| Transition | Result |
| --- | --- |
| `code -> declareDependencies` (3) | Validated. No `feature` left for 1.0.0 (the gallery is `shootGallery`); build 0 warnings 0 errors; `[Unreleased]` of `CHANGELOG.md` (and the copy in `Mod/`) now lists the plural change; DLL in `Mod/Assemblies` rebuilt and committed with this audit |
| `declareDependencies -> auditSettings` (4) | Validated, rechecked 2026-10-11: `brrainz.harmony` is the only hard dependency and the only mod the code references (`Source/` mentions no RIMMSQOL, no other mod); `loadAfter` lists Harmony and the DLCs, order only; no `LoadFolders.xml`, no `Patches/`, so no conditional patch to guard; RIMMSQOL is only thanked and a test companion; `supportedVersions` 1.6 |
| `auditSettings -> localize` (5) | Validated, `settings_audit: complete`: options, hidden shortcut `JoyRescue_Settings` (`buttonVisible=false`, same `Mod` instance), automated effect, persistence and robustness cases pass (133/133). In-game verification recorded in the old journal (F01-F19, 2026-09-28 and 29, tree `0f7a731`); the plural change is a DLL change and replays them (remaining) |
| `localize -> writeTests` (6) | Validated: plural defect fixed (`JoyRescue.Count.*`, `SummaryTip`, `KindHeaderOff`, `KindRemovedOrphans`, each with `.One`/`.Many`/`.Zero` in both languages; `PluralKey`/`Counted` in `JoyRescueMod.cs`); inventory by reading the code; `Check-Translations.ps1`: only the 3 unused keys above; `FRENCH_REVIEW.md` regenerated. `translation_fr` and `translation_en` unchecked, see remaining |
| `writeTests -> playTests` (7) | **Not established, 7.g.** 7.a to 7.f hold: scenarios in `Tests/SCENARIOS.md`, `Tests/MANUAL.md`, `Tests/TAXONOMY.md`; offline suite 133/133 and XML suite 21/21 replayed today; Pickle suites written (34 features, no `@wip`, each `@requires` played). Missing: Virginie's review of the changed texts |

Replayed today: `dotnet build Tests/JoyRescue.Tests.csproj -c Release`, `JoyRescue.Tests.exe` (133 of 133),
`Tests/Test-Xml.ps1` (21 of 21), `scripts/Check-Translations.ps1`, `scripts/Make-FrenchReview.ps1`. No game was launched
by this session; two Pickle tickets are queued through the dispatcher.

Local scripts (AGENTS.md, "Local scripts"): `_tools/Generate-FrenchReview.ps1` deleted, the shared
`scripts/Make-FrenchReview.ps1` replaces it. `Tests/Test-Xml.ps1` remains (cut 2026-11-02; `Tests/Pickle/Check-Steps.ps1` removed for the shared `scripts/Check-Steps.ps1`), with its
rows in `scripts/LOCAL_SCRIPTS.md`. `build/` (renamed from `.build/`, 2026-10-13) holds ignored outputs only; the superseded `taxonomy` and `scratch` folders
were removed.

Update 2026-10-12: Virginie's review is done (Translation audit below), protocols marked read at `8c994dd`,
`workflow_stage: playTests[1.0.0]`. Next: the settings scenarios replay in small tickets, English and French; the two
queued tickets (feature `01`) come first.

## Dependency record (4.g), 2026-11-10, tree after commit `3e764ab`

- Hard dependency: `brrainz.harmony` (the only mod the code references; `Source/` uses HarmonyLib in `JoyRescueMod.cs` and `Patches/`).
- Order only (`loadAfter`): Harmony and the DLCs (Royalty, Ideology, Biotech, Anomaly, Odyssey).
- DLC: none required; no `LoadFolders.xml`, no `Patches/`, so no conditional patch to guard.
- Optional integrations: the 85 mods of `Mod/About/OptionalCompat.xml` (kind synergy): their activities and buildings are moved to better recreation types by the optional preset, from `Source/Runtime/TaxonomyRules.cs`. Never required, no load order imposed.
- Animal mod integrations (4.f): not applicable, the mod adds no animal.

## Translation audit

Where the text lives: `Mod/Languages/{English,French}/Keyed/JoyRescue.xml`, `JoyRescue_Editor.xml`, `JoyRescue_Taxonomy.xml` and
`Mod/Languages/French/DefInjected/MainButtonDef/JoyRescue.xml`; `FRENCH_REVIEW.md` lists all of it. No text agrees with a
pawn, so no gender switch and no flagged row.

- Review by Virginie, 2026-10-12, revision `bd5c2a2` (texts) and `dc67e6b` (CHANGELOG): she read English and French and
  asked for wording fixes (zero-count tooltip, gender-neutral rewordings, interaction cell, two claims of the Steam
  description, PUBLICATION.md opening); all applied in `bd5c2a2`, then confirmed (« ok »). `translation_en` and
  `translation_fr` are `complete`.
- In-game checks (raw keys, fallback text, clipping, English and French) belong to `playTests`: first tickets queued,
  see `remaining`.
- 2026-10-13: the three unused Keyed entries (`NoJoyBuildings`, `NothingToRescue`, `GroupHeaderWithCode`) were removed from both
  languages at Virginie's request; no displayed text changes, `Check-Translations.ps1` passes, `FRENCH_REVIEW.md`
  regenerated. The replay tickets filed at `a0674d8` run on the tree of this commit.
