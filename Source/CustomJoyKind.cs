using Verse;

namespace JoyRescue
{
    /// <summary>
    /// A recreation type created by the player. Only the identity is stored: the real JoyKindDef
    /// is built on the next startup, inside the def generation window.
    ///
    /// WHY NOT AT RUNTIME. `Need_Joy.tolerances` and `bored` are two
    /// <c>DefMap&lt;JoyKindDef, ...&gt;</c>, sized from the number of types that existed when they
    /// were built and indexed by <c>def.index</c>. Creating a type after load would leave them
    /// too short.
    ///
    /// Creating it at `GenerateImpliedDefs_PreResolve`, on the other hand, puts our types **after**
    /// every XML def, so at the tail of the database with the highest indices. Existing indices do
    /// not move, and `DefMap.ExposeData` pads the table on load: an ongoing save survives without
    /// its tolerances changing owner.
    /// </summary>
    public class CustomJoyKind : IExposable
    {
        /// <summary>Stable suffix, never reused. The defName is "JoyRescue_Kind_" + id.</summary>
        public string id;

        public string label;

        /// <summary>
        /// True if the type is produced without any object, the way vanilla Social and Meditative
        /// are. False by default: a type created here normally exists to label buildings.
        /// </summary>
        public bool needsThing = true;

        public string DefName => "JoyRescue_Kind_" + id;

        public CustomJoyKind() { }

        public CustomJoyKind(string id, string label)
        {
            this.id = id;
            this.label = label;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref needsThing, "needsThing", true);
        }
    }
}
