---
localization: complete
translation_en: complete
translation_fr: complete
mod:          Joy Rescue
packageId:    nelim.joyrescue
repo:         Rimworld-Joy-Rescue
visibility:   public
detached:     yes
stage:        done
licence:      original
licence_at:   original work, MIT
license_spdx: MIT
license_files: LICENSE, Mod/LICENSE
dependencies: declared
showcase:     complete
settings_audit: complete
audit_revision: 868ceb3e534dd1d36da9ce9ce532f943fce1bc9e
audit_worktree: clean
automated_tests: passed
xml_tests: passed
tested_on:    2026-08-29
unit_tested_on: 2026-09-13
workshop:     3806137974 (Joy Rescue 0.1.0, published by maintainer; public/subscription check unverified)
remaining:
  - blocking (done -> tested), gates given by the owner on 2026-09-24: (1) no scenario left in
      @wip: MET, none of the 11 Pickle scenarios in the 5 features carries the tag;
      (2) every conditional scenario has run: MET. All 6 that carry a @requires tag have played and
      passed: 03 and 05 (2026-09-24, 05 after a test fix), the three RIMMSQOL scenarios of 02, and 04
      with 05 (2026-09-25). The first 4 unconditional ones of 01 passed in three passes; the fifth, added on 2026-09-25 for the
      list arrangement, has not played. All of it ran on
      the previous DLL except the reverse F14 order, which ran on the new one; the English pass on
      the new DLL is queued (dd64);
      (3) no manual test left to validate, all green: NOT MET, F01-F14 in Tests/MANUAL.md and
      F15-F19 in Tests/TAXONOMY.md are all unexecuted. The stage stays done.
  - defect: the distributed About description lacks the mandatory IF I GO QUIET,
      AI-GENERATED (with actual tool names), THANKS, and attribution/license sections.
      Because the Workshop item already exists, correct the Steam description directly
      before its next update; changing About.xml alone cannot synchronize it.
  - defect: PUBLICATION.md is absent, so the capture order, adult-content decision,
      dependency/DLC declaration, release notes, and individualized thank-you messages
      were not recorded in the repository before publication.
  - feature: gallery images are taken by hand in the nelimZen colony and never come from a Pickle run:
      the test captures are validation evidence only, taken in a sandbox colony. Listed for the
      publication handoff, next to the capture order that PUBLICATION.md should record.
  - note: the owner's fail fast policy of 2026-09-25 (PUBLISHING.md) is not in force here yet: it applies once
      the owner judges the work serene enough. It publishes after the dry run and the approval, runs the
      remaining tests, and on a red publishes a rollback, a new version whose ref is the last good SHA. The
      target has to be chosen before publishing. Candidate for the next version: `b1eb0d1`, whose Mod/ is the
      published 0.1.0 (same DLL blob as `6695ae3`, only CHANGELOG.md differs) and which carries the in-game
      passes on that DLL. To be chosen again at the time.
  - unverified: 0.1.0 has no Git tag and no GitHub release. main was pushed on 2026-09-24
      (de013c4 is on origin/main, with the `## [0.1.0]` section of CHANGELOG.md that the CI
      reads for the release notes). The CI creates the tag and the release after a successful
      upload; until then the Workshop item cannot be matched to a release.
  - unverified: self-subscription to Workshop item 3806137974 and its public visibility
      were reported by the maintainer but were not independently observed in this audit.
  - unverified: common preset F15-F19 gameplay, translated layout, actual Harmony save-load hook and full mod-pack compatibility remain pending; see Tests/TAXONOMY.md.
  - unverified: execute MANUAL.md F01-F14 in RimWorld, including actual pawn behavior,
      logs, FR/EN UI, new game and existing save, settings persistence and shared activities.
  - unverified: the MainButtons shortcut and the RIMMSQOL integration passed in game on 2026-09-25
      through the Pickle sandbox (absent by default, listed and revealed by RIMMSQOL, the revealed
      button opens the same window, hiding restores the hidden state). Still open: the visibility
      across a restart, which is RIMMSQOL's own contract, a real player's workflow, and the RIMMSQOL
      version number, since only its Workshop item 1084452457 is known.
  - unverified: engine-specific mod-pack detection, full load/short hashes, save tolerance
      migration and log rendering are not certified by the isolated definition runner.
session:      local_e3318eb0-b04f-4f8b-b01a-0d4188a56ee4
updated:      2026-09-25, final pass read in part, tooltip name removed (new DLL); stage done retained
---

# Joy Rescue — status

Kept at the root, never inside `Mod/`, so Steam never receives it. Maintained by the session
that holds this mod, not by the sweep that first wrote it.

## Ordered workflow audit — 2026-09-22

**Audited revision:** `868ceb3` (clean worktree before this status record). The repository
root is autonomous and its actual distributed directory is `Mod/`.

| Transition | Result |
| --- | --- |
| `dansMonoRepo → horsMonoRepo` | **Validated.** Git root, public GitHub origin, English root/distribution documentation, coherent Joy Rescue / `nelim.joyrescue` identity, MIT notices, and original-work attribution are present. |
| `horsMonoRepo → ModIcon → Preview → preOptions` | **Validated.** The distributed DLL builds; `ModIcon.png` is 128×128; `Preview.png` is a directly reviewed, readable 896×504 PNG of 585,587 bytes. The English description and final repository link are present. |
| `preOptions → options` | **Validated offline.** Useful settings are implemented through the Mod options page. `JoyRescue_Settings` uses the same settings instance and has `buttonVisible=false`; the executable suite covers defaults, validation, effects, persistence, and the native shortcut contract. In-game interaction remains a later criterion. |
| `options → l10n → preTest` | **Validated offline.** EN/FR Keyed resources and native French MainButton injection pass the resource/call-argument checks; Harmony is the only hard dependency, while DLC entries are load ordering. No LoadFolders or conditional patches exist. |
| `preTest → done` | **Validated.** Written scenarios exist, the delivered DLL and test copy share SHA-256 `E7DD9FFE1A0FD1612A1F3586D617F530EAFF4EC25804FED64B49DAC87393156F`, and the current rerun is 118/118 executable tests plus 20/20 XML tests. |
| `done → tested` | **Not verified.** No RimWorld instance was launched by this audit. Functional scenarios, logs, EN/FR rendered UI, persistence in a game, hidden-shortcut reveal through RIMMSQOL, new/existing saves, and Workshop self-subscription remain unobserved. |
| `tested → prepublished → published` | **Not established.** The maintainer-reported Workshop ID is recorded, but the unverified gameplay gate already blocks advancement; the outstanding remote tag/release/push and publication handoff are listed above. |

This audit deliberately made no code, image, Steam, or game-session change. It retains
independent static validations while assigning global `stage: done`, the last transition
whose mandatory criteria are currently evidenced.

## Pickle suite written — 2026-09-22

### F14 witness preparation and queue check

The test-only `Tests/Pickle/WitnessMod/Mod` now declares two named givers serving vanilla
Chess and Game of Ur tables through the same `Play_Chess` job. Features `03`/`04` write
opposite assignment orders, and `05` checks distinct job identities and credited kinds in
a second game process. The writer preserves the prior WSL settings file; the reader restores
it after its scenario. These files passed XML parsing, all ten local Cucumber expressions
compiled, and the companion assembly built with zero warnings/errors (SHA-256
`91A88B1B90B1BB18764B6BE43FCF7687D2475D6C9959596FD79FAE0329659D58`). No gameplay
run or saved-colony fixture has been produced from them yet.

F10's fixture is a global Mod settings XML, not an `.rws`; F12 needs a metadata capture, not a
save. F11 still requires a live generated job in a copied save. The fixture handoff was
corrected accordingly. The RIMMSQOL feature tests Joy Rescue's button integration only;
the visibility choice across restart is RIMMSQOL's own contract.

At the read-only Pickle status check on 2026-09-22 22:16 local time, WorkStudio held the WSL
game and lock, with ten other tickets queued. No Joy Rescue ticket existed at that time.

The later F14 chess-then-ur `-Then` run (PID 22208) did take `LOCK` and `STAGE`, then
returned `infrastructure-error` / code 137 at 23:14 with **0 scenarios played**; `UNLOCK`
followed. The reader launch did not run, no gameplay or F14 fixture was validated, and
the shared report archive was pruned before its failed report could be recovered into this
repository. The queue stdout log remains in `Tests/Pickle/Evidence/`. The shared launcher
has since been corrected to copy an intermediate failed report (or a fresh `Player.log`
if no report exists) to `-EvidenceDir` before stopping the sequence and releasing the lock.
That correction has passed PowerShell syntax checking but has not yet been exercised by a
new in-game run.

### First in-game F14 run, 2026-09-24

Ticket 52588, four launches under one hold of the lock (`03`, `05`, `04`, `05`), English, after
about three and a half hours in a queue of nineteen. Launch 1 (`03`, the writer) **passed**, 1 of 1, and the
game left with code 137 once its report was complete, which the launcher keeps. Launch 2 (`05`,
the reader after a real restart) **failed**, 0 of 1, and the launcher stopped the chain, so `04`
and the second `05` did not play.

The failure is a defect of the test, not of the mod. The fixture assigned the recreation kind
`Artistic` to the chess witness, and Core declares no such kind (its ten are Meditative, Social,
Gaming_Dexterity, Gaming_Cerebral, Television, Telescope, HighCulture, Chemical, Gluttonous and
Reading). After the restart the mod logged that it skipped the reassignment because the type does
not exist, so the giver kept its own Gaming_Cerebral. The mod did what it should on a type it
cannot find. What F14 asks, that two givers sharing a job keep their own kinds, was **not**
tested by this run.

`SharedJobSteps.cs` now assigns `Social` to chess and keeps `Gaming_Dexterity` for Ur, and the
writer refuses to save an assignment whose kind does not exist in the running game, so the same
mistake fails at the writer, where it is cheap to read, rather than after a restart. The
companion assembly was rebuilt with zero warnings and errors. The evidence of this first run was
deleted once the passing rerun below replaced it; `docs/runs/README.md` keeps its line.

The relaunch queued at 13:12 (ticket 47600, launched directly) never ran: its launcher died with a
session restart and the TicketDispatcher found the ticket gone at 16:35. It was redeposited at
16:39 through the dispatcher's worker as request `20260924-163956-958-a63d`, one pair only, `03`
then `05` (chess then ur), which is what checks the fix. A second request for the reverse order
(`04` then `05`) was withdrawn before it started: the owner's rule is several small tickets, and a
ticket for a fix runs as few scenarios as possible, while the reverse order belongs to the final
pass that runs everything.

The resubmitted pair ran and **passed**, both launches, `exitReason` passed each time
(`docs/runs/README.md` has the line). The writer (`03`) saved chess on `Social` and Ur on
`Gaming_Dexterity`; after a real restart the reader (`05`) found exactly those kinds, with two
independent jobs each crediting its own kind, and no skipped reassignment in the log. So for the
first insertion order the mod does what F14 asks: two givers that shared a job keep their own
kinds. The reverse order (`04` then `05`) played later, on 2026-09-25 (see the final pass below), and
passed too, so F14 is verified in both insertion orders. The launcher also wrote a third evidence folder that copies the second (its last
step is saved twice when the sequence ends); it was deleted, and the quirk belongs to
`scripts/Run-PickleWsl.ps1`, not to this mod. The same runs also exercised the launcher's evidence
copy that was corrected after the failure of 22 September: it saved each launch's report as
`seq1` and `seq2` before releasing the lock, which the earlier paragraph still lists as untried.

### Final pass submitted, 2026-09-24

Four small requests through the dispatcher's worker, one pass each, awaiting `RUN_DONE`:

| Request | Pass | Plays |
| --- | --- | --- |
| `20260924-205336-502-2da8` | minimal English, no filter | `01` (4 scenarios); `02` to `05` skip, their mods are not staged. **Done 2026-09-25: passed**, 4 played and passed, 6 skipped as expected, `exitReason` passed; both captures opened (see below) |
| `20260924-205337-355-3d24` | minimal French, no filter | `01` (4 scenarios). **Done 2026-09-25: passed**, 4 played and passed, 6 skipped as expected, `exitReason` passed; both captures opened: fully translated, no raw key, nothing clipped |
| `20260924-205338-490-f62b` | with RIMMSQOL, no filter | `01` and `02` (7 scenarios). **Done 2026-09-25: passed**, 7 played and passed, 3 skipped as expected, `exitReason` passed; four captures opened, the shortcut is absent by default, listed and revealed by RIMMSQOL, and the revealed button opens the same window |
| `20260924-205339-283-e105` | F14 ur then chess, `04` then `05` | the reverse insertion order, two launches under one lock. **Done 2026-09-25: passed**, both launches, `exitReason` passed each, on the new DLL |

The chess-then-ur pair is not resubmitted: it passed on 2026-09-24 against this same delivered
DLL (`E7DD9FFE...`) and this same companion, and nothing has changed since. A change to either
means it runs again. The shared-job features cannot run unfiltered, since a writer and its reader
must be two game processes.

### Tooltip change, 2026-09-25: the final pass is now on the previous build

The owner chose to remove the internal definition name (`Play_GameOfUr`) from the activity tooltip
of the settings window, which the RIMMSQOL pass had shown. The report to log does not carry it
either, since it lists orphaned buildings only, so the name is no longer shown anywhere in the
interface. Three files changed: the tooltip call in `JoyRescueMod.cs`, and the `GiverTip` key of
the English and French `Keyed` files, now the label alone. It is listed under `[Unreleased]` in
`CHANGELOG.md`, for the next version.

**This changes the delivered DLL** (SHA-256 `9737F8F9B09E128546836CD7A0C5A164CD8CE04748E28E8A869A3A8469A60961`,
was `E7DD9FFE...`), so the final-pass results above and the chess-then-ur pair were obtained on the
previous build. The offline suites pass on the new one: 118 of 118 and 20 of 20, with the delivered
DLL and the test copy sharing the hash. The reverse-order request `20260924-205339-283-e105` ran on it
from 11:06 on 2026-09-25 and passed both launches. The owner asked for the English pass on the new
DLL as the check of the fix: request `20260925-110815-202-dd64`, `01` only, evidence in
`Tests/Pickle/Evidence/tooltip-english`, submitted at 11:08 and awaiting `RUN_DONE`. The other
passes of the final pass stay on the previous build until the owner says otherwise.

Two defects of the test project surfaced while checking, both mine or older:

- `Tests/JoyRescue.Tests.csproj` compiled the sources of `Tests/Pickle`, which need Pickle's
  assemblies, so the executable runner had not built since the Pickle companion was added
  (52 errors). It now excludes that folder.
- Two Scribe cases (D31, D32) wrote into `.build/settings-2026-09-13/`, a folder I deleted on
  2026-09-24 as superseded evidence, and did not create it: both failed. They now write to
  `.build/scratch/`, created by the test. Deleting a folder a test writes into is a check to make
  before removing anything under `.build/`.

### List arrangement, 2026-09-25

At the owner's request the settings list gets a **View** button, beside Save now. It arranges the list
under each type either as each activity followed by the buildings it serves (the default), or as each
building followed by the activities that serve it. Either way the orphaned buildings, and the
activities that serve no building, come last, where the list used to put orphans first. The choice is a
saved setting (listView, 0 or 1, an unknown value reads as 0) and Reset puts it back.

The order is worked out in Source/ListLayout.cs, apart from the drawing, so that it runs without the game.
A building an activity serves is listed once in the activities-first view, under the first activity, and an
activity serving several buildings repeats under each in the buildings-first view; a building whose type
differs from its activity's is still shown, and nothing is dropped. Nine cases cover it (L01 to L09),
and the executable suite is now 127 of 127. Breaking the layout on purpose in four ways, the cases caught three at once;
the fourth, a building served by two activities, only after a case for it was added.

The delivered DLL changes again, to SHA-256 1698F5EC96A5803294F93DA800975E6BB0818A61DB48A18BA7634CDC5424971B.
Feature  1 gets a fifth scenario that sets the arrangement to buildings, opens the real dialog, takes a
capture and checks the log. The English request 20260925-110815-202-dd64, on the previous layout, was
withdrawn before it ran and replaced by 20260925-112444-031-0c17, which plays all five. Nothing has run in the
game on this DLL yet, and the final pass is on older builds.

### Fix set tooltip, 2026-09-25

The tooltip of the common recreation fix set checkbox was also rewritten on 2026-09-25, at the owner's
request, in both Keyed/JoyRescue_Taxonomy.xml files: it now begins with what the option does, moving
activities of other mods to a better recreation category, and names the three types it adds, where it
used to describe the mechanism. The checkbox label and the list layout are unchanged. These are resource
files only, so the DLL is unchanged, and both offline suites still pass. No capture shows this tooltip
yet: the pointer was never on it.

The owner then said the checkbox label itself, "Use the common recreation fix set", meant nothing to them, so it
was renamed after what the option does: "Move other mods' activities to better recreation types (restart
required)". The summary line ("Activities moved this session: N. Skipped: M") and the reset question, which
repeated the old name, follow, and the tooltip now says "type" where it said "category", the word the rest of
the window uses. English and French, resource files only: the DLL is unchanged, and both suites pass. The name
"common recreation fix set" stays in the documents as the name of the feature.

`Tests/Pickle/` now supplies the development-only companion
`nelim.joyrescue.pickletests`. Its minimal English/French feature covers the rendered Mod
options dialog, default hidden/non-greyed shortcut, shortcut activation into the same dialog,
and a sandboxed `requireChairForWatching` write/reopen cycle. The dedicated RIMMSQOL pass
uses the shared `nelim.pickletools.rimmsqol` companion to list, reveal, hide and forget
`JoyRescue_Settings`, then opens the real Joy Rescue dialog through the revealed bar button.
Every `@review` capture still needs human inspection when played.

`dotnet build Tests/Pickle/Source/JoyRescue.PickleSteps.csproj -c Release` succeeds with
zero warnings/errors; the staged `JoyRescue.PickleSteps.dll` SHA-256 is
`331120D4BA13E7E7A1ABA895E0EF29575584E07FB826B5697AB0807D03EEE7F2`.
All seven local Cucumber expressions compile against the installed Pickle engine. The shared
RIMMSQOL checker resolves this suite's RIMMSQOL lines; its overall failure is two unrelated,
pre-existing ScreenshotStudio lines with no staged `ScreenshotStudio` expression, not a Joy
Rescue feature failure. That was the pre-run static assessment, not an in-game result.

F01-F12 and F14 remain the required behavior acceptance matrix in `Tests/MANUAL.md`: they need
real, named orphan/covered/own-code witnesses and actual saves. They are deliberately not
replaced by a fabricated fixture, and remain **unverified** until a pass can stage those witnesses.

## Publication audit — 2026-09-22

The maintainer reported that **Joy Rescue 0.1.0** was published and supplied Workshop
ID `3806137974`. `Mod/About/PublishedFileId.txt` contains exactly that ID and is committed
in `6695ae3` (`Publish JoyRescue 0.1.0`). The repository’s current stage remains
`done`, rather than `published`: the ordered workflow requires the published commit to
be pushed, a matching Git tag and GitHub release, a recorded pre-publication handoff,
and an observed self-subscription/public-visibility check. None is evidenced in the
current repository. This preserves the reported Workshop fact without certifying the
unobserved transition.

### Rechecked offline release evidence

| Check | Actual result |
| --- | --- |
| `dotnet build Tests/JoyRescue.Tests.csproj -c Release --no-restore` | Exit 0; zero warnings/errors |
| `.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe` | Exit 0; **118/118 PASS** |
| `pwsh -NoProfile -File Tests/Test-Xml.ps1` | Exit 0; **20/20 PASS** |
| Distributed/test DLL SHA-256 | Both `E7DD9FFE1A0FD1612A1F3586D617F530EAFF4EC25804FED64B49DAC87393156F` |
| `git diff --check` | PASS |

GitHub was queried live: `vbardales/Rimworld-Joy-Rescue` is public on `main`, but
`origin/main` remains `6e6c48c`; there are no remote tags or releases. The current local
branch is ahead by the PublishId commit. Steam’s item page could not be retrieved through
the available public client, so its public visibility and subscription behavior remain
unverified rather than inferred from the identifier.

## Current settings validation — 2026-09-13

**Stage: `preOptions` → `done`.** The literal user workflow is applied: the settings
source/definition audit and applicable off-game behavior tests now pass (`options`),
localization checks remain complete (`l10n`), dependency inspection passes (`preTest`),
and written functional scenarios plus successful executable/XML suites establish `done`.
This means ready for final in-game acceptance, **not** `tested`. `tested_on` remains the
historical August date; this work does not reuse it as current-version gameplay evidence.

Revision: `2e44782bacb1903e1931f15b62fb2ac6db1fb785` plus the current uncommitted worktree.
Changes from earlier turns were preserved; no commit, push, publication or image change.
Distributed root remains `Mod/`, linked to RimWorld/Mods as established by the audit.
Current DLL and the copy used by the test runner have the same SHA-256:
`1D3013B90D01A51B1D656284F5CBA071EF6E825D9CB9DF273A255AAFF0C65E7B`.

### Executed results and evidence

Evidence: the directories of this pass (`.build/settings-2026-09-13/`, `.build/fix-2026-09-13/`,
`.build/audit-2026-09-13/`) were deleted on 2026-09-24. They described superseded builds (49, 52
and 95 cases, other DLL hashes) and held decompiled game sources. The current build is proved by
`.build/taxonomy-build.txt`, `.build/taxonomy-results.txt` (118/118) and `.build/taxonomy-xml.txt`
(20/20), local and ignored.

| Check | Actual result |
| --- | --- |
| `dotnet build Tests/JoyRescue.Tests.csproj -c Release --no-restore` | Exit 0, zero warnings/errors; build.txt |
| `./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe` | Exit 0, **95/95 PASS**; results.txt |
| `pwsh -NoProfile -File Tests/Test-Xml.ps1` | Exit 0, **18/18 PASS**; xml.txt |
| Native DefInjected paths | Earlier **2 keys, 0 errors** validation retained: neither MainButtonDef nor its translations changed in this continuation |
| `git diff --check` | PASS |
| In-game / RIMMSQOL | **Not executed**; no integration version certified |

The 95 cases comprise the prior 52 plus 43 new integration/contract cases. Some cases
exercise several related scenarios; the scenario IDs are not a claim of exhaustive
line/branch coverage. See the current matrix in Tests/SCENARIOS.md.

### Settings audit: complete within the user-defined off-game gate

Settings inventory, global scope and access routes are unchanged from the correction
record below. The primary route uses the Mod subclass; the hidden optional MainButtons
worker opens the same Dialog_ModSettings and Mod instance. No extra dependency was added.
Actual render/open/reveal/hide interaction remains in final gameplay acceptance.

Verified with the real delivered production DLL and installed Verse/RimWorld classes:

- All nine source/target mode pairs in EN and FR: driver/giver classes, capacities,
  participants, chair/bed flags, translated job reports, stable object identity and
  worker cache invalidation. Both same-mode reapplication and mode changes are covered.
- Live individual, type and own-code global toggles; explicit-choice precedence;
  changing mode while disabled; seat-option toggling; external weight restoration,
  zero-weight preservation, repeated suppression and late-arriving givers.
- Actual generation in all three modes; custom kinds and activity/building assignment
  precedence; repeated scans and retention of original external weights.
- Editor pending names/choices, reference cleanup, grouping, all three sort modes,
  activity counters, SetAll and cache invalidation. These are helper tests, not UI drawing.
- Real Scribe save/extract/load/finalization for all persisted fields, including a
  non-ASCII name with XML-special characters; old empty settings load defaults and
  usable non-null collections. Files stay inside .build; player settings are untouched.
- Earlier numeric-mode/default/reset checks remain passing. No new player options
  were invented merely to satisfy the gate.

### Defects reproduced and fixed during this continuation

1. **R02 / D39:** reassigning one giver changed a shared job's recreation kind while
   leaving another giver/building unchanged. The targeted activity now receives an
   isolated copy of a shared job; both conflicting assignment orders pass.
2. **R03:** a repeated scan classified its own generated givers as external coverage
   and lost repair tracking. Existing generated jobs/givers are reused by identity;
   stale references from a full reload are discarded. External weights are retained
   across replays, and newly supplied external coverage supersedes an old repair.
3. **R04:** the usable-kind count included disabled covered buildings. It now checks
   positive-weight givers serving the building with the matching recreation kind.
4. **D40:** duplicate occurrences of a reassigned building were only partly removed.
   All matching occurrences are now detached.
5. **U36:** invalidating editor caches left tooltips cached. All relevant caches now
   clear, and UI settings application refreshes both ordering and tooltips.

Pre-fix reproductions are retained as reproduced-4-defects.txt and
reproduced-tooltip-cache.txt. The original assertions now pass; they were not weakened.
Generation also clears HasRun before work begins, tested with a controlled failed replay
(D41). Its exception message now accurately states that definitions may be partly updated;
this is not a rollback mechanism and the exact in-game log rendering remains unverified.

The .NET runner exposed a separate build contract omission: GenerateAssemblyInfo=false
prevented Publicizer's generated IgnoresAccessChecksTo attribute from being emitted.
Source/AssemblyInfo.cs now explicitly supplies the existing Publicizer attribute for
Assembly-CSharp. Private worker invalidation and definition operations execute successfully
against the unmodified installed assembly; a compiled-attribute assertion also passes.
The earlier .NET FieldAccessException is not described as an observed Mono gameplay failure.

### Test-environment boundaries

Fixtures initialize actual language objects from the distributed resources, the two
required capability definitions and the custom type's discovery entry. They require empty
definition databases, restore changed globals, and clear their test definitions/state on
cleanup. They do not mock or patch production settings, Scribe, Retarget or Generate.
Unity profiling and unrelated date/colonist text decoration are disabled in the fixtures.
Generation tests temporarily use Verse's native Log.LockMessages to avoid Unity log calls;
therefore these results cannot establish clean in-game logs. The Scribe tests use the actual
saver and loader through all phases, but do not load a colony or test tolerance migration.
Initial runtime setup failures and an optional first-chance trace are preserved separately.

### Remaining final acceptance

Tests/MANUAL.md retains F01-F13 and adds F14 for shared activity jobs. Execute the
applicable scenarios on a new game and a copied existing save, in FR and EN, observing
actual recreation gain, positions/reservations, logs, persistence and restart behavior.
Exercise MainButtons/RIMMSQOL with its version recorded; no universal compatibility is
inferred from source inspection or the contract test. No playable game session or native
RimWorld UI was controlled during this work.

Other unimplemented robustness fixtures in SCENARIOS.md remain a backlog, not implicit
passes. They are distinguished from the representative applicable settings checks above
and from final in-game requirements. No new concrete defect is left open by this suite.

## Historical first correction record — 2026-09-13

The user authorized fixes after the audit. All confirmed defects from that audit have
been corrected. Stage moves from `dansMonoRepo` to **`preOptions`**. This is the literal
workflow stage after Preview/description verification, not a claim of complete settings
validation or final gameplay acceptance. `settings_audit` remains `partial` because
applicable off-game behavior/serialization tests identified by the audit are still missing.
The independently checked localization resources pass; no gameplay result is inferred.

Base revision remains `2e44782bacb1903e1931f15b62fb2ac6db1fb785` with uncommitted local
changes. The preexisting About.xml, STATUS.md and test-document changes were incorporated;
test scripts/scenarios and historical results were retained. No commit, push, Workshop
publication or image change occurred. The same distributed root `Mod/` is used.

### Corrections delivered

- Added English ATTRIBUTION.md and CHANGELOG.md and identical distribution copies.
  Existing LICENSE copies remain identical. Translated README/SCENARIOS/MANUAL test
  documentation into English, preserving every scenario ID and historical result.
- About.xml now ends with the exact Steam-formatted Source code on GitHub link,
  after project/Harmony/AI credits; the verified repository target is unchanged.
- RawMode rejects undefined enum values, including 99 and -1, and falls back to Auto.
  Both regressions now run by default and also verify the resulting heuristic mode.
- SummaryTip now uses placeholder {0} in EN/FR and receives UsableKindCount explicitly.
- Custom-kind names use the existing translated suggestion when the settings UI creates
  them. Stored names, including player edits, are never retranslated during loading.
- Added JoyRescue_Settings MainButtonDef with buttonVisible=false and validWithoutMap=true.
  Its worker uses RimWorld.Dialog_ModSettings with JoyRescueMod.Instance, the same
  settings instance, UI and WriteSettings path used by the main Mod options dialog.
  No customization dependency, forced visibility override or separate settings store.
- Added native French MainButtonDef label/description injections; English is provided
  by source Def fields, with no redundant English DefInjected file.
- Strengthened XML checks for the final link, translation call-site argument counts,
  shortcut definition/resources and matching distribution documents. Added an executable
  definition-to-worker binding/native visibility inheritance test. Added manual F13 for
  actual shortcut visibility, shared values and customization-tool persistence.

### Executed verification

Its evidence directory, `.build/fix-2026-09-13/`, was deleted on 2026-09-24 as superseded.

| Check | Result |
| --- | --- |
| `dotnet build Tests/JoyRescue.Tests.csproj -c Release --no-restore` | PASS, exit 0, zero warnings/errors; production DLL rebuilt into Mod/Assemblies |
| `./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe` | PASS, exit 0, **52/52**, including both formerly failing R01 cases and shortcut contract |
| `pwsh -NoProfile -File Tests/Test-Xml.ps1` | PASS, exit 0, **18/18**, including call-site arity and new Def XML |
| `pwsh -NoProfile -File ../scripts/Check-DefInjected.ps1 -TransMod ./Mod` | PASS, exit 0, **2 keys, 0 errors**, 11587 definitions indexed |
| In-game acceptance and RIMMSQOL | **Not executed**, no integration version certified |

Delivered DLL SHA-256:
`ECBED57B8F23E6AF0DBCC2E52732D75630E72B2EB5B85FCC4B30B0C856659000`.
Build/test logs are build.txt and tests.txt; XML and injection logs are xml.txt and definjected.txt in that same directory.

The attempted direct Visible call in the off-game runner failed when Verse.ModsConfig
initialized Unity save paths (`ECall methods must be packaged into a system module`).
It is preserved in visibility-runtime-attempt.txt as an **environment limitation**, not
as an observed game defect. The final additional test checks the compiled worker binding,
default flag and inherited native Visible property, and does not claim runtime visibility.
Source inspection of the installed RimWorld 1.6 MainButtonWorker confirms Visible returns
def.buttonVisible; Dialog_ModSettings delegates drawing and saving to the same Mod instance.
Actual activation/rendering/reveal/hide remain F13, not a passing mocked integration.

### Settings and localization disposition

Settings defaults/override behavior/mode validation/reset pass their executable cases.
The UI contract is implemented and compiled. Applicable actual-effect tests for weights,
full Retarget/worker reset and Scribe persistence remain unexecuted, so `options` is not
certified. These are missing verifications, not newly asserted defects or a requirement
to add more player options. Existing R02–R04 hypotheses retain their unverified status.

The localization inventory now includes the shortcut Def. All owned literal translation
calls are covered by nonempty EN/FR resources; call arguments meet positional placeholders.
The two native injection paths resolve. The hardcoded generated suggestion is removed.
Proper name Joy Rescue, IDs, technical logs and saved user names remain justified exclusions.
The original six-source inventory plus the new shortcut worker was reviewed. Resource
validation is `complete` independently of the pending settings gate; FR/EN rendering,
PreResolve language behavior and persistence remain part of final in-game validation.

### Cumulative stage and next gate

Standalone/public/pushed baseline, identifiers and MIT notices remain established from
this session's live audit. Required English documentation is now present. Known production
regressions are fixed, build passes, icon/Preview artifacts are unchanged and retain their
direct visual validation. Description and original-title naming now pass. Therefore the
cumulative stage is `preOptions`. To reach `options`, implement and execute the applicable
off-game behavior/persistence cases above; do not use the 52-case suite as proof of those
uncovered paths. Final in-game tests will still be required for `tested`.

## Historical pre-fix workflow audit — 2026-09-13

This is the preserved pre-fix audit snapshot, superseded by the correction record above. Previous stage: `done`.
Retained stage: `dansMonoRepo`, the workflow baseline before the first fully satisfied
transition. **This is not a claim that the repository is physically in the monorepo**:
`detached: yes` remains true. The literal workflow names are used here, not the older
`port`/`showcase` shorthand. No transition to `horsMonoRepo` or later is cumulatively
complete because its required documentation is incomplete. No repository move is needed.

### Scope and preservation

Audited standalone repository: `C:\Users\nelim\Documents\rimworld\JoyRescue`.
Distributed root: its `Mod/` directory. The installed `RimWorld/Mods/JoyRescue` NTFS
junction was inspected and points to this exact `Mod/` directory.
Revision: `2e44782bacb1903e1931f15b62fb2ac6db1fb785`, plus the existing local changes:
modified `Mod/About/About.xml`, `STATUS.md`, `Tests/README.md`; untracked
`Tests/MANUAL.md` and `Tests/Test-Xml.ps1`. These were included in the audit and preserved.
Only this status document and ignored audit/build outputs were written by the audit.
No production fix, new feature, image generation, commit, push or publication was performed.
Historical test outputs remain intact. `tested_on: 2026-08-29` is historical only.

Read the parent `AGENTS.md`, `PUBLISHING.md`, `STYLE_RIMWORLD.md`, `MOD_SETTINGS.md`
and `TRANSLATIONS.md`. The supplied audit prompt overrides conflicting instructions:
in-game settings interaction is a `tested` requirement, not an `options` prerequisite.

### Ordered transition findings

| Transition | Finding in the current files |
| --- | --- |
| dansMonoRepo → horsMonoRepo | **Defect found.** `ATTRIBUTION.md` and `CHANGELOG.md` are absent. `Tests/README.md`, `Tests/SCENARIOS.md` and `Tests/MANUAL.md` are French documentation. The prompt requires initialized English documentation. Independent passes: standalone Git root, public GitHub repository, configured origin, pushed commit, initialized STATUS, coherent identifiers and MIT notices. |
| horsMonoRepo → ModIcon générée | **Partially validated.** Release build passes and distributed DLL is current; PNG icon passes direct inspection and dimensions. Development cannot be certified finished with the reproduced R01 defect still open. |
| ModIcon générée → Preview générée | **Artifact validated independently.** Preview is a readable 896×504 PNG, 585587 bytes, below both 900 KB and 1 MB. Direct visual review found no concrete camera or composition defect. No historical generation report or screenshot comparison is required. |
| Preview générée → preOptions | **Defect found.** English About description ends with `Source code and issue tracker: https://github.com/vbardales/Rimworld-Joy-Rescue`, not the required final `[url=https://github.com/vbardales/Rimworld-Joy-Rescue]Source code on GitHub[/url]`. URL target itself is correct and verified. Naming passes: Joy Rescue is an original public mod; no prefix, continuation suffix, status tag or linking word needs special treatment. |
| preOptions → options | **Defect found / required tests incomplete.** Useful settings and primary access implementation exist, but no MainButtonDef, MainTabWindow or runtime shortcut registration exists anywhere in the sources/distributed files. R01 fails; effect and serialization coverage is incomplete. See Settings audit. |
| options → l10n | **Defect found.** Catalog checks pass, but the SummaryTip argument contract is broken in EN/FR and an owned generated default label bypasses localization. See Translation audit. |
| l10n → preTest | **Dependency declarations validated independently by source/metadata inspection.** Harmony is used and required, with matching loadAfter. RimWorld 1.6 is targeted. DLC entries only express order, not mandatory dependencies. Malay Themed Expansion, Shared Joys and customization tools are not required by the code. No LoadFolders or conditional XML patches exist. |
| preTest → done | **Not validated.** Twelve manual scenarios have prerequisites/actions/expected results; automated nominal tests and XML checks pass, but the complete regression command fails two cases and applicable settings tests remain unimplemented/unexecuted. |
| done → tested | **Not verified.** No in-game scenario was executed in this audit. Current-version logs, FR/EN UI, actual pawn behavior, settings persistence, optional shortcut integration, new game and existing save coverage remain pending. The historical August load check does not certify the current version. |

GitHub was checked live with `gh repo view ... --json name,visibility,url,defaultBranchRef`
and `git ls-remote origin HEAD`: PUBLIC, default branch main, remote HEAD equal to the
revision above. The sandbox initially blocked network/config access; a permitted retry
succeeded. The repository is `vbardales/Rimworld-Joy-Rescue`, display name Joy Rescue,
folder JoyRescue, package ID nelim.joyrescue. Their differences are conventional, not defects.
The recorded original-work rationale is consistent with the inspected source: game/Harmony
APIs are referenced, no third-party assembly is bundled. Root and distributed MIT notices
are identical (copyright 2026 Nelim). This audit does not infer a license for dependencies.

### Build and executable evidence

Its logs, `.build/audit-2026-09-13/`, were deleted on 2026-09-24 as superseded.

| Command | Observed result | Evidence |
| --- | --- | --- |
| `dotnet build Tests/JoyRescue.Tests.csproj -c Release --no-restore` | Exit 0; builds production net48 and test net8.0; zero warnings/errors | `build.txt` |
| `./.build/tests/bin/Release/net8.0/JoyRescue.Tests.exe` | Exit 0; 49/49 PASS | `nominal.txt` |
| Same executable with `--regressions` | Exit 1; 49/51 PASS; R01 values 99 and -1 fail, returning the undefined value instead of Auto | `regressions.txt` |
| `pwsh -NoProfile -File Tests/Test-Xml.ps1` | Exit 0; 13/13 PASS | `xml.txt` |

Initial sandbox build could not read the local Microsoft SDK directory. Retrying with
permitted SDK access passed; this was an environment restriction, not a code defect.
SDK: .NET 8.0.424. Installed game assembly file version: 1.6.9676.17735.
Resolved build references: Krafs.Rimworld.Ref 1.6.4871, Lib.Harmony 2.4.2,
Krafs.Publicizer 2.3.2 (build-only).
Production DLL SHA-256 before and after build, and test-copy SHA-256, are all:
`398FC2A294BE632D874BF6FED60B199304FAFFE6E56814C73E6FA2525247192F`.
Thus these test results concern the delivered DLL, not an older executable.

### Settings audit

`settings_audit: partial`. Settings are relevant: repair toggles per building, handling
mods with their own code, watching-seat requirement, forced interaction mode, custom
recreation types, building/activity reassignment, disabled types, sort order and reset.
The Mod subclass implements SettingsCategory, DoSettingsWindowContents and WriteSettings;
settings are global ModSettings, serialized through Scribe, rather than per-save data.
Toggles/mode changes call ApplySettings immediately; custom definitions/reassignments need
startup, with translated restart notices and an explicit save button.

Defaults and reset are exercised by the nominal suite: own-code rescue false, watching
chair true, Auto via missing overrides, empty collections, sort 0 and next ID 1. Explicit
per-building overrides, heuristic modes, incomplete-entry guards, mode cycling and activity
label cleanup also pass. The UI cycles mode/sort choices rather than accepting free numeric
input, but persisted invalid modes still fail the mandatory robustness check R01.

Missing MainButtons support is an observed implementation defect, not an unperformed
integration test. No RIMMSQOL or other customization integration was tested. Runtime UI
interaction is deferred to `tested` under the user's rule and is not itself an options blocker.
Applicable off-game coverage still missing: actual weight restoration, full Retarget and
worker invalidation, generation/reassignment interactions, and Scribe save/load/upgrade
behavior. The existing scenario matrix identifies these; planned tests are not passing tests.
R02–R04 and other scenario hypotheses remain unverified, not newly certified defects.

### Translation audit

`localization`, `translation_en`, `translation_fr`: `partial`.
Reviewed the four Keyed files and all six C# source files, including UI helpers,
generated jobs/types, settings and patch diagnostics. EN/FR catalogs have matching,
nonempty keys and matching placeholder sets; every literal owned Translate key is present.
MainButton texts cannot be certified while its implementation is absent.

**Confirmed defects beyond the passing XML script:**

- `Source/JoyRescueMod.cs:74` calls `JoyRescue.Settings.SummaryTip.Translate()` with zero
  arguments, while both catalogs contain `{4}`. The call cannot supply the required value.
  The audit establishes the source/resource mismatch; it does not claim to have observed
  the exact rendered failure in game. The XML script synthesizes sufficient arguments
  from the resource and therefore does not test call-site arity.
- `Source/JoyRescueMod.cs:161` creates the default player-visible name `Recreation ` + id
  in hardcoded English. User-entered names are exempt, but this default is authored by
  the mod and appears before any user input. The existing translated NewKindLabel key
  is unused. Persistence of a user rename must be preserved when addressing this defect.

Generated report strings use the three translated Report keys with a building-label
argument. Generated job/giver labels inherit building labels; custom names are user data
once edited. Proper title Joy Rescue, internal IDs, punctuation and technical logs are
not missing translations. No owned static Def XML or DefInjected resources exist:
Check-DefInjected is **not applicable**, not skipped as an assumed success. The actual
language resolution of generated labels/reports during PreResolve and FR/EN UI rendering
remain for in-game validation. No reused literal vanilla translation keys were found.

### Visual audit and optional recommendations

Directly opened the distributed Preview and ModIcon. Icon: PNG 128×128, 21116 bytes,
winking orange mascot with recreation motif. Preview: worn recreation room, warm lit
chess table versus a cold dark table, high oblique view, readable title/summary and amber
rule. No visible clipping or concrete camera concern. Original artwork exists as
`Art/Preview-source.png`; no generation history is needed to verify the delivered PNGs.
There is no secondary-colored suffix/tag in this simple title, so no accent/secondary
collision was observed; no artificial tag is required. The older dark veil is explicitly
allowed by the style guide. No measured contrast ratio or separate 268/32-pixel review
is claimed.

Optional on a future overlay refresh: use the guide's experimental version badge and
retain palette/composition files (currently absent). These are not elevated into new
blocking criteria based on absent historical authoring evidence. No image was regenerated.

### Strict next transition

To establish `horsMonoRepo`, initialize English `ATTRIBUTION.md` and `CHANGELOG.md`, with
accurate original-work/dependency/asset credits and distribution copies where applicable,
and translate the existing test documentation into English while preserving its results.
The autonomous repository, public visibility, pushed commit, identifiers and MIT copies
already pass and need no recreation. Later settings/localization/code fixes and final
in-game checks remain separately recorded above; Workshop publication is not a gate here.

## Historical position before the 2026-09-13 audit

The work is finished and the showcase is complete: preview, mod icon, and the full-resolution
source under `Art/`. The folder left the monorepo on 2026-09-12 and is now a repository of its
own, pushed to `https://github.com/vbardales/Rimworld-Joy-Rescue`, which until that day was
public but empty. One remote, which is not the monorepo.

The four identifiers were already aligned on the display name, so nothing was renamed: folder
`JoyRescue`, `packageId` `nelim.joyrescue`, repository `Rimworld-Joy-Rescue`. The junction from
`RimWorld/Mods` points at `JoyRescue/Mod` and did not have to be rebuilt.

The Workshop item does not exist. Publishing there is still entirely ahead.

The mod is an original work. It owes nothing to another mod: not a name, not an asset, not an
idea traceable to one. Malay Themed Expansion is what revealed the problem, having shipped two
recreation buildings no colonist can use, but nothing of it is reused here. Hence
`licence: original`.

## Automated testing — 2026-09-12

Publication audit: keep the original title `Joy Rescue`, with no continuation or
fork suffix. The GitHub URL appears both in the metadata URL field and in the
visible About.xml description. The project's license is **MIT**, copyright
2026 Nelim, recorded in `LICENSE` and `Mod/LICENSE`; `licence: original` describes
provenance, not a separate license. This does not relicense third-party dependencies.

`Tests/MANUAL.md` contains 12 functional acceptance scenarios with prerequisites,
actions and expected results. They have not been executed. `Tests/Test-Xml.ps1`
checks XML parsing, metadata, the GitHub link, duplicate/empty translation keys,
FR/EN parity, formatting placeholders and literal translation calls in C#.
All 13 XML checks pass. There are no gameplay Def XML files in this mod: its defs
are generated by C#, so these XML checks do not validate their runtime behaviour.

The scenario review added 19 cases and a coverage matrix in `Tests/SCENARIOS.md`.
The first executable suite passes 49/49 nominal cases against the compiled mod and
the installed RimWorld assemblies. The separate `--regressions` run passes 49/51:
the two failures reproduce undefined numeric modes (`99`, `-1`) being accepted.
This defect remains unfixed; the test work changes no production logic.

The tests cover settings, identity, the mode heuristic, incomplete entry guards,
the editor's mode cycle and activity labels. They do not yet cover generation,
weight toggling, full retargeting, worker invalidation, persistence or actual pawn
behaviour. The runner uses .NET 8 and data-only ThingDef fixtures without Unity
graphics initialization. Commands and limitations are in `Tests/README.md`.

The Release build completed with no warnings or errors. Test outputs and local
game dependencies stay under `.build/`, outside the published mod folder.
`tested_on` above remains the last in-game check; `unit_tested_on` records this suite.

## What it is made of

A postfix on `DefGenerator.GenerateImpliedDefs_PreResolve` compares every `ThingDef` carrying a
`joyKind` against every `JoyGiverDef.thingDefs`, and builds the missing `JobDef` and
`JoyGiverDef` for the orphans. That window is the one non-negotiable point of the whole mod:
earlier the `joyKind` is not resolved yet, later `JobGiver_GetJoy` has already sized its
positional `DefMap` on the giver count.

Each of the three modes is a giver and driver pair the base game already uses as-is, so no
behaviour code is written. Choice is made on the shape of the def, never on its name, and every
line is overridable in the settings. Disabling an entry sets `baseChance` to zero rather than
removing the def, which is what keeps the settings applicable to a running game.

## Field vocabulary

`stage`: `port`, `showcase`, `preTest`, `done`, `tested`, `published`. `done` means the work is
finished, not that it is published.

`licence`: `open` an explicit licence, `silent` no licence and a dead source, `alive` no licence
but a living source, `forbidden` a written refusal, `original` owing nothing to anyone.

`remaining`: `feature` for something missing from a first release, `defect` for a known fault
left unfixed, `unverified` for what could not be checked.

## Common taxonomy fix set — 2026-09-13

Placement: Joy Rescue, the original compatibility/settings engine. Joy Preservation's
private assets and authoritative standalone repository were not changed. No commit,
push, repository visibility or publication operation accompanies this implementation.
The historical registry and decompilation evidence stay outside delivery commits.

Settings audit: complete under the user's source/automated-test precedence. One useful
opt-in global setting selects the common fix set, default false, applied on full restart.
Primary and hidden-shortcut access continue to use the same Mod settings instance.
Pending-restart detection, settings persistence, stable custom-kind identities/reset
retention and manual-choice precedence are covered by the executable suite. No new
shortcut or mandatory dependency was introduced. Actual UI/game/RIMMSQOL checks remain
in the final gameplay gate; `done` means ready for that acceptance, not `tested`.

The initial 304 inventory candidates were narrowed to 224 rules by excluding 80 legacy
rows without a reliable package ID, then supplemented with six freshly checked rules
for the detached WA-Amusement Renew and ZARS Tribal Reborn Renew (230 total). Rules only update compatible building activity
chains. Unsupported drivers, changed structures and shared conflicts remain untouched
and are logged. See Tests/TAXONOMY.md for exclusions and save-transfer semantics.

Automated evidence: .build/taxonomy-build.txt, .build/taxonomy-results.txt and
.build/taxonomy-xml.txt. Production and test builds succeeded without warnings/errors;
118/118 executable cases and 20/20 XML/resource checks pass. This includes the earlier
95 cases plus 23 taxonomy cases, with real Scribe serialization in an isolated runner.
It does not certify the actual Harmony lifecycle in Unity, player saves or load order.

Translation audit after settings validation: new checkbox, tooltip, summary, reset
message and three initial type names use matching English/French Keyed resources.
The unchanged MainButtonDef and French DefInjected paths pass the existing XML suite.
Saved custom names remain user-editable names, preserving the established behavior.
Translated in-game layout is unverified (F15/F19), independently of complete static
localization/EN/FR coverage. No workflow stage beyond `done` is claimed.

Delivered DLL SHA-256: `e7dd9ffe1a0fd1612a1f3586d617f530eaff4ec25804fed64b49dac87393156f`.
