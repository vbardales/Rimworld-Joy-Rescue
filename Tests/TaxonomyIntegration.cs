using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyRescue;
using RimWorld;
using Verse;
using Verse.AI;

internal static partial class Program
{
    private static ModContentPack TaxonomyPack(string id)
    {
        var pack = Blank<ModContentPack>();
        typeof(ModContentPack).GetField("packageIdPlayerFacingInt", InstanceFields).SetValue(pack, id);
        return pack;
    }
    private static JoyKindDef Target(string name = "NewKind")
    {
        var def = new JoyKindDef { defName = name, label = name };
        DefDatabase<JoyKindDef>.Add(def); return def;
    }
    private static CommonTaxonomy.Rule RuleFor(RescueEntry e, string target = "NewKind")
    {
        e.giver.modContentPack = TaxonomyPack("tests.taxonomy");
        e.giver.giverClass = typeof(JoyGiver_InteractBuildingSitAdjacent);
        e.job.driverClass = typeof(JobDriver_SitFacingBuilding);
        return new CommonTaxonomy.Rule("tests.taxonomy", e.giver.defName, "TestKind", target, e.job.defName, e.Key);
    }
    private static void RegisterTaxonomyTests()
    {
        foreach (string language in new[] { "English", "French" })
        Test("T01 preset kinds/order/labels " + language, () =>
        {
            using var f = new SettingsFixture(language);
            Equal(false, f.Settings.commonTaxonomy);
            f.Settings.customKinds.Add(new CustomJoyKind("old", "User label"));
            CommonTaxonomy.EnsureKinds(f.Settings); CommonTaxonomy.EnsureKinds(f.Settings);
            Equal(4, f.Settings.customKinds.Count); Equal("old", f.Settings.customKinds[0].id);
            Equal(language == "French" ? "jeux vidéo et simulations" : "video games and simulations", f.Settings.customKinds[1].label);
            Equal(true, f.Settings.customKinds.All(k => k.needsThing));
            f.Settings.commonTaxonomy = true; f.Settings.Reset();
            Equal(false, f.Settings.commonTaxonomy); Equal(4, f.Settings.customKinds.Count);
            Equal("User label", f.Settings.customKinds[0].label);
        });
        Test("T02 exact correction preserves job/driver/rewards and is idempotent", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair(); var target = Target();
            var rule = RuleFor(e); e.job.joyGainRate = 2.3f; e.job.joyMaxParticipants = 7; e.giver.baseChance = 4;
            var job = e.job; f.Settings.commonTaxonomy = true;
            CommonTaxonomy.Apply(f.Settings, new[] { rule });
            Equal(target, e.giver.joyKind); Equal(target, e.job.joyKind); Equal(target, e.building.building.joyKind);
            Equal(job, e.giver.jobDef); Equal(2.3f, e.job.joyGainRate); Equal(7, e.job.joyMaxParticipants); Equal(4f, e.giver.baseChance);
            CommonTaxonomy.Apply(f.Settings, new[] { rule }); Equal(1, CommonTaxonomy.Applied.Count);
            Equal(1, CommonTaxonomy.MigrationLinks.Count);
        });
        foreach (string guard in new[] { "off", "package", "oldKind", "job", "things", "nullThing", "manualGiver", "manualBuilding", "driver", "giverClass", "missingTarget", "mismatch" })
        Test("T03 guard " + guard, () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair(); var rule = RuleFor(e);
            if (guard != "missingTarget") Target();
            f.Settings.commonTaxonomy = guard != "off";
            if (guard == "package") e.giver.modContentPack = TaxonomyPack("another.mod");
            if (guard == "oldKind") e.giver.joyKind = Target("Other");
            if (guard == "job") e.job.defName = "Changed";
            if (guard == "things") e.giver.thingDefs.Clear();
            if (guard == "nullThing") e.giver.thingDefs.Add(null);
            if (guard == "manualGiver") f.Settings.giverKindOverrides[e.giver.defName] = "NewKind";
            if (guard == "manualBuilding") f.Settings.kindOverrides[e.Key] = "NewKind";
            if (guard == "driver") e.job.driverClass = typeof(JobDriver_Wait);
            if (guard == "giverClass") e.giver.giverClass = typeof(JoyGiver_Meditate);
            if (guard == "mismatch") e.job.joyKind = Target("Different");
            var oldJobKind = e.job.joyKind; var oldBuildingKind = e.building.building.joyKind;
            CommonTaxonomy.Apply(f.Settings, new[] { rule });
            Equal(0, CommonTaxonomy.Applied.Count); Equal(oldJobKind, e.job.joyKind); Equal(oldBuildingKind, e.building.building.joyKind);
        });
        Test("T03 guard a building that names another type is left alone", () =>
        {
            using var f = new SettingsFixture(); var e = f.Repair(); var rule = RuleFor(e); Target();
            f.Settings.commonTaxonomy = true; e.building.building.joyKind = Target("Different");
            CommonTaxonomy.Apply(f.Settings, new[] { rule });
            Equal(0, CommonTaxonomy.Applied.Count); Equal(f.Kind, e.job.joyKind);
            Equal(true, CommonTaxonomy.Diagnostics.Any(d => d.Contains("building/giver mismatch")));
        });
        Test("T03 a building that names no type follows its giver", () =>
        {
            // The game checks a building against its job only when the building names a type: the piano of a music
            // mod names none, and the rule for it must still apply.
            using var f = new SettingsFixture(); var e = f.Repair(); var rule = RuleFor(e); var target = Target();
            f.Settings.commonTaxonomy = true; e.building.building.joyKind = null;
            CommonTaxonomy.Apply(f.Settings, new[] { rule });
            Equal(1, CommonTaxonomy.Applied.Count); Equal(target, e.giver.joyKind); Equal(target, e.job.joyKind); Equal(target, e.building.building.joyKind);
        });        Test("G01 a deleted type that exists is kept inert in its place, one that was never built goes", () =>
        {
            using var f = new SettingsFixture();
            var built = new CustomJoyKind("2", "built"); var pending = new CustomJoyKind("3", "pending"); var later = new CustomJoyKind("4", "later");
            f.Settings.customKinds.AddRange(new[] { built, pending, later });
            DefDatabase<JoyKindDef>.Add(new JoyKindDef { defName = built.DefName }); DefDatabase<JoyKindDef>.Add(new JoyKindDef { defName = later.DefName });
            f.Settings.DeleteKind(built); f.Settings.DeleteKind(pending);
            Equal(true, built.retired); Equal(3 - 1, f.Settings.customKinds.Count);
            Equal(true, f.Settings.customKinds.Contains(built)); Equal(false, f.Settings.customKinds.Contains(pending));
            Equal(1, f.Settings.LiveKinds.Count()); Equal("4", f.Settings.LiveKinds.Single().id);
            Equal(true, f.Settings.IsRetired(built.DefName)); Equal(false, f.Settings.IsRetired(later.DefName));
            Equal(0, f.Settings.customKinds.IndexOf(built));      // still in front of the type created after it
        });
        Test("G02 Reset retires the types that exist, drops the pending ones and does not reuse their numbers", () =>
        {
            using var f = new SettingsFixture();
            var built = new CustomJoyKind("2", "built"); var pending = new CustomJoyKind("3", "pending");
            f.Settings.customKinds.AddRange(new[] { built, pending }); f.Settings.nextCustomKindId = 4;
            DefDatabase<JoyKindDef>.Add(new JoyKindDef { defName = built.DefName });
            f.Settings.Reset();
            Equal(1, f.Settings.customKinds.Count); Equal(true, built.retired); Equal(0, f.Settings.LiveKinds.Count());
            Equal(4, f.Settings.nextCustomKindId);
            f.Settings.customKinds.Clear(); f.Settings.Reset();
            Equal(1, f.Settings.nextCustomKindId);      // nothing holds a place: the numbers start over as they always did
        });
        Test("G03 the retired flag survives the saved settings", () =>
        {
            using var f = new SettingsFixture(); var path = ScratchFile("retired.xml");
            f.Settings.customKinds.Add(new CustomJoyKind("2", "gone") { retired = true });
            f.Settings.customKinds.Add(new CustomJoyKind("3", "kept"));
            Scribe.saver.InitSaving(path, "settings"); f.Settings.ExposeData(); Scribe.saver.FinalizeSaving();
            var restored = new JoyRescueSettings();
            Scribe.loader.InitLoading(path); restored.ExposeData(); Scribe.loader.FinalizeLoading();
            Equal(2, restored.customKinds.Count); Equal(true, restored.customKinds[0].retired); Equal(false, restored.customKinds[1].retired);
            Equal("3", restored.LiveKinds.Single().id);
        });
        Test("G04 a retired type reads zero tolerance and no boredom, the others are untouched", () =>
        {
            using var f = new SettingsFixture();
            var gone = new JoyKindDef { defName = "JoyRescue_Kind_2" }; var kept = new JoyKindDef { defName = "JoyRescue_Kind_3" };
            DefDatabase<JoyKindDef>.Add(gone); DefDatabase<JoyKindDef>.Add(kept);
            var set = new JoyToleranceSet();
            var values = (DefMap<JoyKindDef, float>)typeof(JoyToleranceSet).GetField("tolerances", InstanceFields).GetValue(set);
            var bored = (DefMap<JoyKindDef, bool>)typeof(JoyToleranceSet).GetField("bored", InstanceFields).GetValue(set);
            values[gone] = .3f; bored[gone] = true; values[kept] = .2f;
            f.Settings.customKinds.Add(new CustomJoyKind("2", "gone") { retired = true }); f.Settings.customKinds.Add(new CustomJoyKind("3", "kept"));
            var mod = typeof(JoyRescueMod).GetProperty("Settings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            var before = mod.GetValue(null); mod.SetValue(null, f.Settings);
            try { Patch_TaxonomyTolerance.ZeroRetired(set); } finally { mod.SetValue(null, before); }
            Equal(0f, values[gone]); Equal(false, bored[gone]); Equal(.2f, values[kept]);
        });        Test("T04 shared unlisted consumer blocks atomic correction", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A"); var b = f.Repair("B");
            var rule = RuleFor(a); b.giver.thingDefs.Add(a.building); Target(); f.Settings.commonTaxonomy = true;
            CommonTaxonomy.Apply(f.Settings, new[] { rule }); Equal(0, CommonTaxonomy.Applied.Count);
            Equal(f.Kind, a.job.joyKind); Equal(f.Kind, a.building.building.joyKind);
        });
        Test("T05 same-target shared consumers change together", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A"); var b = f.Repair("B");
            b.giver.thingDefs = new List<ThingDef> { a.building }; b.giver.jobDef = a.job;
            var ra = RuleFor(a); RuleFor(b);
            var rb = new CommonTaxonomy.Rule("tests.taxonomy", b.giver.defName, "TestKind", "NewKind", a.job.defName, a.Key);
            var target = Target(); f.Settings.commonTaxonomy = true;
            CommonTaxonomy.Apply(f.Settings, new[] { ra, rb }); Equal(2, CommonTaxonomy.Applied.Count);
            Equal(target, a.job.joyKind); Equal(target, b.giver.joyKind);
        });
        Test("T06 conflicting shared plans propagate rejection", () =>
        {
            using var f = new SettingsFixture(); var a = f.Repair("A"); var b = f.Repair("B");
            b.giver.thingDefs = new List<ThingDef> { a.building };
            var ra = RuleFor(a); RuleFor(b);
            var rb = new CommonTaxonomy.Rule("tests.taxonomy", b.giver.defName, "TestKind", "AnotherKind", b.job.defName, a.Key);
            Target(); Target("AnotherKind"); f.Settings.commonTaxonomy = true;
            CommonTaxonomy.Apply(f.Settings, new[] { rb, ra }); Equal(0, CommonTaxonomy.Applied.Count);
            Equal(f.Kind, a.job.joyKind); Equal(f.Kind, b.job.joyKind);
        });
        Test("T07 tolerance merge/split/max/once and no cascading", () =>
        {
            using var f = new SettingsFixture(); var b = Target("B"); var c = Target("C");
            var set = new JoyToleranceSet();
            var values = (DefMap<JoyKindDef,float>)typeof(JoyToleranceSet).GetField("tolerances", InstanceFields).GetValue(set);
            var bored = (DefMap<JoyKindDef,bool>)typeof(JoyToleranceSet).GetField("bored", InstanceFields).GetValue(set);
            values[f.Kind] = .7f; bored[f.Kind] = true; values[b] = .4f;
            var done = new List<string>();
            Patch_TaxonomyTolerance.Transfer(set, done, new[] { "TestKind>B", "B>C" });
            Equal(.7f, set[b]); Equal(.4f, set[c]); Equal(true, set.BoredOf(b));
            values[b] = .1f;
            Patch_TaxonomyTolerance.Transfer(set, done, new[] { "TestKind>B" }); Equal(.1f, set[b]);
            Patch_TaxonomyTolerance.Transfer(set, done, new[] { "TestKind>C" }); Equal(.7f, set[c]);
        });
        Test("T08 preset settings and transfer markers survive real Scribe", () =>
        {
            using var f = new SettingsFixture(); Target();
            var dir = Path.Combine(RepoRoot(), ".build", "taxonomy"); Directory.CreateDirectory(dir);
            string settingsPath = Path.Combine(dir, "settings.xml");
            f.Settings.commonTaxonomy = true; CommonTaxonomy.EnsureKinds(f.Settings);
            Scribe.saver.InitSaving(settingsPath, "settings"); f.Settings.ExposeData(); Scribe.saver.FinalizeSaving();
            var loaded = new JoyRescueSettings();
            Scribe.loader.InitLoading(settingsPath); loaded.ExposeData(); Scribe.loader.FinalizeLoading();
            Equal(true, loaded.commonTaxonomy); Equal(3, loaded.customKinds.Count);
            var set = new JoyToleranceSet();
            var values = (DefMap<JoyKindDef,float>)typeof(JoyToleranceSet).GetField("tolerances", InstanceFields).GetValue(set);
            values[f.Kind] = .6f; CommonTaxonomy.MigrationLinks.Clear(); CommonTaxonomy.MigrationLinks["TestKind>NewKind"] = "NewKind";
            Patch_TaxonomyTolerance.MigrateLoaded(set);
            string save = Path.Combine(dir, "tolerance.xml");
            Scribe.saver.InitSaving(save, "tolerance"); set.ExposeData(); Patch_TaxonomyTolerance.Postfix(set); Scribe.saver.FinalizeSaving();
            var restored = new JoyToleranceSet();
            Scribe.loader.InitLoading(save); restored.ExposeData(); Patch_TaxonomyTolerance.Postfix(restored); Scribe.loader.FinalizeLoading();
            var restoredValues = (DefMap<JoyKindDef,float>)typeof(JoyToleranceSet).GetField("tolerances", InstanceFields).GetValue(restored);
            restoredValues[DefDatabase<JoyKindDef>.GetNamed("NewKind")] = .05f;
            Patch_TaxonomyTolerance.MigrateLoaded(restored);
            Equal(.05f, restored[DefDatabase<JoyKindDef>.GetNamed("NewKind")]);
        });
        Test("T09 checked rule table has unique package/giver identities and three reserved targets", () =>
        {
            Equal(TaxonomyRules.All.Length, TaxonomyRules.All.Select(r => r.Package + ":" + r.Giver).Distinct().Count());
            Equal(true, TaxonomyRules.All.All(r => r.Things.Length > 0 && r.OldKind != r.Target));
            Equal(3, TaxonomyRules.All.Where(r => r.Target.StartsWith("JoyRescue_Kind_")).Select(r => r.Target).Distinct().Count());
        });
        Test("T10 preset toggle announces restart without changing this session", () =>
        {
            using var f = new SettingsFixture(); CommonTaxonomy.AppliedEnabled = false;
            f.Settings.commonTaxonomy = true; Equal(true, Editor<bool>("PendingRestart"));
            Equal(false, CommonTaxonomy.AppliedEnabled);
            f.Settings.commonTaxonomy = false; Equal(false, Editor<bool>("PendingRestart"));
        });
        Test("T11 new pawn first save marks current transfers without increasing tolerance", () =>
        {
            using var f = new SettingsFixture(); var target = Target(); var set = new JoyToleranceSet();
            var values = (DefMap<JoyKindDef,float>)typeof(JoyToleranceSet).GetField("tolerances", InstanceFields).GetValue(set);
            values[f.Kind] = .8f; values[target] = .1f;
            CommonTaxonomy.AppliedEnabled = true;
            CommonTaxonomy.MigrationLinks.Clear(); CommonTaxonomy.MigrationLinks["TestKind>NewKind"] = "NewKind";
            var dir = Path.Combine(RepoRoot(), ".build", "taxonomy"); Directory.CreateDirectory(dir);
            Scribe.saver.InitSaving(Path.Combine(dir,"new-pawn.xml"), "tolerance");
            set.ExposeData(); Patch_TaxonomyTolerance.Postfix(set); Scribe.saver.FinalizeSaving();
            Patch_TaxonomyTolerance.MigrateLoaded(set); Equal(.1f, set[target]);
            CommonTaxonomy.AppliedEnabled = false;
        });
    }
}
