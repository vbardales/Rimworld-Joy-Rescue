---
localization: complete
translation_en: unchecked
translation_fr: unchecked
mod:          Joy Rescue
packageId:    nelim.joyrescue
repo:         Rimworld-Joy-Rescue
visibility:   public
detached:     yes
workflow_stage: writeTests[1.0.0]
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
  - unverified: review by Virginie of the texts changed on 2026-10-11 (counted phrases, English and French).
      She read the first state on 2026-10-12 and asked for 12 wording fixes (SummaryTip.Zero, KindTipNoThing, KindTipTolerance,
      ModeTip, Mode.InteractionCell, RowTipOrphan, RescueModsWithOwnCodeTip, SortModeTip, two claims of the Steam description,
      PUBLICATION.md opening and private-item wording, report from a committed revision): all applied in `bd5c2a2`, report
      regenerated at `c1a9043`. Her confirmation of these changed texts is still to come.
      `translation_en` and `translation_fr` stay `unchecked` until she reads `FRENCH_REVIEW.md`, regenerated on
      2026-10-11 by `scripts/Make-FrenchReview.ps1` from the tree at the plural commit (TRANSLATIONS.md, last
      paragraph of section 4). Her French review of 2026-09-30 never took place either. No text agrees with a pawn
      (all strings are about buildings, activities and settings), so section 3 of TRANSLATIONS.md, gender
      agreement, applies nowhere and no row is flagged `?`.
  - unverified: settings replay in game on the new DLL (the plural change makes the settings scenarios new again:
      `01`, `06`, `13` to `23`, English and French). Tickets 20261011-234938-670-dfd0 (English, `01`) and
      20261011-234939-528-0a77 (French, `01`) are in the queue; the rest follows at `playTests`.
  - unverified: Workshop self-subscription and public visibility of item 3806137974, reported by the maintainer, not
      observed. 0.1.0 has no Git tag and no GitHub release; the CI creates `v1.0.0` after the upload.
  - unverified: engine-specific mod-pack detection, full load/short hashes, save tolerance migration and log rendering
      are not certified by the isolated definition runner.
  - feature: Workshop gallery images (PUBLICATION.md, AUDIT.md 9): taken from the nelimZen colony or a dedicated Pickle
      scenario, never from test passes. Belongs to `shootGallery`.
  - note: three Keyed entries are used nowhere in `Source/` (`NoJoyBuildings`, `NothingToRescue`,
      `GroupHeaderWithCode`); `Check-Translations.ps1` lists them. Older than the plural change, to remove or wire.
session:      local_131c2e71-2ac1-49e4-b776-db6daa35f631
updated:      2026-10-11 audit against the chain of AUDIT.md (old `l10n`/`showcase` -> `writeTests[1.0.0]`)
protocols_read_sha: ac587c23f2481ac64ab621b92d75aac0742dce9e
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
rows in `scripts/LOCAL_SCRIPTS.md`. `.build/` holds ignored outputs only; the superseded `taxonomy` and `scratch` folders
were removed.

Next transition: Virginie reviews `FRENCH_REVIEW.md` (English and French columns); then `translation_en` and
`translation_fr` become `complete`, the protocols are marked read, `workflow_stage: playTests[1.0.0]`, and the
settings scenarios replay in small tickets in both languages.
