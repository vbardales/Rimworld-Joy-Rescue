# Publication sheet

**Updated 2026-09-25. The mod is at `done`. The owner prepublished 0.1.0 by hand on 2026-09-22: the Workshop item exists
(`3806137974`) and `Mod/About/PublishedFileId.txt` is committed (`6695ae3`). Ahead: `tested`, the 1.0.0 through the CI, the
switch to public (by the owner) and the thank-you comments.** This sheet holds what the Workshop page asks for and the
repository holds nowhere else, so that it can be used again at the next update and by whoever picks the mod up.

Rules that apply, and where they are written: `PUBLISHING.md` and `AUDIT.md` (protocols repository, read versions in
`docs/PROTOCOLS-READ.md`), `Rimworld-Release-Admin/docs/OPERATIONS.md` for the CI.

## Before publishing

- **Stage.** The owner's fail fast policy of 2026-09-25 applies to the 1.0.0 of an item created by its 0.1.0 prepublication
  (`AUDIT.md`): the publication goes once no red is open, and the regression pass runs after it. It never skips: every red
  scenario replayed green on a build with its fix, **the Workshop gallery**, **the owner's manual validations**, the dry run of
  the exact commit, the approval of `steam-production` by the owner alone, and **a rollback target chosen beforehand**. The
  target for this item is `b1eb0d1`, whose `Mod/` is the prepublished 0.1.0 (same DLL blob as `6695ae3`); the CI creates the
  tag `v1.0.0` after the upload, so the item has no tag until then.
- **Publication goes through the CI, not the in-game button.** The mod has no workflow yet. It is an update of an existing
  item, so the manual workflow of `Rimworld-Release-Admin/scripts/generate-publish-workflow.sh` applies:

  ```bash
  Rimworld-Release-Admin/scripts/generate-publish-workflow.sh <this repository> \
    --workshop-id 3806137974 --package-id nelim.joyrescue \
    --release-title "Joy Rescue {version}" \
    --require Assemblies/JoyRescue.dll --require Defs --require Languages \
    --description-file PUBLICATION.md --description-heading '^## Description' \
    --gallery-dir Gallery
  ```

  then a dry run on the exact commit (run id and SHA noted in `STATUS.md`), then
  `dispatch-publish.sh <owner/repo> publish-tag.yml <full SHA> 1.0.0`. Only the owner approves `steam-production`. The CI
  creates the tag and the GitHub release (the `## [1.0.0]` section of `CHANGELOG.md`, dated first) after a successful
  upload: not by hand. The version that arrives with `published` is 1.0.0; until then the changelog keeps it as
  `[Unreleased]` above 0.1.0.
- **The description has to be replaced.** The page still carries the text 0.1.0 was created with, which lacks `IF I GO QUIET`,
  `AI-GENERATED`, `THANKS` and the attribution line. `update_description` is switched on for that publish, sending the
  block under `## Description` below. The item is private, so the dry run cannot diff it against the public page: read the
  text by hand, its SHA-256 identifies it.
- **Payload.** `Mod/` as committed, including the DLL built from `Source/` (the runner has no game assemblies to build with):
  `Assemblies/JoyRescue.dll`, `Defs/`, `Languages/`, `About/`, `ATTRIBUTION.md`, `CHANGELOG.md`, `LICENSE`. Nothing of
  `Tests/` or `Source/` ships.

## Description

Sent to Steam only when the item is created, or by a publish with `update_description`. `Mod/About/About.xml` is the source
of the first, and this block of the second. **The two are the same text: keep them identical**, `Test-Xml.ps1` checks that
the About ends with the source link, not that they agree, nor that it reads well. It ends, in this order, with `IF I GO
QUIET` (adoption clause verbatim), `AI-GENERATED`, `THANKS`, the line pointing to `ATTRIBUTION.md` and the licence, and
`[url=https://github.com/vbardales/Rimworld-Joy-Rescue]Source code on GitHub[/url]`. There is no removal commitment at the
top: the mod is `original` work and owes nothing to another mod's code.

```
Makes unusable recreation buildings usable again.

Plenty of mods add a building, tag it with a recreation type, and stop there. That tag is only a label: without a JoyGiverDef pointing at the building, no colonist will ever walk up to it. The building gets built, looks the part, and is never used.

Joy Rescue finds those buildings at startup and builds the missing recreation giver and job for them, picking the right vanilla behaviour from the shape of the building itself:
- a building with an interaction cell gets the telescope treatment (stand on the cell, face the building),
- a television-type building gets watched from a chair,
- anything else gets played at from an adjacent cell, chair or no chair.

Everything found is listed in the mod settings, one line per building, with an on/off toggle and a manual override of the chosen behaviour. Buildings coming from mods that ship their own recreation code are listed too, but left off by default, so nothing is ever fixed twice.

You can also create recreation types of your own and move activities or buildings between types, and arrange the list by activity or by building. An optional preset, off by default, moves known activities of other mods to better types at the next restart, and adds three types: video games and simulations, creative recreation, sensory wellbeing.

This matters more than it looks: recreation tolerance is tracked per type, and expectations ask for up to 6 different types. A recreation type that no giver can produce is a type your colony simply does not have.

No save data, unless the optional preset is turned on: it records tolerance-transfer markers in saves. Can be added to or removed from an ongoing game; removing it loses the recreation types you created and any activity it generated.

IF I GO QUIET

If I do not answer within a reasonable time after being contacted, anyone may freely update this or any other of my mods, including publishing a continuation of it. All credit must be preserved.

AI-GENERATED

This mod's code was written with Codex (OpenAI) and Claude Code (Anthropic) and its images generated with DALL-E (OpenAI), under human direction, review and testing. Stated openly: designing with these tools is my job.

THANKS

Andreas Pardeike for [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077]Harmony[/url].
The authors of [url=https://steamcommunity.com/sharedfiles/filedetails/?id=1084452457]RIMMSQOL[/url], whose main-button customization can reveal this mod's settings shortcut, and which it is tested with.
[url=https://steamcommunity.com/sharedfiles/filedetails/?id=2884249920]Malay Themed Expansion[/url], whose unusable recreation buildings showed the problem, and [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3719496210]Shared Joys[/url], which informed the thinking on group recreation.
RimWorks for [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678]Pickle[/url] and [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3733484696]RimLogging[/url], and my own [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3806142401]Nelim's Pickle Tools[/url]: development tools for the in-game tests, never dependencies of this mod.

What this mod studied and what it reproduced is listed in ATTRIBUTION.md, and it is released under the MIT licence (LICENSE); both are in the repository linked below.

[url=https://github.com/vbardales/Rimworld-Joy-Rescue]Source code on GitHub[/url]
```

## Change notes (Steam), one block per version

The CI reads the block under `### <version>`. They are sent again at every update.

### 1.0.0

> First public release. Finds the recreation buildings that mods ship without any activity to use them, and gives each one the
> missing recreation activity, chosen from the shape of the building. Every finding is listed in the settings with a switch and
> a manual choice of behaviour; buildings from mods that carry their own recreation code are listed but left off. You can create
> recreation types of your own and move activities or buildings between them, and arrange the list by activity or by building.
> An optional preset, off by default, moves known activities of other mods to better types at the next restart. No save data
> unless that preset is on. English and French.

Confirm every sentence against the build that is sent, and against the last in-game passes: it says what a person was shown,
not what the counts say.

### 0.1.0 (prepublished by hand on 2026-09-22, kept for the record)

> Creation of the Workshop item: the `PublishedFileId.txt` file. It carried the mod as it was at commit `6695ae3`, not a tested
> release. Steam creates every item private.

## Dependencies and DLC

- **Harmony** (`brrainz.harmony`, `2009463077`): a hard dependency, declared in `modDependencies` with its Steam URL and download
  link, and in `loadAfter`. The mod patches `DefGenerator.GenerateImpliedDefs_PreResolve` with it.
- **No DLC is required.** `loadAfter` names Royalty, Ideology, Biotech, Anomaly and Odyssey for ordering only; the mod reads
  whatever recreation buildings and givers those and other mods declare at runtime, so load order changes nothing. No
  `LoadFolders.xml`, no conditional patch.
- **Supported versions: 1.6 only.** No optional mod is declared and no incompatibility. RIMMSQOL is exercised by a test pass,
  not required: the shortcut is hidden by default, and a customization tool such as RIMMSQOL can reveal it.
- **Not checked from here:** the state of Malay Themed Expansion's own Workshop page in 1.6, which is named in the thanks and
  is not a dependency.

## Gallery (manual: no tool of the chain can send it)

The Workshop gallery is added by hand on the Steam page, in the order below. The folder `Gallery/` (at the repository root,
not in `Mod/`) holds **only the images to upload, numbered `01-`, `02-`, ... in the order they go on the page, and nothing
else**: it is also the workflow's `--gallery-dir`. Raw captures stay outside git and are deleted once cropped. **The folder does
not exist yet and no gallery image has been made.**

The images come from the nelimZen colony, by hand or from a scenario of their own that mounts the scene (the
`nelim-zen-meadow-studio` fixture of `ScreenshotStudio`, staged with ClearScreen), never from the test passes: their captures
are validation evidence taken in a sandbox colony, with the developer interface showing. Each image is opened before it is
kept: a green capture scenario shows that the trajectory ran, not that the picture shows anything.

Steam shows the first one large: the most demonstrative, not the prettiest. **A proposed order, to confirm on the images
themselves, and not decided by anyone yet:**

1. **A repaired building in use**: a colonist at a building that was inert before, the settings window open beside it saying
   it was rescued. The one that says what the mod is.
2. **The settings window listing an orphaned building** with its mode and switch, in the arrangement by building.
3. **The same list by activity**, showing an activity followed by the buildings it serves.
4. **The optional preset**: the checkbox and its tooltip.
5. **The window in French.**

What is missing to make them: a colony with a real orphaned building. Malay Themed Expansion's two are the reason the mod
exists, but as of 2026-08-29 it loses its two cookers in 1.6 (a framework type it references is gone), so a test-only
orphan witness is the realistic source, which is also what the manual acceptance cases need (`TESTING.md`).

## Content boxes (adult content, violence)

Nothing in the mod, the Preview or the ModIcon shows nudity, gore or sexual content: both images were opened on 2026-09-25
(a chess table under a lamp, and a round orange mascot with a small dog). Answer **no adult content** after opening the final
gallery images: the boxes commit the page.

## Thanks to post, after the item is public

A link to a private item opens for nobody, so post only once it is public. The register `WORKSHOP_COMMENTS.md` decides whether a
comment is still needed. Harmony (`2009463077`), Pickle (`3791648678`), RimLogging (`3733484696`) and RIMMSQOL (`1084452457`)
are already `posted`: their `Covers` now names Joy Rescue and nothing is reposted. PickleTools (`3806142401`) is the owner's own
project: `not_applicable`, also covered. **Malay Themed Expansion (`2884249920`) and Shared Joys (`3719496210`) had no row**,
now `drafted`. One recipient each, under 1000 characters, BBCode allowed; a bare item URL on its own line makes a thumbnail.

**Malay Themed Expansion** (`https://steamcommunity.com/sharedfiles/filedetails/?id=2884249920`), comment page. Status: `drafted`.

> Thank you for Malay Themed Expansion 😊 Your buildings are so charming that two of them, a Dam Haji board and a weaving
> spot, made me notice something: a recreation type on a building is only a label, and without an activity pointing at it
> nobody ever uses it. I made a small mod, Joy Rescue, that finds buildings like that and gives them the missing activity.
> Nothing of yours is copied or shipped, it only reads your defs when the game loads. If you would rather I did something
> differently, tell me and I will ✨ https://steamcommunity.com/sharedfiles/filedetails/?id=3806137974

**Shared Joys** (`https://steamcommunity.com/sharedfiles/filedetails/?id=3719496210`), comment page. Status: `drafted`.

> Thank you for Shared Joys 💛 Your work on recreation for several colonists at once shaped how I thought about group capacity
> in Joy Rescue, a small mod that gives inert recreation buildings the activity they were missing. Nothing of yours is copied,
> required or shipped, it is only credited in the attribution file. If you would rather I did something differently, tell me
> and I will ✨ https://steamcommunity.com/sharedfiles/filedetails/?id=3806137974

Update the register the moment each is posted.

## Right after an upload, and it cannot be undone

- **`Mod/About/PublishedFileId.txt` is committed** (done 2026-09-22, item 3806137974, `6695ae3`). Lost, the next upload would
  create a second item.
- **Steam creates every item private**; RimWorld never calls `SetItemVisibility`, and the CI never sends it. Subscribe to your
  own item, test it, then switch it to public by hand. When the 1.0.0 goes to production, the owner also subscribes to the
  comments and watches all activity of the item and of its parent mods, by hand (`PUBLISHING.md`): record the date and the
  three points in `STATUS.md` before marking it `published`.
- After a publish, read the public page (description, change notes, images): a green release does not prove Steam is up to
  date. Record the evidence in `STATUS.md`.
