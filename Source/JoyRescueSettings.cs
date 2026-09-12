using System;
using System.Collections.Generic;
using Verse;

namespace JoyRescue
{
    public class JoyRescueSettings : ModSettings
    {
        /// <summary>
        /// Also rescue buildings from mods that ship their own recreation code. False by default:
        /// those mods may already serve their building another way, and a duplicate giver would
        /// have two jobs taking turns on the same piece of furniture.
        /// </summary>
        public bool rescueModsWithOwnCode;

        /// <summary>
        /// "Television" mode: require a chair or a bed, the way the base game does. False lets
        /// pawns watch standing up, which keeps a rescued screen from going unused for want of a
        /// chair.
        /// </summary>
        public bool requireChairForWatching = true;

        /// <summary>Explicit player decisions, keyed by building defName.</summary>
        public Dictionary<string, bool> enabledOverrides = new Dictionary<string, bool>();

        /// <summary>Mode forced by the player. Absent means the heuristic decides.</summary>
        public Dictionary<string, string> modeOverrides = new Dictionary<string, string>();

        /// <summary>Recreation types created here. Materialised on the next startup.</summary>
        public List<CustomJoyKind> customKinds = new List<CustomJoyKind>();

        /// <summary>
        /// Reassignment: building defName -> wanted recreation type defName.
        /// Applied at startup, because a building served by a shared giver has to be detached from
        /// it, which means building a giver - so before the DefMaps are sized.
        /// </summary>
        public Dictionary<string, string> kindOverrides = new Dictionary<string, string>();

        /// <summary>
        /// Reassignment of an ACTIVITY: JoyGiverDef defName -> wanted type defName.
        ///
        /// Distinct from <see cref="kindOverrides"/>, which moves one building. Moving an activity
        /// moves every building it serves at once, with no new giver to fabricate.
        /// </summary>
        public Dictionary<string, string> giverKindOverrides = new Dictionary<string, string>();

        /// <summary>
        /// Types made unreachable. The def is NEVER removed: its givers drop to baseChance 0.
        /// Removing a type would shift the index of every type after it and scramble the
        /// tolerances already saved for the whole colony.
        /// </summary>
        public List<string> disabledKinds = new List<string>();

        /// <summary>
        /// Order of the type list: 0 = by state (whatever needs attention first),
        /// 1 = by building count descending, 2 = alphabetical.
        /// </summary>
        public int kindSortMode = 0;

        /// <summary>Naming counter, so two created types never share an id.</summary>
        public int nextCustomKindId = 1;

        public bool IsEnabled(RescueEntry entry)
        {
            if (enabledOverrides.TryGetValue(entry.Key, out var value)) return value;
            return DefaultEnabled(entry);
        }

        public bool DefaultEnabled(RescueEntry entry)
            => !entry.sourceShipsJoyCode || rescueModsWithOwnCode;

        public void SetEnabled(RescueEntry entry, bool value)
        {
            if (value == DefaultEnabled(entry)) enabledOverrides.Remove(entry.Key);
            else enabledOverrides[entry.Key] = value;
        }

        /// <summary>The mode the player picked, or Auto if they forced nothing.</summary>
        public RescueMode RawMode(RescueEntry entry)
        {
            if (modeOverrides.TryGetValue(entry.Key, out var raw)
                && Enum.TryParse<RescueMode>(raw, out var mode))
            {
                return mode;
            }
            return RescueMode.Auto;
        }

        /// <summary>The mode actually applied: never Auto.</summary>
        public RescueMode ModeFor(RescueEntry entry)
        {
            var mode = RawMode(entry);
            return mode == RescueMode.Auto ? JoyRescueGenerator.Heuristic(entry.building) : mode;
        }

        public void SetMode(RescueEntry entry, RescueMode mode)
        {
            if (mode == RescueMode.Auto) modeOverrides.Remove(entry.Key);
            else modeOverrides[entry.Key] = mode.ToString();
        }

        public void Reset()
        {
            rescueModsWithOwnCode = false;
            requireChairForWatching = true;
            enabledOverrides.Clear();
            modeOverrides.Clear();
            customKinds.Clear();
            kindOverrides.Clear();
            giverKindOverrides.Clear();
            disabledKinds.Clear();
            kindSortMode = 0;
            nextCustomKindId = 1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rescueModsWithOwnCode, "rescueModsWithOwnCode", false);
            Scribe_Values.Look(ref requireChairForWatching, "requireChairForWatching", true);
            Scribe_Collections.Look(ref enabledOverrides, "enabledOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref modeOverrides, "modeOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref customKinds, "customKinds", LookMode.Deep);
            Scribe_Collections.Look(ref kindOverrides, "kindOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref giverKindOverrides, "giverKindOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref disabledKinds, "disabledKinds", LookMode.Value);
            Scribe_Values.Look(ref kindSortMode, "kindSortMode", 0);
            Scribe_Values.Look(ref nextCustomKindId, "nextCustomKindId", 1);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                enabledOverrides = enabledOverrides ?? new Dictionary<string, bool>();
                modeOverrides = modeOverrides ?? new Dictionary<string, string>();
                customKinds = customKinds ?? new List<CustomJoyKind>();
                kindOverrides = kindOverrides ?? new Dictionary<string, string>();
                giverKindOverrides = giverKindOverrides ?? new Dictionary<string, string>();
                disabledKinds = disabledKinds ?? new List<string>();
            }
        }
    }
}
