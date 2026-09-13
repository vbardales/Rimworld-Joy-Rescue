using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JoyRescue
{
    /// <summary>
    /// The mod's only patch. See <see cref="JoyRescueGenerator"/> for why it hooks precisely
    /// here and nowhere else.
    /// </summary>
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class Patch_GenerateImpliedDefs_PreResolve
    {
        public static void Postfix()
        {
            try
            {
                JoyRescueGenerator.Generate();
            }
            catch (Exception ex)
            {
                // An exception here would abort the whole game load: log it and let RimWorld
                // start with nothing rescued rather than break the session.
                Log.Error("[Joy Rescue] generation failed; definitions may be partially updated:\n" + ex);
            }
        }
    }
}
