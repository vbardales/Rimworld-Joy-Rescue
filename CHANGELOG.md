# Changelog

## Unreleased — 2026-09-13

- Isolate shared jobs when reassigning activities, preserving other activities' types.
- Preserve generated repair identities and original external weights across repeated scans.
- Detach all duplicate building references during reassignment.
- Count only recreation types with active matching building givers.
- Refresh editor ordering and tooltip caches after settings changes.
- Clear scan-success state before generation and report partial failure accurately.
- Restore Publicizer's runtime access attribute when assembly-info generation is disabled.
- Extend the executable suite to 95 passing cases, including real Scribe serialization,
  EN/FR mode transitions, weight restoration, generation and regression reproductions.

- Reject undefined numeric recreation modes and fall back to automatic selection.
- Supply the recreation-count argument to the settings summary tooltip.
- Localize suggested custom recreation names while preserving saved user names.
- Add an optional MainButtons settings shortcut, hidden by default, using the
  same RimWorld settings dialog and persistence as the primary Mod options entry.
- Add French shortcut translations, attribution and distribution documentation.
- Correct the final GitHub link in the mod description.
- Translate test documentation into English and strengthen regression/XML checks.

## Development baseline — 2026-09-12

- Established the standalone repository and initial automated suite.
- Delivered recreation repair generation, per-building controls, custom recreation
  types and reassignment tools, English/French resources, and showcase images.
- Historical validation: 49/49 nominal tests, 49/51 with numeric-mode regressions,
  and 13/13 XML checks. These figures describe that baseline, not the fixed build.
- Final gameplay acceptance was not completed; no Workshop release is recorded.
