using BepInEx.Configuration;

using CalApi.Patches;

using HarmonyLib;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CalUiTweaks.Patches.LoadingScreen;

// ReSharper disable once UnusedType.Global
internal class NonBlockingLoadingScreen : IPatch {
    // ReSharper disable once UnusedMember.Local
    private enum LoadingMode { LoadAfterAnimation, LoadWhileAnimatingAndWait, LoadWhileAnimating }

    private readonly ConfigEntry<LoadingMode> _loadingMode;

    public NonBlockingLoadingScreen() => _loadingMode = CalUiTweaksPlugin.instance!.Config.Bind("LoadingScreen",
        "LoadingMode", LoadingMode.LoadAfterAnimation, @"How and when is the game actually loading.
Load After Animation - only load after the animation has finished (vanilla behavior)
Load While Animating And Wait - load while the animation is playing and wait for the animation to finish before switching to the main menu
Load While Animating - load while the animation is playing and switch to the main menu immediately when loaded");

    public void Apply() {
        AsyncOperation? loadingScene = null;
        On.Intro.LoadGame += (orig, self) => {
            if(_loadingMode.Value == LoadingMode.LoadAfterAnimation) return orig(self);
            string gameSceneName = (string)AccessTools.Field(typeof(Intro), "gameSceneName").GetValue(self);
            loadingScene = SceneManager.LoadSceneAsync(gameSceneName);
            loadingScene.allowSceneActivation = _loadingMode.Value == LoadingMode.LoadWhileAnimating;
            return orig(self);
        };

        // if _loadingMode.Value isn't 0 (which is the default, vanilla behavior),
        // we skip the LoadScene call because we already called it before the coroutine
        IDetour loadGameDetour =
            new ILHook(AccessTools.Method(AccessTools.TypeByName("<LoadGame>d__7"), "MoveNext"), il => {
                ILCursor cursor = new(il);
                cursor.GotoNext(code => code.MatchCall<SceneManager>("LoadScene"));
                cursor.Index++; // the last instruction
                ILLabel endLabel = cursor.MarkLabel();
                cursor.Index -= 3; // before the LoadScene call

                // loadingScene.allowSceneActivation = true;
                cursor.EmitReference(loadingScene);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit<AsyncOperation>(OpCodes.Call, $"set_{nameof(loadingScene.allowSceneActivation)}");

                // if(_loadingMode.Value == LoadingMode.LoadAfterAnimation) return;
                cursor.EmitReference((int)_loadingMode.Value);
                cursor.Emit(OpCodes.Brtrue, endLabel);
            });
        loadGameDetour.Apply();
    }
}
