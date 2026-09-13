using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace JoyRescue
{
    /// <summary>
    /// Finds orphaned recreation buildings and builds the JobDef and JoyGiverDef they were
    /// missing.
    ///
    /// The moment of generation is not negotiable: a postfix on
    /// <c>DefGenerator.GenerateImpliedDefs_PreResolve</c>. It is the only window where both
    /// conditions hold:
    ///   - cross-references are already resolved, so <c>building.joyKind</c> is a real object and
    ///     not a string still waiting to be looked up;
    ///   - <c>DefDatabase&lt;T&gt;.ResolveAllReferences()</c> has not run yet. That matters because
    ///     <c>JobGiver_GetJoy.ResolveReferences</c> allocates a <c>DefMap&lt;JoyGiverDef, float&gt;</c>
    ///     sized from the number of JoyGiverDefs that exist and indexed by <c>def.index</c>: adding
    ///     a giver afterwards would index past the end of that array on every recreation tick.
    /// Short hashes are handed out later in the load ("Short hash giving"), so our defs get one
    /// without us doing anything.
    /// </summary>
    public static class JoyRescueGenerator
    {
        /// <summary>Selection weight, in line with vanilla building givers (2 to 4).</summary>
        public const float BaseChance = 2f;

        /// <summary>The orphans, the ones we repair.</summary>
        public static readonly List<RescueEntry> Entries = new List<RescueEntry>();

        /// <summary>
        /// EVERY recreation building, served or not. This feeds the settings inventory: the
        /// question "which recreation types do I actually have?" is at least as useful as "what is
        /// broken?", since expectations ask for up to 6 different ones.
        /// </summary>
        public static readonly List<RescueEntry> AllEntries = new List<RescueEntry>();

        /// <summary>Total buildings carrying a joyKind, orphans included.</summary>
        public static int JoyBuildingsSeen;

        /// <summary>Those a JoyGiverDef was already serving.</summary>
        public static int AlreadyCovered;

        /// <summary>
        /// The original baseChance of every giver in the game, recorded once. Switching a type off
        /// sets its givers to 0; switching it back on has to give them their value back, including
        /// for vanilla givers whose numbers nobody knows by heart.
        /// </summary>
        public static readonly Dictionary<JoyGiverDef, float> OriginalChances
            = new Dictionary<JoyGiverDef, float>();

        public static bool HasRun;

        public static void Generate()
        {
            HasRun = false;
            // A replay on the same databases must reuse its repairs, not mistake its own
            // givers for third-party coverage. Discard stale references after a full reload.
            var previous = AllEntries.Concat(Entries).Distinct().Where(e =>
                    DefDatabase<ThingDef>.GetNamedSilentFail(e.Key) == e.building
                    && e.job != null && e.giver != null && e.job.generated && e.giver.generated
                    && DefDatabase<JobDef>.GetNamedSilentFail(e.job.defName) == e.job
                    && DefDatabase<JoyGiverDef>.GetNamedSilentFail(e.giver.defName) == e.giver)
                .ToDictionary(e => e.Key);
            var previousGivers = new HashSet<JoyGiverDef>(previous.Values.Select(e => e.giver));
            Entries.Clear();
            AllEntries.Clear();
            foreach (var stale in OriginalChances.Keys.Where(g =>
                         DefDatabase<JoyGiverDef>.GetNamedSilentFail(g.defName) != g).ToList())
                OriginalChances.Remove(stale);
            JoyBuildingsSeen = 0;
            AlreadyCovered = 0;

            // Player-created types have to exist before anything else: a reassignment may target
            // one, and the scan has to be able to count them.
            if (JoyRescueMod.Settings.commonTaxonomy) CommonTaxonomy.EnsureKinds(JoyRescueMod.Settings);
            CreateCustomKinds();
            CommonTaxonomy.Apply(JoyRescueMod.Settings);

            // Then the reassignments. They rewrite the building's joyKind and detach it from its
            // givers: it becomes an orphan again, and the repair pass below builds it a giver of
            // its own on the right type. No special-case code needed.
            ApplyGiverKindOverrides();
            ApplyKindOverrides();

            var covered = new HashSet<ThingDef>();
            foreach (var giver in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (previousGivers.Contains(giver)) continue;
                if (giver.thingDefs == null) continue;
                foreach (var td in giver.thingDefs)
                {
                    if (td != null) covered.Add(td);
                }
            }

            var joyCodeCache = new Dictionary<ModContentPack, bool>();

            foreach (var td in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!IsRescuableJoyBuilding(td)) continue;

                JoyBuildingsSeen++;
                var isCovered = covered.Contains(td);

                previous.TryGetValue(td.defName, out var entry);
                entry = entry ?? new RescueEntry();
                entry.building = td;
                entry.joyKind = td.building.joyKind;
                entry.sourceMod = td.modContentPack?.Name ?? "?";
                entry.covered = isCovered;
                entry.sourceShipsJoyCode = !isCovered && ModShipsJoyCode(td.modContentPack, joyCodeCache);

                AllEntries.Add(entry);

                if (isCovered)
                {
                    // Newly supplied external coverage supersedes an earlier repair.
                    if (entry.giver != null) entry.giver.baseChance = 0f;
                    AlreadyCovered++;
                    continue;
                }

                Entries.Add(entry);
            }

            foreach (var entry in Entries)
            {
                if (entry.job == null || entry.giver == null) BuildDefsFor(entry);
                else
                {
                    entry.job.joyKind = entry.giver.joyKind = entry.joyKind;
                    entry.job.joySkill = SkillFor(entry.joyKind);
                    entry.job.joyXpPerTick = entry.job.joySkill != null ? 0.002f : 0f;
                    entry.giver.thingDefs = new List<ThingDef> { entry.building };
                }
            }

            ApplySettings();
            HasRun = true;

            // One message, multi-line: this is what we will ask people to paste when something
            // goes wrong, and it saves opening the settings to find out what was done. We log even
            // when there is nothing to repair: without that, a silent mod is indistinguishable
            // from a broken one.
            Log.Message(Report());
            Log.Message(CommonTaxonomy.Report());
        }

        /// <summary>
        /// A building is rescuable if it declares a recreation type and really is a building set
        /// down on the map. Seats are excluded: an armchair tagged with a joyKind is not a source
        /// of recreation, it is the place you watch from.
        /// </summary>
        private static bool IsRescuableJoyBuilding(ThingDef td)
        {
            if (td?.building == null) return false;
            if (td.building.joyKind == null) return false;
            if (td.category != ThingCategory.Building) return false;
            if (td.IsBlueprint || td.IsFrame) return false;
            if (td.entityDefToBuild != null) return false;
            if (td.building.isSittable) return false;
            return true;
        }

        /// <summary>
        /// Does the source mod define JoyGivers or JobDrivers of its own? If so it may serve its
        /// building from code without listing it in any thingDefs, and rescuing it would create a
        /// duplicate. Only the mod's own assemblies are inspected: those of its dependencies (VEF
        /// and friends) are not in its ModContentPack.
        /// </summary>
        private static bool ModShipsJoyCode(ModContentPack pack, Dictionary<ModContentPack, bool> cache)
        {
            if (pack == null) return false;
            if (cache.TryGetValue(pack, out var known)) return known;

            var result = false;
            try
            {
                foreach (var asm in pack.assemblies.loadedAssemblies)
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (System.Reflection.ReflectionTypeLoadException ex) { types = ex.Types; }

                    foreach (var t in types)
                    {
                        if (t == null) continue;
                        if (typeof(JoyGiver).IsAssignableFrom(t) || typeof(JobDriver).IsAssignableFrom(t))
                        {
                            result = true;
                            break;
                        }
                    }
                    if (result) break;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[Joy Rescue] could not inspect assemblies of {pack.Name}: {ex.Message}");
            }

            cache[pack] = result;
            return result;
        }

        /// <summary>
        /// The mode is picked from the shape of the def, never from its name.
        /// An interaction cell is an explicit statement of intent by the author: we honour it.
        /// </summary>
        public static RescueMode Heuristic(ThingDef td)
        {
            if (td.hasInteractionCell) return RescueMode.InteractionCell;
            if (td.building?.joyKind?.defName == "Television") return RescueMode.Watch;
            return RescueMode.SitAdjacent;
        }

        private static void BuildDefsFor(RescueEntry entry)
        {
            var mode = JoyRescueMod.Settings.ModeFor(entry);
            entry.resolvedMode = mode;

            var pack = JoyRescueMod.Instance?.Content;
            var skill = SkillFor(entry.joyKind);

            var job = new JobDef
            {
                defName = "JoyRescue_" + entry.building.defName,
                label = entry.BuildingLabel,
                joyKind = entry.joyKind,
                joyDuration = 4000,
                joyXpPerTick = skill != null ? 0.002f : 0f,
                joySkill = skill,
                modContentPack = pack,
            };

            var giver = new JoyGiverDef
            {
                defName = "JoyRescue_Giver_" + entry.building.defName,
                label = entry.BuildingLabel,
                giverClass = typeof(JoyGiver_InteractBuildingSitAdjacent),
                baseChance = BaseChance,
                thingDefs = new List<ThingDef> { entry.building },
                jobDef = job,
                joyKind = entry.joyKind,
                requireChair = false,
                modContentPack = pack,
            };

            entry.job = job;
            entry.giver = giver;
            Retarget(entry, mode);

            AddDef(job);
            AddDef(giver);
        }

        /// <summary>
        /// Writes everything that depends on the mode into the giver and the job. Safe to call at
        /// runtime: both defs already exist in the database, we only rewrite their fields.
        /// </summary>
        public static void Retarget(RescueEntry entry, RescueMode mode)
        {
            var job = entry.job;
            var giver = entry.giver;
            if (job == null || giver == null) return;

            entry.resolvedMode = mode;

            switch (mode)
            {
                case RescueMode.Watch:
                    giver.giverClass = typeof(JoyGiver_WatchBuilding);
                    job.driverClass = typeof(JobDriver_WatchBuilding);
                    // Eight, like vanilla WatchTelevision, and not one. This mode exists to reproduce a
                    // television: with a single slot, Shared Joys computes joyMaxParticipants minus the
                    // reservations already placed, finds zero the moment one pawn sits down, and refuses
                    // every group with "not enough space for everyone to chill". The real limit stays the
                    // number of watch cells the building actually offers.
                    job.joyMaxParticipants = 8;
                    // A bed counts as a seat for JobDriver_WatchBuilding, but only that driver
                    // knows how to put the pawn on one: the other modes must stay false.
                    giver.canDoWhileInBed = true;
                    giver.desireSit = JoyRescueMod.Settings.requireChairForWatching;
                    giver.requiredCapacities = new List<PawnCapacityDef> { PawnCapacityDefOf.Sight };
                    job.reportString = "JoyRescue.Report.Watching".Translate(entry.BuildingLabel).Resolve();
                    break;

                case RescueMode.InteractionCell:
                    giver.giverClass = typeof(JoyGiver_InteractBuildingInteractionCell);
                    job.driverClass = typeof(JobDriver_WatchBuilding);
                    job.joyMaxParticipants = 1;
                    giver.canDoWhileInBed = false;
                    giver.desireSit = false;
                    giver.requiredCapacities = new List<PawnCapacityDef>
                        { PawnCapacityDefOf.Sight, PawnCapacityDefOf.Manipulation };
                    job.reportString = "JoyRescue.Report.Using".Translate(entry.BuildingLabel).Resolve();
                    break;

                default:
                    giver.giverClass = typeof(JoyGiver_InteractBuildingSitAdjacent);
                    job.driverClass = typeof(JobDriver_SitFacingBuilding);
                    // A game table seats two players, like vanilla chess.
                    job.joyMaxParticipants = 2;
                    giver.canDoWhileInBed = false;
                    giver.desireSit = false;
                    // requireChair false: with no seat around, the pawn plays standing up rather
                    // than leaving the building unused a second time.
                    giver.requireChair = false;
                    giver.requiredCapacities = new List<PawnCapacityDef>
                        { PawnCapacityDefOf.Sight, PawnCapacityDefOf.Manipulation };
                    job.reportString = "JoyRescue.Report.Playing".Translate(entry.BuildingLabel).Resolve();
                    break;
            }

            // JoyGiverDef caches its worker on first use: without this purge, a mode change made
            // mid-game would only take effect on the next startup.
            giver.workerInt = null;
        }

        /// <summary>
        /// Materialises the recreation types created in the settings. Called from Generate(), so
        /// at PreResolve: they land at the tail of the database and shift the index of no existing
        /// type, which leaves already-saved tolerances intact.
        /// </summary>
        private static void CreateCustomKinds()
        {
            foreach (var custom in JoyRescueMod.Settings.customKinds)
            {
                if (custom?.id.NullOrEmpty() ?? true) continue;
                if (DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName) != null) continue;

                AddDef(new JoyKindDef
                {
                    defName = custom.DefName,
                    label = custom.label.NullOrEmpty() ? custom.DefName : custom.label,
                    needsThing = custom.needsThing,
                    modContentPack = JoyRescueMod.Instance?.Content,
                });
            }
        }

        /// <summary>
        /// Moves a whole ACTIVITY to another recreation type, along with its job and the buildings
        /// it serves.
        ///
        /// WHY THREE DEFS AND NOT ONE. Checked in <c>JoyUtility.JoyTickCheckEnd</c>: the type
        /// actually credited to the colonist is <c>curJob.def.joyKind</c>, the JOB's, not the
        /// giver's. Changing only the giver would give an activity picked to satisfy one type and
        /// crediting another. And that same method logs an error as soon as the BUILDING's
        /// <c>joyKind</c> differs from the job's:
        ///
        ///     Log.ErrorOnce("Joy source joyKind and jobDef.joyKind are not the same...")
        ///
        /// So all three have to move together. That is what makes this simpler than moving a
        /// single building: nothing to detach, nothing to fabricate.
        ///
        /// Runs BEFORE <see cref="ApplyKindOverrides"/>: a building reassignment is more specific
        /// and must be able to contradict the one made on its activity.
        /// </summary>
        private static void ApplyGiverKindOverrides()
        {
            foreach (var pair in JoyRescueMod.Settings.giverKindOverrides)
            {
                var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(pair.Key);
                if (giver == null) continue;

                // A reassignment aimed at a type that no longer exists used to be skipped in
                // SILENCE: you assigned, you restarted, and nothing happened without one line
                // saying why. That is exactly what happens when you delete a type you created
                // after having assigned something to it.
                var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(pair.Value);
                if (kind == null)
                {
                    Log.Warning($"[Joy Rescue] activity {pair.Key} pointed at type {pair.Value}, "
                              + "which does not exist. Reassignment skipped: that type was most "
                              + "likely deleted after being assigned.");
                    continue;
                }

                if (giver.joyKind == kind) continue;

                // Isolate a shared job before changing its kind. Otherwise untouched givers
                // would select one recreation type but credit another through the shared job.
                if (giver.jobDef != null)
                {
                    if (DefDatabase<JoyGiverDef>.AllDefsListForReading.Any(other =>
                            other != giver && other.jobDef == giver.jobDef))
                    {
                        var isolated = (JobDef)typeof(object).GetMethod("MemberwiseClone",
                            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(giver.jobDef, null);
                        isolated.defName = "JoyRescue_Activity_" + giver.defName;
                        isolated.shortHash = 0;
                        isolated.modContentPack = JoyRescueMod.Instance?.Content;
                        AddDef(isolated);
                        giver.jobDef = isolated;
                    }
                    giver.jobDef.joyKind = kind;
                }

                giver.joyKind = kind;

                foreach (var td in giver.thingDefs ?? new List<ThingDef>())
                {
                    if (td?.building?.joyKind != null) td.building.joyKind = kind;
                }
            }
        }

        /// <summary>
        /// Changes a building's recreation type, and detaches it from every giver that listed it.
        ///
        /// The detaching is essential: a giver carries ITS OWN joyKind, and
        /// <c>JoyUtility.JoyTickCheckEnd</c> logs an error on every tick when the building's and
        /// the job's disagree. A shared giver - vanilla <c>WatchTelevision</c> covers three
        /// televisions - therefore cannot follow: we pull the building out of it, it becomes an
        /// orphan again, and it gets its own giver on the right type.
        /// </summary>
        private static void ApplyKindOverrides()
        {
            foreach (var pair in JoyRescueMod.Settings.kindOverrides)
            {
                var building = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
                if (building?.building == null) continue;

                // Same silence to break as for activities: a deleted type leaves its reassignments
                // orphaned, and nothing said so.
                var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(pair.Value);
                if (kind == null)
                {
                    Log.Warning($"[Joy Rescue] building {pair.Key} pointed at type {pair.Value}, "
                              + "which does not exist. Reassignment skipped: that type was most "
                              + "likely deleted after being assigned.");
                    continue;
                }

                if (building.building.joyKind == kind) continue;

                foreach (var giver in DefDatabase<JoyGiverDef>.AllDefsListForReading)
                {
                    giver.thingDefs?.RemoveAll(td => td == building);
                }

                building.building.joyKind = kind;
            }
        }

        /// <summary>Re-reads the settings and applies them to the defs already created.</summary>
        public static void ApplySettings()
        {
            var settings = JoyRescueMod.Settings;
            foreach (var entry in Entries)
            {
                if (entry.giver == null) continue;

                var mode = settings.ModeFor(entry);
                Retarget(entry, mode);

                // baseChance 0 means a selection weight of zero in JobGiver_GetJoy, so the giver
                // is never picked. That switches things off without ever removing a def from the
                // database, which would break DefMap indexing.
                entry.giver.baseChance = settings.IsEnabled(entry) ? BaseChance : 0f;
            }

            ApplyDisabledKinds();
        }

        /// <summary>
        /// Makes disabled types unreachable by zeroing the weight of ALL their givers - ours, the
        /// base game's and other mods' alike.
        ///
        /// This is the only acceptable form of "deletion". Removing a JoyKindDef from the database
        /// would shift the index of every type after it, and since `Need_Joy.tolerances` is a
        /// positional DefMap already written into saves, every colonist would wake up tired of the
        /// wrong pastime. Here nothing moves: the type stays, it simply produces nothing any more,
        /// and the inventory shows it in red like any other dead type.
        /// </summary>
        private static void ApplyDisabledKinds()
        {
            var disabled = JoyRescueMod.Settings.disabledKinds;
            var ours = JoyRescueMod.Instance?.Content;

            foreach (var giver in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (!OriginalChances.TryGetValue(giver, out var original))
                {
                    original = giver.baseChance;
                    OriginalChances[giver] = original;
                }

                if (giver.joyKind != null && disabled.Contains(giver.joyKind.defName))
                {
                    giver.baseChance = 0f;
                    continue;
                }

                // Our own givers are governed by the entry loop just above: "re-enabling" them
                // here would overwrite the per-building choice.
                if (giver.modContentPack == ours) continue;

                if (giver.baseChance == 0f && original > 0f)
                {
                    giver.baseChance = original;
                }
            }
        }

        private static SkillDef SkillFor(JoyKindDef kind)
        {
            switch (kind?.defName)
            {
                case "Gaming_Cerebral":
                case "Telescope":
                    return SkillDefOf.Intellectual;
                case "Gaming_Dexterity":
                    return SkillDefOf.Shooting;
                case "HighCulture":
                    return SkillDefOf.Artistic;
                default:
                    return null;
            }
        }

        /// <summary>Tolerant add, for hot reloads that replay the generation.</summary>
        private static void AddDef<T>(T def) where T : Def, new()
        {
            var existing = DefDatabase<T>.GetNamedSilentFail(def.defName);
            if (existing != null) DefDatabase<T>.Remove(existing);
            DefGenerator.AddImpliedDef(def);
        }

        /// <summary>Readable report, for the settings button.</summary>
        public static string Report()
        {
            if (Entries.Count == 0)
            {
                return $"[Joy Rescue] {JoyBuildingsSeen} recreation buildings, all already served. "
                     + "Nothing to rescue.";
            }

            var lines = Entries
                .OrderBy(e => e.sourceShipsJoyCode)
                .ThenBy(e => e.sourceMod)
                .ThenBy(e => e.BuildingLabel)
                .Select(e => $"  {e.sourceMod} | {e.building.defName} ({e.BuildingLabel}) | "
                           + $"{e.joyKind.defName} | {e.resolvedMode} | "
                           + (JoyRescueMod.Settings.IsEnabled(e) ? "on" : "off")
                           + (e.sourceShipsJoyCode ? " | source mod ships joy code" : ""));

            return $"[Joy Rescue] {JoyBuildingsSeen} recreation buildings, {AlreadyCovered} already served, "
                 + $"{Entries.Count} orphaned:\n" + string.Join("\n", lines);
        }
    }
}
