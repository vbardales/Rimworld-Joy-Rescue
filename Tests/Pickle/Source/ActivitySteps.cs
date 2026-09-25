using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimWorld;
using RimWorks.Pickle;
using Verse;
using Verse.AI;

namespace JoyRescue.PickleSteps
{
    // What Joy Rescue makes of a recreation building in a running colony: what the scan found, what a
    // colonist is then offered, what the real recreation choice picks, and what a job credits.
    //
    // A scenario never fabricates the choice. "Takes the activity" asks the giver Joy Rescue built,
    // through the same Worker.TryGiveJob the base game calls, and starts what it returns. "Draws" call
    // the base game's own JobGiver_GetJoy, the class that weighs every giver and picks one.
    //
    // No step spells a translated word: the suite runs unchanged in English and in French.
    [PickleSteps]
    public sealed class ActivitySteps
    {
        private const BindingFlags InstanceAny = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // What a colonist had when the scenario handed them an activity: the reference every later
        // "gained" is measured against.
        private sealed class Baseline
        {
            public float joy;
            public JobDef job;
            public JoyKindDef kind;
            public readonly Dictionary<JoyKindDef, float> tolerance = new Dictionary<JoyKindDef, float>();
        }

        private static readonly Dictionary<Pawn, Baseline> Baselines = new Dictionary<Pawn, Baseline>();

        private static Map CurrentMap(PickleContext ctx)
        {
            var map = Find.CurrentMap;
            ctx.Require(map != null, "no map is loaded");
            return map;
        }

        private static Pawn PawnNamed(PickleContext ctx, string name)
        {
            var spawned = CurrentMap(ctx).mapPawns.AllPawnsSpawned;
            var found = spawned.FirstOrDefault(p =>
                (p.Name is NameTriple triple && triple.Nick == name)
                || (p.Name is NameSingle single && single.Name == name)
                || p.LabelShort == name);
            ctx.Assert(found != null,
                $"no spawned pawn named \"{name}\"; the map holds: " + string.Join(", ", spawned.Select(p => p.LabelShort)));
            return found;
        }

        private static Building BuildingAt(PickleContext ctx, int x, int z)
        {
            var map = CurrentMap(ctx);
            var cell = new IntVec3(x, 0, z);
            ctx.Require(cell.InBounds(map), $"x={x} z={z} is off the map");
            var found = cell.GetThingList(map).OfType<Building>().FirstOrDefault();
            ctx.Assert(found != null, $"no building at x={x} z={z}; the cell holds: "
                + string.Join(", ", cell.GetThingList(map).Select(t => t.def.defName)));
            return found;
        }

        private static RescueEntry EntryOf(PickleContext ctx, string defName)
        {
            var entry = JoyRescueGenerator.AllEntries.FirstOrDefault(e => e.building.defName == defName);
            ctx.Assert(entry != null, $"Joy Rescue's scan has no entry for '{defName}'. It saw: "
                + string.Join(", ", JoyRescueGenerator.AllEntries.Select(e => e.building.defName).Take(40)));
            return entry;
        }

        // The giver that serves this kind of building: ours if the building was an orphan, the game's
        // or another mod's otherwise.
        private static JoyGiverDef GiverFor(PickleContext ctx, ThingDef def)
        {
            var entry = JoyRescueGenerator.Entries.FirstOrDefault(e => e.building == def);
            if (entry?.giver != null) return entry.giver;
            var giver = DefDatabase<JoyGiverDef>.AllDefsListForReading
                .FirstOrDefault(g => g.thingDefs != null && g.thingDefs.Contains(def) && g.jobDef != null);
            ctx.Assert(giver != null, $"no JoyGiverDef serves '{def.defName}'");
            return giver;
        }

        private static JoyGiverDef GiverOfJob(PickleContext ctx, string jobName)
        {
            var giver = DefDatabase<JoyGiverDef>.AllDefsListForReading.FirstOrDefault(g => g.jobDef?.defName == jobName);
            ctx.Assert(giver != null, $"no JoyGiverDef uses the job '{jobName}'");
            return giver;
        }

        private static bool OnOff(PickleContext ctx, string word)
        {
            ctx.Require(word == "on" || word == "off", $"'{word}' is not 'on' or 'off'");
            return word == "on";
        }

        private static JoyRescueMod Mod(PickleContext ctx)
        {
            ctx.Require(JoyRescueMod.Instance != null, "Joy Rescue is not a loaded mod");
            return JoyRescueMod.Instance;
        }

        // ---------------------------------------------------------------- what the scan found

        [Then("Joy Rescue: the building {string} is rescued as {string} on the recreation type {string}")]
        public void AssertRescued(PickleContext ctx, string defName, string mode, string kind)
        {
            var entry = EntryOf(ctx, defName);
            ctx.Assert(!entry.covered, $"{defName} is already served by a giver, so it is not an orphan");
            ctx.Assert(JoyRescueGenerator.Entries.Contains(entry), $"{defName} is not among the rescued buildings");
            ctx.Assert(entry.giver != null && entry.job != null, $"{defName} has no generated giver or job");
            ctx.Assert(entry.resolvedMode.ToString() == mode, $"{defName} is in mode {entry.resolvedMode}, expected {mode}");
            ctx.Assert(entry.joyKind?.defName == kind, $"{defName} is on type {entry.joyKind?.defName}, expected {kind}");

            Type giverClass, driverClass;
            switch (mode)
            {
                case "InteractionCell":
                    giverClass = typeof(JoyGiver_InteractBuildingInteractionCell);
                    driverClass = typeof(JobDriver_WatchBuilding);
                    break;
                case "SitAdjacent":
                    giverClass = typeof(JoyGiver_InteractBuildingSitAdjacent);
                    driverClass = typeof(JobDriver_SitFacingBuilding);
                    break;
                case "Watch":
                    giverClass = typeof(JoyGiver_WatchBuilding);
                    driverClass = typeof(JobDriver_WatchBuilding);
                    break;
                default:
                    ctx.Require(false, $"'{mode}' is not InteractionCell, SitAdjacent or Watch");
                    return;
            }
            ctx.Assert(entry.giver.giverClass == giverClass,
                $"{defName}: giver class is {entry.giver.giverClass?.Name}, expected {giverClass.Name}");
            ctx.Assert(entry.job.driverClass == driverClass,
                $"{defName}: driver class is {entry.job.driverClass?.Name}, expected {driverClass.Name}");

            // The three places the game reads the type from must agree: the game logs an error on every
            // tick when the building's type differs from the job's.
            ctx.Assert(entry.job.joyKind == entry.joyKind && entry.giver.joyKind == entry.joyKind
                       && entry.building.building.joyKind == entry.joyKind,
                $"{defName}: building {entry.building.building.joyKind?.defName}, giver {entry.giver.joyKind?.defName}, "
                + $"job {entry.job.joyKind?.defName} do not agree");
            ctx.Assert(entry.giver.thingDefs != null && entry.giver.thingDefs.Count == 1 && entry.giver.thingDefs[0] == entry.building,
                $"{defName}: the giver does not serve exactly this building");
            ctx.Assert(DefDatabase<JoyGiverDef>.GetNamedSilentFail(entry.giver.defName) == entry.giver
                       && DefDatabase<JobDef>.GetNamedSilentFail(entry.job.defName) == entry.job,
                $"{defName}: its generated defs are not in the def databases");
        }

        [Then("Joy Rescue: the building {string} is already served and is not rescued")]
        public void AssertCovered(PickleContext ctx, string defName)
        {
            var entry = EntryOf(ctx, defName);
            ctx.Assert(entry.covered, $"{defName} is not marked as already served");
            ctx.Assert(!JoyRescueGenerator.Entries.Contains(entry), $"{defName} is served yet Joy Rescue rescued it");
            ctx.Assert(entry.giver == null && entry.job == null, $"{defName} has a generated giver or job although it was served");
            ctx.Assert(DefDatabase<JoyGiverDef>.AllDefsListForReading.Any(g => g.thingDefs != null && g.thingDefs.Contains(entry.building)),
                $"no giver serves {defName}");
        }

        [Then("Joy Rescue: the building {string} comes from a mod that ships its own recreation code")]
        public void AssertOwnCode(PickleContext ctx, string defName)
        {
            var entry = EntryOf(ctx, defName);
            ctx.Assert(entry.sourceShipsJoyCode, $"{defName} is not flagged as coming from a mod with its own recreation code");
            ctx.Assert(!entry.covered, $"{defName} is covered, so the flag can never apply");
        }

        [Then("Joy Rescue: the building {string} comes from a mod without recreation code of its own")]
        public void AssertNoOwnCode(PickleContext ctx, string defName)
        {
            ctx.Assert(!EntryOf(ctx, defName).sourceShipsJoyCode, $"{defName} is flagged as coming from a mod with its own recreation code");
        }

        [Then("Joy Rescue: the activity of the building {string} is {word}")]
        public void AssertActivityState(PickleContext ctx, string defName, string state)
        {
            var wanted = OnOff(ctx, state);
            var entry = EntryOf(ctx, defName);
            ctx.Assert(entry.giver != null, $"{defName} has no generated activity");
            ctx.Assert(JoyRescueMod.Settings.IsEnabled(entry) == wanted,
                $"{defName}: the setting says {(JoyRescueMod.Settings.IsEnabled(entry) ? "on" : "off")}, expected {state}");
            ctx.Assert((entry.giver.baseChance > 0f) == wanted,
                $"{defName}: the giver's weight is {entry.giver.baseChance}, which is {(entry.giver.baseChance > 0f ? "on" : "off")}, expected {state}");
        }

        [Then("Joy Rescue: the scan counters agree with the entries and the report")]
        public void AssertCounters(PickleContext ctx)
        {
            var all = JoyRescueGenerator.AllEntries;
            var covered = all.Count(e => e.covered);
            ctx.Assert(JoyRescueGenerator.JoyBuildingsSeen == all.Count,
                $"the scan says {JoyRescueGenerator.JoyBuildingsSeen} buildings, the inventory holds {all.Count}");
            ctx.Assert(JoyRescueGenerator.AlreadyCovered == covered,
                $"the scan says {JoyRescueGenerator.AlreadyCovered} already served, the inventory marks {covered}");
            ctx.Assert(JoyRescueGenerator.Entries.Count == all.Count - covered,
                $"{JoyRescueGenerator.Entries.Count} rescued buildings, expected {all.Count - covered}");
            ctx.Assert(JoyRescueGenerator.Entries.All(e => !e.covered), "a rescued building is marked as already served");
            var report = JoyRescueGenerator.Report();
            var head = $"{all.Count} recreation buildings, {covered} already served, {JoyRescueGenerator.Entries.Count} orphaned";
            ctx.Assert(report.Contains(head), $"the report does not say \"{head}\". It starts: {report.Split('\n')[0]}");
        }

        // ---------------------------------------------------------------- settings, the way the window applies them

        [When("Joy Rescue: the activity of the building {string} is switched {word}")]
        public void SwitchActivity(PickleContext ctx, string defName, string state)
        {
            var entry = EntryOf(ctx, defName);
            ctx.Require(entry.giver != null, $"{defName} has no generated activity to switch");
            JoyRescueMod.Settings.SetEnabled(entry, OnOff(ctx, state));
            Mod(ctx).WriteSettings();
        }

        [When("Joy Rescue: the mode of the building {string} is forced to {string}")]
        public void ForceMode(PickleContext ctx, string defName, string mode)
        {
            ctx.Require(Enum.TryParse<RescueMode>(mode, out var parsed), $"'{mode}' is not a mode");
            var entry = EntryOf(ctx, defName);
            ctx.Require(entry.giver != null, $"{defName} has no generated activity");
            JoyRescueMod.Settings.SetMode(entry, parsed);
            Mod(ctx).WriteSettings();
        }

        [When("Joy Rescue: the recreation type {string} is switched {word}")]
        public void SwitchKind(PickleContext ctx, string kind, string state)
        {
            ctx.Require(DefDatabase<JoyKindDef>.GetNamedSilentFail(kind) != null, $"no recreation type '{kind}'");
            var disabled = JoyRescueMod.Settings.disabledKinds;
            if (OnOff(ctx, state)) disabled.Remove(kind);
            else if (!disabled.Contains(kind)) disabled.Add(kind);
            Mod(ctx).WriteSettings();
        }

        [Then("Joy Rescue: the job {string} is offered by a giver at {word} weight")]
        public void AssertGiverWeight(PickleContext ctx, string jobName, string weight)
        {
            ctx.Require(weight == "its" || weight == "no", $"'{weight}' is not 'its' or 'no'");
            var giver = GiverOfJob(ctx, jobName);
            if (weight == "no")
            {
                ctx.Assert(giver.baseChance == 0f, $"the giver of {jobName} weighs {giver.baseChance}, expected 0");
                return;
            }
            var entry = JoyRescueGenerator.Entries.FirstOrDefault(e => e.giver == giver);
            var original = entry != null ? JoyRescueGenerator.BaseChance
                : (JoyRescueGenerator.OriginalChances.TryGetValue(giver, out var recorded) ? recorded : giver.baseChance);
            ctx.Assert(giver.baseChance == original && original > 0f,
                $"the giver of {jobName} weighs {giver.baseChance}, its own weight is {original}");
        }

        // ---------------------------------------------------------------- the colonist

        [Given("Joy Rescue: {string} is ready for recreation at any hour")]
        public void ReadyForRecreation(PickleContext ctx, string name)
        {
            var pawn = PawnNamed(ctx, name);
            ctx.Require(pawn.needs?.joy != null, $"{name} has no recreation need");
            pawn.needs.joy.CurLevelPercentage = 0.05f;
            // A colonist's timetable can forbid recreation: the job ends the moment it is not allowed.
            // The test wants the recreation, not the schedule, so every hour becomes recreation time.
            if (pawn.timetable != null)
                for (var hour = 0; hour < 24; hour++) pawn.timetable.SetAssignment(hour, TimeAssignmentDefOf.Joy);
        }

        [When("Joy Rescue: {string} takes the activity of the building at x={int} z={int}")]
        public void TakeActivity(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var building = BuildingAt(ctx, x, z);
            var giver = GiverFor(ctx, building.def);
            var baseline = new Baseline { joy = pawn.needs.joy.CurLevel, job = giver.jobDef, kind = giver.jobDef.joyKind };
            foreach (var kind in DefDatabase<JoyKindDef>.AllDefsListForReading) baseline.tolerance[kind] = pawn.needs.joy.tolerances[kind];
            Baselines[pawn] = baseline;

            var job = giver.Worker.TryGiveJob(pawn);
            ctx.Assert(job != null, $"the activity of {building.def.defName} ({giver.defName}) offered {name} no job");
            ctx.Assert(job.def == giver.jobDef, $"the job offered is {job.def.defName}, the giver's own is {giver.jobDef.defName}");
            var taken = pawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            ctx.Assert(taken, $"{name} refused the job. Current job: {pawn.CurJob?.def.defName ?? "none"}");
            ctx.Assert(pawn.CurJob != null && pawn.CurJob.def == giver.jobDef,
                $"{name} is doing {pawn.CurJob?.def.defName ?? "nothing"} instead of {giver.jobDef.defName}");
            ctx.Assert(pawn.jobs.curDriver != null && pawn.jobs.curDriver.GetType() == giver.jobDef.driverClass,
                $"{name}'s driver is {pawn.jobs.curDriver?.GetType().Name ?? "none"}, the job's is {giver.jobDef.driverClass?.Name}");
        }

        [Then("Joy Rescue: {string} is offered no activity by the building at x={int} z={int}")]
        public void AssertNoOffer(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var giver = GiverFor(ctx, BuildingAt(ctx, x, z).def);
            var job = giver.Worker.TryGiveJob(pawn);
            ctx.Assert(job == null, $"{name} was offered {job?.def.defName} by the building at x={x} z={z}, expected nothing");
        }

        [Then("Joy Rescue: {string} is offered the activity of the building at x={int} z={int}")]
        public void AssertOffer(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var giver = GiverFor(ctx, BuildingAt(ctx, x, z).def);
            var job = giver.Worker.TryGiveJob(pawn);
            ctx.Assert(job != null && job.def == giver.jobDef,
                $"{name} was offered {job?.def.defName ?? "nothing"} by the building at x={x} z={z}, expected {giver.jobDef.defName}");
        }

        [Then("Joy Rescue: the job of {string} was given the cell x={int} z={int}")]
        public void AssertJobCell(PickleContext ctx, string name, int x, int z)
        {
            var job = PawnNamed(ctx, name).CurJob;
            ctx.Assert(job != null, $"{name} has no job");
            ctx.Assert(job.targetB.IsValid && job.targetB.Cell == new IntVec3(x, 0, z),
                $"{name}'s job aims at {(job.targetB.IsValid ? job.targetB.Cell.ToString() : "no cell")}, expected ({x}, 0, {z})");
        }

        [Then("Joy Rescue: the job of {string} was given {word} seat")]
        public void AssertJobSeat(PickleContext ctx, string name, string word)
        {
            ctx.Require(word == "a" || word == "no", $"'{word}' is not 'a' or 'no'");
            var job = PawnNamed(ctx, name).CurJob;
            ctx.Assert(job != null, $"{name} has no job");
            var seat = job.targetC.HasThing ? job.targetC.Thing : null;
            if (word == "no")
            {
                ctx.Assert(seat == null, $"{name}'s job is given the seat {seat?.def.defName}, expected none");
                return;
            }
            ctx.Assert(seat != null, $"{name}'s job has no seat, expected a chair or a bed");
        }

        [Then("Joy Rescue: the job of {string} was given a bed")]
        public void AssertJobBed(PickleContext ctx, string name)
        {
            var job = PawnNamed(ctx, name).CurJob;
            ctx.Assert(job != null, $"{name} has no job");
            ctx.Assert(job.targetC.HasThing && job.targetC.Thing is Building_Bed,
                $"{name}'s job has {(job.targetC.HasThing ? job.targetC.Thing.def.defName : "no third target")}, expected a bed");
        }

        // A colonist does not arrive by teleporting: the scenario waits until they stand where the
        // mode says they should, reads what they are doing while they wait, and says why on failure.
        [Then("Joy Rescue: {string} comes to the {string} of the building at x={int} z={int}")]
        public async Task AssertArrival(PickleContext ctx, string name, string place, int x, int z)
        {
            ctx.Require(place == "interaction cell" || place == "adjacent cell" || place == "watch cell",
                $"'{place}' is not 'interaction cell', 'adjacent cell' or 'watch cell'");
            var pawn = PawnNamed(ctx, name);
            var building = BuildingAt(ctx, x, z);
            var trace = new List<string>();
            string last = null;
            var t0 = UnityEngine.Time.realtimeSinceStartup;

            bool Arrived()
            {
                var job = pawn.CurJob?.def.defName ?? "none";
                if (job != last)
                {
                    trace.Add($"+{UnityEngine.Time.realtimeSinceStartup - t0:0.0}s {job} at ({pawn.Position.x},{pawn.Position.z})");
                    last = job;
                }
                switch (place)
                {
                    case "interaction cell": return pawn.Position == building.InteractionCell;
                    case "adjacent cell": return GenAdjFast.AdjacentCellsCardinal(building).Contains(pawn.Position);
                    default: return WatchBuildingUtility.CalculateWatchCells(building.def, building.Position, building.Rotation, building.Map)
                        .Contains(pawn.Position);
                }
            }

            try { await ctx.WaitUntil(Arrived, 90f); }
            catch (Exception) { /* reported below, with the trace */ }
            ctx.Assert(Arrived(), $"{name} is at ({pawn.Position.x},{pawn.Position.z}), not on the {place} of {building.def.defName} at x={x} z={z}. "
                + "Job trace: " + (trace.Count == 0 ? "(nothing sampled)" : string.Join(" | ", trace)));
        }

        [Then("Joy Rescue: {string} is credited with the recreation type {string} and with no other within {int} seconds")]
        public async Task AssertCredited(PickleContext ctx, string name, string kindName, int seconds)
        {
            var pawn = PawnNamed(ctx, name);
            ctx.Require(Baselines.TryGetValue(pawn, out var baseline), $"{name} was never handed an activity by a step of this suite");
            var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(kindName);
            ctx.Require(kind != null, $"no recreation type '{kindName}'");
            var tolerances = pawn.needs.joy.tolerances;

            bool Gained() => tolerances[kind] > baseline.tolerance[kind] + 0.0005f && pawn.needs.joy.CurLevel > baseline.joy;

            try { await ctx.WaitUntil(Gained, seconds); }
            catch (Exception) { /* reported below */ }
            ctx.Assert(Gained(), $"{name}: recreation {pawn.needs.joy.CurLevel:0.000} (was {baseline.joy:0.000}), "
                + $"tolerance of {kindName} {tolerances[kind]:0.0000} (was {baseline.tolerance[kind]:0.0000}), job {pawn.CurJob?.def.defName ?? "none"}");
            ctx.Assert(baseline.kind == kind, $"the job {baseline.job.defName} is of type {baseline.kind.defName}, the scenario expected {kindName}");
            foreach (var other in DefDatabase<JoyKindDef>.AllDefsListForReading.Where(k => k != kind))
            {
                ctx.Assert(tolerances[other] <= baseline.tolerance[other] + 0.00001f,
                    $"{name} was also credited with {other.defName}: tolerance {tolerances[other]:0.0000}, was {baseline.tolerance[other]:0.0000}");
            }
        }

        [Then("Joy Rescue: {string} is still doing the job {string}")]
        public void AssertStillDoing(PickleContext ctx, string name, string jobName)
        {
            var current = PawnNamed(ctx, name).CurJob?.def.defName ?? "none";
            ctx.Assert(current == jobName, $"{name} is doing {current}, expected {jobName}");
        }

        [When("Joy Rescue: the building at x={int} z={int} is removed")]
        public void RemoveBuilding(PickleContext ctx, int x, int z)
        {
            BuildingAt(ctx, x, z).Destroy();
        }

        [When("Joy Rescue: {string} lies down in the bed at x={int} z={int}")]
        public async Task LieDown(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var bed = BuildingAt(ctx, x, z) as Building_Bed;
            ctx.Require(bed != null, $"the building at x={x} z={z} is not a bed");
            var taken = pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.LayDown, bed), JobTag.SatisfyingNeeds);
            ctx.Assert(taken, $"{name} refused to lie down. Current job: {pawn.CurJob?.def.defName ?? "none"}");
            try { await ctx.WaitUntil(() => pawn.InBed(), 90f); }
            catch (Exception) { /* reported below */ }
            ctx.Assert(pawn.InBed(), $"{name} is not in bed: job {pawn.CurJob?.def.defName ?? "none"} at ({pawn.Position.x},{pawn.Position.z})");
        }

        [Then("Joy Rescue: {string} in bed is offered the activity of the building at x={int} z={int}, in the bed")]
        public void AssertBedOffer(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var giver = GiverFor(ctx, BuildingAt(ctx, x, z).def);
            ctx.Assert(pawn.InBed(), $"{name} is not in bed");
            ctx.Assert(giver.canDoWhileInBed, $"the giver {giver.defName} is not allowed in bed: the game would never ask it");
            var job = giver.Worker.TryGiveJobWhileInBed(pawn);
            ctx.Assert(job != null && job.def == giver.jobDef && job.targetC.HasThing && job.targetC.Thing is Building_Bed,
                $"{name}, in bed at ({pawn.Position.x},{pawn.Position.z}), was offered {job?.def.defName ?? "nothing"}");
        }

        [Then("Joy Rescue: {string} in bed is offered no activity by the building at x={int} z={int}")]
        public void AssertNoBedOffer(PickleContext ctx, string name, int x, int z)
        {
            var pawn = PawnNamed(ctx, name);
            var giver = GiverFor(ctx, BuildingAt(ctx, x, z).def);
            ctx.Assert(pawn.InBed(), $"{name} is not in bed");
            var job = giver.Worker.TryGiveJobWhileInBed(pawn);
            ctx.Assert(job == null, $"{name}, in bed at ({pawn.Position.x},{pawn.Position.z}), was offered {job?.def.defName}, expected nothing");
        }

        // ---------------------------------------------------------------- the real choice

        // The base game's own recreation choice: every giver weighed, one picked, the next one tried when
        // the pick finds nothing to do. Asked many times for the same colonist, it shows which jobs can
        // come up and which never do, which no single call to a giver could show.
        private static int Draws(PickleContext ctx, Pawn pawn, int draws, Func<Job, bool> counts)
        {
            var node = new JobGiver_GetJoy();
            node.ResolveReferences();
            var method = typeof(JobGiver_GetJoy).GetMethod("TryGiveJob", InstanceAny | BindingFlags.DeclaredOnly, null, new[] { typeof(Pawn) }, null);
            ctx.Require(method != null, "JobGiver_GetJoy.TryGiveJob(Pawn) is unavailable");
            ctx.Require(pawn.needs.joy.CurLevel < 0.99f, $"{pawn.LabelShort}'s recreation is full, the choice returns nothing");
            var count = 0;
            for (var i = 0; i < draws; i++)
            {
                var job = (Job)method.Invoke(node, new object[] { pawn });
                if (job != null && counts(job)) count++;
            }
            return count;
        }

        [Then("Joy Rescue: in {int} draws of the real recreation choice for {string} the job {string} comes up")]
        public void AssertJobComesUp(PickleContext ctx, int draws, string name, string jobName)
        {
            var count = Draws(ctx, PawnNamed(ctx, name), draws, j => j.def.defName == jobName);
            ctx.Assert(count > 0, $"the job {jobName} never came up in {draws} draws");
        }

        [Then("Joy Rescue: in {int} draws of the real recreation choice for {string} the job {string} never comes up")]
        public void AssertJobNeverComesUp(PickleContext ctx, int draws, string name, string jobName)
        {
            var count = Draws(ctx, PawnNamed(ctx, name), draws, j => j.def.defName == jobName);
            ctx.Assert(count == 0, $"the job {jobName} came up {count} times in {draws} draws");
        }

        [Then("Joy Rescue: in {int} draws of the real recreation choice for {string} no job of the recreation type {string} comes up")]
        public void AssertKindNeverComesUp(PickleContext ctx, int draws, string name, string kindName)
        {
            var offenders = new Dictionary<string, int>();
            Draws(ctx, PawnNamed(ctx, name), draws, j =>
            {
                if (j.def.joyKind?.defName != kindName) return false;
                offenders[j.def.defName] = offenders.TryGetValue(j.def.defName, out var n) ? n + 1 : 1;
                return true;
            });
            ctx.Assert(offenders.Count == 0, $"jobs of the type {kindName} came up in {draws} draws: "
                + string.Join(", ", offenders.Select(p => $"{p.Key} x{p.Value}")));
        }
    }
}
