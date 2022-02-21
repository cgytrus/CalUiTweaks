using CalApi.Patches;

using HarmonyLib;

using JetBrains.Annotations;

using UnityEngine;

namespace CalUiTweaks.Patches.Gameplay;

[UsedImplicitly]
internal class CatHealthDisplayPatch : ConfigurablePatch {
    public CatHealthDisplayPatch() : base(CalUiTweaksPlugin.instance!.Config, "Gameplay", "CatHealthDisplay",
        true, null) { }

    public override void Apply() => On.GameUI.CheckIfShouldShowCompanionHealth += (orig, self, previousAbility, ability) => {
        ((GameObject)AccessTools.Field(typeof(GameUI), "catHealthIndicatorsParent").GetValue(self))
            .SetActive(enabled);
        orig(self, previousAbility, ability);
    };
}
