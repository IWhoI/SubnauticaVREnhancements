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
        static bool seaglideEquipped = false;
        static Transform barsPanel;
        static Transform quickSlots;
        static Transform compass;
        static Transform powerIndicator;
        static Transform seamothHUD;
        static Transform exosuitHUD;
        static Transform sunbeamCountdown;

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
            if (sceneHUD.GetComponent<CanvasGroup>())
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
            //sunbeam timer is not a child of the hud so alpha has to be set separately.
            if (sunbeamCountdown.GetComponent<CanvasGroup>())
                sunbeamCountdown.GetComponent<CanvasGroup>().alpha = alpha;
           //to keep the reticle always fully visible
            if (HandReticle.main.GetComponent<CanvasGroup>())
                HandReticle.main.GetComponent<CanvasGroup>().alpha = 1;
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
            //TODO: Consider if only the HUD should be scaled or the whole screen canvas
            /* 
            if(sunbeamCountdown)
                sunbeamCountdown.GetComponent<RectTransform>().localScale = Vector3.one * scale;*/
        }

        static void InitHUD()
        {
            sceneHUD.gameObject.AddComponent<CanvasGroup>();//add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
            AddHUDElement("SunbeamCountdown");
            UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            UpdateHUDScale(AdditionalVROptions.HUD_Scale);
            if (!quickSlots.GetComponent<UIFader>())
            {
                UIFader qsFader = quickSlots.gameObject.AddComponent<UIFader>();
                if (qsFader)
                    qsFader.autoFadeOut = true;
            }
                
            if (barsPanel)
                barsPanel.localPosition = new Vector3(-300, -260, 0);
        }
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnDraw))]
        class Seaglide_OnDraw_Patch
        {
            static void Postfix(Seaglide __instance)
            {
                seaglideEquipped = true;
            }
        }
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnHolster))]
        class Seaglide_OnHolster_Patch
        {
            static void Postfix(Seaglide __instance)
            {
                seaglideEquipped = false;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.Begin))]
        class SceneLoading_Begin_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {
                //only update HUD parameters late to make sure AdditionalVROptions are loaded first.
                //TODO: Find a better way to know when settings have been serialized after startup.
                InitHUD();
            }
        }


        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;
                barsPanel = __instance.transform.Find("Content/BarsPanel");
                quickSlots = __instance.transform.Find("Content/QuickSlots");
                compass = __instance.transform.Find("Content/DepthCompass");
                powerIndicator = __instance.transform.Find("Content/PowerIndicator");
                seamothHUD = __instance.transform.Find("Content/Seamoth");
                exosuitHUD = __instance.transform.Find("Content/Exosuit");
            }
        }

        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.NotifySelect))]
        class QuickSlots_NotifySelect_Patch
        {
            static void Postfix(QuickSlots __instance)
            {
                UIFader qsFader = quickSlots.GetComponent<UIFader>();
                qsFader.Fade(AdditionalVROptions.HUD_Alpha, 0, 0, true);//make quickslot visible as soon as the slot changes. Using Fade to cancel any running fades.
                if(!seaglideEquipped && AdditionalVROptions.DynamicHUD)
                    qsFader.Fade(0, 1, 2);
                else if(seaglideEquipped)
                    qsFader.Fade(0, 1, 1, true);//fade with shorter delay if seaglide is active.
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
                //TODO: only use dynamic hud for fading elements. create new option for look down for hud.
                /*if (AdditionalVROptions.DynamicHUD && MainCamera.camera)
                {
                    //fades the hud in based on the view pitch. Forward is 360/0 degrees and straight down is 90 degrees.
                    if (MainCamera.camera.transform.localEulerAngles.x < 180)
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - fadeInStart) / fadeRange, 0, 1) * AdditionalVROptions.HUD_Alpha);
                    else
                        UpdateHUDOpacity(0);
                }*/
                //TODO: This was just a test and needs to be removed from update and done in a better way.
                barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);//LookRotatation(PositionOfObjectToRotate - lookatTargetPosition) MainCamera (UI) is always at (0,0,0);
                quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
                compass.rotation = Quaternion.LookRotation(compass.position);
                powerIndicator.rotation = Quaternion.LookRotation(powerIndicator.position);
                seamothHUD.rotation = Quaternion.LookRotation(seamothHUD.position);
                exosuitHUD.rotation = Quaternion.LookRotation(exosuitHUD.position);
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
                sunbeamCountdown = __instance.transform;
                RectTransform SunbeamRect = __instance.countdownHolder.GetComponent<RectTransform>();
                SunbeamRect.anchorMax = SunbeamRect.anchorMin = SunbeamRect.pivot = new Vector2(0.5f, 0.5f);
                SunbeamRect.anchoredPosition = new Vector2(0f, -275f);
                SunbeamRect.localScale = Vector3.one * 0.75f;
                __instance.transform.Find("Background").gameObject.SetActive(false);
                __instance.gameObject.AddComponent<CanvasGroup>();
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
