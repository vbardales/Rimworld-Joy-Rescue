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
unit_tested_on: 2026-10-11
workshop:     3806137974 (Joy Rescue 0.1.0, prepublished by the maintainer; public/subscription check unverified)
remaining:
  - resolved 2026-10-12: French and English review by Virginie, recorded in Translation audit below (revision bd5c2a2, CHANGELOG at dc67e6b).
  - defect (2026-10-14, F07 French capture, `c139`): the mode button of a building with an interaction cell clips its label without an
      ellipsis in French ("Rejoindre la case d'inter", key `JoyRescue.Mode.InteractionCell`, button about 150 px). English fits. Fix: truncate
      the label with an ellipsis like the type button, or shorten the French text (a text change goes back to Virginie).
  - defect (2026-10-14, F10 French captures, `6e65`): group header lines run under the "Éteindre" button without an ellipsis in French
      ("Consommation de drogues : Aucun bâtiment, 1 activité, cela vient (", "Gourmandise : ... d'objets ou de", "... en attente de réparat").
      The same headers fit in English. Fix: truncate the header to the width left of the button, or shorten the French text.
  - defect (test, 2026-10-14, F09 French, `50d6`): `AssertEveryTextTranslated` (`Tests/Pickle/Source/RestartSteps.cs:381`) reads the keys from
      `LanguageDatabase.defaultLanguage.keyedReplacements`, which is empty when the game runs in French (the English default is not
      loaded then; the game is now 1.6.4871, the last passing French run of this feature was on `0f7a731`, built for 1.6.4633). English
      passes. Fix: take the keys from the mod's own `Languages/English/Keyed/*.xml` instead, rebuild `JoyRescue.PickleSteps.dll`, replay `21`+`22` in French alone.
  - fixed 2026-10-14 at `e87e873`, awaiting replay: the three defects above (French clipping of the mode button and of the group
      headers: ellipsis in `JoyRescueMod.cs`; test step `AssertEveryTextTranslated` reads the keys from the mod's English files).
      DLLs rebuilt (offline 133/133, XML 21/21). Replay tickets, small and in French unless noted: 13+14 `20261014-164344-541-b00a`,
      18-20 `20261014-164347-831-17e4`, 21+22 `20261014-164350-435-f8e6`, 13+14 English `20261014-164352-892-acbf`; the English
      18-20 retry `20261014-130757-188-0c05` runs on the same tree. Remove the three defect lines once the captures show the ellipsis.
  - unverified: settings replay in game on the new DLL (the plural change makes the settings scenarios new again:
      Filed 2026-10-13 at a0674d8 (12 tickets, by this session): 06 EN 20261013-172627-144-2d3d, 06 FR 20261013-172729-322-7ec2; chains 13+14, 15-17, 18-20, 21+22 and 23, EN 20261013-172735-899-c348, 172740-530-8b98, 172745-035-9894, 172749-157-8617, 172753-193-147f, FR 20261013-172757-860-c139, 172803-750-ef6c, 172808-986-6e65, 172814-908-50d6, 172820-184-9d74.
      `01`, `06`, `13` to `23`, English and French). Tickets 20261011-234938-670-dfd0 (English, `01`) and
      20261011-234939-528-0a77 (French, `01`) are in the queue; the rest follows at `playTests`.
  - unverified: Workshop self-subscription and public visibility of item 3806137974, reported by the maintainer, not
      observed. 0.1.0 has no Git tag and no GitHub release; the CI creates `v1.0.0` after the upload.
  - unverified: engine-specific mod-pack detection, full load/short hashes, save tolerance migration and log rendering
      are not certified by the isolated definition runner.
  - feature: Workshop gallery images (PUBLICATION.md, AUDIT.md 9): taken from the nelimZen colony or a dedicated Pickle
      scenario, never from test passes. Belongs to `shootGallery`.
session:      local_5d402cea-359a-464a-ad85-0c5485fb6609
updated:      2026-10-12 Virginie's review recorded, writeTests -> playTests[1.0.0]
protocols_read_sha: 2f4e0002e2543fcde57999c51ff8b2f0dfc8efdb
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
`scripts/Make-FrenchReview.ps1` replaces it. `Tests/Test-Xml.ps1` and `Tests/Pickle/Check-Steps.ps1` remain, with their
rows in `scripts/LOCAL_SCRIPTS.md`. `build/` (renamed from `.build/`, 2026-10-13) holds ignored outputs only; the superseded `taxonomy` and `scratch` folders
were removed.

Update 2026-10-12: Virginie's review is done (Translation audit below), protocols marked read at `8c994dd`,
`workflow_stage: playTests[1.0.0]`. Next: the settings scenarios replay in small tickets, English and French; the two
queued tickets (feature `01`) come first.

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
