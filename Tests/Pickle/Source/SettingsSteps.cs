using System;
using System.IO;
using System.Reflection;
using RimWorld;
using RimWorks.Pickle;
using Verse;

namespace JoyRescue.PickleSteps
{
    // Runtime-only checks. The offline runner already proves configuration rules and XML resources.
    [PickleSteps]
    public sealed class SettingsSteps
    {
        private const BindingFlags InstanceAny = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticAny = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static JoyRescueMod Mod(PickleContext ctx)
        {
            var mod = JoyRescueMod.Instance;
            ctx.Require(mod != null, "Joy Rescue is not a loaded mod");
            return mod;
        }

        private static MainButtonDef Button(PickleContext ctx, string defName)
        {
            var button = DefDatabase<MainButtonDef>.GetNamedSilentFail(defName);
            ctx.Require(button != null, $"no MainButtonDef named '{defName}'");
            return button;
        }

        [When("I open the Joy Rescue settings dialog")]
        public async System.Threading.Tasks.Task OpenSettings(PickleContext ctx)
        {
            Find.WindowStack.Add(new Dialog_ModSettings(Mod(ctx)));
            await ctx.WaitFrames(3);
        }

        [Then("the Joy Rescue settings dialog is open")]
        public void AssertSettingsDialog(PickleContext ctx)
        {
            foreach (var window in Find.WindowStack.Windows)
            {
                if (!(window is Dialog_ModSettings)) continue;
                foreach (var field in typeof(Dialog_ModSettings).GetFields(InstanceAny))
                {
                    if (typeof(Mod).IsAssignableFrom(field.FieldType)
                        && ReferenceEquals(field.GetValue(window), Mod(ctx))) return;
                }
            }
            ctx.Assert(false, "no Dialog_ModSettings for Joy Rescue is open");
        }

        [Then("Joy Rescue MainButtonDef {string} is hidden and not greyed")]
        public void AssertHidden(PickleContext ctx, string defName)
        {
            var button = Button(ctx, defName);
            ctx.Assert(!button.Worker.Visible,
                $"{defName} is visible before a customization mod reveals it");
            ctx.Assert(!button.Worker.Disabled,
                $"{defName} is greyed although a hidden shortcut must be usable once revealed");
        }

        [When("Joy Rescue activates MainButtonDef {string}")]
        public async System.Threading.Tasks.Task ActivateShortcut(PickleContext ctx, string defName)
        {
            Button(ctx, defName).Worker.Activate();
            await ctx.WaitFrames(3);
        }

        [When("Joy Rescue setting {string} is set to {string}")]
        public void SetBoolean(PickleContext ctx, string field, string value)
        {
            ctx.Require(bool.TryParse(value, out var parsed), $"'{value}' is not true or false");
            var target = typeof(JoyRescueSettings).GetField(field);
            ctx.Require(target != null && target.FieldType == typeof(bool),
                $"JoyRescueSettings has no Boolean field '{field}'");
            target.SetValue(JoyRescueMod.Settings, parsed);
        }

        [Then("Joy Rescue setting {string} reads {string}")]
        public void AssertBoolean(PickleContext ctx, string field, string expected)
        {
            ctx.Require(bool.TryParse(expected, out var wanted), $"'{expected}' is not true or false");
            var target = typeof(JoyRescueSettings).GetField(field);
            ctx.Require(target != null && target.FieldType == typeof(bool),
                $"JoyRescueSettings has no Boolean field '{field}'");
            var actual = (bool)target.GetValue(JoyRescueMod.Settings);
            ctx.Assert(actual == wanted, $"JoyRescueSettings.{field} is {actual}, expected {wanted}");
        }

        [When("Joy Rescue settings are written")]
        public void WriteSettings(PickleContext ctx) => Mod(ctx).WriteSettings();

        // These scenarios must never retain an audit value in the shared WSL profile.  The
        // backup also survives a killed run: the next scenario restores it before continuing.
        [BeforeScenario("@joyrescue-sandbox")]
        public void BackUpAndReset(PickleContext ctx)
        {
            var settingsPath = SettingsPath(ctx);
            var backup = BackupPath(ctx);
            if (File.Exists(backup)) RestoreBackup(ctx);
            Mod(ctx).WriteSettings();
            if (File.Exists(settingsPath)) File.Copy(settingsPath, backup, false);
            JoyRescueMod.Settings.Reset();
            Mod(ctx).WriteSettings();
        }

        [AfterScenario("@joyrescue-sandbox")]
        public void Restore(PickleContext ctx)
        {
            if (File.Exists(BackupPath(ctx))) RestoreBackup(ctx);
        }

        private static string SettingsPath(PickleContext ctx)
        {
            var method = typeof(LoadedModManager).GetMethod("GetSettingsFilename", StaticAny);
            ctx.Require(method != null, "LoadedModManager.GetSettingsFilename is unavailable");
            return (string)method.Invoke(null, new object[] { Mod(ctx).Content.FolderName, typeof(JoyRescueMod).Name });
        }

        private static string BackupPath(PickleContext ctx) => SettingsPath(ctx) + ".pickle-backup";

        private static void RestoreBackup(PickleContext ctx)
        {
            var path = SettingsPath(ctx);
            var backup = BackupPath(ctx);
            File.Copy(backup, path, true);
            File.Delete(backup);
            var mod = Mod(ctx);
            var modSettings = typeof(Mod).GetField("modSettings", InstanceAny);
            ctx.Require(modSettings != null, "Verse.Mod.modSettings is unavailable");
            modSettings.SetValue(mod, null);
            var restored = mod.GetSettings<JoyRescueSettings>();
            var property = typeof(JoyRescueMod).GetProperty("Settings", StaticAny);
            ctx.Require(property != null, "JoyRescueMod.Settings is unavailable");
            property.SetValue(null, restored, null);
        }
    }
}
