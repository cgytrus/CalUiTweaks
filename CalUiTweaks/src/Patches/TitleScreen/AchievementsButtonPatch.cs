extern alias SteamworksPosix64;
extern alias SteamworksWin64;

using CalApi.Patches;

using HarmonyLib;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using UnityEngine;

namespace CalUiTweaks.Patches.TitleScreen;

// ReSharper disable once UnusedType.Global
public class AchievementsButtonPatch : ConfigurablePatch {
    public AchievementsButtonPatch() : base(CalUiTweaksPlugin.instance!.Config, "TitleScreen", "AchievementsButton",
        false, "There is a hidden achievements button that does nothing.\nRequires restart to take effect.") { }

    public override void Apply() {
        IDetour startDetour =
            new ILHook(AccessTools.Method(AccessTools.TypeByName("<Start>d__16"), "MoveNext"), il => {
                ILCursor cursor = new(il);
                cursor.GotoNext(code => code.MatchCallvirt<GameObject>(nameof(GameObject.SetActive)));
                cursor.Index--;
                cursor.Remove();
                cursor.EmitReference(enabled);
            });
        startDetour.Apply();

        On.TitleScreen.AchievementsButton += (orig, self) => {
            orig(self); // useless but just in case
            if(Application.platform == RuntimePlatform.WindowsPlayer)
                SteamworksWin64::Steamworks.SteamFriends.OpenOverlay("achievements");
            else SteamworksPosix64::Steamworks.SteamFriends.OpenOverlay("achievements");
            //DialogBoxUtility.DialogBoxCreator.Create(DialogBox.Type.Neutral, "hi", "69");
        };
    }
}
