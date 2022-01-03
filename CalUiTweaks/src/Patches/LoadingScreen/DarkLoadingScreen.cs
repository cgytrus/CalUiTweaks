using CalApi.Patches;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace CalUiTweaks.Patches.LoadingScreen;

// ReSharper disable once UnusedType.Global
internal class DarkLoadingScreen : ConfigurablePatch {
    public DarkLoadingScreen() : base(CalUiTweaksPlugin.instance!.Config, "LoadingScreen", "Dark", false, "") { }

    public override void Apply() => On.Intro.Awake += (orig, self) => {
        orig(self);
        if(!enabled) return;

        Camera camera = Camera.main!;
        Color tempColor = camera.backgroundColor;
        tempColor.r = 0f;
        tempColor.g = 0f;
        tempColor.b = 0f;
        camera.backgroundColor = tempColor;

        Text loadingText = ((RectTransform)AccessTools.Field(typeof(Intro), "loadingText").GetValue(self))
            .GetComponent<Text>();
        tempColor = loadingText.color;
        tempColor.r = 1f;
        tempColor.g = 1f;
        tempColor.b = 1f;
        loadingText.color = tempColor;

        SpriteRenderer logoText = self.transform.Find("Logo Text").GetComponent<SpriteRenderer>();
        tempColor = logoText.color;
        tempColor.r = 1f;
        tempColor.g = 1f;
        tempColor.b = 1f;
        logoText.color = tempColor;

        Transform logoCoverParent = self.transform.Find("Logo Cover Parent");
        foreach(Transform child in logoCoverParent) {
            SpriteRenderer sprite = child.GetComponent<SpriteRenderer>();
            tempColor = sprite.color;
            tempColor.r = 0f;
            tempColor.g = 0f;
            tempColor.b = 0f;
            sprite.color = tempColor;
        }
    };
}