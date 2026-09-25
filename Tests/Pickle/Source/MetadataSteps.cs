using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace JoyRescue.PickleSteps
{
    // What the game's mod list says about Joy Rescue: the entry the player reads before installing or
    // enabling it. The list is drawn from the ModMetaData the game read out of About.xml, so that is what
    // is asserted; the captured page is for a person to look at.
    [PickleSteps]
    public sealed class MetadataSteps
    {
        private static ModMetaData Entry(PickleContext ctx)
        {
            var mod = ModLister.GetActiveModWithIdentifier("nelim.joyrescue", true);
            ctx.Assert(mod != null, "the game's mod list has no active mod with the package id nelim.joyrescue");
            return mod;
        }

        [Then("Joy Rescue: the mod list entry is titled {string}")]
        public void AssertTitle(PickleContext ctx, string title)
        {
            ctx.Assert(Entry(ctx).Name == title, $"the mod list entry is titled \"{Entry(ctx).Name}\", expected \"{title}\"");
        }

        [Then("Joy Rescue: the mod list entry is compatible with the game version this suite runs on")]
        public void AssertCompatible(PickleContext ctx)
        {
            var mod = Entry(ctx);
            ctx.Assert(mod.VersionCompatible, $"the game marks Joy Rescue as not compatible with {VersionControl.CurrentVersionString}");
            ctx.Assert(mod.SupportedVersionsReadOnly.Any(v => v.Major == VersionControl.CurrentMajor && v.Minor == VersionControl.CurrentMinor),
                "About.xml does not list the running game version " + VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor);
        }

        [Then("Joy Rescue: the mod list entry requires the mod {string}")]
        public void AssertRequires(PickleContext ctx, string packageId)
        {
            var dependencies = Entry(ctx).Dependencies;
            ctx.Assert(dependencies.Any(d => string.Equals(d.packageId, packageId, StringComparison.OrdinalIgnoreCase)),
                $"Joy Rescue does not require {packageId}; it requires: " + string.Join(", ", dependencies.Select(d => d.packageId)));
        }

        [Then("Joy Rescue: the mod list entry links to {string} in its metadata and ends its description with that link")]
        public void AssertLink(PickleContext ctx, string url)
        {
            var mod = Entry(ctx);
            ctx.Assert(mod.Url == url, $"the metadata link is \"{mod.Url}\", expected \"{url}\"");
            ctx.Assert(mod.Description.TrimEnd().EndsWith("[/url]") && mod.Description.Contains("[url=" + url + "]"),
                "the description does not end with a [url=" + url + "] link");
        }

        // The page the player opens from the main menu, with Joy Rescue selected, so that the capture shows
        // its title, its description and its link where they are read.
        [When("Joy Rescue: the mod list is open on Joy Rescue")]
        public async Task OpenModList(PickleContext ctx)
        {
            var page = new Page_ModsConfig();
            var selected = typeof(Page_ModsConfig).GetField("selectedMod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            ctx.Require(selected != null, "Page_ModsConfig.selectedMod is unavailable");
            Find.WindowStack.Add(page);
            selected.SetValue(page, Entry(ctx));
            await ctx.WaitFrames(5);
        }

        [When("Joy Rescue: the mod list is closed")]
        public void CloseModList(PickleContext ctx)
        {
            var page = Find.WindowStack.Windows.OfType<Page_ModsConfig>().FirstOrDefault();
            ctx.Assert(page != null, "the mod list is not open");
            Find.WindowStack.TryRemove(page, false);
        }
    }
}
