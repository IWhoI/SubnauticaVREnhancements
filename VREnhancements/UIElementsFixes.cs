using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace VREnhancements
{
    class UIElementsFixes
    {
        public static void SetSubtitleHeight(float percentage)
        {
            Subtitles.main.popup.oy = GraphicsUtil.GetScreenSize().y * percentage / 100;
        }
        
        
        //Adjusting the scale is also changing the position. See uGUI_PopupMessage GetCoords to work out how to fix this.
        public static void SetSubtitleScale(float scale)
        {
            Subtitles.main.popup.GetComponent<RectTransform>().localScale = Vector3.one * scale;
        }

        [HarmonyPatch(typeof(Subtitles), nameof(Subtitles.Start))]
        class SubtitlesPosition_Patch
        {
            //Bring up the subtitles into view while in VR
            static void Postfix(Subtitles __instance)
            {
                __instance.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);//to keep subtitles centered when scaling.
                SetSubtitleHeight(AdditionalVROptions.subtitleYPos);                
                SetSubtitleScale(AdditionalVROptions.subtitleScale);
            }
        }

        [HarmonyPatch(typeof(uGUI_SunbeamCountdown), nameof(uGUI_SunbeamCountdown.Start))]
        class SunbeamCountdown_Start_Patch
        {
            public static void Postfix(uGUI_SunbeamCountdown __instance)
            {
                Vector2 midCenter = new Vector2(0.5f, 0.5f);
                __instance.countdownHolder.GetComponent<RectTransform>().anchorMax = midCenter;
                __instance.countdownHolder.GetComponent<RectTransform>().anchorMin = midCenter;
                __instance.countdownHolder.GetComponent<RectTransform>().pivot = midCenter;
                __instance.countdownHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -275f);
                __instance.countdownHolder.GetComponent<RectTransform>().localScale = Vector3.one * 0.75f;
                __instance.transform.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0f);
            }

        }

        [HarmonyPatch(typeof(uGUI_CameraDrone), nameof(uGUI_CameraDrone.Awake))]
        class CameraDrone_Awake_Patch
        {
            //Reduce the size of the HUD in the Drone Camera to make the health and energy bars visible
            //Look into moving the HUD further back instead of scaling it down.
            static void Postfix(uGUI_CameraDrone __instance)
            {
                GameObject droneCamera = __instance.transform.Find("Content").Find("CameraScannerRoom").gameObject;
                if (droneCamera != null)
                {
                    droneCamera.GetComponent<RectTransform>().localScale = new Vector3(0.6f, 0.6f, 1f);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_CameraCyclops), nameof(uGUI_CameraCyclops.Awake))]
        class CameraCyclops_Awake_Patch
        {
            static void Postfix(uGUI_CameraCyclops __instance)
            {
                GameObject cyclopsCamera = __instance.transform.Find("Content").Find("CameraCyclops").gameObject;
                if (cyclopsCamera != null)
                {
                    cyclopsCamera.GetComponent<RectTransform>().localScale = new Vector3(0.7f, 0.7f, 1f);
                    return;
                }
            }
        }
        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake))]
        class MM_Awake_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                //shift the main menu up a little. Fix this. Possibly make the menu track the players head with a delay.
                GameObject mainMenu = __instance.transform.Find("Panel").Find("MainMenu").gameObject;
                if (mainMenu != null)
                {
                    mainMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 385);
                    return;
                }
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
        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
        class HR_LateUpdate_Patch
        {
            static bool Prefix(HandReticle __instance)
            {
                Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject activeTarget, out float activeHitDistance, null);
                __instance.SetTargetDistance(activeHitDistance);    
                // Traverse.Create(__instance).Field("targetDistance").SetValue(activeHitDistance);
                if (Input.GetKeyUp(KeyCode.P))
                {
                    ErrorMessage.AddDebug("Target/Distance: " + activeTarget.name + "/" + activeHitDistance);
                }
                return true;
            }
        }
    }
}
