# Changelog

## [Unreleased]

- Remove the internal definition name from the activity tooltip of the settings window. The activity's
  label, the buildings it serves and the explanation remain.
- Rename the common recreation fix set option after what it does, "Move other mods' activities to better
  recreation types", and rewrite its tooltip, summary line and reset question in English and French so that
  they say what the option does and the three types it adds, instead of how it works.
- Add a View button to the settings window that arranges the list under each type either as each activity
  followed by the buildings it serves, or as each building followed by the activities that serve it.
  Orphaned buildings, and activities that serve no building, come last in both. The choice is saved.

## [0.1.0] - 2026-09-22

- Create the `PublishedFileId.txt` file (Workshop item 3806137974) for the pre-published item.
- Repair recreation buildings their own mod left inert: build the missing recreation giver
  and job for each orphan, with per-building controls and a manual choice of behaviour.
- Add custom recreation types and reassignment tools, English/French resources and showcase images.
- Add an opt-in common recreation fix set with 230 exact package/giver rules,
  applied after restart and guarded against structural changes and shared conflicts.
- Preserve manual choices, original drivers, durations, weights and rewards.
- Add three retained custom type identities with English/French initial labels.
- Transfer tolerance once per source/target pair with per-pawn save markers;
  retain original types and prevent repeated or cascading transfers.
- Retain custom-type ordering after using the preset, including during Reset.
- Isolate shared jobs when reassigning activities, preserving other activities' types.
- Preserve generated repair identities and original external weights across repeated scans.
- Detach all duplicate building references during reassignment.
- Count only recreation types with active matching building givers.
- Refresh editor ordering and tooltip caches after settings changes.
- Clear scan-success state before generation and report partial failure accurately.
- Restore Publicizer's runtime access attribute when assembly-info generation is disabled.
- Reject undefined numeric recreation modes and fall back to automatic selection.
- Supply the recreation-count argument to the settings summary tooltip.
- Localize suggested custom recreation names while preserving saved user names.
- Add an optional MainButtons settings shortcut, hidden by default, using the
  same RimWorld settings dialog and persistence as the primary Mod options entry.
- Add French shortcut translations, attribution and distribution documentation.
- Correct the final GitHub link in the mod description.
- Executable suite: 118 passing cases, including real Scribe serialization, EN/FR mode
  transitions, weight restoration, generation and regression reproductions.

## Development baseline - 2026-09-12

- Established the standalone repository and initial automated suite.
- Delivered recreation repair generation, per-building controls, custom recreation
  types and reassignment tools, English/French resources, and showcase images.
- Historical validation: 49/49 nominal tests, 49/51 with numeric-mode regressions,
  and 13/13 XML checks. These figures describe that baseline, not the fixed build.
- Final gameplay acceptance was not completed at that date.
