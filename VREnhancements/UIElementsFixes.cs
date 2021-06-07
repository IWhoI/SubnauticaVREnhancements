using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using System.Collections.Generic;

namespace VREnhancements
{
    class UIElementsFixes
    {
        static RectTransform CameraCyclopsHUD;
        static RectTransform CameraDroneHUD;
        static float CameraHUDScaleFactor = 0.75f;        
        private static List<GameObject> DynamicHUDElements = new List<GameObject>();
        static uGUI_SceneHUD sceneHUD;
        static bool seaglideActive = false;
        
        static Transform barsPanelTransform;
        static Transform quickSlotsTransform;
        static Transform compassTransform;
        static Transform powerIndicatorTransform;
        static Transform seamothHUDTransform;
        static Transform exosuitHUDTransform;

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
            //TODO: Check if there is any need for the HUDElements list anymore since it seems like only the Sunbeam Timer might be an exception after using the CavasGroup.
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
                HandReticleCG.alpha = 1;

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
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnDraw))]
        class Seaglide_OnDraw_Patch
        {
            static void Postfix(Seaglide __instance)
            {
                seaglideActive = true;
            }
        }
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnHolster))]
        class Seaglide_OnHolster_Patch
        {
            static void Postfix(Seaglide __instance)
            {
                seaglideActive = false;
            }
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
                sceneHUD = __instance;
                sceneHUD.gameObject.AddComponent<CanvasGroup>();//add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
                barsPanelTransform = __instance.transform.Find("Content/BarsPanel");
                quickSlotsTransform = __instance.transform.Find("Content/QuickSlots");
                compassTransform = __instance.transform.Find("Content/DepthCompass");
                powerIndicatorTransform = __instance.transform.Find("Content/PowerIndicator");
                seamothHUDTransform = __instance.transform.Find("Content/Seamoth");
                exosuitHUDTransform = __instance.transform.Find("Content/Exosuit");
            }
        }
        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.UpdateElements))]
        class SceneHUD_UpdateElements_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if(barsPanelTransform)    
                    barsPanelTransform.localPosition = new Vector3(-300, -260, 0);
            }
        }

        [HarmonyPatch(typeof(uGUI_QuickSlots), nameof(uGUI_QuickSlots.Init))]
        class uGUI_QuickSlots_Init_Patch
        {
            static void Postfix(uGUI_QuickSlots __instance)
            {
                if (!__instance.transform.GetComponent<UIFader>())
                    __instance.gameObject.AddComponent<UIFader>();
            }
        }

        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.NotifySelect))]
        class QuickSlots_NotifySelect_Patch
        {
            static void Postfix(QuickSlots __instance)
            {
                UIFader qsFader = quickSlotsTransform.GetComponent<UIFader>();
                qsFader.Fade(AdditionalVROptions.HUD_Alpha, 0, 0, true);//make quickslot visible as soon as the slot changes
                if(!seaglideActive && AdditionalVROptions.DynamicHUD)
                    qsFader.Fade(0, 1, 2);
                else if(seaglideActive)
                    qsFader.Fade(0, 1, 0, true);//fade without delay if seaglide is active.
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                //add CanvasGroup to the HandReticle to be able to override the HUD CanvasGroup alpha settings to keep the Reticle always opaque.
                if (HandReticle.main)
                {
                    HandReticle.main.gameObject.AddComponent<CanvasGroup>().ignoreParentGroups = true;//not sure if this will cause issues when changes are made to the ScreenCanvas CanvasGroup;
                }
                   
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Update))]
        class SceneHUD_Update_Patch
        {
            static float fadeInStart = 25;
            static float fadeRange = 10;//max alpha at start+range degrees
            static void Postfix(uGUI_SceneHUD __instance)
            {
                //don't change the HUD if using cameras.
                if(uGUI_CameraDrone.main.content.activeInHierarchy || uGUI_CameraCyclops.main.content.activeInHierarchy)
                    return;

                if (AdditionalVROptions.DynamicHUD && MainCamera.camera)
                {
                    //fades the hud in based on the view pitch. Forward is 360/0 degrees and straight down is 90 degrees.
                    if (MainCamera.camera.transform.localEulerAngles.x < 180)
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - fadeInStart) / fadeRange, 0, 1) * AdditionalVROptions.HUD_Alpha);
                    else
                        UpdateHUDOpacity(0);
                }
                //TODO: This was just a test and needs to be removed from update and done in a better way.
                barsPanelTransform.rotation = Quaternion.LookRotation(barsPanelTransform.position);//LookRotatation(PositionOfObjectToRotate - lookatTargetPosition) MainCamera (UI) is always at (0,0,0);
                quickSlotsTransform.rotation = Quaternion.LookRotation(quickSlotsTransform.position);
                compassTransform.rotation = Quaternion.LookRotation(compassTransform.position);
                powerIndicatorTransform.rotation = Quaternion.LookRotation(powerIndicatorTransform.position);
                seamothHUDTransform.rotation = Quaternion.LookRotation(seamothHUDTransform.position);
                exosuitHUDTransform.rotation = Quaternion.LookRotation(exosuitHUDTransform.position);
            }
        }
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
        [HarmonyPatch(typeof(uGUI_CameraDrone), nameof(uGUI_CameraDrone.OnEnable))]
        class CameraDrone_OnEnable_Patch
        {
            //make sure the camera HUD is visible
            static void Postfix(uGUI_CameraDrone __instance)
            {
                UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
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

        [HarmonyPatch(typeof(uGUI_CameraCyclops), nameof(uGUI_CameraCyclops.OnEnable))]
        class CameraCyclops_OnEnable_Patch
        {
            //make sure the camera HUD is visible
            static void Postfix(uGUI_CameraCyclops __instance)
            {
                UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
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
            //fixes the reticle distance being locked to the interaction distance after interaction. eg Entering Seamoth and piloting Cyclops
            static bool Prefix(HandReticle __instance)
            {
                if (Player.main)
                {
                    Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject activeTarget, out float reticleDistance, null);
                    SubRoot currSub = Player.main.GetCurrentSub();
                    //if piloting the cyclops and not using cyclops cameras
                    //TODO: find a way to use the raycast distance for the ui elements instead of the fixed value of 1.55
                    if (Player.main.isPiloting && currSub && currSub.isCyclops && !CameraCyclopsHUD.gameObject.activeInHierarchy)
                    {
                        __instance.SetTargetDistance(reticleDistance > 1.55f ? 1.55f : reticleDistance);
                    }
                    else if (Player.main.GetMode() == Player.Mode.LockedPiloting || CameraCyclopsHUD.gameObject.activeInHierarchy)
                    {
                        __instance.SetTargetDistance(AdditionalVROptions.HUD_Distance);
                    }
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
                //TODO: Check if this is the best way to fix this. The cursor still goes invisible if you click off the canvas. Check lastgroup variable in FPSInputModule
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
