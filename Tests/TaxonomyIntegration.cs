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
        Test("T04 shared unlisted consumer blocks atomic correction", () =>
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
