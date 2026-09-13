using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace JoyRescue
{
    /// <summary>Opt-in, exact-match corrections. No runtime name guessing or new drivers.</summary>
    public static class CommonTaxonomy
    {
        public const string Video = "JoyRescue_Kind_taxonomy_video";
        public const string Creative = "JoyRescue_Kind_taxonomy_creative";
        public const string Wellbeing = "JoyRescue_Kind_taxonomy_wellbeing";
        public static bool AppliedEnabled;
        public static readonly List<string> Diagnostics = new List<string>();
        public static readonly Dictionary<string, string> Applied = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> MigrationLinks = new Dictionary<string, string>();

        public static bool Reserved(CustomJoyKind kind) => kind != null &&
            (kind.DefName == Video || kind.DefName == Creative || kind.DefName == Wellbeing);

        public static void EnsureKinds(JoyRescueSettings settings)
        {
            Ensure(settings, "taxonomy_video", () => "JoyRescue.Taxonomy.Video".Translate());
            Ensure(settings, "taxonomy_creative", () => "JoyRescue.Taxonomy.Creative".Translate());
            Ensure(settings, "taxonomy_wellbeing", () => "JoyRescue.Taxonomy.Wellbeing".Translate());
        }

        private static void Ensure(JoyRescueSettings settings, string id, Func<string> label)
        {
            // Retain insertion order and identities, even when the preset is turned off.
            if (!settings.customKinds.Any(k => k?.id == id))
                settings.customKinds.Add(new CustomJoyKind(id, label()) { needsThing = true });
        }

        public sealed class Rule
        {
            public readonly string Package, Giver, OldKind, Target, Job;
            public readonly string[] Things;
            public Rule(string package, string giver, string oldKind, string target, string job, params string[] things)
            { Package = package; Giver = giver; OldKind = oldKind; Target = target; Job = job; Things = things; }
        }

        public static void Apply(JoyRescueSettings settings, IEnumerable<Rule> rules = null)
        {
            AppliedEnabled = settings.commonTaxonomy;
            Diagnostics.Clear(); Applied.Clear(); MigrationLinks.Clear();
            if (!AppliedEnabled) return;
            var givers = DefDatabase<JoyGiverDef>.AllDefsListForReading;
            var plan = new Dictionary<JoyGiverDef, Rule>();
            foreach (var rule in rules ?? TaxonomyRules.All)
            {
                if (string.IsNullOrWhiteSpace(rule.Package)) continue;
                var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(rule.Giver);
                if (giver == null) continue; // Optional mod absent.
                string reason = null;
                var package = giver.modContentPack?.PackageIdPlayerFacing;
                if (!string.Equals(package, rule.Package, StringComparison.OrdinalIgnoreCase)) continue;
                var target = DefDatabase<JoyKindDef>.GetNamedSilentFail(rule.Target);
                if (settings.giverKindOverrides.ContainsKey(rule.Giver)
                    || (giver.thingDefs?.Any(t => t != null && settings.kindOverrides.ContainsKey(t.defName)) ?? false))
                    reason = "explicit player assignment";
                else if (target == null) reason = "target type missing";
                else if (giver.jobDef == null || giver.jobDef.defName != rule.Job) reason = "job changed";
                else if (giver.joyKind?.defName != rule.OldKind && giver.joyKind != target) reason = "giver kind changed";
                else if (giver.jobDef.joyKind != giver.joyKind) reason = "job/giver mismatch";
                else if (!KnownDriver(giver.jobDef.driverClass)) reason = "custom or unsupported driver";
                else if (giver.giverClass != typeof(JoyGiver_WatchBuilding)
                    && giver.giverClass != typeof(JoyGiver_InteractBuildingSitAdjacent)
                    && giver.giverClass != typeof(JoyGiver_InteractBuildingInteractionCell)) reason = "custom or unsupported giver";
                else if (giver.thingDefs == null || giver.thingDefs.Any(t => t == null) || !new HashSet<string>(giver.thingDefs.Select(t => t.defName)).SetEquals(rule.Things))
                    reason = "equipment set changed";
                else if (giver.thingDefs.Any(t => t.building == null || t.building.joyKind != giver.joyKind))
                    reason = "building/giver mismatch";
                if (reason != null) { Diagnostics.Add(rule.Giver + ": skipped (" + reason + ")"); continue; }
                if (plan.TryGetValue(giver, out var previous) && previous.Target != rule.Target)
                { Diagnostics.Add(rule.Giver + ": conflicting rules"); plan.Remove(giver); continue; }
                plan[giver] = rule;
            }

            // Prune whole connected components to a fixed point BEFORE changing any definition.
            // A shared job or object is safe only if every consumer ends on the same kind.
            bool changed;
            do
            {
                changed = false;
                foreach (var pair in plan.ToList())
                {
                    var giver = pair.Key; var rule = pair.Value;
                    bool conflict = givers.Any(other => other != giver &&
                        (other.jobDef == giver.jobDef || (other.thingDefs?.Intersect(giver.thingDefs).Any() ?? false)) &&
                        ((plan.TryGetValue(other, out var otherRule) ? otherRule.Target : other.joyKind?.defName) != rule.Target
                         || settings.giverKindOverrides.ContainsKey(other.defName)));
                    // Jobs can also be referenced from non-giver code. This preset never clones them.
                    if (!conflict) continue;
                    Diagnostics.Add(rule.Giver + ": skipped (shared job or equipment)" );
                    plan.Remove(giver); changed = true;
                }
            } while (changed);

            foreach (var pair in plan)
            {
                var giver = pair.Key; var rule = pair.Value;
                var target = DefDatabase<JoyKindDef>.GetNamed(rule.Target);
                giver.joyKind = target; giver.jobDef.joyKind = target;
                foreach (var thing in giver.thingDefs) thing.building.joyKind = target;
                Applied[giver.defName] = rule.Target;
                // Source/target names, not indices, describe migration provenance.
                MigrationLinks[rule.OldKind + ">" + rule.Target] = rule.Target;
            }
        }

        private static bool KnownDriver(Type type) => type == typeof(JobDriver_SitFacingBuilding)
            || type == typeof(JobDriver_WatchBuilding) || type == typeof(JobDriver_WatchTelevision)
            // Audited private adapter changes only the visual effect of the inherited play toil.
            || (type?.FullName == "JoyPreservation.JobDriver_PlayMahjong"
                && type.Assembly.GetName().Name == "JoyPreservation" && type.BaseType == typeof(JobDriver_SitFacingBuilding));

        public static string Report() => "[Joy Rescue] common taxonomy " + (AppliedEnabled ? "on" : "off")
            + ": " + Applied.Count + " corrections, " + Diagnostics.Count + " skipped.\n"
            + string.Join("\n", Diagnostics.Select(s => "  " + s));
    }
}
