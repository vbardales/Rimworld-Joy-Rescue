using RimWorld;
using Verse;

namespace JoyRescue
{
    /// <summary>Optional shortcut; visibility is controlled by the standard MainButtonDef.</summary>
    public sealed class MainButtonWorker_JoyRescueSettings : MainButtonWorker
    {
        public override void Activate()
        {
            if (JoyRescueMod.Instance != null)
                Find.WindowStack.Add(new Dialog_ModSettings(JoyRescueMod.Instance));
        }
    }
}
