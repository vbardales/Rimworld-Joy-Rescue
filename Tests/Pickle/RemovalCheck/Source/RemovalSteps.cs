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

        // What the game logs is counted here as it is logged: Log.Messages keeps the last thousand only, and a
        // colonist whose job is gone logs a hundred and fifty a second, so a count read from it stops growing.
        private static readonly object TapLock = new object();
        private static bool tapping;
        private static int errorTotal;
        private static int errorsWhenCounted;
        private static readonly System.Collections.Generic.List<string> UnrelatedErrors = new System.Collections.Generic.List<string>();
        private static string relatedName = "";
        private static DateTime lastRelatedAt = DateTime.MinValue;

        // The game itself wraps a failed job's exception in a second, separate log line ("Could not do
        // PostLoadInit on ...") that does not repeat the pawn's name or the mod's. It is not a second, distinct
        // error: it is the engine's own echo of the one just above, logged a few milliseconds later. Anything
        // that close behind a related error, and that reads as that same kind of echo, is related too.
        private static readonly TimeSpan EchoWindow = TimeSpan.FromMilliseconds(500);

        private static void OnLog(string text, string stack, UnityEngine.LogType type)
        {
            if (type != UnityEngine.LogType.Error && type != UnityEngine.LogType.Exception) return;
            lock (TapLock)
            {
                errorTotal++;
                var now = DateTime.UtcNow;
                var related = text.IndexOf("JoyRescue", StringComparison.OrdinalIgnoreCase) >= 0
                              || text.IndexOf("Joy Rescue", StringComparison.OrdinalIgnoreCase) >= 0
                              || (relatedName.Length > 0 && text.IndexOf(relatedName, StringComparison.Ordinal) >= 0)
                              || ((text.IndexOf("PostLoadInit", StringComparison.Ordinal) >= 0
                                   || text.IndexOf("JobDriver", StringComparison.Ordinal) >= 0)
                                  && now - lastRelatedAt <= EchoWindow);
                if (related) lastRelatedAt = now;
                if (!related && UnrelatedErrors.Count < 5) UnrelatedErrors.Add(text.Split('\n')[0]);
            }
        }

        [Given("Joy Rescue removal: the errors of this launch are being watched for the job of {string}")]
        public void WatchErrors(PickleContext ctx, string name)
        {
            lock (TapLock)
            {
                errorTotal = 0; errorsWhenCounted = 0; UnrelatedErrors.Clear(); relatedName = name; lastRelatedAt = DateTime.MinValue;
                if (!tapping) { UnityEngine.Application.logMessageReceivedThreaded += OnLog; tapping = true; }
            }
        }

        // The errors a load without the mod is allowed to log are the ones about what the mod supplied, and the
        // ones of the colonist who was in the middle of its job: the game cannot rebuild a job whose definition
        // is gone. Any other error is a save the removal broke.
        [Then("Joy Rescue removal: every error logged concerns the removed mod or that job")]
        public void AssertErrorsConcernTheMod(PickleContext ctx)
        {
            lock (TapLock)
            {
                Log.Message($"[Joy Rescue removal] {errorTotal} error(s) logged, {UnrelatedErrors.Count} unrelated");
                ctx.Assert(UnrelatedErrors.Count == 0, $"{UnrelatedErrors.Count} errors do not concern the removed mod or {relatedName}'s job. First: {UnrelatedErrors.FirstOrDefault()}");
            }
        }

        [When("Joy Rescue removal: the errors logged so far are counted")]
        public void CountErrors(PickleContext ctx) { lock (TapLock) errorsWhenCounted = errorTotal; }

        [Then("Joy Rescue removal: no error has been logged since they were counted")]
        public void AssertNoNewErrors(PickleContext ctx)
        {
            lock (TapLock)
                ctx.Assert(errorTotal == errorsWhenCounted, $"{errorTotal - errorsWhenCounted} errors were logged since the count");
        }

        // What a player does with a colonist stuck in a job that cannot run: give them an order. Drafting ends the
        // job they were in; released, they go back to their day.
        [When("Joy Rescue removal: {string} is drafted and released")]
        public async Task DraftAndRelease(PickleContext ctx, string name)
        {
            var pawn = PawnNamed(ctx, name);
            ctx.Require(pawn.drafter != null, $"{name} cannot be drafted");
            try { pawn.drafter.Drafted = true; }
            catch (Exception e)
            {
                Log.Message($"[Joy Rescue removal] drafting {name} threw {e.GetType().Name}; the job is stopped directly instead");
                try { pawn.jobs.StopAll(); } catch (Exception) { }
            }
            await ctx.WaitTicks(30);
            try { pawn.drafter.Drafted = false; } catch (Exception) { }
            await ctx.WaitTicks(30);
        }
        [When("Joy Rescue removal: the game is saved as {string}")]
        public void SaveGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            // A saved game of that name that remains is the leftover of a chain that stopped before its end.
            if (File.Exists(path)) File.Delete(path);
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
