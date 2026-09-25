using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace JoyRescue.PickleSteps
{
    // What a restart changes: settings that only apply at startup (types created, buildings and activities
    // reassigned), a saved game carried from one game process to the next, and the settings file kept
    // between them.
    //
    // A chain is played with `-Filter <writer> -Then <reader>`: one game process each, under one hold of the
    // lock, so nothing can replace the settings file in between. A writer, or a scenario in the middle of a
    // chain, leaves the file as it wrote it; the LAST scenario of the LAST feature carries the tag
    // @joyrescue-restart-last, which puts the original file back and deletes the saved games afterwards,
    // whatever its verdict.
    //
    // No step spells a translated word: the suite runs unchanged in English and in French.
    [PickleSteps]
    public sealed class RestartSteps
    {
        private const BindingFlags InstanceAny = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly string ProcessMarker = Guid.NewGuid().ToString("N");

        // Saved games a step of this process made or loaded: the reader deletes them, the shared WSL
        // profile must not keep them.
        private static readonly System.Collections.Generic.HashSet<string> Saves = new System.Collections.Generic.HashSet<string>();

        private static JoyRescueMod Mod(PickleContext ctx)
        {
            ctx.Require(JoyRescueMod.Instance != null && JoyRescueMod.Settings != null, "Joy Rescue is not a loaded mod");
            return JoyRescueMod.Instance;
        }

        private static string SettingsPath(PickleContext ctx)
        {
            var method = typeof(LoadedModManager).GetMethod("GetSettingsFilename",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ctx.Require(method != null, "LoadedModManager.GetSettingsFilename is unavailable");
            return (string)method.Invoke(null, new object[] { Mod(ctx).Content.FolderName, typeof(JoyRescueMod).Name });
        }

        private static string BackupPath(PickleContext ctx) => SettingsPath(ctx) + ".joyrescue-restart-backup";
        private static string MarkerPath(PickleContext ctx) => SettingsPath(ctx) + ".joyrescue-restart-writer";

        private static Pawn PawnNamed(PickleContext ctx, string name)
        {
            var map = Find.CurrentMap;
            ctx.Require(map != null, "no map is loaded");
            var spawned = map.mapPawns.AllPawnsSpawned;
            var found = spawned.FirstOrDefault(p =>
                (p.Name is NameTriple triple && triple.Nick == name)
                || (p.Name is NameSingle single && single.Name == name)
                || p.LabelShort == name);
            ctx.Assert(found != null,
                $"no spawned pawn named \"{name}\"; the map holds: " + string.Join(", ", spawned.Select(p => p.LabelShort)));
            return found;
        }

        private static JoyKindDef Kind(PickleContext ctx, string defName)
        {
            var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(defName);
            ctx.Assert(kind != null, $"no recreation type '{defName}' in this game");
            return kind;
        }

        private static CustomJoyKind Custom(PickleContext ctx, int number)
        {
            var list = JoyRescueMod.Settings.customKinds;
            ctx.Assert(number >= 1 && number <= list.Count,
                $"there is no custom recreation type number {number}: the settings hold {list.Count}");
            return list[number - 1];
        }

        // ---------------------------------------------------------------- the settings file between two processes

        [Given("Joy Rescue: the settings file of this game is saved aside")]
        public void SaveSettingsAside(PickleContext ctx)
        {
            var backup = BackupPath(ctx);
            ctx.Require(!File.Exists(backup),
                $"a settings backup of an earlier chain remains at {backup}: inspect it and restore it by hand before retrying");
            Mod(ctx).WriteSettings();
            File.Copy(SettingsPath(ctx), backup);
        }

        [When("Joy Rescue: this game is marked as the one that wrote the settings")]
        public void MarkWriter(PickleContext ctx)
        {
            Mod(ctx).WriteSettings();
            File.WriteAllText(MarkerPath(ctx), ProcessMarker);
        }

        [Then("Joy Rescue: an earlier game process wrote the settings")]
        public void AssertEarlierWriter(PickleContext ctx)
        {
            ctx.Require(File.Exists(MarkerPath(ctx)) && File.ReadAllText(MarkerPath(ctx)) != ProcessMarker,
                "the writer did not run in an earlier game process of this chain");
        }

        [AfterScenario("@joyrescue-restart-last")]
        public void RestoreSettings(PickleContext ctx)
        {
            foreach (var name in Saves.ToList())
            {
                var path = GenFilePaths.FilePathForSavedGame(name);
                if (File.Exists(path)) File.Delete(path);
                Saves.Remove(name);
            }
            var backup = BackupPath(ctx);
            if (File.Exists(backup))
            {
                File.Copy(backup, SettingsPath(ctx), true);
                File.Delete(backup);
            }
            if (File.Exists(MarkerPath(ctx))) File.Delete(MarkerPath(ctx));
        }

        // ---------------------------------------------------------------- a saved game across processes

        [When("Joy Rescue: the game is saved as {string}")]
        public void SaveGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            ctx.Require(!File.Exists(path), $"a saved game called '{name}' remains from an earlier chain: {path}");
            GameDataSaveLoader.SaveGame(name);
            ctx.Assert(File.Exists(path), $"saving '{name}' wrote no file: the scribe error is in the log");
            Saves.Add(name);
        }

        // The saved game is opened the way the game opens one, with Pickle's dialog suppression on so that a
        // save made with another mod list does not stop at a confirmation nobody answers.
        [When("Joy Rescue: the saved game {string} is loaded")]
        public async Task LoadSavedGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            ctx.Require(File.Exists(path), $"no saved game called '{name}' at {path}: the writer of this chain did not leave it");
            Saves.Add(name);
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

        [When("Joy Rescue: the saved game {string} is deleted")]
        public void DeleteSavedGame(PickleContext ctx, string name)
        {
            var path = GenFilePaths.FilePathForSavedGame(name);
            if (File.Exists(path)) File.Delete(path);
            Saves.Remove(name);
        }

        // ---------------------------------------------------------------- tolerances

        [Given("Joy Rescue: {string} has the tolerance {float} for the recreation type {string}")]
        public void SetTolerance(PickleContext ctx, string name, float value, string kindName)
        {
            var pawn = PawnNamed(ctx, name);
            var kind = Kind(ctx, kindName);
            var field = typeof(JoyToleranceSet).GetField("tolerances", InstanceAny);
            ctx.Require(field != null, "JoyToleranceSet.tolerances is unavailable");
            var map = (DefMap<JoyKindDef, float>)field.GetValue(pawn.needs.joy.tolerances);
            map[kind] = value;
        }

        [Then("Joy Rescue: the tolerance of {string} for the recreation type {string} is {float}")]
        public void AssertTolerance(PickleContext ctx, string name, string kindName, float expected)
        {
            var pawn = PawnNamed(ctx, name);
            var actual = pawn.needs.joy.tolerances[Kind(ctx, kindName)];
            ctx.Assert(Math.Abs(actual - expected) < 0.005f,
                $"{name}'s tolerance for {kindName} is {actual:0.0000}, expected {expected:0.0000}");
        }

        // ---------------------------------------------------------------- custom types and reassignments in the settings

        [When("Joy Rescue: a custom recreation type is added")]
        public void AddCustom(PickleContext ctx)
        {
            // The same two lines the Add button runs.
            var settings = JoyRescueMod.Settings;
            var id = settings.nextCustomKindId++;
            settings.customKinds.Add(new CustomJoyKind(id.ToString(), "JoyRescue.Settings.NewKindLabel".Translate(id).Resolve()));
        }

        [When("Joy Rescue: the custom recreation type number {int} is renamed {string}")]
        public void RenameCustom(PickleContext ctx, int number, string label)
        {
            Custom(ctx, number).label = label;
        }

        [Then("Joy Rescue: there are {int} custom recreation types in the settings")]
        public void AssertCustomCount(PickleContext ctx, int count)
        {
            ctx.Assert(JoyRescueMod.Settings.customKinds.Count == count,
                $"the settings hold {JoyRescueMod.Settings.customKinds.Count} custom types: "
                + string.Join(", ", JoyRescueMod.Settings.customKinds.Select(c => c.DefName + " \"" + c.label + "\"")));
        }

        [Then("Joy Rescue: the custom recreation type number {int} is named {string}")]
        public void AssertCustomName(PickleContext ctx, int number, string label)
        {
            var actual = Custom(ctx, number).label;
            ctx.Assert(actual == label, $"custom type {number} is named \"{actual}\", expected \"{label}\"");
        }

        // The initial name is the translation of a key, resolved once when the type is created and saved as
        // text afterwards: it must equal what the game translates NOW for that id, in the language it runs in.
        [Then("Joy Rescue: the custom recreation type number {int} still has the name the game translates for a new type")]
        public void AssertCustomInitialName(PickleContext ctx, int number)
        {
            var custom = Custom(ctx, number);
            var expected = "JoyRescue.Settings.NewKindLabel".Translate(int.Parse(custom.id)).Resolve();
            ctx.Assert(custom.label == expected, $"custom type {number} is named \"{custom.label}\", the translation gives \"{expected}\"");
            ctx.Assert(!custom.label.Contains("JoyRescue.Settings"), $"custom type {number} shows a raw key: \"{custom.label}\"");
        }

        [Then("Joy Rescue: the custom recreation type number {int} exists in the game as a recreation type")]
        public void AssertCustomExists(PickleContext ctx, int number)
        {
            var custom = Custom(ctx, number);
            var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName);
            ctx.Assert(kind != null, $"{custom.DefName} was not created by this startup");
            ctx.Assert(kind.label == custom.label, $"{custom.DefName} is labelled \"{kind.label}\", the settings say \"{custom.label}\"");
        }

        [Then("Joy Rescue: the custom recreation type number {int} is still waiting for a restart")]
        public void AssertCustomPending(PickleContext ctx, int number)
        {
            var custom = Custom(ctx, number);
            ctx.Assert(DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName) == null,
                $"{custom.DefName} already exists in this game, so it is not pending");
        }

        [Then("Joy Rescue: the recreation type {string} does not exist")]
        public void AssertNoKind(PickleContext ctx, string defName)
        {
            ctx.Assert(DefDatabase<JoyKindDef>.GetNamedSilentFail(defName) == null, $"the recreation type {defName} exists");
        }

        [When("Joy Rescue: the building {string} is reassigned to the custom recreation type number {int}")]
        public void ReassignBuilding(PickleContext ctx, string building, int number)
        {
            ctx.Require(DefDatabase<ThingDef>.GetNamedSilentFail(building) != null, $"no building '{building}'");
            JoyRescueMod.Settings.kindOverrides[building] = Custom(ctx, number).DefName;
        }

        [When("Joy Rescue: the activity {string} is reassigned to the custom recreation type number {int}")]
        public void ReassignActivity(PickleContext ctx, string giver, int number)
        {
            ctx.Require(DefDatabase<JoyGiverDef>.GetNamedSilentFail(giver) != null, $"no activity '{giver}'");
            JoyRescueMod.Settings.giverKindOverrides[giver] = Custom(ctx, number).DefName;
        }

        [Then("Joy Rescue: the settings hold {int} building reassignments and {int} activity reassignments")]
        public void AssertReassignmentCounts(PickleContext ctx, int buildings, int activities)
        {
            var s = JoyRescueMod.Settings;
            ctx.Assert(s.kindOverrides.Count == buildings && s.giverKindOverrides.Count == activities,
                $"the settings hold {s.kindOverrides.Count} building and {s.giverKindOverrides.Count} activity reassignments ("
                + string.Join(", ", s.kindOverrides.Select(p => p.Key + "->" + p.Value)
                    .Concat(s.giverKindOverrides.Select(p => p.Key + "->" + p.Value))) + ")");
        }

        [Then("Joy Rescue: no reassignment in the settings points at the custom recreation type {string}")]
        public void AssertNoReassignmentTo(PickleContext ctx, string defName)
        {
            var s = JoyRescueMod.Settings;
            var pointing = s.kindOverrides.Where(p => p.Value == defName).Select(p => p.Key)
                .Concat(s.giverKindOverrides.Where(p => p.Value == defName).Select(p => p.Key)).ToList();
            ctx.Assert(pointing.Count == 0, $"still pointing at {defName}: " + string.Join(", ", pointing));
        }

        [Then("Joy Rescue: the activity {string} serves the recreation type {string}")]
        public void AssertActivityKind(PickleContext ctx, string giverName, string kindName)
        {
            var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(giverName);
            ctx.Assert(giver != null, $"no activity '{giverName}'");
            ctx.Assert(giver.joyKind?.defName == kindName, $"the activity {giverName} is on {giver.joyKind?.defName}, expected {kindName}");
            ctx.Assert(giver.jobDef != null && giver.jobDef.joyKind?.defName == kindName,
                $"the job of {giverName} credits {giver.jobDef?.joyKind?.defName}, expected {kindName}");
            foreach (var thing in giver.thingDefs ?? new System.Collections.Generic.List<ThingDef>())
                ctx.Assert(thing.building?.joyKind?.defName == kindName,
                    $"the building {thing.defName} of the activity {giverName} is on {thing.building?.joyKind?.defName}, expected {kindName}");
        }

        [Then("Joy Rescue: the building {string} is on the recreation type {string}")]
        public void AssertBuildingKind(PickleContext ctx, string buildingName, string kindName)
        {
            var building = DefDatabase<ThingDef>.GetNamedSilentFail(buildingName);
            ctx.Assert(building?.building != null, $"no building '{buildingName}'");
            ctx.Assert(building.building.joyKind?.defName == kindName,
                $"{buildingName} is on {building.building.joyKind?.defName}, expected {kindName}");
        }

        // The other settings a persistence check reads back, all in one place.
        [When("Joy Rescue: the settings are given non-default values")]
        public void GiveNonDefaults(PickleContext ctx)
        {
            var s = JoyRescueMod.Settings;
            s.requireChairForWatching = false;
            s.kindSortMode = 2;
            s.listView = ListLayout.BuildingsFirst;
        }

        [Then("Joy Rescue: the settings hold the non-default values")]
        public void AssertNonDefaults(PickleContext ctx)
        {
            var s = JoyRescueMod.Settings;
            ctx.Assert(!s.requireChairForWatching, "requireChairForWatching went back to true");
            ctx.Assert(s.kindSortMode == 2, $"kindSortMode is {s.kindSortMode}, expected 2");
            ctx.Assert(s.listView == ListLayout.BuildingsFirst, $"listView is {s.listView}, expected {ListLayout.BuildingsFirst}");
        }

        // Dialog_ModSettings pauses the ticks, so a click on one of its buttons is taken on a frame.
        [When("Joy Rescue: the window is given {int} frames")]
        public async Task GiveFrames(PickleContext ctx, int frames)
        {
            await ctx.WaitFrames(frames);
        }

        // What the Remove button of a type row does, with the mod's own clean-up of the reassignments that
        // pointed at the type. Used when two rows are on screen and a click could not tell their buttons apart.
        [When("Joy Rescue: the custom recreation type number {int} is deleted")]
        public void DeleteCustom(PickleContext ctx, int number)
        {
            var custom = Custom(ctx, number);
            var purge = typeof(JoyRescueMod).GetMethod("PurgeOverridesTargeting", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ctx.Require(purge != null, "JoyRescueMod.PurgeOverridesTargeting is unavailable");
            purge.Invoke(null, new object[] { custom.DefName });
            JoyRescueMod.Settings.customKinds.Remove(custom);
        }

        // A language file that misses a key does not fail: the game falls back to English, or shows the key. So
        // every key of the mod's English file is looked up in the language the game runs in.
        [Then("Joy Rescue: every text of the mod exists in the language the game runs in")]
        public void AssertEveryTextTranslated(PickleContext ctx)
        {
            var active = LanguageDatabase.activeLanguage;
            var keys = LanguageDatabase.defaultLanguage.keyedReplacements.Keys.Where(k => k.StartsWith("JoyRescue.")).ToList();
            ctx.Assert(keys.Count > 0, "the default language holds no JoyRescue.* text");
            var missing = keys.Where(k => !active.HaveTextForKey(k)).ToList();
            ctx.Assert(missing.Count == 0, $"{missing.Count} of {keys.Count} texts are missing in {active.folderName}: "
                + string.Join(", ", missing.Take(10)));
            var raw = keys.Where(k => k.Translate().ToString() == k).ToList();
            ctx.Assert(raw.Count == 0, $"{raw.Count} texts show their own key in {active.folderName}: " + string.Join(", ", raw.Take(10)));
        }

        [Then("Joy Rescue: the settings hold their default values and no reassignment")]
        public void AssertDefaults(PickleContext ctx)
        {
            var s = JoyRescueMod.Settings;
            ctx.Assert(s.requireChairForWatching, "requireChairForWatching is off");
            ctx.Assert(!s.rescueModsWithOwnCode && !s.commonTaxonomy, "an option that is off by default is on");
            ctx.Assert(s.kindSortMode == 0 && s.listView == ListLayout.ActivitiesFirst,
                $"sort {s.kindSortMode}, arrangement {s.listView}: not the defaults");
            ctx.Assert(s.enabledOverrides.Count == 0 && s.modeOverrides.Count == 0
                       && s.kindOverrides.Count == 0 && s.giverKindOverrides.Count == 0 && s.disabledKinds.Count == 0,
                $"overrides remain: enabled {s.enabledOverrides.Count}, modes {s.modeOverrides.Count}, "
                + $"buildings {s.kindOverrides.Count}, activities {s.giverKindOverrides.Count}, disabled types {s.disabledKinds.Count}");
        }
    }
}
