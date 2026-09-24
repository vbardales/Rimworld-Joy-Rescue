using System;
using System.IO;
using System.Reflection;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace JoyRescue.PickleSteps
{
    [PickleSteps]
    public sealed class SharedJobSteps
    {
        private const string Chess = "JoyRescueWitness_PlayChess";
        private const string Ur = "JoyRescueWitness_PlayUr";
        // Two kinds that exist in Core and differ from the witnesses' own Gaming_Cerebral. A kind that
        // does not exist is skipped by the mod with a warning, so it must never be picked here.
        private const string ChessKind = "Social";
        private const string UrKind = "Gaming_Dexterity";
        private static readonly string ProcessMarker = Guid.NewGuid().ToString("N");

        private static string SettingsPath(PickleContext ctx)
        {
            var method = typeof(LoadedModManager).GetMethod("GetSettingsFilename",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ctx.Require(method != null && JoyRescueMod.Instance != null,
                "cannot locate Joy Rescue's settings file");
            return (string)method.Invoke(null, new object[] {
                JoyRescueMod.Instance.Content.FolderName, typeof(JoyRescueMod).Name });
        }

        private static string BackupPath(PickleContext ctx) => SettingsPath(ctx) + ".joyrescue-f14-backup";
        private static string MarkerPath(PickleContext ctx) => SettingsPath(ctx) + ".joyrescue-f14-writer";

        private static JoyGiverDef Giver(PickleContext ctx, string name)
        {
            var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(name);
            ctx.Require(giver != null, $"shared-job witness '{name}' was not loaded");
            return giver;
        }

        [Given("Joy Rescue shared-job witnesses are loaded and share their original job")]
        public void RequireWitnesses(PickleContext ctx)
        {
            var chess = Giver(ctx, Chess);
            var ur = Giver(ctx, Ur);
            ctx.Require(chess.jobDef != null && ReferenceEquals(chess.jobDef, ur.jobDef),
                $"witnesses do not share a job: chess={chess.jobDef?.defName}, ur={ur.jobDef?.defName}");
            ctx.Require(chess.thingDefs?.Count == 1 && chess.thingDefs[0]?.defName == "ChessTable",
                "chess witness does not serve ChessTable");
            ctx.Require(ur.thingDefs?.Count == 1 && ur.thingDefs[0]?.defName == "GameOfUrBoard",
                "Ur witness does not serve GameOfUrBoard");
        }

        [When("Joy Rescue shared-job assignments are saved in {string} order")]
        public void SaveAssignments(PickleContext ctx, string order)
        {
            ctx.Require(order == "chess-then-ur" || order == "ur-then-chess",
                $"unknown order '{order}'");
            var settings = JoyRescueMod.Settings;
            ctx.Require(settings != null && JoyRescueMod.Instance != null, "Joy Rescue settings are unavailable");
            foreach (var kind in new[] { ChessKind, UrKind })
                ctx.Require(DefDatabase<JoyKindDef>.GetNamedSilentFail(kind) != null,
                    $"recreation kind '{kind}' does not exist in this game: the mod would skip the assignment");
            var backup = BackupPath(ctx);
            ctx.Require(!File.Exists(backup),
                $"a previous F14 settings backup remains at {backup}; inspect and restore it first");
            JoyRescueMod.Instance.WriteSettings();
            File.Copy(SettingsPath(ctx), backup);
            settings.giverKindOverrides.Clear();
            if (order == "chess-then-ur")
            {
                settings.giverKindOverrides.Add(Chess, ChessKind);
                settings.giverKindOverrides.Add(Ur, UrKind);
            }
            else
            {
                settings.giverKindOverrides.Add(Ur, UrKind);
                settings.giverKindOverrides.Add(Chess, ChessKind);
            }
            JoyRescueMod.Instance.WriteSettings();
            File.WriteAllText(MarkerPath(ctx), ProcessMarker);
        }

        [Then("Joy Rescue shared-job witnesses have independent assigned jobs after restart")]
        public void AssertRestarted(PickleContext ctx)
        {
            ctx.Require(File.Exists(MarkerPath(ctx)) && File.ReadAllText(MarkerPath(ctx)) != ProcessMarker,
                "F14 writer did not run in a previous game process");
            var chess = Giver(ctx, Chess);
            var ur = Giver(ctx, Ur);
            ctx.Assert(chess.joyKind?.defName == ChessKind,
                $"{Chess} kind is {chess.joyKind?.defName}, expected {ChessKind}");
            ctx.Assert(ur.joyKind?.defName == UrKind,
                $"{Ur} kind is {ur.joyKind?.defName}, expected {UrKind}");
            ctx.Assert(chess.jobDef != null && ur.jobDef != null
                && !ReferenceEquals(chess.jobDef, ur.jobDef),
                $"witnesses still share job {chess.jobDef?.defName}/{ur.jobDef?.defName}");
            ctx.Assert(chess.jobDef.joyKind == chess.joyKind && ur.jobDef.joyKind == ur.joyKind,
                "isolated jobs do not credit their selected recreation kinds");
        }

        [AfterScenario("@joyrescue-shared-reader")]
        public void RestoreSettings(PickleContext ctx)
        {
            var backup = BackupPath(ctx);
            if (!File.Exists(backup)) return;
            File.Copy(backup, SettingsPath(ctx), true);
            File.Delete(backup);
            if (File.Exists(MarkerPath(ctx))) File.Delete(MarkerPath(ctx));
        }
    }
}
