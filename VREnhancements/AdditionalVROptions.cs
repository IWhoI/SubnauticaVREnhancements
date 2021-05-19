using HarmonyLib;

namespace VREnhancements
{
    class AdditionalVROptions
    {
        public static int generalTabIndex = 0;
        public static bool immersiveHUD = false;
        public static float subtitleYPos = 40;
        public static float PDA_Distance = 0.28f;
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddGeneralTab))]
        class GeneralTab_VROptionsPatch
        {
            static void Postfix(uGUI_OptionsPanel __instance)
            {
                __instance.AddHeading(generalTabIndex, "Additional VR Options");//add new heading under the General Tab
                __instance.AddToggleOption(generalTabIndex, "Enable VR Animations", GameOptions.enableVrAnimations, delegate (bool v)
                {
                    GameOptions.enableVrAnimations = v;
                    //playerAnimator vr_active is normally set in the Start function of Player so we need to update it if option changed during gameplay
                    if (Player.main != null)
                        Player.main.playerAnimator.SetBool("vr_active", !v);
                });
                __instance.AddToggleOption(generalTabIndex, "Immersive HUD", immersiveHUD, delegate (bool v)
                {
                    immersiveHUD = v;
                    //immediately disable the immersive HUD if option toggled off while the hud was invisible
                    if (!immersiveHUD)
                        ImmersiveHUD.Disable();
                });
                __instance.AddSliderOption(generalTabIndex, "Walk Speed(Default: 60%)", VROptions.groundMoveScale * 100, 50, 100, 60, delegate (float v)
                {
                    VROptions.groundMoveScale = v / 100f;
                });
                __instance.AddSliderOption(generalTabIndex, "Subtitle Height", subtitleYPos, 20, 75, 50, delegate (float v)
                {
                    subtitleYPos = v;
                    UIElementsFixes.SetSubtitleHeight(subtitleYPos);
                });
                __instance.AddSliderOption(generalTabIndex, "PDA Distance", PDA_Distance * 100f, 15, 40, 28, delegate (float v)
                {
                    PDA_Distance = v / 100f;
                });
            }

        }
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab))]
        class AddTab_Patch
        {
            static void Postfix(int __result, string label)
            {
                //get the tabIndex of the general tab to be able to use it in  AddGeneralTab_Postfix
                if (label.Equals("General"))
                    generalTabIndex = __result;
            }
        }
        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeVRSettings))]
        class SerializeVRSettings_Patch
        {
            static void Postfix(GameSettings.ISerializer serializer)
            {
                //for saving the VR animation setting
                GameOptions.enableVrAnimations = serializer.Serialize("VR/EnableVRAnimations", GameOptions.enableVrAnimations);
                VROptions.groundMoveScale = serializer.Serialize("VR/GroundMoveScale", VROptions.groundMoveScale);
                immersiveHUD = serializer.Serialize("VR/ImmersiveHUD", immersiveHUD);
                subtitleYPos = serializer.Serialize("VR/SubtitleYPos", subtitleYPos);
                PDA_Distance = serializer.Serialize("VR/PDA_Distance", PDA_Distance);
            }
        }

    }
}
