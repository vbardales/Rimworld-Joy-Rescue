using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace JoyRescue.PickleSteps
{
    // The common recreation fix set (F15 to F18, and F19 with RIMMSQOL): what the set corrected at startup, what
    // it refused to touch and why, and what it left behind when it was switched off.
    //
    // The set applies at startup, from a table of exact identifiers (TaxonomyRules.All). Three ways to look at it
    // here, none of them a fabricated stand-in for a mod:
    //   - the rule for the base game's own telescope, which every game has;
    //   - the rules whose mods are staged in a pass that names them by Workshop id;
    //   - the guards, exercised on witness mods with rules written for those witnesses and handed to the same
    //     CommonTaxonomy.Apply that startup calls.
    //
    // No step spells a translated word: the suite runs unchanged in English and in French.
    [PickleSteps]
    public sealed class TaxonomySteps
    {
        private const string RuleWitnessPackage = "nelim.joyrescue.rulewitness";
        private const string OwnCodePackage = "nelim.joyrescue.owncodewitness";

        private static bool OnOff(PickleContext ctx, string word)
        {
            ctx.Require(word == "on" || word == "off", $"'{word}' is not 'on' or 'off'");
            return word == "on";
        }

        private static JoyGiverDef Giver(PickleContext ctx, string defName)
        {
            var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(defName);
            ctx.Assert(giver != null, $"no activity '{defName}' in this game: is its mod staged in this pass?");
            return giver;
        }

        // The two lines of the check box: the option, and the three types the set carries.
        [When("Joy Rescue: the common fix set is switched {word}")]
        public void SwitchFixSet(PickleContext ctx, string state)
        {
            ctx.Require(JoyRescueMod.Instance != null, "Joy Rescue is not a loaded mod");
            var settings = JoyRescueMod.Settings;
            settings.commonTaxonomy = OnOff(ctx, state);
            if (settings.commonTaxonomy) CommonTaxonomy.EnsureKinds(settings);
            JoyRescueMod.Instance.WriteSettings();
        }

        [Then("Joy Rescue: the common fix set is {word} in this game")]
        public void AssertFixSet(PickleContext ctx, string state)
        {
            var wanted = OnOff(ctx, state);
            ctx.Assert(CommonTaxonomy.AppliedEnabled == wanted,
                $"the fix set is {(CommonTaxonomy.AppliedEnabled ? "on" : "off")} in this game, expected {state}");
        }

        [Then("Joy Rescue: the fix set corrected the activity {string} to the recreation type {string}")]
        public void AssertCorrected(PickleContext ctx, string giverName, string kindName)
        {
            var giver = Giver(ctx, giverName);
            ctx.Assert(CommonTaxonomy.Applied.TryGetValue(giverName, out var target) && target == kindName,
                $"{giverName} was not corrected to {kindName}. Applied: "
                + string.Join(", ", CommonTaxonomy.Applied.Select(p => p.Key + "->" + p.Value).Take(20))
                + ". Skipped: " + string.Join("; ", CommonTaxonomy.Diagnostics.Where(d => d.StartsWith(giverName + ":"))));
            ctx.Assert(giver.joyKind?.defName == kindName && giver.jobDef?.joyKind?.defName == kindName,
                $"{giverName}: giver {giver.joyKind?.defName}, job {giver.jobDef?.joyKind?.defName}, expected {kindName}");
            foreach (var thing in giver.thingDefs)
                ctx.Assert(thing.building?.joyKind?.defName == kindName,
                    $"the building {thing.defName} of {giverName} is on {thing.building?.joyKind?.defName}, expected {kindName}");
        }

        [Then("Joy Rescue: the fix set left the activity {string} on the recreation type {string}")]
        public void AssertLeftAlone(PickleContext ctx, string giverName, string kindName)
        {
            var giver = Giver(ctx, giverName);
            ctx.Assert(!CommonTaxonomy.Applied.ContainsKey(giverName),
                $"{giverName} was corrected to {(CommonTaxonomy.Applied.TryGetValue(giverName, out var corrected) ? corrected : null)}");
            ctx.Assert(giver.joyKind?.defName == kindName && giver.jobDef?.joyKind?.defName == kindName,
                $"{giverName}: giver {giver.joyKind?.defName}, job {giver.jobDef?.joyKind?.defName}, expected {kindName}");
            foreach (var thing in giver.thingDefs ?? new List<ThingDef>())
                ctx.Assert(thing.building?.joyKind?.defName == kindName,
                    $"the building {thing.defName} of {giverName} is on {thing.building?.joyKind?.defName}, expected {kindName}");
        }

        [Then("Joy Rescue: the fix set skipped the activity {string} because of {string}")]
        public void AssertSkipped(PickleContext ctx, string giverName, string reason)
        {
            var wanted = giverName + ": skipped (" + reason + ")";
            ctx.Assert(CommonTaxonomy.Diagnostics.Contains(wanted),
                $"the fix set did not say \"{wanted}\". It said: " + string.Join("; ", CommonTaxonomy.Diagnostics.Where(d => d.StartsWith(giverName + ":"))
                    .DefaultIfEmpty("(nothing about this activity)")));
        }

        // The three types are created with a name in the language the game runs in, and kept as saved text: the
        // check compares them with what the game translates now, in the same language.
        [Then("Joy Rescue: the three types of the fix set exist with their translated names")]
        public void AssertReservedKinds(PickleContext ctx)
        {
            var expected = new Dictionary<string, string>
            {
                { CommonTaxonomy.Video, "JoyRescue.Taxonomy.Video".Translate().Resolve() },
                { CommonTaxonomy.Creative, "JoyRescue.Taxonomy.Creative".Translate().Resolve() },
                { CommonTaxonomy.Wellbeing, "JoyRescue.Taxonomy.Wellbeing".Translate().Resolve() },
            };
            foreach (var pair in expected)
            {
                var kind = DefDatabase<JoyKindDef>.GetNamedSilentFail(pair.Key);
                ctx.Assert(kind != null, $"the type {pair.Key} does not exist in this game");
                ctx.Assert(kind.label == pair.Value, $"{pair.Key} is labelled \"{kind.label}\", the game translates \"{pair.Value}\"");
                ctx.Assert(JoyRescueMod.Settings.customKinds.Any(c => c.DefName == pair.Key), $"{pair.Key} is not in the saved list of custom types");
            }
        }

        // Every rule of the table whose activity exists in the running game, from the mod the rule names, is either
        // applied or skipped with a reason: none is dropped without a word.
        [Then("Joy Rescue: every rule of the fix set whose activity is in this game was applied or skipped with a reason")]
        public void AssertEveryRuleAccountedFor(PickleContext ctx)
        {
            var unaccounted = new List<string>();
            var present = 0;
            foreach (var rule in TaxonomyRules.All)
            {
                var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(rule.Giver);
                if (giver == null) continue;
                if (!string.Equals(giver.modContentPack?.PackageIdPlayerFacing, rule.Package, StringComparison.OrdinalIgnoreCase)) continue;
                present++;
                if (CommonTaxonomy.Applied.ContainsKey(rule.Giver)) continue;
                if (CommonTaxonomy.Diagnostics.Any(d => d.StartsWith(rule.Giver + ":"))) continue;
                unaccounted.Add(rule.Package + "/" + rule.Giver);
            }
            Log.Message($"[Joy Rescue tests] fix set: {present} rules matched an activity in this game, {CommonTaxonomy.Applied.Count} applied, {CommonTaxonomy.Diagnostics.Count} skipped");
            ctx.Assert(present > 0, "no rule of the fix set matches an activity of this game: the mods it names are not staged");
            ctx.Assert(unaccounted.Count == 0, $"{unaccounted.Count} rules neither applied nor skipped with a reason: " + string.Join(", ", unaccounted.Take(10)));
        }

        [Then("Joy Rescue: the fix set corrected at least {int} activities of the mod {string}")]
        public void AssertCorrectedCount(PickleContext ctx, int count, string package)
        {
            var corrected = CommonTaxonomy.Applied.Keys
                .Where(g => string.Equals(DefDatabase<JoyGiverDef>.GetNamedSilentFail(g)?.modContentPack?.PackageIdPlayerFacing, package, StringComparison.OrdinalIgnoreCase))
                .ToList();
            ctx.Assert(corrected.Count >= count, $"the fix set corrected {corrected.Count} activities of {package}, expected at least {count}. "
                + "It skipped: " + string.Join("; ", CommonTaxonomy.Diagnostics.Take(12)));
        }

        // ---------------------------------------------------------------- the guards, on witnesses

        [When("Joy Rescue: the activity {string} is assigned by hand to the recreation type {string}")]
        public void AssignByHand(PickleContext ctx, string giverName, string kindName)
        {
            Giver(ctx, giverName);
            ctx.Require(DefDatabase<JoyKindDef>.GetNamedSilentFail(kindName) != null, $"no recreation type '{kindName}'");
            JoyRescueMod.Settings.giverKindOverrides[giverName] = kindName;
        }

        // The same call startup makes, with rules written for the witnesses: the table of the mod names real mods
        // and cannot name these. The guards are what is under test, on definitions loaded from XML and from a
        // mod's own assembly.
        [When("Joy Rescue: the fix set is applied with the rules of the witnesses")]
        public void ApplyWitnessRules(PickleContext ctx)
        {
            var target = "Reading";
            var rules = new[]
            {
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_CleanGiver", "Gaming_Cerebral", target, "JoyRescueRule_CleanUse", "JoyRescueRule_CleanTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_ShareGiverA", "Gaming_Cerebral", target, "JoyRescueRule_ShareUse", "JoyRescueRule_SharedTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_AgreeGiverA", "Gaming_Cerebral", target, "JoyRescueRule_AgreeUse", "JoyRescueRule_AgreeTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_AgreeGiverB", "Gaming_Cerebral", target, "JoyRescueRule_AgreeUse", "JoyRescueRule_AgreeTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_ManualGiver", "Gaming_Cerebral", target, "JoyRescueRule_ManualUse", "JoyRescueRule_ManualTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_StaleJobGiver", "Gaming_Cerebral", target, "JoyRescueRule_NotItsJob", "JoyRescueRule_StaleJobTable"),
                new CommonTaxonomy.Rule(RuleWitnessPackage, "JoyRescueRule_StaleSetGiver", "Gaming_Cerebral", target, "JoyRescueRule_StaleSetUse", "JoyRescueRule_StaleSetTable"),
                new CommonTaxonomy.Rule(OwnCodePackage, "JoyRescueOwnCode_Giver", "Gaming_Cerebral", target, "JoyRescueOwnCode_Use", "JoyRescueOwnCode_Kiosk"),
            };
            ctx.Require(JoyRescueMod.Settings.commonTaxonomy, "the fix set is off in the settings: Apply does nothing");
            CommonTaxonomy.Apply(JoyRescueMod.Settings, rules);
        }
    }
}
