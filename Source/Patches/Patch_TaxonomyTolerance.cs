using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JoyRescue
{
    /// <summary>One transfer per source/target pair and pawn; never sum old tolerances.</summary>
    [HarmonyPatch(typeof(JoyToleranceSet), nameof(JoyToleranceSet.ExposeData))]
    public static class Patch_TaxonomyTolerance
    {
        private sealed class State { public List<string> migrated = new List<string>(); }
        private static readonly ConditionalWeakTable<JoyToleranceSet, State> States = new ConditionalWeakTable<JoyToleranceSet, State>();

        public static void Postfix(JoyToleranceSet __instance)
        {
            var state = States.GetValue(__instance, _ => new State());
            // New pawns already live under this preset. Their first reload must not perform
            // a legacy transfer merely because they have never previously been serialized.
            if (Scribe.mode == LoadSaveMode.Saving && CommonTaxonomy.AppliedEnabled)
                foreach (var link in CommonTaxonomy.MigrationLinks.Keys)
                    if (!state.migrated.Contains(link)) state.migrated.Add(link);
            Scribe_Collections.Look(ref state.migrated, "joyRescueTaxonomyTransfers", LookMode.Value);
            state.migrated = state.migrated ?? new List<string>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && CommonTaxonomy.AppliedEnabled)
                MigrateLoaded(__instance);
        }

        public static void MigrateLoaded(JoyToleranceSet set) => Transfer(set,
            States.GetValue(set, _ => new State()).migrated, CommonTaxonomy.MigrationLinks.Keys);

        public static void Transfer(JoyToleranceSet set, List<string> migrated, IEnumerable<string> links)
        {
            // Snapshot prevents cascading A->B->C transfers within the same migration.
            var tolerance = DefDatabase<JoyKindDef>.AllDefsListForReading.ToDictionary(k => k, k => set[k]);
            var boredom = DefDatabase<JoyKindDef>.AllDefsListForReading.ToDictionary(k => k, k => set.BoredOf(k));
            foreach (var link in links.OrderBy(s => s, StringComparer.Ordinal))
            {
                if (migrated.Contains(link)) continue;
                var parts = link.Split('>');
                if (parts.Length != 2) continue;
                var from = DefDatabase<JoyKindDef>.GetNamedSilentFail(parts[0]);
                var to = DefDatabase<JoyKindDef>.GetNamedSilentFail(parts[1]);
                if (from == null || to == null || from == to) continue;
                set.tolerances[to] = Math.Max(set.tolerances[to], tolerance[from]);
                set.bored[to] = set.bored[to] || boredom[from] || set.tolerances[to] > 0.5f;
                migrated.Add(link);
            }
        }
    }
}
