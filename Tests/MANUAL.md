# Manual functional acceptance

Status: **automated**, see [the map below](#where-each-case-is-automated). Every case F01 to F19 is
written as a Pickle scenario (`AUDIT.md`: no manual test left before `tested`); a case counts as
passed only for the tree and the run recorded in `TESTING.md` and `docs/runs/README.md`. The table
below stays as the specification of what each scenario must show. Prerequisites: RimWorld 1.6,
Harmony, Joy Rescue and identified witness buildings, which are the witness mods under
`Tests/Pickle/` (`OrphanWitness`, `OwnCodeWitness`, `RuleWitness`, `RemovalCheck`, `WitnessMod`).
If a witness is unavailable, the scenario reports it and the case is BLOCKED, not PASS.

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

Off-game success does not mark a case PASS. Inspect the logs of every run and repeat the affected
scenarios after any fix.

## Where each case is automated

Features are in `Tests/Pickle/Mod/Pickle/Features/`, the pass to play them with is in
`TESTING.md`. A chain (`a` then `b`) is several game launches under one hold of the lock,
because a restart cannot happen inside one process.

| Case | Feature | Note |
| --- | --- | --- |
| F01 | `07` | Scan, counters and the window against the witness mod |
| F02 | `08` | The colonist is given each activity by the real recreation choice and credited the declared type only |
| F03 | `09` | Not applicable, with its reason: the group-reservation mod part, no such mod is in the test set |
| F04 | `10` | Disable, re-enable, in each mode |
| F05 | `11` | A mod with its own driver: off by default, the explicit decision wins |
| F06 | `12` | A disabled type, and an entry disabled on its own |
| F07 | `13` then `14` | Create, reassign, restart |
| F08 | `15` then `16`, `17` | Three launches: pending, materialized, reloaded copy |
| F09 | `21` then `22`, English and French | Every text of the mod exists in the language the game runs in |
| F10 | `18` then `19`, `20` | Real clicks on Cancel and Confirm |
| F11 | `27` then `removal-check`, then `28` | The second launch has no Joy Rescue, the third loads its save with it |
| F12 | `23` | Not applicable, with its reason: the click on the source link, which the game hands to the operating system's browser; the address is asserted |
| F13 | `24` then `25`, `26` | RIMMSQOL, with a restart |
| F14 | `03`, `04` then `05` | Both insertion orders, and a colonist doing each shared activity |
| F15 to F19 | see [TAXONOMY.md](TAXONOMY.md) | |

## Common preset F15-F19

See [TAXONOMY.md](TAXONOMY.md) for the EN/FR, conflict, behavior and save cases.
