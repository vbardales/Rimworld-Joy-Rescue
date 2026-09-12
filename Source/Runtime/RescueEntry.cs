using RimWorld;
using Verse;

namespace JoyRescue
{
    /// <summary>
    /// How a pawn reaches the building. Each mode is a giver + driver pair the base game already
    /// uses as it stands, so nothing is invented here:
    ///   InteractionCell = telescope and instruments (JoyGiver_InteractBuildingInteractionCell
    ///                     + JobDriver_WatchBuilding, exactly the pair UseTelescope uses);
    ///   SitAdjacent     = chess, game of Ur, poker;
    ///   Watch           = televisions.
    /// </summary>
    public enum RescueMode
    {
        Auto = 0,
        InteractionCell = 1,
        SitAdjacent = 2,
        Watch = 3,
    }

    /// <summary>A building carrying a joyKind that no JoyGiverDef was serving.</summary>
    public sealed class RescueEntry
    {
        public ThingDef building;
        public JoyKindDef joyKind;

        /// <summary>Readable name of the source mod, for grouping in the settings.</summary>
        public string sourceMod;

        /// <summary>
        /// The source mod ships at least one JoyGiver or JobDriver of its own. If so it may well
        /// serve its building from code, without going through thingDefs: we report it but do not
        /// rescue it by default, or the building would end up with two competing jobs.
        /// </summary>
        public bool sourceShipsJoyCode;

        /// <summary>
        /// A JoyGiverDef already serves this building, so there is nothing to fix. These entries
        /// exist only for the inventory shown in the settings - knowing what you have counts as
        /// much as knowing what is broken.
        /// </summary>
        public bool covered;

        /// <summary>The mode actually applied (never Auto: the heuristic is already resolved).</summary>
        public RescueMode resolvedMode;

        public JobDef job;
        public JoyGiverDef giver;

        public string Key => building.defName;

        public string BuildingLabel => building.label.NullOrEmpty() ? building.defName : building.label;
    }
}
