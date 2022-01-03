using CalApi.Patches;

using Cat;

using HarmonyLib;

using UnityEngine;

namespace CalUiTweaks.Patches.Gameplay;

// ReSharper disable once UnusedType.Global
public class CompanionHealthDisplayPatch : ConfigurablePatch {
    public CompanionHealthDisplayPatch() : base(CalUiTweaksPlugin.instance!.Config, "Gameplay",
        "CompanionHealthDisplay",
        true, null) { }

    public override void Apply() => On.GameUI.CheckIfShouldShowCompanionHealth += (orig, self, previousAbility, ability) => {
        ((GameObject)AccessTools.Field(typeof(GameUI), "companionHealthIndicatorParent").GetValue(self))
            .SetActive(enabled);
        orig(self, previousAbility, ability);
    };
}
