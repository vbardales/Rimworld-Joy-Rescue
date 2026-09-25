using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace JoyRescue.RemovalCheck
{
    // F11, the half that runs in a game WITHOUT Joy Rescue. This assembly must not reference the mod: it is
    // loaded by a launch from which the mod, and the test mod that depends on it, have been taken out.
    //
    // Every step has its own prefix, "Joy Rescue removal:", because the steps of the main test mod are not
    // loaded in this launch and must not be assumed.
    [PickleSteps]
    public sealed class RemovalSteps
    {
        private static Pawn PawnNamed(PickleContext ctx, string name)
        {
            var map = Find.CurrentMap;
            ctx.Require(map != null, "no map is loaded");
            var spawned = map.mapPawns.AllPawnsSpawned;
            var found = spawned.FirstOrDefault(p =>
                (p.Name is NameTriple triple && triple.Nick == name)
                || (p.Name is NameSingle single && single.Name == name)
                || p.LabelShort == name);
            ctx.Assert(found != null, $"no spawned pawn named \"{name}\"; the map holds: " + string.Join(", ", spawned.Select(p => p.LabelShort)));
            return found;
        }

        [Then("Joy Rescue removal: the mod {string} is not loaded")]
        public void AssertNotLoaded(PickleContext ctx, string packageId)
        {
            ctx.Assert(!ModsConfig.IsActive(packageId), $"{packageId} is loaded in this launch: the removal is not being tested");
        }

        [When("Joy Rescue removal: the saved game {string} is loaded")]
        public async Task LoadSavedGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            ctx.Require(File.Exists(path), $"no saved game called '{name}' at {path}: the writer of this chain did not leave it");
            var before = Current.Game;
            await ctx.WaitUntil(() => !LongEventHandler.AnyEventNowOrWaiting, 175f);
            SuppressFixtureDialogs(true);
            try
            {
                GameDataSaveLoader.LoadGame(name);
                await ctx.WaitUntil(() => Current.Game != null && !ReferenceEquals(Current.Game, before)
                    && Current.ProgramState == ProgramState.Playing && !LongEventHandler.AnyEventNowOrWaiting
                    && (Current.Game.Maps.Count == 0 || Find.CurrentMap != null), 175f);
                await ctx.WaitTicks(2);
                await ctx.WaitUntil(() => !ScreenFader.IsFading(), 15f);
            }
            finally { SuppressFixtureDialogs(false); }
        }

        private static void SuppressFixtureDialogs(bool on)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type;
                try { type = assembly.GetType("RimWorks.Pickle.Autorun.AutorunState"); }
                catch (Exception) { continue; }
                type?.GetProperty("SuppressingFixtureLoad", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, on, null);
            }
        }

        [Then("Joy Rescue removal: {string} is alive and on the map")]
        public void AssertAlive(PickleContext ctx, string name)
        {
            var pawn = PawnNamed(ctx, name);
            ctx.Assert(!pawn.Dead && !pawn.Destroyed, $"{name} is dead or destroyed");
        }

        [Then("Joy Rescue removal: the tolerance of {string} for the recreation type {string} is {float}")]
        public void AssertTolerance(PickleContext ctx, string name, string kindName, float expected)
        {
            var pawn = PawnNamed(ctx, name);
            var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(kindName);
            ctx.Assert(kind != null, $"no recreation type '{kindName}'");
            var actual = pawn.needs.joy.tolerances[kind];
            ctx.Assert(Math.Abs(actual - expected) < 0.005f, $"{name}'s tolerance for {kindName} is {actual:0.0000}, expected {expected:0.0000}");
        }

        // The job a colonist was doing is the loss the manual case documents: its definition is gone with the
        // mod. What must hold is that the colonist has no job of the removed mod, and does something else.
        [Then("Joy Rescue removal: {string} is not doing a job of the removed mod")]
        public void AssertNoRemovedJob(PickleContext ctx, string name)
        {
            var job = PawnNamed(ctx, name).CurJob;
            ctx.Assert(job == null || job.def == null || !job.def.defName.StartsWith("JoyRescue_"),
                $"{name} is still doing {job?.def?.defName}");
        }

        [When("Joy Rescue removal: the game runs for {int} ticks")]
        public async Task RunTicks(PickleContext ctx, int ticks)
        {
            Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
            await ctx.WaitTicks(ticks);
        }

        // The errors a load without the mod is allowed to log are the ones about what the mod supplied. Any
        // other error, or an error about nothing, is a save the removal broke.
        [Then("Joy Rescue removal: every error logged concerns a definition of the removed mod")]
        public void AssertErrorsConcernTheMod(PickleContext ctx)
        {
            var errors = Log.Messages.Where(m => m.type == LogMessageType.Error).Select(m => m.text).ToList();
            var others = errors.Where(e => e.IndexOf("JoyRescue", StringComparison.OrdinalIgnoreCase) < 0
                                           && e.IndexOf("Joy Rescue", StringComparison.OrdinalIgnoreCase) < 0).ToList();
            Log.Message($"[Joy Rescue removal] {errors.Count} error(s) logged, {errors.Count - others.Count} about the removed mod");
            ctx.Assert(others.Count == 0, $"{others.Count} of {errors.Count} errors do not concern the removed mod. First: "
                + others[0].Split('\n')[0]);
        }

        [When("Joy Rescue removal: the game is saved as {string}")]
        public void SaveGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            ctx.Require(!File.Exists(path), $"a saved game called '{name}' remains from an earlier chain: {path}");
            GameDataSaveLoader.SaveGame(name);
            ctx.Assert(File.Exists(path), $"saving '{name}' wrote no file: the scribe error is in the log");
        }

        [When("Joy Rescue removal: the saved game {string} is deleted")]
        public void DeleteSavedGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
