using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace VREnhancements
{
    class AdditionalVROptions
    {
        //TODO: These should probably be private with public getters
        public static int generalTabIndex = 0;
        public static bool DynamicHUD = false;
        public static float subtitleYPos = 40;
        public static float subtitleScale = 1;
        public static float PDA_Distance = 0.28f;
        public static float HUD_Alpha = 1;
        public static float HUD_Distance = 1;
        public static float HUD_Scale = 1;
        
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddGeneralTab))]
        class GeneralTab_VROptionsPatch
        {
            //TODO: Create a new tab instead of using general for all the additional VR options and if possible move existing VR to the same tab
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
                __instance.AddSliderOption(generalTabIndex, "Walk Speed(Default: 60%)", VROptions.groundMoveScale * 100, 50, 100, 60, delegate (float v)
                {
                    VROptions.groundMoveScale = v / 100f;
                });
                __instance.AddSliderOption(generalTabIndex, "Subtitle Height", subtitleYPos, 20, 75, 50, delegate (float v)
                {
                    subtitleYPos = v;
                    UIElementsFixes.SetSubtitleHeight(subtitleYPos);
                });
                __instance.AddSliderOption(generalTabIndex, "Subtitle Scale", subtitleScale * 100, 50, 150, 100, delegate (float v)
                {
                    subtitleScale = v / 100;
                    UIElementsFixes.SetSubtitleScale(subtitleScale);
                });
                __instance.AddSliderOption(generalTabIndex, "PDA Distance", PDA_Distance * 100f, 25, 40, 40, delegate (float v)
                {
                    PDA_Distance = v / 100f;
                });
                __instance.AddHeading(generalTabIndex, "VR HUD Options");//add new heading under the General Tab
                __instance.AddToggleOption(generalTabIndex, "Dynamic HUD", DynamicHUD, delegate (bool v)
                {
                    DynamicHUD = v;
                    if(!DynamicHUD)
                        VRHUD.UpdateHUDOpacity(HUD_Alpha);

                });
                __instance.AddSliderOption(generalTabIndex, "HUD Opacity", HUD_Alpha * 100f, 20, 100, 100, delegate (float v)
                {
                    HUD_Alpha = v / 100f;
                    VRHUD.UpdateHUDOpacity(HUD_Alpha);
                });
                __instance.AddSliderOption(generalTabIndex, "HUD Distance", HUD_Distance / 0.5f, 2, 4, 3, delegate (float v)
                {
                    HUD_Distance = v * 0.5f;
                    VRHUD.UpdateHUDDistance(HUD_Distance);
                });
                __instance.AddSliderOption(generalTabIndex, "HUD Scale", HUD_Scale / 0.5f, 1, 4, 2, delegate (float v)
                {
                    HUD_Scale = v * 0.5f;
                    VRHUD.UpdateHUDScale(HUD_Scale);
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

        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.Awake))]
        class IGM_Awake_Patch
        {
            private static Button recenterVRButton;
            //code copied from the quit to desktop mod and modified
            static void Postfix(IngameMenu __instance)
            {
                if (__instance != null && recenterVRButton == null)
                {
                    //Clone the quitToMainMenuButton and update it
                    Button menuButton = __instance.quitToMainMenuButton.transform.parent.GetChild(0).gameObject.GetComponent<Button>();
                    recenterVRButton = UnityEngine.Object.Instantiate<Button>(menuButton, __instance.quitToMainMenuButton.transform.parent);
                    recenterVRButton.transform.SetSiblingIndex(1);//put the button in the second position in the menu
                    recenterVRButton.name = "RecenterVR";
                    recenterVRButton.onClick.RemoveAllListeners();//remove cloned listeners
                    //add new listener
                    recenterVRButton.onClick.AddListener(delegate ()
                    {
                        VRUtil.Recenter();
                    });
                    //might be a better way to replace the text of the copied button
                    IEnumerable<Text> enumerable = recenterVRButton.GetComponents<Text>().Concat(recenterVRButton.GetComponentsInChildren<Text>());
                    foreach (Text text in enumerable)
                    {
                        text.text = "Recenter VR";
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeVRSettings))]
        class SerializeVRSettings_Patch
        {
            static void Postfix(GameSettings.ISerializer serializer)
            {
                //TODO: Serialize all additional options
                GameOptions.enableVrAnimations = serializer.Serialize("VR/EnableVRAnimations", GameOptions.enableVrAnimations);
                VROptions.groundMoveScale = serializer.Serialize("VR/GroundMoveScale", VROptions.groundMoveScale);
                subtitleScale = serializer.Serialize("VR/SubtitleScale", subtitleScale);
                subtitleYPos = serializer.Serialize("VR/SubtitleYPos", subtitleYPos);
                PDA_Distance = serializer.Serialize("VR/PDA_Distance", PDA_Distance);
                DynamicHUD = serializer.Serialize("VR/DynamicHUD", DynamicHUD);
                HUD_Distance = serializer.Serialize("VR/HUD_Distance", HUD_Distance);
                HUD_Scale = serializer.Serialize("VR/HUD_Scale", HUD_Scale);
                HUD_Alpha = serializer.Serialize("VR/HUD_Alpha", HUD_Alpha);
            }
        }

    }
}
