using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JoyRescue;
using RimWorld;
using Verse;
using Verse.AI;

internal static partial class Program
{
    private static readonly BindingFlags StaticFields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Source", "JoyRescue.csproj"))) dir = dir.Parent;
        return dir?.FullName ?? throw new Exception("Repository not found");
    }
    private static T Blank<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    private static void SetStatic(Type type, string field, object value) => type.GetField(field, StaticFields).SetValue(null, value);
    private static object Generator(string name, params object[] args) => typeof(JoyRescueGenerator)
        .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
    private static void LoadTestLanguage(string name)
    {
        // Supply actual distributed resources, without creating Unity language icons or loading mods.
        var lang = Blank<LoadedLanguage>();
        lang.folderName = name;
        lang.info = new LanguageInfo();
        lang.keyedReplacements = new Dictionary<string, LoadedLanguage.KeyedReplacement>();
        foreach (var file in Directory.GetFiles(Path.Combine(RepoRoot(), "Mod", "Languages", name, "Keyed"), "*.xml"))
            foreach (var node in XDocument.Load(file).Root.Elements())
                lang.keyedReplacements.Add(node.Name.LocalName, new LoadedLanguage.KeyedReplacement
                { key = node.Name.LocalName, value = node.Value.Replace("\\n", "\n") });
        typeof(LoadedLanguage).GetField("dataIsLoaded", InstanceFields).SetValue(lang, true);
        LanguageDatabase.activeLanguage = lang;
        LanguageDatabase.defaultLanguage = lang;
        // Date/currency/colonist decoration is unrelated to these plain job reports.
        SetStatic(typeof(ColoredText), "DateTimeRegexes", new List<System.Text.RegularExpressions.Regex>());
        SetStatic(typeof(ColoredText), "ColonistCountRegex", new System.Text.RegularExpressions.Regex("(?!)"));
        ColoredText.ClearCache();
    }
    private sealed class SettingsFixture : IDisposable
    {
        private readonly JoyRescueSettings oldSettings = JoyRescueMod.Settings;
        private readonly LoadedLanguage oldLanguage = LanguageDatabase.activeLanguage;
        private readonly LoadedLanguage oldDefault = LanguageDatabase.defaultLanguage;
        private readonly bool oldProfiler = DeepProfiler.enabled;
        private readonly bool oldModsChanged = ScribeMetaHeaderUtility.modListChanged;
        private readonly int oldMajor = ScribeMetaHeaderUtility.loadedGameVersionMajor;
        private readonly int oldMinor = ScribeMetaHeaderUtility.loadedGameVersionMinor;
        private readonly object oldDateRegexes = typeof(ColoredText).GetField("DateTimeRegexes", StaticFields).GetValue(null);
        private readonly object oldColonistRegex = typeof(ColoredText).GetField("ColonistCountRegex", StaticFields).GetValue(null);
        private readonly bool oldBinding = (bool)typeof(DefOfHelper).GetField("bindingNow", StaticFields).GetValue(null);
        private PawnCapacityDef oldSight, oldManipulation;
        private bool capacitiesInitialized;
        private object typeKey, oldCachedType;
        private System.Collections.IDictionary typeCache;
        private bool hadCachedType;
        public readonly JoyRescueSettings Settings = new JoyRescueSettings();
        public readonly JoyKindDef Kind = new JoyKindDef { defName = "TestKind", label = "test kind" };
        public SettingsFixture(string language = "English")
        {
            if (DefDatabase<ThingDef>.DefCount != 0 || DefDatabase<JobDef>.DefCount != 0
                || DefDatabase<JoyGiverDef>.DefCount != 0 || DefDatabase<JoyKindDef>.DefCount != 0)
                throw new Exception("Integration fixtures require empty definition databases");
            try
            {
                DeepProfiler.enabled = false;
                ScribeMetaHeaderUtility.modListChanged = true;
                ScribeMetaHeaderUtility.loadedGameVersionMajor = 1;
                ScribeMetaHeaderUtility.loadedGameVersionMinor = 6;
                // A running mod normally supplies this type to Verse discovery.
                var keyType = typeof(GenTypes).GetNestedType("TypeCacheKey", BindingFlags.NonPublic);
                var key = Activator.CreateInstance(keyType, new object[] { typeof(CustomJoyKind).FullName, null });
                var cache = (System.Collections.IDictionary)typeof(GenTypes).GetField("typeCache", StaticFields).GetValue(null);
                typeKey = key; typeCache = cache; hadCachedType = cache.Contains(key); oldCachedType = cache[key];
                cache[key] = typeof(CustomJoyKind);
                SetStatic(typeof(JoyRescueMod), "<Settings>k__BackingField", Settings);
                LoadTestLanguage(language);
                // DefOf binding normally happens during game startup. Provide only the two capabilities
                // these data-only tests use; do not invoke the game-wide binding/Unity initialization.
                SetStatic(typeof(DefOfHelper), "bindingNow", true);
                oldSight = PawnCapacityDefOf.Sight; oldManipulation = PawnCapacityDefOf.Manipulation;
                capacitiesInitialized = true;
                PawnCapacityDefOf.Sight = new PawnCapacityDef { defName = "Sight" };
                PawnCapacityDefOf.Manipulation = new PawnCapacityDef { defName = "Manipulation" };
                SetStatic(typeof(DefOfHelper), "bindingNow", oldBinding);
                DefDatabase<JoyKindDef>.Add(Kind);
            }
            catch { Dispose(); throw; }
        }
        public RescueEntry Repair(string name = "TestBuilding", bool ownCode = false)
        {
            var entry = Entry(ownCode, name);
            entry.building.label = "test table";
            entry.building.category = ThingCategory.Building;
            entry.building.building.joyKind = Kind;
            entry.joyKind = Kind;
            entry.job = new JobDef { defName = "TestJob_" + name, joyKind = Kind };
            entry.giver = new JoyGiverDef { defName = "TestGiver_" + name, jobDef = entry.job,
                joyKind = Kind, thingDefs = new List<ThingDef> { entry.building }, requireChair = false };
            DefDatabase<ThingDef>.Add(entry.building);
            DefDatabase<JobDef>.Add(entry.job);
            DefDatabase<JoyGiverDef>.Add(entry.giver);
            JoyRescueGenerator.Entries.Add(entry);
            JoyRescueGenerator.AllEntries.Add(entry);
            return entry;
        }
        public JoyGiverDef External(float weight, string name)
        {
            var giver = new JoyGiverDef { defName = name, joyKind = Kind, baseChance = weight,
                modContentPack = Blank<ModContentPack>() };
            DefDatabase<JoyGiverDef>.Add(giver);
            return giver;
        }
        public void Dispose()
        {
            Scribe.ForceStop();
            DefDatabase<ThingDef>.Clear(); DefDatabase<JobDef>.Clear();
            DefDatabase<JoyGiverDef>.Clear(); DefDatabase<JoyKindDef>.Clear();
            JoyRescueGenerator.Entries.Clear(); JoyRescueGenerator.AllEntries.Clear();
            JoyRescueGenerator.OriginalChances.Clear();
            JoyRescueGenerator.HasRun = false;
            JoyRescueGenerator.JoyBuildingsSeen = JoyRescueGenerator.AlreadyCovered = 0;
            SetStatic(typeof(JoyRescueMod), "<Settings>k__BackingField", oldSettings);
            LanguageDatabase.activeLanguage = oldLanguage;
            LanguageDatabase.defaultLanguage = oldDefault;
            DeepProfiler.enabled = oldProfiler;
            ScribeMetaHeaderUtility.modListChanged = oldModsChanged;
            ScribeMetaHeaderUtility.loadedGameVersionMajor = oldMajor;
            ScribeMetaHeaderUtility.loadedGameVersionMinor = oldMinor;
            SetStatic(typeof(ColoredText), "DateTimeRegexes", oldDateRegexes);
            SetStatic(typeof(ColoredText), "ColonistCountRegex", oldColonistRegex);
            ColoredText.ClearCache();
            SetStatic(typeof(DefOfHelper), "bindingNow", oldBinding);
            if (capacitiesInitialized) { PawnCapacityDefOf.Sight = oldSight; PawnCapacityDefOf.Manipulation = oldManipulation; }
            if (typeCache != null) { if (hadCachedType) typeCache[typeKey] = oldCachedType; else typeCache.Remove(typeKey); }
        }
    }
    private static void RegisterSettingsIntegrationTests()
    {
        foreach (var language in new[] { "English", "French" })
        foreach (var from in new[] { RescueMode.InteractionCell, RescueMode.SitAdjacent, RescueMode.Watch })
        foreach (var to in new[] { RescueMode.InteractionCell, RescueMode.SitAdjacent, RescueMode.Watch })
            Test($"U17-U21 Retarget {language} {from} -> {to}", () =>
            {
                using var f = new SettingsFixture(language);
                var e = f.Repair();
                JoyRescueGenerator.Retarget(e, from);
                var job = e.job; var giver = e.giver;
                var worker = typeof(JoyGiverDef).GetField("workerInt", InstanceFields);
                worker.SetValue(giver, new JoyGiver_WatchBuilding());
                JoyRescueGenerator.Retarget(e, to);
                Equal(job, e.job); Equal(giver, e.giver); Equal(null, worker.GetValue(giver));
                Equal(to, e.resolvedMode);
                bool watch = to == RescueMode.Watch;
                Equal(watch ? typeof(JoyGiver_WatchBuilding) : to == RescueMode.InteractionCell
                    ? typeof(JoyGiver_InteractBuildingInteractionCell) : typeof(JoyGiver_InteractBuildingSitAdjacent), giver.giverClass);
                Equal(to == RescueMode.SitAdjacent ? typeof(JobDriver_SitFacingBuilding) : typeof(JobDriver_WatchBuilding), job.driverClass);
                Equal(watch ? 8 : to == RescueMode.InteractionCell ? 1 : 2, job.joyMaxParticipants);
                Equal(watch, giver.canDoWhileInBed); Equal(watch, giver.desireSit); Equal(false, giver.requireChair);
                Equal(watch ? 1 : 2, giver.requiredCapacities.Count);
                Equal(PawnCapacityDefOf.Sight, giver.requiredCapacities[0]);
                if (!watch) Equal(PawnCapacityDefOf.Manipulation, giver.requiredCapacities[1]);
                string verb = language == "French" ? (watch ? "regarde" : to == RescueMode.InteractionCell ? "utilise" : "joue à")
                    : (watch ? "watching" : to == RescueMode.InteractionCell ? "using" : "playing");
                Equal(verb + " test table.", job.reportString);
            });
        Test("U19 D14 D44 live weights, mode while disabled and chair toggling", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair();
            JoyRescueGenerator.ApplySettings(); Equal(2f, e.giver.baseChance);
            f.Settings.SetEnabled(e, false); f.Settings.SetMode(e, RescueMode.Watch);
            JoyRescueGenerator.ApplySettings(); Equal(0f, e.giver.baseChance); Equal(RescueMode.Watch, e.resolvedMode);
            Equal(true, e.giver.desireSit);
            f.Settings.requireChairForWatching = false;
            JoyRescueGenerator.ApplySettings(); Equal(false, e.giver.desireSit); Equal(0f, e.giver.baseChance);
            f.Settings.SetEnabled(e, true); JoyRescueGenerator.ApplySettings(); Equal(2f, e.giver.baseChance);
            Equal(1, DefDatabase<JobDef>.DefCount); Equal(1, DefDatabase<JoyGiverDef>.DefCount);
        });
        Test("D15-D19 D45 type suppression restores external weights and respects individual choice", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair();
            var a = f.External(3, "External3"); var b = f.External(4, "External4"); var zero = f.External(0, "External0");
            f.Settings.SetEnabled(e, false); f.Settings.disabledKinds.Add(f.Kind.defName);
            JoyRescueGenerator.ApplySettings(); JoyRescueGenerator.ApplySettings();
            Equal(0f, a.baseChance); Equal(0f, b.baseChance); Equal(0f, e.giver.baseChance);
            var added = f.External(5, "Added"); JoyRescueGenerator.ApplySettings(); Equal(0f, added.baseChance);
            f.Settings.SetEnabled(e, true); JoyRescueGenerator.ApplySettings(); Equal(0f, e.giver.baseChance);
            f.Settings.SetEnabled(e, false); f.Settings.disabledKinds.Clear(); JoyRescueGenerator.ApplySettings();
            Equal(3f, a.baseChance); Equal(4f, b.baseChance); Equal(0f, zero.baseChance); Equal(5f, added.baseChance);
            Equal(0f, e.giver.baseChance);
        });
        foreach (var mode in new[] { RescueMode.InteractionCell, RescueMode.SitAdjacent, RescueMode.Watch })
            Test($"D02 D07 Generate orphan {mode}", () =>
            {
                using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
                var td = Building("Orphan"); td.label = "test table"; td.category = ThingCategory.Building;
                td.thingClass = typeof(Verse.Building); td.building.joyKind = f.Kind;
                td.hasInteractionCell = mode == RescueMode.InteractionCell;
                f.Settings.modeOverrides[td.defName] = mode.ToString();
                DefDatabase<ThingDef>.Add(td);
                JoyRescueGenerator.Generate();
                var e = JoyRescueGenerator.Entries.Single();
                Equal(true, JoyRescueGenerator.HasRun); Equal(1, JoyRescueGenerator.JoyBuildingsSeen);
                Equal(0, JoyRescueGenerator.AlreadyCovered); Equal(mode, e.resolvedMode);
                Equal(f.Kind, e.job.joyKind); Equal(f.Kind, e.giver.joyKind); Equal(td, e.giver.thingDefs.Single());
                Equal(4000, e.job.joyDuration); Equal(2f, e.giver.baseChance);
                Equal("JoyRescue_Orphan", e.job.defName); Equal("JoyRescue_Giver_Orphan", e.giver.defName);
            });
        Test("D23 D24 D30 custom type and specific building assignment at generation", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            var e = f.Repair(); var second = Building("Second"); second.category = ThingCategory.Building;
            second.thingClass = typeof(Verse.Building); second.building.joyKind = f.Kind;
            DefDatabase<ThingDef>.Add(second); e.giver.thingDefs.Add(second);
            f.Settings.customKinds.Add(new CustomJoyKind("10", "Whole activity"));
            f.Settings.customKinds.Add(new CustomJoyKind("11", "Specific building"));
            f.Settings.giverKindOverrides[e.giver.defName] = "JoyRescue_Kind_10";
            f.Settings.kindOverrides[e.Key] = "JoyRescue_Kind_11";
            Equal(true, Editor<bool>("PendingRestart"));
            JoyRescueGenerator.Generate();
            Equal("JoyRescue_Kind_10", e.giver.joyKind.defName);
            Equal(e.giver.joyKind, e.job.joyKind); Equal(e.giver.joyKind, second.building.joyKind);
            Equal(second, e.giver.thingDefs.Single());
            var repair = JoyRescueGenerator.Entries.Single();
            Equal("JoyRescue_Kind_11", repair.job.joyKind.defName);
            Equal(repair.job.joyKind, repair.building.building.joyKind);
            Equal(false, Editor<bool>("PendingRestart"));
        });
        Test("R02 shared activity job reassignment preserves the other activity", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            var a = f.Repair("A"); var b = f.Repair("B"); b.giver.jobDef = a.job;
            var target = new JoyKindDef { defName = "Target" }; DefDatabase<JoyKindDef>.Add(target);
            f.Settings.giverKindOverrides[a.giver.defName] = target.defName;
            Generator("ApplyGiverKindOverrides");
            Equal(target, a.giver.joyKind); Equal(target, a.giver.jobDef.joyKind);
            Equal(target, a.building.building.joyKind);
            Equal(f.Kind, b.giver.joyKind); Equal(f.Kind, b.giver.jobDef.joyKind);
            Equal(f.Kind, b.building.building.joyKind);
        });
        Test("R03 generation replay preserves repair tracking and definition identities", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            var td = Building("Replay"); td.label = "test table"; td.category = ThingCategory.Building;
            td.thingClass = typeof(Verse.Building); td.building.joyKind = f.Kind; DefDatabase<ThingDef>.Add(td);
            JoyRescueGenerator.Generate(); var first = JoyRescueGenerator.Entries.Single();
            JoyRescueGenerator.Generate(); var second = JoyRescueGenerator.Entries.Single();
            Equal(first.job, second.job); Equal(first.giver, second.giver);
            f.Settings.SetEnabled(second, false); JoyRescueGenerator.ApplySettings(); Equal(0f, second.giver.baseChance);
            Equal(1, DefDatabase<JobDef>.DefCount); Equal(1, DefDatabase<JoyGiverDef>.DefCount);
        });
        Test("R04 usable kinds follow effective giver weights", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair();
            JoyRescueGenerator.ApplySettings(); Equal(1, Editor<int>("UsableKindCount"));
            e.covered = true;
            f.Settings.disabledKinds.Add(f.Kind.defName); JoyRescueGenerator.ApplySettings();
            Equal(0, Editor<int>("UsableKindCount"));
            f.Settings.disabledKinds.Clear(); JoyRescueGenerator.ApplySettings(); Equal(1, Editor<int>("UsableKindCount"));
        });
        Test("D40 reassignment detaches every duplicate building occurrence", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair(); e.giver.thingDefs.Add(e.building);
            var target = new JoyKindDef { defName = "Target" }; DefDatabase<JoyKindDef>.Add(target);
            f.Settings.kindOverrides[e.Key] = target.defName; Generator("ApplyKindOverrides");
            Equal(0, e.giver.thingDefs.Count); Equal(target, e.building.building.joyKind);
        });
        Test("U22 U23 U24 editor pending names choices and reference purge", () =>
        {
            using var f = new SettingsFixture();
            f.Settings.customKinds.Add(new CustomJoyKind("10", "New custom"));
            f.Settings.kindOverrides["A"] = "JoyRescue_Kind_10";
            f.Settings.giverKindOverrides["G"] = "JoyRescue_Kind_10";
            f.Settings.kindOverrides["Keep"] = f.Kind.defName;
            Equal("New custom", Editor<string>("KindNameByDefName", "JoyRescue_Kind_10"));
            var choices = Editor<List<(string, string, bool)>>("KindChoices");
            Equal(2, choices.Count); Equal(true, choices.Single(c => c.Item1 == "JoyRescue_Kind_10").Item3);
            Equal(2, Editor<int>("PurgeOverridesTargeting", "JoyRescue_Kind_10"));
            Equal(f.Kind.defName, f.Settings.kindOverrides["Keep"]); Equal(0, f.Settings.giverKindOverrides.Count);
        });
        foreach (bool reverse in new[] { false, true })
            Test($"D39 conflicting shared-job assignments order={reverse}", () =>
            {
                using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
                var a = f.Repair("A"); var b = f.Repair("B"); b.giver.jobDef = a.job;
                var x = new JoyKindDef { defName = "X" }; var y = new JoyKindDef { defName = "Y" };
                DefDatabase<JoyKindDef>.Add(x); DefDatabase<JoyKindDef>.Add(y);
                if (reverse) { f.Settings.giverKindOverrides[b.giver.defName] = "Y"; f.Settings.giverKindOverrides[a.giver.defName] = "X"; }
                else { f.Settings.giverKindOverrides[a.giver.defName] = "X"; f.Settings.giverKindOverrides[b.giver.defName] = "Y"; }
                Generator("ApplyGiverKindOverrides");
                Equal(x, a.giver.joyKind); Equal(x, a.giver.jobDef.joyKind); Equal(x, a.building.building.joyKind);
                Equal(y, b.giver.joyKind); Equal(y, b.giver.jobDef.joyKind); Equal(y, b.building.building.joyKind);
            });
        Test("R03 replay while kind disabled preserves external restoration weights", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            var external = f.External(3, "External");
            f.Settings.disabledKinds.Add(f.Kind.defName);
            JoyRescueGenerator.Generate(); JoyRescueGenerator.Generate();
            Equal(0f, external.baseChance);
            f.Settings.disabledKinds.Clear(); JoyRescueGenerator.ApplySettings(); Equal(3f, external.baseChance);
        });
        Test("R03 newly supplied coverage supersedes an old repair across repeated scans", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            var td = Building("Replay"); td.label = "test table"; td.category = ThingCategory.Building;
            td.thingClass = typeof(Verse.Building); td.building.joyKind = f.Kind; DefDatabase<ThingDef>.Add(td);
            JoyRescueGenerator.Generate(); var first = JoyRescueGenerator.Entries.Single();
            var external = f.External(3, "External"); external.thingDefs = new List<ThingDef> { td };
            JoyRescueGenerator.Generate(); JoyRescueGenerator.Generate();
            Equal(0, JoyRescueGenerator.Entries.Count); Equal(1, JoyRescueGenerator.AlreadyCovered);
            Equal(0f, first.giver.baseChance); Equal(3f, external.baseChance);
        });
        Test("D41 failed replay clears HasRun instead of retaining success", () =>
        {
            using var f = new SettingsFixture(); using var quiet = Log.LockMessages();
            JoyRescueGenerator.Generate(); Equal(true, JoyRescueGenerator.HasRun);
            f.Settings.customKinds = null;
            bool threw = false;
            try { JoyRescueGenerator.Generate(); } catch (NullReferenceException) { threw = true; }
            Equal(true, threw); Equal(false, JoyRescueGenerator.HasRun);
        });
        Test("U36 editor invalidation includes tooltip cache", () =>
        {
            var editor = Blank<JoyRescueMod>();
            typeof(JoyRescueMod).GetField("tipCache", InstanceFields).SetValue(editor,
                new Dictionary<JoyKindDef, string> { [new JoyKindDef()] = "stale" });
            typeof(JoyRescueMod).GetField("tipsCounted", InstanceFields).SetValue(editor, 1);
            typeof(JoyRescueMod).GetMethod("InvalidateCaches", InstanceFields).Invoke(editor, null);
            Equal(null, typeof(JoyRescueMod).GetField("tipCache", InstanceFields).GetValue(editor));
            Equal(-1, (int)typeof(JoyRescueMod).GetField("tipsCounted", InstanceFields).GetValue(editor));
        });
        Test("U37 SetAll applies weights and retains disabled-kind precedence", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A"); var b = f.Repair("B", true);
            var editor = Blank<JoyRescueMod>(); var setAll = typeof(JoyRescueMod).GetMethod("SetAll", InstanceFields);
            setAll.Invoke(editor, new object[] { false }); Equal(0f, a.giver.baseChance); Equal(0f, b.giver.baseChance);
            setAll.Invoke(editor, new object[] { true }); Equal(2f, a.giver.baseChance); Equal(2f, b.giver.baseChance);
            f.Settings.disabledKinds.Add(f.Kind.defName);
            setAll.Invoke(editor, new object[] { true }); Equal(0f, a.giver.baseChance); Equal(0f, b.giver.baseChance);
        });
        Test("Assembly carries Publicizer runtime private-access contract", () =>
            Equal(true, typeof(JoyRescueMod).Assembly.GetCustomAttributesData().Any(a =>
                a.AttributeType.FullName == "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute"
                && (string)a.ConstructorArguments[0].Value == "Assembly-CSharp")));
        Test("Global own-code option changes actual weights but preserves explicit off", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A", true); var b = f.Repair("B", true);
            f.Settings.enabledOverrides[b.Key] = false;
            JoyRescueGenerator.ApplySettings(); Equal(0f, a.giver.baseChance); Equal(0f, b.giver.baseChance);
            f.Settings.rescueModsWithOwnCode = true; JoyRescueGenerator.ApplySettings();
            Equal(2f, a.giver.baseChance); Equal(0f, b.giver.baseChance);
            f.Settings.rescueModsWithOwnCode = false; JoyRescueGenerator.ApplySettings(); Equal(0f, a.giver.baseChance);
        });
        Test("U28-U30 sort modes group and order actual definition data", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A"); var b = f.Repair("B");
            a.building.label = "Zulu"; b.building.label = "Alpha"; b.covered = true;
            var lonely = new JoyKindDef { defName = "Lonely", label = "alone" }; DefDatabase<JoyKindDef>.Add(lonely);
            JoyRescueGenerator.ApplySettings();
            var editor = Blank<JoyRescueMod>();
            var grouped = (Dictionary<JoyKindDef, List<RescueEntry>>)typeof(JoyRescueMod)
                .GetMethod("EntriesByKind", InstanceFields).Invoke(editor, null);
            Equal(a, grouped[f.Kind][0]); Equal(b, grouped[f.Kind][1]);
            var sorted = typeof(JoyRescueMod).GetMethod("SortedKinds", InstanceFields);
            var invalidate = typeof(JoyRescueMod).GetMethod("InvalidateCaches", InstanceFields);
            foreach (int mode in new[] { 0, 1, 2 })
            {
                f.Settings.kindSortMode = mode; invalidate.Invoke(editor, null);
                var kinds = (List<JoyKindDef>)sorted.Invoke(editor, null);
                Equal(mode == 1 ? f.Kind : lonely, kinds[0]);
            }
        });
        Test("U32 counts ignore zero and negative weights for activity availability", () =>
        {
            using var f = new SettingsFixture(); f.External(3, "Active"); f.External(0, "Off"); f.External(-1, "Negative");
            Equal(1, Editor<int>("ActiveGiverCount", f.Kind)); Equal(3, Editor<int>("TotalGiverCount", f.Kind));
        });
        Test("D20 incomplete entries and unknown disabled kinds are harmless", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair();
            JoyRescueGenerator.Entries.Add(Entry(false, "Incomplete"));
            f.Settings.disabledKinds.Add("Missing");
            var withoutKind = f.External(3, "WithoutKind"); withoutKind.joyKind = null;
            JoyRescueGenerator.ApplySettings(); Equal(2f, e.giver.baseChance); Equal(3f, withoutKind.baseChance);
        });
        Test("D31 actual Scribe settings round trip", () =>
        {
            using var f = new SettingsFixture();
            var path = ScratchFile("roundtrip.xml");
            f.Settings.rescueModsWithOwnCode = true; f.Settings.requireChairForWatching = false;
            f.Settings.enabledOverrides["A"] = false; f.Settings.modeOverrides["A"] = "Watch";
            f.Settings.customKinds.Add(new CustomJoyKind("7", "Échecs <custom> & music") { needsThing = false });
            f.Settings.kindOverrides["A"] = "JoyRescue_Kind_7"; f.Settings.giverKindOverrides["G"] = "JoyRescue_Kind_7";
            f.Settings.disabledKinds.Add("Disabled"); f.Settings.kindSortMode = 2; f.Settings.nextCustomKindId = 8;
            Scribe.saver.InitSaving(path, "settings"); f.Settings.ExposeData(); Scribe.saver.FinalizeSaving();
            var restored = new JoyRescueSettings();
            Scribe.loader.InitLoading(path); restored.ExposeData(); Scribe.loader.FinalizeLoading();
            Equal(true, restored.rescueModsWithOwnCode); Equal(false, restored.requireChairForWatching);
            Equal(false, restored.enabledOverrides["A"]); Equal("Watch", restored.modeOverrides["A"]);
            Equal("JoyRescue_Kind_7", restored.kindOverrides["A"]); Equal("JoyRescue_Kind_7", restored.giverKindOverrides["G"]);
            Equal("Disabled", restored.disabledKinds.Single()); Equal(2, restored.kindSortMode); Equal(8, restored.nextCustomKindId);
            Equal("7", restored.customKinds.Single().id); Equal("Échecs <custom> & music", restored.customKinds[0].label);
            Equal(false, restored.customKinds[0].needsThing);
        });
        Test("D32 actual Scribe legacy empty configuration", () =>
        {
            using var f = new SettingsFixture();
            var path = ScratchFile("legacy.xml");
            File.WriteAllText(path, "<settings />");
            var restored = new JoyRescueSettings();
            Scribe.loader.InitLoading(path); restored.ExposeData(); Scribe.loader.FinalizeLoading();
            Defaults(restored);
        });
    }

    // Scratch files of the Scribe cases. The folder is created here rather than assumed: it used to be
    // a dated evidence folder that a cleanup could delete, which silently broke both cases.
    private static string ScratchFile(string name)
    {
        var dir = Path.Combine(RepoRoot(), ".build", "scratch");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, name);
    }
}