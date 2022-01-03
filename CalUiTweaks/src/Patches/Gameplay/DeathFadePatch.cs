using CalApi.Patches;

namespace CalUiTweaks.Patches.Gameplay;

// ReSharper disable once UnusedType.Global
public class DeathFadePatch : ConfigurablePatch {
    public DeathFadePatch() : base(CalUiTweaksPlugin.instance!.Config, "Gameplay", "DeathFade",
        true, "Toggle the fade to white on death.") { }

    public override void Apply() => On.GameUI.DeathFade += (orig, self) => {
        if(!enabled) return;
        orig(self);
    };
}
