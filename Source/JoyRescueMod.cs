using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace JoyRescue
{
    public class JoyRescueMod : Mod
    {
        public const string HarmonyId = "nelim.joyrescue";

        public static JoyRescueMod Instance { get; private set; }
        public static JoyRescueSettings Settings { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        private const float RowHeight = 30f;
        private const float HeaderHeight = 28f;
        private const float ModeColumnWidth = 150f;
        private const float KindColumnWidth = 140f;
        private const float InfoIconSize = 16f;

        private Vector2 scrollPosition;
        private Dictionary<JoyKindDef, List<RescueEntry>> byKindCache;
        private List<JoyKindDef> kindsCache;
        private Dictionary<JoyKindDef, List<JoyGiverDef>> giversByKindCache;
        private Dictionary<JoyKindDef, string> tipCache;
        private int entriesCounted = -1;
        private int tipsCounted = -1;

        public JoyRescueMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<JoyRescueSettings>();

            HarmonyInstance = new Harmony(HarmonyId);
            HarmonyInstance.PatchAll();
        }

        public override string SettingsCategory() => "Joy Rescue";

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettingsAndRefresh();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (!JoyRescueGenerator.HasRun)
            {
                Widgets.Label(inRect, "JoyRescue.Settings.NotScanned".Translate());
                return;
            }

            var listing = new Listing_Standard();
            listing.Begin(inRect);

            // One line, not a paragraph. The why - tolerance is per type, expectations ask for six
            // types - is an explanation you read once: it moves into a tooltip and gives its room back
            // to the list, which is what this window is opened for.
            var summaryRect = listing.GetRect(24f);
            Widgets.Label(summaryRect, "JoyRescue.Settings.Summary".Translate(
                JoyRescueGenerator.JoyBuildingsSeen,
                JoyRescueGenerator.AlreadyCovered,
                JoyRescueGenerator.Entries.Count,
                EnabledCount(),
                UsableKindCount()));
            TooltipHandler.TipRegion(summaryRect, "JoyRescue.Settings.SummaryTip".Translate(UsableKindCount()));

            var beforeTaxonomy = Settings.commonTaxonomy;
            listing.CheckboxLabeled("JoyRescue.Taxonomy.Enable".Translate(), ref Settings.commonTaxonomy,
                "JoyRescue.Taxonomy.Tooltip".Translate());
            if (Settings.commonTaxonomy && !beforeTaxonomy)
            {
                CommonTaxonomy.EnsureKinds(Settings);
                InvalidateCaches();
            }
            var taxonomyRect = listing.GetRect(24f);
            Widgets.Label(taxonomyRect, "JoyRescue.Taxonomy.Summary".Translate(
                CommonTaxonomy.Applied.Count, CommonTaxonomy.Diagnostics.Count));

            var beforeOwnCode = Settings.rescueModsWithOwnCode;
            var beforeChair = Settings.requireChairForWatching;

            listing.CheckboxLabeled("JoyRescue.Settings.RescueModsWithOwnCode".Translate(),
                ref Settings.rescueModsWithOwnCode,
                "JoyRescue.Settings.RescueModsWithOwnCodeTip".Translate());

            listing.CheckboxLabeled("JoyRescue.Settings.RequireChair".Translate(),
                ref Settings.requireChairForWatching,
                "JoyRescue.Settings.RequireChairTip".Translate());

            if (beforeOwnCode != Settings.rescueModsWithOwnCode
                || beforeChair != Settings.requireChairForWatching)
            {
                ApplySettingsAndRefresh();
            }

            // Five buttons on one row rather than four plus a separate sort row: that is thirty pixels
            // handed back to the list.
            var buttonRow = listing.GetRect(30f);
            var quarter = buttonRow.width / 5f;

            var sortRect = new Rect(buttonRow.x + quarter * 4f, buttonRow.y, quarter - 6f, buttonRow.height);
            if (Widgets.ButtonText(sortRect, "JoyRescue.Settings.SortMode".Translate(SortModeLabel())))
            {
                Settings.kindSortMode = (Settings.kindSortMode + 1) % 3;
                InvalidateCaches();
            }
            TooltipHandler.TipRegion(sortRect, "JoyRescue.Settings.SortModeTip".Translate());

            if (Widgets.ButtonText(new Rect(buttonRow.x, buttonRow.y, quarter - 6f, buttonRow.height),
                    "JoyRescue.Settings.EnableAll".Translate()))
            {
                SetAll(true);
            }
            if (Widgets.ButtonText(new Rect(buttonRow.x + quarter, buttonRow.y, quarter - 6f, buttonRow.height),
                    "JoyRescue.Settings.DisableAll".Translate()))
            {
                SetAll(false);
            }
            if (Widgets.ButtonText(new Rect(buttonRow.x + quarter * 2f, buttonRow.y, quarter - 6f, buttonRow.height),
                    "JoyRescue.Settings.LogReport".Translate()))
            {
                Log.Message(JoyRescueGenerator.Report());
                Log.Message(CommonTaxonomy.Report());
                Messages.Message("JoyRescue.Settings.LogReportDone".Translate(), MessageTypeDefOf.TaskCompletion, false);
            }
            if (Widgets.ButtonText(new Rect(buttonRow.x + quarter * 3f, buttonRow.y, quarter - 6f, buttonRow.height),
                    "JoyRescue.Settings.Reset".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    (Settings.customKinds.Any(CommonTaxonomy.Reserved)
                        ? "JoyRescue.Taxonomy.Reset".Translate() : "JoyRescue.Settings.ConfirmReset".Translate()),
                    delegate
                    {
                        Settings.Reset();
                        ApplySettingsAndRefresh();
                    },
                    destructive: true));
            }

            // The help text about saving moved into the tooltip of the button it describes: three
            // fixed lines at the top of a window you come to for the LIST is not the right place for
            // an explanation you read once.
            var addRow = listing.GetRect(30f);

            // An explicit save button. RimWorld ships none: `Dialog_ModSettings` writes the settings
            // on close, and nothing on screen says so. Saying it in a help text was not enough - people
            // want a button, and this one is not a decoy: it really calls WriteSettings(), so the file
            // is on disk before you leave.
            var saveRect = new Rect(addRow.xMax - 200f, addRow.y, 200f, addRow.height);
            if (Widgets.ButtonText(saveRect, "JoyRescue.Settings.SaveNow".Translate()))
            {
                WriteSettings();
                Messages.Message("JoyRescue.Settings.SaveNowDone".Translate(),
                    MessageTypeDefOf.TaskCompletion, false);
            }
            TooltipHandler.TipRegion(saveRect, "JoyRescue.Settings.SaveHint".Translate());

            if (Widgets.ButtonText(new Rect(addRow.x, addRow.y, 260f, addRow.height),
                    "JoyRescue.Settings.AddKind".Translate()))
            {
                var id = Settings.nextCustomKindId++;
                // Translate only the initial suggestion, while the settings UI is open and language
                // resources are loaded. Thereafter preserve the saved, player-editable name.
                Settings.customKinds.Add(new CustomJoyKind(id.ToString(),
                    "JoyRescue.Settings.NewKindLabel".Translate(id).Resolve()));
                InvalidateCaches();
            }

            foreach (var custom in Settings.customKinds.ToList())
            {
                var row = listing.GetRect(28f);
                // The text field is optional: the type exists and works without being renamed, so the
                // window stays usable without a keyboard.
                custom.label = Widgets.TextField(
                    new Rect(row.x + 8f, row.y, 300f, row.height - 2f), custom.label);

                var exists = DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName) != null;
                GUI.color = exists ? new Color(0.65f, 0.8f, 0.65f) : ColorLibrary.Yellow;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(row.x + 316f, row.y, row.width - 460f, row.height),
                    exists ? "JoyRescue.Settings.KindLive".Translate()
                           : "JoyRescue.Settings.KindOnRestart".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                if (!Settings.customKinds.Any(CommonTaxonomy.Reserved) && Widgets.ButtonText(new Rect(row.xMax - 130f, row.y, 128f, row.height - 2f),
                        "JoyRescue.Settings.RemoveKind".Translate()))
                {
                    // Deleting a type without cleaning up what had been assigned to it left reassignments
                    // pointing at a defName that would never exist again: silent, without effect, and
                    // impossible to make sense of from the interface.
                    var orphaned = PurgeOverridesTargeting(custom.DefName);

                    Settings.customKinds.Remove(custom);
                    InvalidateCaches();

                    if (orphaned > 0)
                    {
                        Messages.Message("JoyRescue.Settings.KindRemovedOrphans".Translate(orphaned),
                            MessageTypeDefOf.CautionInput, false);
                    }
                }
            }

            // One line instead of three, the detail in a tooltip.
            if (PendingRestart())
            {
                var warnRect = listing.GetRect(24f);
                GUI.color = ColorLibrary.Yellow;
                Widgets.Label(warnRect, "JoyRescue.Settings.PendingRestartShort".Translate());
                GUI.color = Color.white;
                TooltipHandler.TipRegion(warnRect, "JoyRescue.Settings.PendingRestart".Translate());
            }

            listing.GapLine();
            var usedHeight = listing.CurHeight;
            listing.End();

            var outRect = new Rect(inRect.x, inRect.y + usedHeight, inRect.width, inRect.height - usedHeight);
            DrawEntryList(outRect);
        }

        /// <summary>
        /// The full inventory, grouped by recreation TYPE rather than by mod. That is the question
        /// that matters in play: expectations ask for up to 6 different types, and tolerance is
        /// counted per type - knowing you own eight chess tables is worth nothing if they are all
        /// the same type. Orphans stay identified and adjustable; the rest are here for the
        /// inventory.
        /// </summary>
        private void DrawEntryList(Rect outRect)
        {
            var byKind = EntriesByKind();
            var kinds = SortedKinds();

            var giversByKind = GiversByKind();
            var giverRows = 0;
            foreach (var k in kinds) if (giversByKind.TryGetValue(k, out var gl)) giverRows += gl.Count;

            var viewHeight = kinds.Count * HeaderHeight
                           + (JoyRescueGenerator.AllEntries.Count + giverRows) * RowHeight + 8f;
            var viewRect = new Rect(0f, 0f, outRect.width - 20f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            var y = 0f;
            foreach (var kind in kinds)
            {
                byKind.TryGetValue(kind, out var list);
                DrawKindHeader(new Rect(0f, y, viewRect.width, HeaderHeight), kind, list);
                y += HeaderHeight;

                // Activities first: they explain the type, buildings only serve it.
                if (giversByKind.TryGetValue(kind, out var givers))
                {
                    foreach (var giver in givers)
                    {
                        DrawGiverRow(new Rect(0f, y, viewRect.width, RowHeight), giver);
                        y += RowHeight;
                    }
                }

                if (list == null) continue;
                foreach (var entry in list)
                {
                    DrawEntryRow(new Rect(0f, y, viewRect.width, RowHeight), entry);
                    y += RowHeight;
                }
            }

            Widgets.EndScrollView();
        }

        /// <summary>
        /// How many givers are actually live for this type. A type with no giver at all is a DEAD
        /// type: it exists in the database but nothing can produce it, so it counts for nothing
        /// against expectations. That is exactly the fault this mod repairs, and seeing it stated
        /// beats having to work it out.
        /// </summary>
        private static int ActiveGiverCount(JoyKindDef kind)
        {
            var n = 0;
            foreach (var g in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (g.joyKind == kind && g.baseChance > 0f) n++;
            }
            return n;
        }

        /// <summary>
        /// Every giver of the type, live or not.
        ///
        /// Needed for the header of a type that has been TURNED OFF: turning it off sets their
        /// `baseChance` to zero, so <see cref="ActiveGiverCount"/> returns zero and we would show
        /// "no giver" to someone who has just turned three of them off. What they want to read is
        /// what will start again if they turn it back on.
        /// </summary>
        private static int TotalGiverCount(JoyKindDef kind)
        {
            var n = 0;
            foreach (var g in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (g.joyKind == kind) n++;
            }
            return n;
        }

        private void DrawKindHeader(Rect rect, JoyKindDef kind, List<RescueEntry> list)
        {
            // The button that turns the type off. "Delete" does not exist here and never will: every
            // giver goes quiet, the def stays in place, no index moves.
            var offRect = new Rect(rect.xMax - 130f, rect.y + 1f, 128f, rect.height - 3f);
            var isOff = Settings.disabledKinds.Contains(kind.defName);
            if (Widgets.ButtonText(offRect, isOff
                    ? "JoyRescue.Settings.KindEnable".Translate()
                    : "JoyRescue.Settings.KindDisable".Translate()))
            {
                if (isOff) Settings.disabledKinds.Remove(kind.defName);
                else Settings.disabledKinds.Add(kind.defName);
                ApplySettingsAndRefresh();
            }
            rect.width -= 134f;

            var total = list?.Count ?? 0;
            var broken = 0;
            if (list != null)
            {
                foreach (var e in list)
                {
                    if (!e.covered) broken++;
                }
            }

            var givers = ActiveGiverCount(kind);
            var name = kind.LabelCap.NullOrEmpty() ? kind.defName : kind.LabelCap.ToString();

            TaggedString label;
            Color colour;
            if (isOff)
            {
                // Turned off ON PURPOSE. Without this branch, the `baseChance` values having been set to
                // zero, the type would fall back into the "no giver" case and show up in red as
                // "unreachable" - sending someone hunting for a fault they created themselves one button
                // ago. So we count the REAL givers, to say what will start again when it is switched back
                // on.
                label = "JoyRescue.Settings.KindHeaderOff".Translate(name, TotalGiverCount(kind));
                colour = new Color(0.55f, 0.55f, 0.62f);
            }
            else if (givers == 0)
            {
                label = "JoyRescue.Settings.KindHeaderDead".Translate(name);
                colour = ColorLibrary.RedReadable;
            }
            else if (broken > 0)
            {
                label = "JoyRescue.Settings.KindHeaderBroken".Translate(name, total, broken);
                colour = ColorLibrary.Yellow;
            }
            else if (total == 0)
            {
                // Types with no building: item or action recreation (drinking, reading, meditating,
                // talking). Normal, and worth saying rather than letting it look like a hole.
                label = "JoyRescue.Settings.KindHeaderNoBuilding".Translate(name, givers);
                colour = new Color(0.6f, 0.6f, 0.6f);
            }
            else
            {
                label = "JoyRescue.Settings.KindHeader".Translate(name, total, givers);
                colour = new Color(0.65f, 0.8f, 0.65f);
            }

            // The "i" signals there is something to hover, but aiming at sixteen pixels with the mouse
            // is unpleasant: the hot zone therefore covers the whole title band. The off button is
            // already outside `rect` (width -= 134), so there is no conflict.
            var iconRect = new Rect(rect.x + 2f, rect.y + (rect.height - InfoIconSize) / 2f,
                                    InfoIconSize, InfoIconSize);
            GUI.DrawTexture(iconRect, TexButton.Info);
            TooltipHandler.TipRegion(rect, KindTip(kind, list));

            GUI.color = colour;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(
                new Rect(iconRect.xMax + 6f, rect.y, rect.width - InfoIconSize - 8f, rect.height)
                    .ContractedBy(2f),
                label);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1f, rect.width);
            GUI.color = Color.white;
        }

        /// <summary>
        /// What this recreation type actually is, stated from the database.
        ///
        /// WHY NOT SIMPLY `kind.description`. `JoyKindDef` does inherit the `description` field
        /// from `Def`, but NONE of the base game's ten types fills it in: they carry only `label`
        /// and sometimes `needsThing`. A tooltip that showed the description alone would be empty
        /// nine times out of ten. So we show it when a mod has bothered to write one - some have -
        /// and always add what the defs can say about themselves: where the type comes from, what
        /// produces it, and on which buildings.
        ///
        /// Nothing here depends on the settings: neither the number of LIVE givers nor the on/off
        /// state appears in it, since the header already shows both. That is what lets the text be
        /// cached without going stale on the first click.
        /// </summary>
        private string KindTip(JoyKindDef kind, List<RescueEntry> list)
        {
            // A counter of its own, separate from `entriesCounted`: that one is already refreshed by
            // EntriesByKind() before a single header is drawn, so using it here would amount to never
            // invalidating this cache at all.
            if (tipCache == null || tipsCounted != JoyRescueGenerator.AllEntries.Count)
            {
                tipCache = new Dictionary<JoyKindDef, string>();
                tipsCounted = JoyRescueGenerator.AllEntries.Count;
            }
            if (tipCache.TryGetValue(kind, out var cached)) return cached;

            var sb = new StringBuilder();

            sb.Append(kind.LabelCap.NullOrEmpty() ? kind.defName : kind.LabelCap.ToString());
            sb.Append(" (").Append(kind.defName).Append(')');

            var mod = kind.modContentPack?.Name;
            if (!mod.NullOrEmpty())
            {
                sb.Append('\n').Append("JoyRescue.Settings.KindTipFrom".Translate(mod));
            }

            if (!kind.description.NullOrEmpty())
            {
                sb.Append("\n\n").Append(kind.description);
            }

            // `needsThing` defaults to true: in vanilla, only solitary relaxation and social
            // interaction set it explicitly false, and they are the only two that call for no object
            // at all.
            sb.Append("\n\n").Append(kind.needsThing
                ? "JoyRescue.Settings.KindTipNeedsThing".Translate()
                : "JoyRescue.Settings.KindTipNoThing".Translate());

            var activities = Activities(kind);
            if (activities.Count > 0)
            {
                sb.Append("\n\n").Append("JoyRescue.Settings.KindTipActivities"
                    .Translate(Join(activities)));
            }

            if (list != null && list.Count > 0)
            {
                sb.Append('\n').Append("JoyRescue.Settings.KindTipBuildings"
                    .Translate(Join(list.Select(e => e.BuildingLabel).ToList())));
            }

            // What an "activity" is. The game's technical term is JoyGiverDef, which means nothing to
            // anyone; explaining it once here beats leaving the bare word standing in every header.
            sb.Append("\n\n").Append("JoyRescue.Settings.KindTipWhatIsActivity".Translate());
            sb.Append("\n\n").Append("JoyRescue.Settings.KindTipTolerance".Translate());

            var text = sb.ToString();
            tipCache[kind] = text;
            return text;
        }

        /// <summary>
        /// What you DO to get this type, read from the `reportString` of the jobs that grant it -
        /// "playing chess", "watching television". That speaks louder than a defName.
        ///
        /// reportStrings carrying a token ("watching TargetA") are dropped: outside a job context
        /// the token is replaced by nothing and would produce a nonsense phrase.
        /// </summary>
        private static List<string> Activities(JoyKindDef kind)
        {
            var seen = new List<string>();
            foreach (var g in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (g.joyKind != kind) continue;

                var s = g.jobDef?.reportString;
                if (s.NullOrEmpty()) continue;

                // "playing TargetA.": outside a job context the token is replaced by nothing. We STRIP it
                // instead of dropping the entry, as I did at first: the generic givers are precisely the
                // ones serving several pieces of furniture - all music, all televisions - and throwing
                // them away left the "Activities" line empty on the very types that are best served.
                s = Regex.Replace(s, @"\s*Target[A-C]\b", "").TrimEnd('.', ',', ' ');
                if (s.NullOrEmpty()) s = g.defName;

                if (!seen.Contains(s)) seen.Add(s);
            }
            return seen;
        }

        /// <summary>A readable list, truncated: past eight entries we count the rest.</summary>
        private static string Join(List<string> items)
        {
            const int Max = 8;
            if (items.Count <= Max) return items.ToCommaList();

            return items.Take(Max).ToList().ToCommaList()
                 + " " + "JoyRescue.Settings.KindTipMore".Translate(items.Count - Max);
        }

        private void DrawEntryRow(Rect rect, RescueEntry entry)
        {
            Widgets.DrawHighlightIfMouseover(rect);

            var enabled = true;

            // A building that is already served has nothing to adjust: no checkbox, no mode button.
            // Giving it one would suggest it can be turned off, which is false - its giver belongs to
            // its own mod.
            if (!entry.covered)
            {
                enabled = Settings.IsEnabled(entry);
                var wasEnabled = enabled;
                Widgets.Checkbox(new Vector2(rect.x + 4f, rect.y + 3f), ref enabled, 24f);
                if (enabled != wasEnabled)
                {
                    Settings.SetEnabled(entry, enabled);
                    ApplySettingsAndRefresh();
                }
            }

            var iconRect = new Rect(rect.x + 34f, rect.y + 3f, 24f, 24f);
            Widgets.DefIcon(iconRect, entry.building);

            var rightEdge = rect.xMax - 4f;
            if (!entry.covered)
            {
                var modeRect = new Rect(rect.xMax - ModeColumnWidth - 4f, rect.y + 2f,
                    ModeColumnWidth, rect.height - 4f);
                var rawMode = Settings.RawMode(entry);
                if (Widgets.ButtonText(modeRect, ModeButtonLabel(entry, rawMode)))
                {
                    Settings.SetMode(entry, NextMode(rawMode));
                    ApplySettingsAndRefresh();
                }
                TooltipHandler.TipRegion(modeRect, "JoyRescue.Settings.ModeTip".Translate());
                rightEdge = modeRect.x - 6f;
            }

            // The reassignment button. The source mod moves into the tooltip: the room is better spent
            // on an action than on a piece of information you rarely consult.
            var kindRect = new Rect(rightEdge - KindColumnWidth, rect.y + 2f,
                KindColumnWidth, rect.height - 4f);
            if (Widgets.ButtonText(kindRect, CurrentKindLabel(entry)))
            {
                OpenKindMenu(entry);
            }

            // The building name, then its source mod in grey on the same line. With a list of over a
            // thousand mods, knowing where a piece of furniture comes from counts as much as its name:
            // keeping that for the tooltip was a false economy of space.
            var labelRect = new Rect(iconRect.xMax + 6f, rect.y, kindRect.x - iconRect.xMax - 10f, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;

            var name = entry.BuildingLabel.CapitalizeFirst();
            var textWidth = Text.CalcSize(name).x;
            var maxName = labelRect.width * 0.55f;
            if (textWidth > maxName) textWidth = maxName;

            if (entry.covered) GUI.color = new Color(0.75f, 0.75f, 0.75f);
            else if (!enabled) GUI.color = Color.gray;
            Widgets.Label(new Rect(labelRect.x, labelRect.y, textWidth + 2f, labelRect.height), name);
            GUI.color = Color.white;

            // The "i" sits against the name, and signals there is a description to read. The hot zone,
            // though, is the whole row (registered further down): aiming at sixteen pixels with the
            // mouse is unpleasant, and hovering the item's own image has to work too.
            var infoRect = new Rect(labelRect.x + textWidth + 6f,
                                    labelRect.y + (labelRect.height - InfoIconSize) / 2f,
                                    InfoIconSize, InfoIconSize);
            GUI.DrawTexture(infoRect, TexButton.Info);

            GUI.color = new Color(0.55f, 0.55f, 0.55f);
            Widgets.Label(
                new Rect(infoRect.xMax + 6f, labelRect.y,
                         Mathf.Max(0f, labelRect.xMax - infoRect.xMax - 6f), labelRect.height),
                entry.sourceMod);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            var tip = "JoyRescue.Settings.RowTip".Translate(
                entry.building.defName, entry.sourceMod, entry.joyKind.defName);

            // The building's description, as its author wrote it. That is what you want to read when
            // you do not recognise an object in a list of several dozen.
            if (!entry.building.description.NullOrEmpty())
            {
                tip += "\n\n" + entry.building.description;
            }

            tip += "\n\n" + (entry.covered
                ? "JoyRescue.Settings.RowTipCovered".Translate()
                : "JoyRescue.Settings.RowTipOrphan".Translate());
            if (entry.sourceShipsJoyCode)
            {
                tip += "\n\n" + "JoyRescue.Settings.RowTipOwnCode".Translate();
            }
            TooltipHandler.TipRegion(rect, tip);
        }

        /// <summary>
        /// The type currently wanted for this building: the pending reassignment if there is one,
        /// otherwise the real type. A reassignment only takes effect on the next startup, so we
        /// have to show the intent and not the state, or the button looks like it does nothing.
        /// </summary>
        private static TaggedString CurrentKindLabel(RescueEntry entry)
        {
            if (Settings.kindOverrides.TryGetValue(entry.Key, out var wanted)
                && wanted != entry.joyKind.defName)
            {
                return "JoyRescue.Settings.KindPending".Translate(KindNameByDefName(wanted));
            }
            return (TaggedString)KindName(entry.joyKind);
        }

        /// <summary>
        /// Removes every reassignment, of a building or of an activity, that pointed at this type.
        /// Returns how many were removed.
        ///
        /// Called when a type created here is deleted. Without it those entries survived in the
        /// settings pointing at a defName that would never be materialised again: the generator
        /// ignored them, and the interface showed the raw defName in place of a name.
        /// </summary>
        private static int PurgeOverridesTargeting(string kindDefName)
        {
            var removed = 0;

            foreach (var key in Settings.kindOverrides
                         .Where(p => p.Value == kindDefName).Select(p => p.Key).ToList())
            {
                Settings.kindOverrides.Remove(key);
                removed++;
            }

            foreach (var key in Settings.giverKindOverrides
                         .Where(p => p.Value == kindDefName).Select(p => p.Key).ToList())
            {
                Settings.giverKindOverrides.Remove(key);
                removed++;
            }

            return removed;
        }

        private static TaggedString SortModeLabel()
        {
            switch (Settings.kindSortMode)
            {
                case 1:  return "JoyRescue.Settings.SortByCount".Translate();
                case 2:  return "JoyRescue.Settings.SortByName".Translate();
                default: return "JoyRescue.Settings.SortByState".Translate();
            }
        }

        private static string KindName(JoyKindDef kind)
        {
            if (kind == null) return null;
            return kind.LabelCap.NullOrEmpty() ? kind.defName : kind.LabelCap.ToString();
        }

        /// <summary>
        /// The readable name of a type designated by its defName, including one that does NOT EXIST
        /// YET. Without this, a type just created showed up under its raw defName
        /// ("JoyRescue_Kind_3") everywhere it had been assigned.
        /// </summary>
        private static string KindNameByDefName(string defName)
        {
            var def = DefDatabase<JoyKindDef>.GetNamedSilentFail(defName);
            if (def != null) return KindName(def);

            foreach (var custom in Settings.customKinds)
            {
                if (custom.DefName == defName)
                {
                    // The name field is free-form: it can be empty. Falling back on the defName beats an
                    // invisible menu entry.
                    return custom.label.NullOrEmpty() ? defName : custom.label;
                }
            }
            return defName;
        }

        /// <summary>
        /// Every assignable type: those that exist, PLUS those just created that do not exist yet.
        ///
        /// THIS IS THE POINT OF THE WHOLE MECHANISM. A type created here is only materialised on the
        /// next startup. With the menu showing nothing but the def database, you had to restart once
        /// to SEE the type, then a second time for the assignment to take: two restarts for a single
        /// gesture.
        ///
        /// But reassignments are stored by defName, a plain string, and <c>CreateCustomKinds()</c>
        /// runs BEFORE <c>ApplyKindOverrides()</c> during generation. Pointing at a type that does
        /// not exist yet is therefore perfectly safe: on the next launch it will exist before anyone
        /// tries to apply it. One restart.
        /// </summary>
        private static List<(string defName, string label, bool pending)> KindChoices()
        {
            var list = new List<(string, string, bool)>();

            foreach (var kind in DefDatabase<JoyKindDef>.AllDefsListForReading)
            {
                list.Add((kind.defName, KindName(kind), false));
            }

            foreach (var custom in Settings.customKinds)
            {
                if (DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName) != null) continue;
                list.Add((custom.DefName, custom.label.NullOrEmpty() ? custom.DefName : custom.label, true));
            }

            list.SortBy(c => c.Item2);
            return list;
        }

        /// <summary>A float menu of every type, plus the option of going back to the original one.</summary>
        private void OpenKindMenu(RescueEntry entry)
        {
            var options = new List<FloatMenuOption>();

            if (Settings.kindOverrides.ContainsKey(entry.Key))
            {
                options.Add(new FloatMenuOption("JoyRescue.Settings.KindRevert".Translate(), delegate
                {
                    Settings.kindOverrides.Remove(entry.Key);
                    InvalidateCaches();
                }));
            }

            foreach (var choice in KindChoices())
            {
                var captured = choice;
                var label = captured.pending
                    ? "JoyRescue.Settings.KindPending".Translate(captured.label).ToString()
                    : captured.label;

                options.Add(new FloatMenuOption(label, delegate
                {
                    if (captured.defName == entry.joyKind.defName)
                    {
                        Settings.kindOverrides.Remove(entry.Key);
                    }
                    else
                    {
                        Settings.kindOverrides[entry.Key] = captured.defName;
                    }
                    InvalidateCaches();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>
        /// True if a setting is waiting for the next startup: a type created but not materialised,
        /// or a reassignment not yet applied. Creating a type and moving a building both require
        /// writing into the database before the DefMaps are built.
        /// </summary>
        private static bool PendingRestart()
        {
            if (Settings.commonTaxonomy != CommonTaxonomy.AppliedEnabled) return true;
            foreach (var custom in Settings.customKinds)
            {
                if (DefDatabase<JoyKindDef>.GetNamedSilentFail(custom.DefName) == null) return true;
            }

            foreach (var pair in Settings.kindOverrides)
            {
                var building = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
                if (building?.building == null) continue;
                if (building.building.joyKind?.defName != pair.Value) return true;
            }

            foreach (var pair in Settings.giverKindOverrides)
            {
                var giver = DefDatabase<JoyGiverDef>.GetNamedSilentFail(pair.Key);
                if (giver == null) continue;
                if (giver.joyKind?.defName != pair.Value) return true;
            }

            return false;
        }

        private void ApplySettingsAndRefresh()
        {
            JoyRescueGenerator.ApplySettings();
            InvalidateCaches();
        }

        private void InvalidateCaches()
        {
            byKindCache = null;
            kindsCache = null;
            giversByKindCache = null;
            tipCache = null;
            tipsCounted = -1;
            entriesCounted = -1;
        }

        /// <summary>The activities of each type, to list under its header.</summary>
        private Dictionary<JoyKindDef, List<JoyGiverDef>> GiversByKind()
        {
            if (giversByKindCache != null) return giversByKindCache;

            giversByKindCache = new Dictionary<JoyKindDef, List<JoyGiverDef>>();
            foreach (var g in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (g.joyKind == null) continue;
                if (!giversByKindCache.TryGetValue(g.joyKind, out var list))
                {
                    list = new List<JoyGiverDef>();
                    giversByKindCache[g.joyKind] = list;
                }
                list.Add(g);
            }
            foreach (var list in giversByKindCache.Values)
            {
                list.SortBy(g => ActivityName(g));
            }
            return giversByKindCache;
        }

        /// <summary>
        /// The readable name of an activity: "playing chess" rather than `Play_Chess`. The `TargetA`
        /// token is stripped, not avoided - otherwise every generic activity would disappear.
        /// </summary>
        private static string ActivityName(JoyGiverDef giver)
        {
            var s = giver.jobDef?.reportString;
            if (s.NullOrEmpty()) return giver.defName;

            s = Regex.Replace(s, @"\s*Target[A-C]\b", "").TrimEnd('.', ',', ' ');
            return s.NullOrEmpty() ? giver.defName : s.CapitalizeFirst();
        }

        private static TaggedString CurrentGiverKindLabel(JoyGiverDef giver)
        {
            if (Settings.giverKindOverrides.TryGetValue(giver.defName, out var wanted)
                && wanted != giver.joyKind?.defName)
            {
                return "JoyRescue.Settings.KindPending".Translate(KindNameByDefName(wanted));
            }
            return (TaggedString)KindName(giver.joyKind);
        }

        private void OpenGiverKindMenu(JoyGiverDef giver)
        {
            var options = new List<FloatMenuOption>();

            if (Settings.giverKindOverrides.ContainsKey(giver.defName))
            {
                options.Add(new FloatMenuOption("JoyRescue.Settings.KindRevert".Translate(), delegate
                {
                    Settings.giverKindOverrides.Remove(giver.defName);
                    InvalidateCaches();
                }));
            }

            foreach (var choice in KindChoices())
            {
                var captured = choice;
                var label = captured.pending
                    ? "JoyRescue.Settings.KindPending".Translate(captured.label).ToString()
                    : captured.label;

                options.Add(new FloatMenuOption(label, delegate
                {
                    if (captured.defName == giver.joyKind?.defName)
                    {
                        Settings.giverKindOverrides.Remove(giver.defName);
                    }
                    else
                    {
                        Settings.giverKindOverrides[giver.defName] = captured.defName;
                    }
                    InvalidateCaches();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>
        /// One activity row. Deliberately quieter than a building row: indented, grey, with no
        /// checkbox and no mode button - an activity is not turned off on its own, it is its whole
        /// type that goes off.
        /// </summary>
        private void DrawGiverRow(Rect rect, JoyGiverDef giver)
        {
            Widgets.DrawHighlightIfMouseover(rect);

            var kindRect = new Rect(rect.xMax - KindColumnWidth - 4f, rect.y + 2f,
                KindColumnWidth, rect.height - 4f);
            if (Widgets.ButtonText(kindRect, CurrentGiverKindLabel(giver)))
            {
                OpenGiverKindMenu(giver);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = new Color(0.62f, 0.66f, 0.72f);
            Widgets.Label(
                new Rect(rect.x + 40f, rect.y, kindRect.x - rect.x - 46f, rect.height),
                "» " + ActivityName(giver));
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            var served = giver.thingDefs == null || giver.thingDefs.Count == 0
                ? null
                : giver.thingDefs.Where(t => t != null)
                       .Select(t => t.LabelCap.ToString()).ToList();

            var tip = "JoyRescue.Settings.GiverTip".Translate(giver.defName, ActivityName(giver));
            if (served != null && served.Count > 0)
            {
                tip += "\n" + "JoyRescue.Settings.KindTipBuildings".Translate(Join(served));
            }
            tip += "\n\n" + "JoyRescue.Settings.GiverTipMove".Translate();
            TooltipHandler.TipRegion(rect, tip);
        }

        private static TaggedString ModeButtonLabel(RescueEntry entry, RescueMode rawMode)
        {
            if (rawMode != RescueMode.Auto) return ModeLabel(rawMode);
            return "JoyRescue.Mode.Auto".Translate(ModeLabel(JoyRescueGenerator.Heuristic(entry.building)));
        }

        private static TaggedString ModeLabel(RescueMode mode)
        {
            switch (mode)
            {
                case RescueMode.InteractionCell: return "JoyRescue.Mode.InteractionCell".Translate();
                case RescueMode.SitAdjacent: return "JoyRescue.Mode.SitAdjacent".Translate();
                case RescueMode.Watch: return "JoyRescue.Mode.Watch".Translate();
                default: return "JoyRescue.Mode.AutoShort".Translate();
            }
        }

        private static RescueMode NextMode(RescueMode mode)
        {
            switch (mode)
            {
                case RescueMode.Auto: return RescueMode.InteractionCell;
                case RescueMode.InteractionCell: return RescueMode.SitAdjacent;
                case RescueMode.SitAdjacent: return RescueMode.Watch;
                default: return RescueMode.Auto;
            }
        }

        private void SetAll(bool value)
        {
            foreach (var entry in JoyRescueGenerator.Entries)
            {
                Settings.SetEnabled(entry, value);
            }
            ApplySettingsAndRefresh();
        }

        private static int EnabledCount()
            => JoyRescueGenerator.Entries.Count(e => Settings.IsEnabled(e));

        /// <summary>
        /// How many building-borne recreation types are actually reachable - a served building, or
        /// an orphan we repair. That is the figure to compare against the `joyKindsNeeded` of
        /// expectations, not the number of pieces of furniture.
        /// </summary>
        private static int UsableKindCount()
        {
            var kinds = new HashSet<JoyKindDef>();
            foreach (var e in JoyRescueGenerator.AllEntries)
            {
                if (DefDatabase<JoyGiverDef>.AllDefsListForReading.Any(g =>
                        g.baseChance > 0f && g.joyKind == e.joyKind
                        && g.thingDefs != null && g.thingDefs.Contains(e.building)))
                    kinds.Add(e.joyKind);
            }
            return kinds.Count;
        }

        /// <summary>The buildings grouped by type, orphans first within each group.</summary>
        private Dictionary<JoyKindDef, List<RescueEntry>> EntriesByKind()
        {
            if (byKindCache != null && entriesCounted == JoyRescueGenerator.AllEntries.Count)
            {
                return byKindCache;
            }

            byKindCache = new Dictionary<JoyKindDef, List<RescueEntry>>();
            foreach (var e in JoyRescueGenerator.AllEntries
                         .OrderBy(e => e.covered)
                         .ThenBy(e => e.BuildingLabel))
            {
                if (!byKindCache.TryGetValue(e.joyKind, out var list))
                {
                    list = new List<RescueEntry>();
                    byKindCache[e.joyKind] = list;
                }
                list.Add(e);
            }
            entriesCounted = JoyRescueGenerator.AllEntries.Count;
            return byKindCache;
        }

        /// <summary>
        /// EVERY recreation type in the game, not only those with a building. The order puts what
        /// there is something to do about at the top: dead types first (no giver), then those with
        /// an orphan, then the served types, then those with no building.
        /// </summary>
        private List<JoyKindDef> SortedKinds()
        {
            if (kindsCache != null && entriesCounted == JoyRescueGenerator.AllEntries.Count)
            {
                return kindsCache;
            }

            var byKind = EntriesByKind();

            var all = DefDatabase<JoyKindDef>.AllDefsListForReading;

            switch (Settings.kindSortMode)
            {
                case 1:
                    // Most served to least served. The name stays the tie-breaker, without which two types on
                    // equal footing would swap places from one opening to the next.
                    kindsCache = all
                        .OrderByDescending(k => byKind.TryGetValue(k, out var c) ? c.Count : 0)
                        .ThenBy(k => KindName(k))
                        .ToList();
                    break;

                case 2:
                    kindsCache = all.OrderBy(k => KindName(k)).ToList();
                    break;

                default:
                    // By state: whatever calls for action first. Dead types, then those with an orphan, then
                    // the served types, then those with no building.
                    kindsCache = all
                        .OrderBy(k => ActiveGiverCount(k) == 0 ? 0
                                    : (byKind.TryGetValue(k, out var l) && l.Any(e => !e.covered)) ? 1
                                    : (byKind.ContainsKey(k) ? 2 : 3))
                        .ThenBy(k => KindName(k))
                        .ToList();
                    break;
            }

            return kindsCache;
        }
    }
}
