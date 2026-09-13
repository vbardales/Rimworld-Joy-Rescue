# Manual functional acceptance

Status: ready, **not executed**. For every case record date, RimWorld version, mod
list, observed result, PASS/FAIL and log excerpt. Use a test colony and copied saves.
Prerequisites: RimWorld 1.6, Harmony, Joy Rescue and identified witness buildings:
an interaction-cell orphan, a Television orphan, an adjacent-play orphan, an already
covered building and a building from a mod with its own recreation code. Record each
defName. If a witness is unavailable, report BLOCKED, not PASS.

| ID | Preconditions | Actions | Expected result |
| --- | --- | --- | --- |
| F01 Load | Witnesses installed | Start, inspect log, open settings | No scan exception; covered/orphan distinction and report counters correct. |
| F02 Repairs | Three orphan buildings built and reachable; capable pawn needs recreation | Let pawn select each activity; inspect position, job and need | Correct mode and credited recreation type; no repeated errors. |
| F03 Seats/group | Watch screen; seats/bed available then removed | Toggle seat requirement; test group with enough then too few cells, using a group mod if installed | Seat/bed used when required, standing permitted otherwise; no over-reservation or exception. |
| F04 Live changes | Repair available | Disable, wait for a new selection, re-enable; repeat for all modes | Disabled activity not selected anew; enabled activity resumes in selected mode. An already running job may finish. |
| F05 Own code | Witness mod supplies JoyGiver/JobDriver | Inspect, explicitly enable, change global option | Disabled with warning by default; explicit decision wins; observe whether duplicate activities compete. |
| F06 Disabled type | Vanilla, third-party and repaired givers; one individually disabled repair | Disable/re-enable the type | No new selection while off; weights/behavior restored; individually disabled entry stays off. |
| F07 Create/reassign | Save with known tolerances | Create type, reassign activity/building, save and restart once | New type available; building override takes precedence; correct credit and unchanged existing tolerances. |
| F08 Delete/revert | Pending and materialized custom types with assignments | Delete/revert, inspect, save and reload a copy | References cleaned, pending state consistent; no silent missing references or tolerance reassignment. Record failure if contract fails. |
| F09 Persistence/UI | Nontrivial settings | Save by button and close; reopen/restart in FR and EN | Values retained; readable translations; no unexpected raw IDs; counters, order and tooltips refreshed. Check summary count and translated initial custom name; edited names remain unchanged. |
| F10 Reset | Overrides, types and disabled entries | Cancel reset, then confirm; restart | Cancel changes nothing; confirm restores defaults; deferred changes apply at restart. |
| F11 Add/remove | Save copies, with/without generated job in progress | Load after adding then removing mod | Save remains usable; loss of generated job on removal is documented; play continues. |
| F12 Presentation | Mod list | Inspect Joy Rescue description and GitHub link | Joy Rescue title, 1.6, Harmony dependency and correct clickable source link. |
| F14 Shared activities | Two activities sharing a job, with distinct witness buildings | Assign different kinds; save/restart; repeat with reversed assignment order | Each activity credits its selected kind; other activities unchanged; no repeated mismatch errors. |
| F13 Optional shortcut | Clean configuration, first without customization mod; repeat with RIMMSQOL | Verify default absence; reveal JoyRescue_Settings using customization tool; open, edit, close; compare primary Mod options; hide and restart | Neither visible nor greyed by default. Both routes open the same configuration and retain edits. Tool retains visibility choice. No customization mod required for primary access. Record exact integration version; other tools require separate tests. |

Run applicable cases on a new game and an existing save. Inspect logs after each
interaction and repeat affected regressions after any fix. These cases supplement
SCENARIOS.md; off-game success does not mark them PASS.