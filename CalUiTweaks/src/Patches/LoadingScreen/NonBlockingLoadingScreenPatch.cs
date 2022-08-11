using BepInEx.Configuration;

using CalApi.Patches;

using HarmonyLib;

using JetBrains.Annotations;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using UnityEngine;

namespace CalUiTweaks.Patches.LoadingScreen;

[UsedImplicitly]
internal class NonBlockingLoadingScreenPatch : IPatch {
    private readonly ConfigEntry<bool> _waitForAnimation;

    public NonBlockingLoadingScreenPatch() => _waitForAnimation = CalUiTweaksPlugin.instance!.Config.Bind("LoadingScreen",
        "WaitForAnimation", true, @"Whether to wait for the animation to finish when loading the game.
Enabled - wait for the animation to finish before switching to the main menu (vanilla behavior)
Disabled - switch to the main menu immediately when loaded");

    public void Apply() {
        IDetour loadGameDetour =
            new ILHook(AccessTools.Method(AccessTools.TypeByName("<LoadGame>d__8"), "MoveNext"), il => {
                ILCursor cursor = new(il);

                // allowSceneActivation.allowSceneActivation = !_waitForAnimation.Value;
                cursor.GotoNext(code =>
                    code.MatchCallvirt<AsyncOperation>($"set_{nameof(AsyncOperation.allowSceneActivation)}"));
                cursor.Index--;
                cursor.EmitReference(_waitForAnimation.Value);
                cursor.Index++;
                cursor.Emit(OpCodes.Ceq);
            });
        loadGameDetour.Apply();
    }
}
