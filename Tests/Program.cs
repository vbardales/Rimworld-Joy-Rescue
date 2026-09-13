using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using JoyRescue;
using RimWorld;
using Verse;

// Small dependency-free test runner. Exercises the compiled mod and real game types.
// No game startup, mocks, network packages, or writes to the player's settings.
internal static partial class Program
{
    private static readonly List<(string name, Action body)> Cases = new List<(string, Action)>();
    private static void Test(string name, Action body) => Cases.Add((name, body));
    private static T Editor<T>(string method, params object[] args)
    {
        var target = typeof(JoyRescueMod).GetMethod(method,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        if (target == null) throw new Exception("Editor method not found: " + method);
        return (T)target.Invoke(null, args);
    }
    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}, got {actual}");
    }
    // ThingDef's constructor loads Unity shaders. Allocate only the data fixture here;
    // these cases do not depend on constructor defaults or graphics.
    private static ThingDef Building(string name = "TestBuilding")
    {
        var td = (ThingDef)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ThingDef));
        td.defName = name;
        td.building = new BuildingProperties();
        return td;
    }
    private static RescueEntry Entry(bool ownCode = false, string name = "TestBuilding") => new RescueEntry
    { building = Building(name), sourceShipsJoyCode = ownCode };
    private static void Defaults(JoyRescueSettings s)
    {
        Equal(false, s.rescueModsWithOwnCode);
        Equal(false, s.commonTaxonomy);
        Equal(true, s.requireChairForWatching);
        Equal(0, s.kindSortMode);
        Equal(1, s.nextCustomKindId);
        Equal(0, s.enabledOverrides.Count + s.modeOverrides.Count + s.customKinds.Count
            + s.kindOverrides.Count + s.giverKindOverrides.Count + s.disabledKinds.Count);
    }
    private static int Main(string[] args)
    {
        if (args.Contains("--trace-exceptions"))
            AppDomain.CurrentDomain.FirstChanceException += (_, e) => Console.Error.WriteLine("TRACE " + e.Exception);
        Test("U01 defaults", () => Defaults(new JoyRescueSettings()));
        foreach (bool code in new[] { false, true })
        foreach (bool global in new[] { false, true })
        {
            Test($"U02 default code={code} global={global}", () =>
            {
                var s = new JoyRescueSettings { rescueModsWithOwnCode = global };
                Equal(!code || global, s.DefaultEnabled(Entry(code)));
                Equal(!code || global, s.IsEnabled(Entry(code)));
            });
            foreach (bool value in new[] { false, true })
                Test($"U03-U05 override code={code} global={global} value={value}", () =>
                {
                    var s = new JoyRescueSettings { rescueModsWithOwnCode = global };
                    var e = Entry(code);
                    s.enabledOverrides[e.Key] = value;
                    Equal(value, s.IsEnabled(e));
                    s.SetEnabled(e, value);
                    Equal(value, s.IsEnabled(e));
                    Equal(value != s.DefaultEnabled(e), s.enabledOverrides.ContainsKey(e.Key));
                    Equal(!code || global, s.IsEnabled(Entry(code, "Other")));
                    s.SetEnabled(e, s.DefaultEnabled(e));
                    Equal(false, s.enabledOverrides.ContainsKey(e.Key));
                });
        }
        Test("U06 global switch preserves explicit choice", () =>
        {
            var s = new JoyRescueSettings();
            var explicitEntry = Entry(true, "Explicit");
            s.enabledOverrides[explicitEntry.Key] = false;
            s.rescueModsWithOwnCode = true;
            Equal(true, s.IsEnabled(Entry(true)));
            Equal(false, s.IsEnabled(explicitEntry));
        });
        foreach (string raw in new[] { null, "", "nonsense", "Auto" })
            Test($"U07 automatic fallback '{raw ?? "missing"}'", () =>
            {
                var s = new JoyRescueSettings();
                var e = Entry();
                if (raw != null) s.modeOverrides[e.Key] = raw;
                Equal(RescueMode.Auto, s.RawMode(e));
                Equal(RescueMode.SitAdjacent, s.ModeFor(e));
            });
        foreach (RescueMode mode in new[] { RescueMode.InteractionCell, RescueMode.SitAdjacent, RescueMode.Watch })
            Test($"U08-U09 explicit mode {mode} and reset", () =>
            {
                var s = new JoyRescueSettings();
                var e = Entry();
                s.SetMode(e, mode);
                Equal(mode, s.RawMode(e));
                Equal(mode, s.ModeFor(e));
                s.SetMode(e, RescueMode.Auto);
                Equal(false, s.modeOverrides.ContainsKey(e.Key));
                Equal(RescueMode.SitAdjacent, s.ModeFor(e));
            });
        Test("U10 reset all settings", () =>
        {
            var s = new JoyRescueSettings { rescueModsWithOwnCode = true,
                requireChairForWatching = false, kindSortMode = 2, nextCustomKindId = 55 };
            s.enabledOverrides["a"] = false;
            s.modeOverrides["a"] = "Watch";
            s.customKinds.Add(new CustomJoyKind("5", "Music"));
            s.kindOverrides["a"] = "X";
            s.giverKindOverrides["g"] = "X";
            s.disabledKinds.Add("X");
            s.Reset();
            Defaults(s);
            s.Reset();
            Defaults(s);
        });
        foreach (string label in new[] { null, "", "A label" })
            Test($"U11 label '{label ?? "null"}'", () =>
            {
                var e = Entry();
                e.building.label = label;
                Equal("TestBuilding", e.Key);
                Equal(string.IsNullOrEmpty(label) ? e.Key : label, e.BuildingLabel);
            });
        Test("U12 stable custom identity", () =>
        {
            var kind = new CustomJoyKind("7", "Music");
            Equal("JoyRescue_Kind_7", kind.DefName);
            Equal("Music", kind.label);
            Equal(true, kind.needsThing);
            kind.label = "Renamed";
            Equal("JoyRescue_Kind_7", kind.DefName);
        });
        foreach (bool cell in new[] { false, true })
        foreach (string kind in new[] { null, "Television", "Gaming_Cerebral", "television" })
            Test($"U13-U16 heuristic cell={cell} kind={kind ?? "null"}", () =>
            {
                var e = Entry();
                e.building.hasInteractionCell = cell;
                e.building.building.joyKind = kind == null ? null : new JoyKindDef { defName = kind };
                var expected = cell ? RescueMode.InteractionCell
                    : kind == "Television" ? RescueMode.Watch : RescueMode.SitAdjacent;
                Equal(expected, JoyRescueGenerator.Heuristic(e.building));
                e.building.defName = "TelevisionTelescopePoker";
                e.building.label = "Watch this";
                Equal(expected, JoyRescueGenerator.Heuristic(e.building));
            });
        Test("U15 no building properties", () =>
        {
            var td = Building();
            td.building = null;
            Equal(RescueMode.SitAdjacent, JoyRescueGenerator.Heuristic(td));
        });
        foreach (int missing in new[] { 0, 1, 2 })
            Test($"U20 missing job/giver {missing}", () =>
            {
                var e = Entry();
                e.job = missing == 0 ? new JobDef() : null;
                e.giver = missing == 1 ? new JoyGiverDef() : null;
                e.resolvedMode = RescueMode.InteractionCell;
                JoyRescueGenerator.Retarget(e, RescueMode.Watch);
                Equal(RescueMode.InteractionCell, e.resolvedMode);
            });

        foreach (var pair in new[] {
            (RescueMode.Auto, RescueMode.InteractionCell),
            (RescueMode.InteractionCell, RescueMode.SitAdjacent),
            (RescueMode.SitAdjacent, RescueMode.Watch),
            (RescueMode.Watch, RescueMode.Auto) })
            Test($"U27 editor cycle {pair.Item1}", () =>
                Equal(pair.Item2, Editor<RescueMode>("NextMode", pair.Item1)));

        foreach (var pair in new[] {
            ((string)null, "Activity"), ("", "Activity"),
            ("playing TargetA.", "Playing"),
            ("watching TargetB, TargetC.", "Watching"),
            ("TargetA.", "Activity"),
            ("playing TargetAlpha.", "Playing TargetAlpha") })
            Test($"U33 activity label '{pair.Item1 ?? "null"}'", () =>
            {
                var giver = new JoyGiverDef { defName = "Activity",
                    jobDef = new JobDef { reportString = pair.Item1 } };
                Equal(pair.Item2, Editor<string>("ActivityName", giver));
            });
        Test("U33 activity without job", () =>
            Equal("Activity", Editor<string>("ActivityName", new JoyGiverDef { defName = "Activity" })));

        // Always run the regression; retain --regressions as a compatible command-line alias.
            foreach (string raw in new[] { "99", "-1" })
                Test($"R01 undefined numeric mode {raw}", () =>
                {
                    var s = new JoyRescueSettings();
                    var e = Entry();
                    s.modeOverrides[e.Key] = raw;
                    Equal(RescueMode.Auto, s.RawMode(e));
                    Equal(RescueMode.SitAdjacent, s.ModeFor(e));
                });

        Test("Settings shortcut definition binds to a worker with native visibility", () =>
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "Source", "JoyRescue.csproj")))
                root = root.Parent;
            if (root == null) throw new Exception("Cannot find repository for distributed definition check");
            var xml = XDocument.Load(Path.Combine(root.FullName, "Mod", "Defs", "MainButtonDefs", "JoyRescue.xml"))
                .Root.Element("MainButtonDef");
            var workerType = typeof(JoyRescueMod).Assembly.GetType((string)xml.Element("workerClass"), true);
            var def = new MainButtonDef
            {
                defName = (string)xml.Element("defName"),
                workerClass = workerType,
                buttonVisible = bool.Parse((string)xml.Element("buttonVisible")),
                validWithoutMap = bool.Parse((string)xml.Element("validWithoutMap"))
            };
            Equal(false, def.buttonVisible);
            Equal(true, def.validWithoutMap);
            Equal(workerType, def.Worker.GetType());
            Equal(def, def.Worker.def);
            // Calling Visible initializes ModsConfig and Unity save paths, unavailable in this
            // data-only runner. Verify inheritance here; exercise actual reveal/hide in F13.
            Equal(typeof(MainButtonWorker), workerType.GetProperty("Visible").GetMethod.DeclaringType);
        });

        RegisterSettingsIntegrationTests();
        RegisterTaxonomyTests();
        int failures = 0;
        foreach (var test in Cases)
        {
            try { test.body(); Console.WriteLine("PASS " + test.name); }
            catch (Exception ex) { failures++; Console.WriteLine("FAIL " + test.name + "\n" + ex); }
        }
        Console.WriteLine($"{Cases.Count - failures}/{Cases.Count} passed; {failures} failed.");
        return failures == 0 ? 0 : 1;
    }
}
