using CalApi.Patches;

using JetBrains.Annotations;

namespace CalUiTweaks.Patches.Gameplay;

[UsedImplicitly]
internal class DeathFadePatch : ConfigurablePatch {
    public DeathFadePatch() : base(CalUiTweaksPlugin.instance!.Config, "Gameplay", "DeathFade",
        true, "Toggle the fade to white on death.") { }

    public override void Apply() => On.GameUI.DeathFade += (orig, self) => {
        if(!enabled) return;
        orig(self);
    };
}
