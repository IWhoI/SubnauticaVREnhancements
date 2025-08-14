using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using BepInEx;
using BepInEx.Configuration;
using static OVRHaptics;

namespace VREnhancements
{
    class AdditionalVROptions
    {
        static int generalTabIndex = -1;
        //using BepInEx config system to save config. I'm not sure if there is a better way to do this but it works.
        public static ConfigEntry<bool> dynamicHUD;
        public static ConfigEntry<bool> enableVRAnimations;
        //public static ConfigEntry<bool> disableInputPitch;
        public static ConfigEntry<float> walkingSpeed;
        public static ConfigEntry<float> PDA_Distance;
        public static ConfigEntry<float> HUD_Alpha;
        public static ConfigEntry<float> HUD_Distance;
        public static ConfigEntry<float> HUD_Scale;
        public static ConfigEntry<int> HUD_Separation;

        //this will load/create the configuration values in the VREnhancements.cfg file in the BepInEx config folder.
        public static void LoadVRConfig()
        {
            enableVRAnimations = MainPatcher.VRConfig.Bind("General", "enableVRAnimations", true, "Wether or not animations for climbing ladders etc are enabled");
            GameOptions.enableVrAnimations = enableVRAnimations.Value;
            walkingSpeed = MainPatcher.VRConfig.Bind("General", "walkingSpeed", 1.0f, "Default VR walking speed is 60%(0.6) of the base game walk speed.");
            VROptions.groundMoveScale = walkingSpeed.Value;
            /* disabled this since I didn't want to take the time to figure out how to fix the problem where the input pitch direction changes if
             * recentering is done while your head is rotated left or right.
            disableInputPitch = MainPatcher.VRConfig.Bind("Input", "disableInputPitch", true, "Wether or not looking up and down is possible with an input device");
            VROptions.disableInputPitch = disableInputPitch.Value;
            */
            dynamicHUD = MainPatcher.VRConfig.Bind("UI", "dynamicHUD", true, "Wether or not the dynamic HUD is enabled");            
            PDA_Distance = MainPatcher.VRConfig.Bind("UI", "PDA_Distance", 0.4f, "The distance that the PDA is held");
            HUD_Alpha = MainPatcher.VRConfig.Bind("UI", "HUD_Alpha", 1.0f, "Opacity of the HUD. 1 is fully opaque");
            HUD_Distance = MainPatcher.VRConfig.Bind("UI", "HUD_Distance", 1.5f, "Distance of the HUD in meters");
            HUD_Scale = MainPatcher.VRConfig.Bind("UI", "HUD_Scale", 1.0f, "Size of the HUD");
            HUD_Separation = MainPatcher.VRConfig.Bind("UI", "HUD_Separation", 0, "Preset for the spacing between elements of the HUD. 0 - Default, 1-Small, 2-Medium, 3-Large");
        }

        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab))]
        class AddTab_Patch
        {
            static void Postfix(int __result, string label)
            {
                //get the tabIndex of the general tab to be able to use it in AddGeneralTab_Postfix
                if (label.Equals("General"))
                    generalTabIndex = __result;
            }
        }

        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
        class GeneralTab_VROptionsPatch
        {
            static void Postfix(uGUI_OptionsPanel __instance)
            {
                __instance.AddHeading(generalTabIndex, "General VR Options");
                __instance.AddToggleOption(generalTabIndex, "Enable VR Animations", GameOptions.enableVrAnimations, delegate (bool v)
                {
                    enableVRAnimations.Value = GameOptions.enableVrAnimations = v;
                    //playerAnimator vr_active is normally set in the Start function of Player so we need to update it if option changed during gameplay
                    if (Player.main)
                        Player.main.playerAnimator.SetBool("vr_active", !v);
                });
                __instance.AddSliderOption(generalTabIndex, "Walk Speed(VR Default: 60%)", VROptions.groundMoveScale * 100, 50, 100, (float)walkingSpeed.DefaultValue * 100, 1f, delegate (float v)
                {
                   walkingSpeed.Value = VROptions.groundMoveScale = v / 100f;
                }, SliderLabelMode.Float, "F0");
                /* see note in LoadConfig for why this is disabled
                __instance.AddToggleOption(generalTabIndex, "Disable Vertical Input", disableInputPitch.Value, delegate (bool v)
                {
                    disableInputPitch.Value = VROptions.disableInputPitch = v;
                    VRUtil.Recenter();
                });*/
                __instance.AddHeading(generalTabIndex, "VR User Interface Options");
                __instance.AddSliderOption(generalTabIndex, "PDA Distance", PDA_Distance.Value * 100f, 25, 40, (float)PDA_Distance.DefaultValue * 100, 1f, delegate (float v)
                {
                    PDA_Distance.Value = v / 100f;
                    PDAFixes.SetPDADistance(PDA_Distance.Value);
                }, SliderLabelMode.Float, "F0");
                __instance.AddToggleOption(generalTabIndex, "Dynamic HUD", dynamicHUD.Value, delegate (bool v)
                {
                    dynamicHUD.Value = v;
                    UIElementsFixes.SetDynamicHUD(v);
                });
                __instance.AddSliderOption(generalTabIndex, "HUD Opacity %", HUD_Alpha.Value * 100f, 40, 100, (float)HUD_Alpha.DefaultValue*100, 1f, delegate (float v)
                {
                    HUD_Alpha.Value = v / 100f;
                    UIElementsFixes.UpdateHUDOpacity(HUD_Alpha.Value);
                }, SliderLabelMode.Float, "F0");
                __instance.AddSliderOption(generalTabIndex, "HUD Distance (cm)", HUD_Distance.Value * 100f, 75, 200f, (float)HUD_Distance.DefaultValue * 100, 1f, delegate (float v)
                {
                    HUD_Distance.Value = v / 100f;
                    UIElementsFixes.UpdateHUDDistance(HUD_Distance.Value);
                }, SliderLabelMode.Float, "F0");
                __instance.AddSliderOption(generalTabIndex, "HUD Scale %", HUD_Scale.Value * 100f, 50, 200, (float)HUD_Scale.DefaultValue * 100, 1f, delegate (float v)
                {
                    HUD_Scale.Value = v / 100f;
                    UIElementsFixes.UpdateHUDScale(HUD_Scale.Value);
                }, SliderLabelMode.Float, "F0");
                __instance.AddChoiceOption(generalTabIndex, "HUD Separation", new string[] { "Default", "Small", "Medium", "Large" }, HUD_Separation.Value, delegate (int separation)
                {
                    HUD_Separation.Value = separation;
                    UIElementsFixes.UpdateHUDSeparation(separation);
                });
            }
        }
        //Adds Recenter VR button to the in game menu.
        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.Awake))]
        class IGM_Awake_Patch
        {
            private static Button recenterVRButton;
            //code copied from the quit to desktop mod and modified
            static void Postfix(IngameMenu __instance)
            {
                if (__instance && recenterVRButton == null)
                {
                    //Clone the quitToMainMenuButton and update it
                    Button menuButton = __instance.quitToMainMenuButton.transform.parent.GetChild(0).gameObject.GetComponent<Button>();
                    recenterVRButton = UnityEngine.Object.Instantiate<Button>(menuButton, __instance.quitToMainMenuButton.transform.parent);
                    recenterVRButton.transform.SetSiblingIndex(1);//put the button in the second position in the menu
                    recenterVRButton.name = "RecenterVR";
                    recenterVRButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Recenter VR");
                    recenterVRButton.onClick.RemoveAllListeners();//remove cloned listeners
                    //add new listener
                    recenterVRButton.onClick.AddListener(delegate ()
                    {
                        VRUtil.Recenter();
                    });
                    
                }
            }
        }

        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeVRSettings))]
        class SerializeVRSettings_Patch
        {
            static void Postfix(GameSettings.ISerializer serializer)
            {                
                LoadVRConfig();
            }
        }

    }
}
