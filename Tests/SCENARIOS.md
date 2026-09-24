# Joy Rescue test scenarios

Scenarios describe intended behavior, not completed tests. See README.md and STATUS.md
for actual results. P0 protects behavior/data; P1 covers normal use; P2 covers presentation
and diagnostics. Run parameterized variants separately.

## Test organization

Unit cases use minimal definitions without a pawn or map. Definition integration cases
use Verse static services and are not isolated unit tests. Gameplay cases exercise actual
loading, reservations, movement and recreation gain.

For static fixtures, reset settings, Entries, AllEntries, OriginalChances, counters and
HasRun. Isolate DefDatabase collections and clean up even after failed assertions; never
run these cases concurrently. Use distinct mod packs for Joy Rescue and third-party givers:
two null packs compare equal in ApplyDisabledKinds and would hide the behavior under test.
Initialize languages and required DefOf objects for translated reports/capacities/skills.
Reference assemblies alone do not constitute an executable Verse environment. Private
editor rules may be tested by reflection, without exposing them in the production API.

## Settings and identity

Source: JoyRescueSettings.cs, CustomJoyKind.cs, Runtime/RescueEntry.cs.

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| U01 | P1 | Read fresh settings | own-code rescue false, chair true, collections empty, sort 0, next ID 1. |
| U02 | P0 | Own-code entry yes/no and global option true/false; call DefaultEnabled | Only own-code with global false is disabled. |
| U03 | P0 | Explicit true/false opposite the default; call IsEnabled | Explicit choice wins in either direction. |
| U04 | P1 | Existing override; SetEnabled to default | Override removed; default applies. |
| U05 | P1 | Two entries; change one away from default | Only its key saved; other entry unchanged. |
| U06 | P0 | Two own-code entries, one explicitly off; enable global option | Unspecified entry enabled; explicit off preserved. |
| U07 | P1 | Missing, Auto, invalid or empty stored mode; RawMode then ModeFor | Auto then heuristic mode. |
| U08 | P1 | Each valid explicit mode; SetMode/RawMode/ModeFor | Explicit mode preserved, overrides heuristic. |
| U09 | P1 | Saved explicit mode; SetMode Auto | Override removed; heuristic restored. |
| U10 | P1 | All settings/collections populated; Reset, then repeat | U01 defaults restored; no old custom type or assignment remains. |
| U11 | P2 | Building defName A, label filled/empty/null | Key always A; nonempty label used, otherwise A. |
| U12 | P1 | CustomJoyKind("7", "Music"); inspect then rename | JoyRescue_Kind_7, supplied label, needsThing true; identity survives rename. |

## Mode selection and reconfiguration

Source: Runtime/JoyRescueGenerator.cs, Heuristic and Retarget.

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| U13 | P0 | Interaction cell, Television or other kind; Heuristic | InteractionCell wins over kind. |
| U14 | P1 | No interaction cell, Television | Watch. |
| U15 | P1 | No cell; other/null kind or null building properties on non-null ThingDef | SitAdjacent in all cases. |
| U16 | P1 | Same structure, different activity-like names/labels | Same heuristic result; names do not select modes. |
| U17 | P0 | Entry with job/giver; apply every mode | Matrix below and resolvedMode match target. |
| U18 | P0 | Cached worker; each of six transitions between distinct modes | Target classes, capacities, participants, seating/bed/report; workerInt null. |
| U19 | P0 | Watch, chair option true then false; reapply | desireSit follows option; eight participants/bed retained; worker invalidated. |
| U20 | P1 | Missing job, giver, then both; Retarget | No exception or partial resolvedMode change. |
| U21 | P1 | Configured entry; repeat same mode | Same settings and job/giver instances; worker invalidated. |

Generated entries start with requireChair=false. Reports must include the building label.

| Field | InteractionCell | SitAdjacent | Watch |
| --- | --- | --- | --- |
| giverClass | JoyGiver_InteractBuildingInteractionCell | JoyGiver_InteractBuildingSitAdjacent | JoyGiver_WatchBuilding |
| driverClass | JobDriver_WatchBuilding | JobDriver_SitFacingBuilding | JobDriver_WatchBuilding |
| joyMaxParticipants | 1 | 2 | 8 |
| canDoWhileInBed | false | false | true |
| desireSit | false | false | requireChairForWatching |
| requiredCapacities | Sight, Manipulation | Sight, Manipulation | Sight |
| Report key | JoyRescue.Report.Using | JoyRescue.Report.Playing | JoyRescue.Report.Watching |

## Detection and generation integration

Source: Runtime/JoyRescueGenerator.cs, public Generate entry point.

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| D01 | P1 | Empty databases; Generate | Empty lists/counters, HasRun true, Nothing to rescue report. |
| D02 | P0 | Recreation building without giver; Generate | One entry in each list, linked job/giver, seen 1, covered 0. |
| D03 | P0 | Building in existing giver.thingDefs; Generate | AllEntries only, covered true, covered count 1; no new job/giver. |
| D04 | P1 | Several givers cover same building, including zero weight | Single covered entry; weight does not affect detection. |
| D05 | P0 | Separately: no building properties, no joyKind, wrong category, blueprint, frame, entityDefToBuild, seat; include valid witness | Excluded defs absent from lists/counters; witness detected. |
| D06 | P1 | Null thingDefs, then list with null entry | No exception; valid references counted. |
| D07 | P0 | Orphan A; Generate | Unique JoyRescue_A and JoyRescue_Giver_A; building/job/giver kind equal; only A served; duration 4000, active weight 2, requireChair false. |
| D08 | P1 | Gaming_Cerebral, Telescope, Gaming_Dexterity, HighCulture, other orphans | Intellectual, Intellectual, Shooting, Artistic, no skill respectively; XP/tick .002 with skill, otherwise 0. |
| D09 | P0 | Source pack with JoyGiver then JobDriver subclass | Own-code warning; repair listed, giver weight 0 by default; explicit enable gives 2. |
| D10 | P1 | No relevant code, null pack, or code only in dependency pack | No own-code warning; enabled by default; source ? for null pack. |
| D11 | P1 | GetTypes raises ReflectionTypeLoadException with partial types | Non-null types examined; relevant subclass still detected. |
| D12 | P1 | Other pack-inspection exception | Warning includes pack name; scan continues without propagated exception. |
| D13 | P1 | Multiple buildings from same pack; instrument inspection | One inspection per pack per generation, shared by orphan entries. |

## Live activation integration

Entry point: ApplySettings. Capture definition identities, counts and indices before each
action; settings application must not create or remove definitions.

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| D14 | P0 | Active repair; disable/re-enable | Weight 2 → 0 → 2; same defs/indices. |
| D15 | P0 | JoyRescue/vanilla/third-party givers with weights 2/3/4 share type; disable type | All zero; unrelated types unchanged. |
| D16 | P0 | Disabled type with original external weights 3/4/0; re-enable | Restore 3/4/0; initially inactive giver stays inactive. |
| D17 | P0 | Individually disabled entry, then disabled type; re-enable type | Entry remains zero; other allowed givers restored. |
| D18 | P0 | Disabled type; individually enable entry | Still zero: disabled type takes precedence. |
| D19 | P0 | Remembered external weights; repeatedly apply while disabled, then enable | Originals not overwritten with zero; correctly restored. |
| D20 | P1 | Unknown disabled kind, giver without kind, entry without giver | No exception; unrelated valid givers unaffected. |

## Custom kinds and reassignment integration

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| D21 | P0 | Custom ID/label, needsThing true/false; Generate | Correct JoyRescue_Kind_<id>; preexisting defs preserved. |
| D22 | P1 | Null custom entry, empty/null ID/label, existing ID; separate runs | Invalid entries/IDs skipped; missing label falls back to defName; existing def neither duplicated nor overwritten. |
| D23 | P0 | New type and assignment saved before startup; Generate | Type exists before reassignment; building/new job/giver use it in one pass. |
| D24 | P0 | Shared giver serves A/B on X; assign A to Y | A detached and repaired on Y; B/shared giver/job stay X. |
| D25 | P1 | Assign covered building to current kind | No detach/new giver. |
| D26 | P1 | Missing building/properties, then missing target kind | Invalid source skipped; missing target warning includes IDs; building/coverage unchanged. |
| D27 | P0 | Activity job and buildings on X; assign activity Y | Giver/job/buildings move together; no new giver for covered buildings. |
| D28 | P1 | Activity without job, null thingDefs, null/no-kind entries | No exception; only eligible buildings changed. |
| D29 | P1 | Missing activity, then missing target kind | Missing activity skipped; missing target warned; defs unchanged. |
| D30 | P0 | Activity assigned Y, one building specifically Z | Activity/other buildings stay Y; specific building detached and repaired on Z. |

## Editor logic without Unity drawing

Source: JoyRescueMod.cs.

| ID | Priority | Given / action | Expected |
| --- | --- | --- | --- |
| U22 | P1 | Building/activity overrides to two kinds; purge one | Only matching targets removed; count sums both dictionaries. |
| U23 | P1 | Existing and pending custom types; KindChoices | All offered, label-sorted; only unmaterialized entries pending, no duplicates. |
| U24 | P2 | Existing/pending type, empty label, unknown ID; resolve name | Available label else defName, including before restart. |
| U25 | P1 | No changes/new type/different assignment/already applied; PendingRestart | false/true/true/false; cover buildings and activities. |
| U26 | P1 | Assignment source building/activity disappeared | PendingRestart false with no other changes. |
| U27 | P2 | Each current mode; NextMode | Auto → InteractionCell → SitAdjacent → Watch → Auto. |
| U28 | P2 | Covered/orphan entries across kinds; group | One group per kind; orphans first then label order. |
| U29 | P2 | Different building counts, including ties; sort by count/name | Count descending with name tie-break; alphabetical mode by name only. |
| U30 | P2 | No-active-giver kinds, orphan kinds, covered-only kinds, active kinds without building | State order follows these categories, name within each. |

## Diagnostics and persistence

| ID | Level | Priority | Given / action | Expected |
| --- | --- | --- | --- | --- |
| D31 | Integration | P1 | Save/reload nontrivial settings/custom type | Options, dictionaries, kinds, needsThing, sort and next ID preserved. |
| D32 | Integration | P1 | Load older settings missing new fields | Defaults restored; all collections non-null and usable. |
| U31 | Unit | P2 | Report with zero orphans then multiple sources/states | Accurate counters; no-own-code first, then source/label order; ID/kind/mode/individual state/warning included. |
| D33 | Integration | P0 | Controlled Generate exception through postfix | No propagation; Joy Rescue error includes original exception. Does not prove rollback of prior mutations. |

## Regression contracts

These are intended guarantees; do not change assertions to legitimize a defect.

| ID | Priority | Scenario | Expected / outstanding question |
| --- | --- | --- | --- |
| R01 | P1 | Stored mode 99 or -1 | Auto fallback. Failed in the September 12 baseline and September 13 audit; now fixed and always tested. |
| R02 | P0 | Reassign giver with job shared by another giver | Selected/credited types coherent. A shared job is now cloned for the reassigned giver. Reproduced and fixed; automated PASS. |
| R03 | P0 | Generate twice on same databases | Repair tracking and disabling remain possible. Own generated givers are now distinguished from external coverage and reused. Reproduced and fixed; automated PASS. |
| R04 | P1 | Disable all givers of kind with covered buildings; count usable kinds | Type no longer counted. Count now uses positive matching giver weights. Reproduced and fixed; automated PASS. |

## Additional gameplay checks

1. Load one orphan per mode: PreResolve generation, references, indices and short hashes
   valid; recreation selection causes no DefMap errors.
2. Observe actual use and correct recreation credit, positions and capacities, with no
   repeated errors.
3. Watch with seat/bed/no seat according to option; group recreation with enough and
   insufficient cells.
4. Change mode/disable repair during play; new selections follow settings, without
   requiring an already running job to stop immediately.
5. Create/assign a kind, restart once; availability and existing saved tolerances preserved.
6. Open editor in FR/EN: labels, pending states, warnings, sorting and reset agree with changes.

## Additions from the September 12 coverage review

The first plan covered generator rules but missed some editor rules and sequential
interactions. This is an inspection-based functional inventory, not measured line/branch
coverage or a guarantee for all third-party mods.

| ID | Level | Priority | Given / action | Expected |
| --- | --- | --- | --- | --- |
| U32 | Unit | P2 | Same-kind givers with positive/zero/negative weights and another kind; count | ActiveGiverCount counts strictly positive matching weights; TotalGiverCount counts all matching. |
| U33 | Unit | P2 | TargetA/B/C reports, punctuation, duplicates, null report/job; Activities/ActivityName | Tokens removed without damaging TargetAlpha; reports cleaned/deduplicated; defName fallback. |
| U34 | Unit | P2 | Lists of 0/1/8/9 items; Join | Up to 8 displayed; thereafter 8 and exact remaining count. |
| U35 | Unit | P2 | Several kinds and giver without kind; GiversByKind | No-kind skipped; others grouped and sorted by activity name. |
| U36 | Unit | P2 | Filled caches; change state/name/assignment without count change; invalidate/read | Groups/order/tooltips current; no stale cached values. |
| U37 | Unit | P1 | Repairs including own-code; SetAll false then true | All choices updated and weights applied; preexisting coverage unchanged; disabled kind stays zero. |
| U38 | Unit | P2 | Multiple same-kind buildings and disabled repairs; count | EnabledCount counts individual choices; UsableKindCount counts distinct kinds; R04 covers effective weights. |
| D34 | Integration/UI | P1 | Create/delete pending kind then create another | ID not reused, old assignments purged, new identity distinct; counter persists. |
| D35 | Integration/UI | P1 | Assign, select current kind, Revert; building/activity variants | Dictionary added/removed as appropriate; no live def mutation; cache/pending display current. |
| D36 | Integration/UI | P0 | Delete materialized custom kind | Def/indices retained this session; assignments purged; separately inspect removal impact on saved tolerances at reload. |
| D37 | Integration/UI | P1 | Rename materialized kind; save/restart | Check name before/after restart; CreateCustomKinds skips existing kinds. Establish live rename contract before final assertion. |
| D38 | Integration/UI | P1 | Save by button and closing dialog | Persistence and ApplySettings invoked; values survive reopen. |
| D39 | Integration | P0 | Two activities sharing job assigned conflicting kinds | Coherent result regardless of dictionary insertion order; shared job isolation preserves each requested kind; both insertion orders pass. |
| D40 | Integration | P0 | Building occurs twice in thingDefs; reassign | Every occurrence detached so new-kind repair can happen. Reproduced and fixed using RemoveAll; automated PASS. |
| D41 | Integration | P0 | Successful generation followed by failure after mutation | No false scan-success signal or claim of rollback; check HasRun and partial state. |
| D42 | Integration | P1 | Generated name collides with existing def | No duplicate; check AddDef replacement's reference impact. Must never replace after DefMaps allocation. |
| D43 | Integration | P1 | Remove applied override; restart from original XML defs | Original kind restored; compare ApplySettings alone, which does not replay assignments. |
| D44 | Integration | P1 | Change mode while entry disabled; apply then enable | Target mode/worker updated while weight remains zero until enable. |
| D45 | Integration | P0 | Disable kind; add giver before reapplication | New giver neutralized; original weight remembered/restored when re-enabled. |

## Coverage baseline and current extensions

| Area | Scenarios | September 12 automated coverage |
| --- | --- | --- |
| Settings/identity | U01–U12 | 26 parameterized cases |
| Heuristic | U13–U16 | 9 cases |
| Reconfiguration | U17–U21, D44 | U20 only, 3 cases |
| Invalid settings | R01 | 2 failing opt-in cases; fixed and mandatory since September 13 |
| Repeated reset | U10 | Included |
| Detection/generation | D01–D13, D42 | Not implemented |
| Live activation | D14–D20, D45 | Not implemented |
| Kinds/reassignment | D21–D30, D34–D37, D39–D40, D43 | Not implemented |
| Editor/counts/caches | U22–U30, U32–U38, D35, D38 | U27: 4; U33 ActivityName: 7; rest pending |
| Reports/persistence/errors | U31, D31–D33, D41 | Not implemented |
| Replay/shared jobs/usable kinds | R02–R04 | Inspection hypotheses, not reproduced |
| Actual game/saves | Gameplay 1–6, D36 | Requires in-game execution |

Baseline nominal total: 49 (26 settings, 9 heuristic, 3 guards, 11 editor); with R01,
51. A scenario may represent multiple cases. Current shortcut/localization checks are
also described in README.md, MANUAL.md F13 and STATUS.md; no planned case is implicitly PASS.
## Current settings integration results — 2026-09-13

**95/95 total PASS**, against the delivered rebuilt DLL. The September 12 matrix above
is retained as history, not current coverage. New cases in SettingsIntegration.cs:

| Area | Executed scope |
| --- | --- |
| Reconfiguration | U17–U21: all 9 source/target combinations in both EN and FR (18 cases), exact reports, capacities/classes/participants/seat flags and worker reset |
| Effects | D14–D20, D44–D45 grouped into interaction cases: individual/type/global own-code toggles, seat changes, external positive/zero weights, repeated suppression and newly added givers |
| Generation | Three orphan modes (D02/D07), real job/giver registration; D23/D24/D30 custom types and building override precedence; generated short-hash assignment during full game loading remains a gameplay check |
| Reassignment | R02 and D40 reproduced then fixed; D39 tested in both insertion orders; shared-job isolation preserves other activities |
| Replay | R03 reproduced then fixed; same identities, disabled-type weight restoration and new external coverage tested; D41 verifies cleared HasRun on a controlled failed replay |
| Editor | U22–U24 name/choice/purge helpers, U28–U30 grouping and all sort modes, U32 active/total counters, U36 tooltip invalidation, U37 SetAll and disabled-type precedence, R04 effective availability |
| Persistence | D31 full real Scribe round trip with escaped non-ASCII custom name and every persisted field; D32 old empty configuration; all loader phases run |
| Build contract | Explicit Assembly-CSharp private-access attribute present in compiled mod |

These are representative applicable technical/functional checks, not implementation of
every planned robustness fixture. Remaining engine-specific cases include real mod loading,
reflection over third-party packs, complete error-log capture, UI interaction, save tolerances
and removal/rename effects across actual game restarts. They are not certified by this runner.
D33's catch/log behavior and D41's exact log display remain for engine validation; only the
controlled failure state is asserted off-game. No assertion was weakened to accept R02–R04,
D40 or the stale-tooltip defect. Reproduction logs
were deleted on 2026-09-24 as superseded; the current results and build logs are `.build/taxonomy-*.txt`.