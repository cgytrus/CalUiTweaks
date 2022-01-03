using BepInEx;

using CalApi.API;

namespace CalUiTweaks;

[BepInPlugin("mod.cgytrus.plugins.calUiTweaks", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("mod.cgytrus.plugins.calapi", "0.2.5")]
public class CalUiTweaksPlugin : BaseUnityPlugin {
    public static CalUiTweaksPlugin? instance { get; private set; }

    public CalUiTweaksPlugin() => instance = this;

    private void Awake() {
        Logger.LogInfo("Applying patches");
        Util.ApplyAllPatches();
    }
}