using System;
using System.Collections;

using CalApi.Patches;

using HarmonyLib;

namespace CalUiTweaks.Patches.LoadingScreen;

// ReSharper disable once UnusedType.Global
internal class NoStartupWarningPatch : ConfigurablePatch {
    private static readonly Action<SetupUI> loadIntro =
        (Action<SetupUI>)Delegate.CreateDelegate(typeof(Action<SetupUI>),
            AccessTools.Method(typeof(SetupUI), "LoadIntro"));
    public NoStartupWarningPatch() : base(CalUiTweaksPlugin.instance!.Config, "LoadingScreen", "Enabled", true,
        "Toggle the warning that shows on game startup.") { }

    public override void Apply() {
        On.SetupUI.Start += (orig, self) => {
            if(enabled) return orig(self);
            loadIntro(self);
            IEnumerator Break() { yield break; }
            return Break();
        };

        On.SetupUI.Update += (orig, self) => { if(enabled) orig(self); };
    }
}