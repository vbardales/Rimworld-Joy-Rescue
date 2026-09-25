using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace JoyRescue
{
    public enum RowKind { Header, Activity, Building }

    /// <summary>One line of the settings list. Depth 1 is indented under the line above it.</summary>
    public struct ListRow
    {
        public RowKind kind;
        public JoyKindDef type;
        public JoyGiverDef giver;
        public RescueEntry entry;
        public int depth;
    }

    /// <summary>
    /// The order of the settings list, kept apart from the drawing so that it can be tested without
    /// the game. Under each recreation type the list is arranged one of two ways: each activity
    /// followed by the buildings it serves, or each building followed by the activities that serve
    /// it. Orphaned buildings, and activities that serve no building, come last in either.
    /// </summary>
    public static class ListLayout
    {
        public const int ActivitiesFirst = 0;
        public const int BuildingsFirst = 1;

        public static List<ListRow> Build(
            int view,
            IEnumerable<JoyKindDef> kinds,
            Func<JoyKindDef, List<RescueEntry>> entriesOf,
            Func<JoyKindDef, List<JoyGiverDef>> giversOf,
            IEnumerable<JoyGiverDef> allGivers,
            IEnumerable<RescueEntry> allEntries,
            Func<JoyGiverDef, string> nameOf)
        {
            // Givers the mod generated for orphaned buildings. They sort after the givers of the
            // buildings that were already served, so what was broken reads below what was not.
            var rescued = new HashSet<JoyGiverDef>(
                allEntries.Where(e => !e.covered && e.giver != null).Select(e => e.giver));

            // Which activities serve which building, in a stable order.
            var serving = new Dictionary<ThingDef, List<JoyGiverDef>>();
            foreach (var giver in allGivers)
            {
                if (giver == null || giver.thingDefs == null) continue;
                foreach (var thing in giver.thingDefs)
                {
                    if (thing == null) continue;
                    if (!serving.TryGetValue(thing, out var list))
                    {
                        list = new List<JoyGiverDef>();
                        serving[thing] = list;
                    }
                    if (!list.Contains(giver)) list.Add(giver);
                }
            }
            foreach (var thing in serving.Keys.ToList())
            {
                serving[thing] = serving[thing].OrderBy(nameOf).ThenBy(g => g.defName).ToList();
            }

            // Every activity that appears under some building, whatever the type of that building.
            var underABuilding = new HashSet<JoyGiverDef>();
            foreach (var entry in allEntries)
            {
                if (entry.building != null && serving.TryGetValue(entry.building, out var gs))
                {
                    foreach (var g in gs) underABuilding.Add(g);
                }
            }

            var rows = new List<ListRow>();
            foreach (var kind in kinds)
            {
                rows.Add(new ListRow { kind = RowKind.Header, type = kind });

                // Buildings already served first, orphans after.
                var entries = (entriesOf(kind) ?? new List<RescueEntry>())
                    .OrderBy(e => e.covered ? 0 : 1)
                    .ThenBy(e => e.BuildingLabel, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
                var givers = giversOf(kind) ?? new List<JoyGiverDef>();

                if (view == BuildingsFirst)
                {
                    foreach (var entry in entries)
                    {
                        rows.Add(new ListRow { kind = RowKind.Building, type = kind, entry = entry });
                        if (entry.building != null && serving.TryGetValue(entry.building, out var gs))
                        {
                            foreach (var g in gs)
                            {
                                rows.Add(new ListRow { kind = RowKind.Activity, type = kind, giver = g, depth = 1 });
                            }
                        }
                    }
                    foreach (var g in givers.Where(x => !underABuilding.Contains(x)))
                    {
                        rows.Add(new ListRow { kind = RowKind.Activity, type = kind, giver = g });
                    }
                }
                else
                {
                    var placed = new HashSet<RescueEntry>();
                    foreach (var g in givers.Where(x => !rescued.Contains(x)).Concat(givers.Where(rescued.Contains)))
                    {
                        rows.Add(new ListRow { kind = RowKind.Activity, type = kind, giver = g });
                        foreach (var entry in entries)
                        {
                            if (placed.Contains(entry) || !Serves(g, entry)) continue;
                            placed.Add(entry);
                            rows.Add(new ListRow { kind = RowKind.Building, type = kind, entry = entry, depth = 1 });
                        }
                    }
                    foreach (var entry in entries.Where(e => !placed.Contains(e)))
                    {
                        rows.Add(new ListRow { kind = RowKind.Building, type = kind, entry = entry });
                    }
                }
            }
            return rows;
        }

        private static bool Serves(JoyGiverDef giver, RescueEntry entry)
        {
            return giver.thingDefs != null && entry.building != null && giver.thingDefs.Contains(entry.building);
        }
    }
}
