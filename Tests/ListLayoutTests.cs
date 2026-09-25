using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyRescue;
using RimWorld;
using Verse;

// The order of the settings list: activities then their buildings, or buildings then their
// activities, with orphans last. The order is pure data, so it runs without the game.
internal static partial class Program
{
    private static JoyKindDef LayoutKind(string name) => new JoyKindDef { defName = name };

    private static JoyGiverDef LayoutGiver(string name, JoyKindDef kind, params ThingDef[] serves) =>
        new JoyGiverDef { defName = name, joyKind = kind, thingDefs = serves.ToList() };

    private static RescueEntry LayoutEntry(ThingDef building, JoyKindDef kind, bool covered, JoyGiverDef giver = null) =>
        new RescueEntry { building = building, joyKind = kind, covered = covered, giver = giver };

    // One string per list: H = type header, A = activity, B = building, a trailing + = indented.
    private static string LayoutText(int view, JoyKindDef[] kinds, RescueEntry[] entries, JoyGiverDef[] givers)
    {
        var rows = ListLayout.Build(
            view, kinds,
            k => entries.Where(e => e.joyKind == k).ToList(),
            k => givers.Where(g => g.joyKind == k).OrderBy(g => g.defName).ToList(),
            givers, entries, g => g.defName);
        return string.Join(" | ", rows.Select(r =>
            r.kind == RowKind.Header ? "H:" + r.type.defName
            : (r.kind == RowKind.Activity ? "A:" + r.giver.defName : "B:" + r.entry.building.defName)
              + (r.depth > 0 ? "+" : "")));
    }

    private static void RegisterListLayoutTests()
    {
        var k1 = LayoutKind("K1");
        var k2 = LayoutKind("K2");

        Test("L01 each activity then its building, and the reverse", () =>
        {
            var chess = Building("ChessTable"); var poker = Building("PokerTable");
            var entries = new[] { LayoutEntry(poker, k1, true), LayoutEntry(chess, k1, true) };
            var givers = new[] { LayoutGiver("Poker", k1, poker), LayoutGiver("Chess", k1, chess) };
            Equal("H:K1 | A:Chess | B:ChessTable+ | A:Poker | B:PokerTable+",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:ChessTable | A:Chess+ | B:PokerTable | A:Poker+",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L02 an orphan comes after the buildings already served, in both arrangements", () =>
        {
            var chess = Building("ChessTable"); var orphan = Building("Orph");
            var generated = LayoutGiver("A_Gen", k1, orphan);
            var entries = new[] { LayoutEntry(orphan, k1, false, generated), LayoutEntry(chess, k1, true) };
            var givers = new[] { generated, LayoutGiver("Chess", k1, chess) };
            // The generated activity sorts first by name and must still come last.
            Equal("H:K1 | A:Chess | B:ChessTable+ | A:A_Gen | B:Orph+",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:ChessTable | A:Chess+ | B:Orph | A:A_Gen+",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L03 a building no activity serves comes last and stays visible", () =>
        {
            var chess = Building("ChessTable"); var lonely = Building("Lonely");
            var entries = new[] { LayoutEntry(lonely, k1, false), LayoutEntry(chess, k1, true) };
            var givers = new[] { LayoutGiver("Chess", k1, chess) };
            Equal("H:K1 | A:Chess | B:ChessTable+ | B:Lonely",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:ChessTable | A:Chess+ | B:Lonely",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L04 an activity serving two buildings lists each once, or shows under both", () =>
        {
            var t1 = Building("T1"); var t2 = Building("T2");
            var entries = new[] { LayoutEntry(t1, k1, true), LayoutEntry(t2, k1, true) };
            var givers = new[] { LayoutGiver("Play", k1, t1, t2) };
            Equal("H:K1 | A:Play | B:T1+ | B:T2+",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:T1 | A:Play+ | B:T2 | A:Play+",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L05 an activity that serves no building is never dropped", () =>
        {
            var chess = Building("ChessTable");
            var entries = new[] { LayoutEntry(chess, k1, true) };
            var givers = new[] { LayoutGiver("Chess", k1, chess), LayoutGiver("Chat", k1) };
            Equal("H:K1 | A:Chat | A:Chess | B:ChessTable+",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:ChessTable | A:Chess+ | A:Chat",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L06 a building and its activity of different types are not lost", () =>
        {
            var t = Building("T");
            var entries = new[] { LayoutEntry(t, k1, true) };
            var givers = new[] { LayoutGiver("G", k2, t) };
            Equal("H:K1 | B:T | H:K2 | A:G",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1, k2 }, entries, givers));
            // Under its building, in the type of the building, and not repeated in its own type.
            Equal("H:K1 | B:T | A:G+ | H:K2",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1, k2 }, entries, givers));
        });

        Test("L09 a building served by two activities is listed once when activities come first", () =>
        {
            var t = Building("T");
            var entries = new[] { LayoutEntry(t, k1, true) };
            var givers = new[] { LayoutGiver("A1", k1, t), LayoutGiver("A2", k1, t) };
            // One row for the building, under the first activity: two rows would be two controls
            // on the same building.
            Equal("H:K1 | A:A1 | B:T+ | A:A2",
                LayoutText(ListLayout.ActivitiesFirst, new[] { k1 }, entries, givers));
            Equal("H:K1 | B:T | A:A1+ | A:A2+",
                LayoutText(ListLayout.BuildingsFirst, new[] { k1 }, entries, givers));
        });

        Test("L07 the arrangement survives a Scribe round trip and resets", () =>
        {
            using var f = new SettingsFixture();
            var path = ScratchFile("listview.xml");
            f.Settings.listView = ListLayout.BuildingsFirst;
            Scribe.saver.InitSaving(path, "settings"); f.Settings.ExposeData(); Scribe.saver.FinalizeSaving();
            var restored = new JoyRescueSettings();
            Scribe.loader.InitLoading(path); restored.ExposeData(); Scribe.loader.FinalizeLoading();
            Equal(ListLayout.BuildingsFirst, restored.listView);
            restored.Reset();
            Equal(ListLayout.ActivitiesFirst, restored.listView);
        });

        Test("L08 an unknown arrangement in a saved file falls back to the default", () =>
        {
            using var f = new SettingsFixture();
            var path = ScratchFile("listview-unknown.xml");
            File.WriteAllText(path, "<settings><listView>7</listView></settings>");
            var restored = new JoyRescueSettings();
            Scribe.loader.InitLoading(path); restored.ExposeData(); Scribe.loader.FinalizeLoading();
            Equal(ListLayout.ActivitiesFirst, restored.listView);
        });
    }
}
