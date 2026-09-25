using System.Collections.Generic;
using RimWorld;
using Verse;

namespace JoyRescueOwnCode
{
    /// <summary>
    /// A recreation giver that serves its building from code: it looks the kiosk up by itself and its
    /// def lists no thingDefs. This is the shape of mod Joy Rescue must not rescue by default, since
    /// a second giver on the same building would compete with this one.
    /// </summary>
    public class JoyGiver_Kiosk : JoyGiver_InteractBuildingInteractionCell
    {
        protected override void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
        {
            outCandidates.Clear();
            var kiosk = DefDatabase<ThingDef>.GetNamedSilentFail("JoyRescueOwnCode_Kiosk");
            if (kiosk != null) outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(kiosk));
        }
    }


    /// <summary>
    /// The mod's own driver, a thin subclass of the game's: what a fix-set rule must refuse to touch, since it
    /// cannot know what a driver of a mod does with the type of the job.
    /// </summary>
    public class JobDriver_Kiosk : JobDriver_WatchBuilding
    {
    }
}
