using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
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

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
        class HR_LateUpdate_Patch
        {
            static bool Prefix(HandReticle __instance)
            {
                if (Player.main)
                {
                    Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject activeTarget, out float activeHitDistance, null);
                    __instance.SetTargetDistance(activeHitDistance);
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        ErrorMessage.AddDebug("Target/Distance: " + activeTarget.name + "/" + activeHitDistance);
                    }
                }
                return true;
            }
            static void Postfix(HandReticle __instance)
            {
                __instance.transform.position = new Vector3(0f, 0f, __instance.transform.position.z);
            }
        }

        static bool actualGazedBasedCursor;
        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
        class GetCursorScreenPosition_Patch
        {
            static void Postfix(FPSInputModule __instance, ref Vector2 __result)
            {
                if (XRSettings.enabled)
                {
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        __result = GraphicsUtil.GetScreenSize() * 0.5f;
                    }
                    else if (!actualGazedBasedCursor)
                        //fix cursor snapping to middle of view when cursor goes off canvas due to hack in UpdateCursor
                        //Screen.width gives monitor width and GraphicsUtil.GetScreenSize().x will give either monitor or VR eye texture width
                        __result = new Vector2(Input.mousePosition.x / Screen.width * GraphicsUtil.GetScreenSize().x, Input.mousePosition.y / Screen.height * GraphicsUtil.GetScreenSize().y);

                }
            }
        }

        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.UpdateCursor))]
        class UpdateCursor_Patch
        {
            static void Prefix()
            {
                //save the original value so we can set it back in the postfix
                actualGazedBasedCursor = VROptions.gazeBasedCursor;
                //trying make flag in UpdateCursor be true if Cursor.lockState != CursorLockMode.Locked
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    VROptions.gazeBasedCursor = true;
                }

            }
            static void Postfix(FPSInputModule __instance)
            {
                VROptions.gazeBasedCursor = actualGazedBasedCursor;
                //Fix the problem with the cursor rendering behind UI elements.
                Canvas cursorCanvas = __instance._cursor.GetComponentInChildren<Graphic>().canvas;
                RaycastResult lastRaycastResult = Traverse.Create(__instance).Field("lastRaycastResult").GetValue<RaycastResult>();
                if (cursorCanvas && lastRaycastResult.isValid)
                {
                    cursorCanvas.sortingLayerID = lastRaycastResult.sortingLayer;//put the cursor on the same layer as whatever was hit by the cursor raycast.
                }
            }
        }
    }
}
