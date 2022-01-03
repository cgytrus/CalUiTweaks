using CalApi.Patches;

using HarmonyLib;

using UnityEngine;

namespace CalUiTweaks.Patches.Gameplay;

// ReSharper disable once UnusedType.Global
public class CatHealthDisplayPatch : ConfigurablePatch {
    public CatHealthDisplayPatch() : base(CalUiTweaksPlugin.instance!.Config, "Gameplay", "CatHealthDisplay",
        true, null) { }

    public override void Apply() => On.GameUI.CheckIfShouldShowCompanionHealth += (orig, self, previousAbility, ability) => {
        ((GameObject)AccessTools.Field(typeof(GameUI), "catHealthIndicatorsParent").GetValue(self))
            .SetActive(enabled);
        orig(self, previousAbility, ability);
    };
}
