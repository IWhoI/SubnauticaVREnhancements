using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

namespace VREnhancements
{
    class AdditionalVROptions
    {    
        static int VRETab;
        
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
        class GeneralTab_VROptionsPatch
        {
            //TODO: Create a new tab instead of using general for all the additional VR options and if possible move existing VR to the same tab
            static void Postfix(uGUI_OptionsPanel __instance)
            {
                VRETab = __instance.AddTab("VR Mod Options");
                __instance.AddHeading(VRETab, "General Options");
                __instance.AddToggleOption(VRETab, "Enable VR Animations", GameOptions.enableVrAnimations, delegate (bool v)
                {
                    GameOptions.enableVrAnimations = v;
                    //playerAnimator vr_active is normally set in the Start function of Player so we need to update it if option changed during gameplay
                    if (Player.main)
                        Player.main.playerAnimator.SetBool("vr_active", !v);
                });                
                __instance.AddSliderOption(VRETab, "Walk Speed(Default: 60%)", VROptions.groundMoveScale * 100, 50, 100, 60, delegate (float v)
                {
                    VROptions.groundMoveScale = v / 100f;
                });
                __instance.AddHeading(VRETab, "User Interface Options");
                __instance.AddSliderOption(VRETab, "Subtitle Height", UIElementsFixes.subtitleHeight, 20, 75, 60, delegate (float v)
                {
                    UIElementsFixes.SetSubtitleHeight(v);
                });
                __instance.AddSliderOption(VRETab, "Subtitle Scale", UIElementsFixes.subtitleScale * 100, 50, 150, 100, delegate (float v)
                {
                    UIElementsFixes.SetSubtitleScale(v / 100);
                });
                __instance.AddSliderOption(VRETab, "PDA Distance", PDAFixes.pdaDistance * 100f, 25, 40, 40, delegate (float v)
                {
                    PDAFixes.pdaDistance = v / 100f;
                });
                __instance.AddToggleOption(VRETab, "Dynamic HUD", UIElementsFixes.dynamicHUD, delegate (bool v)
                {
                    UIElementsFixes.SetDynamicHUD(v);

                });
                __instance.AddSliderOption(VRETab, "HUD Opacity", UIElementsFixes.HUDAlpha * 100f, 20, 100, 75, delegate (float v)
                {
                    UIElementsFixes.UpdateHUDOpacity(v / 100);
                });
                __instance.AddSliderOption(VRETab, "HUD Distance", UIElementsFixes.HUDDistance / 0.5f, 2, 4, 3, delegate (float v)
                {
                    UIElementsFixes.UpdateHUDDistance(v * 0.5f);
                });
                __instance.AddSliderOption(VRETab, "HUD Scale", UIElementsFixes.HUDScale / 0.5f, 1, 4, 2, delegate (float v)
                {
                    UIElementsFixes.UpdateHUDScale(v * 0.5f);
                });
            }

        }

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
                UIElementsFixes.subtitleScale = serializer.Serialize("VR/SubtitleScale", UIElementsFixes.subtitleScale);
                UIElementsFixes.subtitleHeight = serializer.Serialize("VR/SubtitleHeight", UIElementsFixes.subtitleHeight);
                PDAFixes.pdaDistance = serializer.Serialize("VR/PDA_Distance", PDAFixes.pdaDistance);
                UIElementsFixes.dynamicHUD = serializer.Serialize("VR/DynamicHUD", UIElementsFixes.dynamicHUD);
                UIElementsFixes.HUDDistance = serializer.Serialize("VR/HUD_Distance", UIElementsFixes.HUDDistance);
                UIElementsFixes.HUDScale = serializer.Serialize("VR/HUD_Scale", UIElementsFixes.HUDScale);
                UIElementsFixes.HUDAlpha = serializer.Serialize("VR/HUD_Alpha", UIElementsFixes.HUDAlpha);
                Debug.Log("VR Enhancements Settings Serialized");
            }
        }

    }
}
