using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.PostProcessing;
using System.Collections.Generic;
using System.Linq;

namespace VREnhancements
{
    class UIElementsFixes
    {
        //TODO: Alot of this is messy and could probably be done better.
        static RectTransform CameraCyclopsHUD;
        static RectTransform CameraDroneHUD;
        static float CameraHUDScaleFactor = 0.6f;        
        private static List<GameObject> DynamicHUDElements = new List<GameObject>();
        public static uGUI_SceneHUD sceneHUD;

        //Add an element by name to the HUD Elements List.
        public static bool AddHUDElement(string name)
        {
            GameObject element = GameObject.Find(name);
            if (element && !DynamicHUDElements.Contains(element))
            {
                DynamicHUDElements.Add(element);
                return true;
            }
            return false;
        }
        public static void UpdateHUDOpacity(float alpha)
        {
            //using the DynamicHUDElements list to set the alpha of elements that are not affected by the HUD CanvasGroup. eg Sunbeam Timer
            foreach (GameObject element in DynamicHUDElements)
            {
                if (element)
                    foreach (CanvasRenderer renderer in element.GetComponentsInChildren<CanvasRenderer>())
                    {
                        //there has to be a better way to do this. This is to maintain the invisible Sunbeam background set in UIElementsFixes
                        if (!(renderer.transform.parent.name == "SunbeamCountdown" && renderer.name == "Background"))
                            renderer.SetAlpha(alpha);
                    }
            }
            if (sceneHUD.GetComponent<CanvasGroup>())
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
            CanvasGroup HandReticleCG = HandReticle.main.GetComponent<CanvasGroup>();
            if (HandReticleCG)
            {
                HandReticleCG.ignoreParentGroups = true;//not sure if this will cause issues when changes are made to the ScreenCanvas CanvasGroup
                HandReticleCG.alpha = 1;
            }

        }
        public static void UpdateHUDDistance(float distance)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponentInParent<Canvas>().transform.position =
                    new Vector3(sceneHUD.GetComponentInParent<Canvas>().transform.position.x,
                    sceneHUD.GetComponentInParent<Canvas>().transform.position.y,
                    distance);
            }
        }
        public static void UpdateHUDScale(float scale)
        {
            if (sceneHUD)
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
        }

        static void InitHUD()
        {
            AddHUDElement("SunbeamCountdown");
            UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            UpdateHUDScale(AdditionalVROptions.HUD_Scale);
        }


        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.End))]
        class SceneLoading_End_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {
                //only update HUD parameters after loading to make sure AdditionalVROptions are loaded first. Might have a better way to do this.
                InitHUD();
            }
        }


        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;//keep a reference to HUD
                //add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
                sceneHUD.gameObject.AddComponent<CanvasGroup>();
            }
        }


        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                //add CanvasGroup to the HandReticle to be able to override the HUD CanvasGroup alpha settings to keep the Reticle always opaque.
                if (HandReticle.main)
                    HandReticle.main.gameObject.AddComponent<CanvasGroup>();
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Update))]
        class SceneHUD_Update_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if(uGUI_CameraDrone.main.content.activeInHierarchy || uGUI_CameraCyclops.main.content.activeInHierarchy)
                {
                    UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
                    return;
                }

                if (AdditionalVROptions.DynamicHUD && MainCamera.camera)
                {
                    //fades the hud in based on the angle that the player is looking in. Straight up is 270 and forward is 360/0
                    if (MainCamera.camera.transform.localEulerAngles.x < 180)
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - 30) / 15, 0, 1) * AdditionalVROptions.HUD_Alpha);
                    else
                        UpdateHUDOpacity(0);
                }
            }
        }
        //TODO: I think there might be a better way to do this than setting the oy value. See SetSubtitleScale.
        public static void SetSubtitleHeight(float percentage)
        {
            Subtitles.main.popup.oy = GraphicsUtil.GetScreenSize().y * percentage / 100;
        }
        public static void SetSubtitleScale(float scale)
        {
            Subtitles.main.popup.GetComponent<RectTransform>().localScale = Vector3.one * scale;
        }

        [HarmonyPatch(typeof(Subtitles), nameof(Subtitles.Start))]
        class SubtitlesPosition_Patch
        {
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
            //makes the Sunbeam timer visible by moving it from the top right to bottom middle. Also hides the timer background.
            //TODO: consider removing the background component completely so the workaround in SetHUDOpacity will be unnescessary.
            public static void Postfix(uGUI_SunbeamCountdown __instance)
            {
                RectTransform SunbeamRect = __instance.countdownHolder.GetComponent<RectTransform>();
                SunbeamRect.anchorMax = SunbeamRect.anchorMin = SunbeamRect.pivot = new Vector2(0.5f, 0.5f);
                SunbeamRect.anchoredPosition = new Vector2(0f, -275f);
                SunbeamRect.localScale = Vector3.one * 0.75f;
                __instance.transform.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0f);//hide background
            }

        }

        [HarmonyPatch(typeof(uGUI_CameraDrone), nameof(uGUI_CameraDrone.Awake))]
        class CameraDrone_Awake_Patch
        {
            //Reduce the size of the HUD in the Drone Camera to make edges visible
            static void Postfix(uGUI_CameraDrone __instance)
            {
                CameraDroneHUD = __instance.transform.Find("Content/CameraScannerRoom").GetComponent<RectTransform>();
                if (CameraDroneHUD)
                {
                    CameraDroneHUD.localScale = new Vector3(CameraHUDScaleFactor * AdditionalVROptions.HUD_Scale, CameraHUDScaleFactor * AdditionalVROptions.HUD_Scale, 1f);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_CameraCyclops), nameof(uGUI_CameraCyclops.Awake))]
        class CameraCyclops_Awake_Patch
        {
            //Reduce the size of the HUD in the Cyclops Camera to make edges visible
            static void Postfix(uGUI_CameraCyclops __instance)
            {
                CameraCyclopsHUD = __instance.transform.Find("Content/CameraCyclops").GetComponent<RectTransform>();
                if (CameraCyclopsHUD)
                {
                    CameraCyclopsHUD.localScale = new Vector3(CameraHUDScaleFactor * AdditionalVROptions.HUD_Scale, CameraHUDScaleFactor * AdditionalVROptions.HUD_Scale, 1f);
                }

            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake))]
        class MM_Awake_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                //shift the main menu up a little.
                //TODO: Make the menu centered to reticle position at some point after startup then have the menu snap centered to the reticle if the reticle leaves the menu area.
                //Consider how the Subnautica background would be affected.
                GameObject mainMenu = __instance.transform.Find("Panel/MainMenu").gameObject;
                if (mainMenu)
                {
                    mainMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 385);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
        class HR_LateUpdate_Patch
        {
            //fixes the reticle distance being locked to the interaction distance after interaction. eg Entering Seamoth
            static bool Prefix(HandReticle __instance)
            {
                if (Player.main)
                {
                    Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject activeTarget, out float reticleDistance, null);
                    SubRoot currSub = Player.main.GetCurrentSub();
                    //if piloting the cyclops and not using cyclops cameras
                    if (Player.main.isPiloting && currSub && currSub.isCyclops && !CameraCyclopsHUD.gameObject.activeInHierarchy)
                    {
                        __instance.SetTargetDistance(reticleDistance > 1.55f ? 1.55f : reticleDistance);
                    }
                    else if (Player.main.GetMode() == Player.Mode.LockedPiloting || CameraCyclopsHUD.gameObject.activeInHierarchy)
                    {
                        __instance.SetTargetDistance(AdditionalVROptions.HUD_Distance);
                    }
                    //TODO: Check how the drone camera is affected. Don't think the drone camera has reticle components like the cyclops cam
                }
                return true;
            }
            //this fixes reticle alignment in menus etc
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
