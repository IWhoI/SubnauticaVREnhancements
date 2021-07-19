using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VREnhancements
{
    class UIElementsFixes
    {
        static RectTransform CameraCyclopsHUD;
        static RectTransform CameraDroneHUD;
        static float CameraHUDScaleFactor = 0.75f;
        static uGUI_SceneHUD sceneHUD;
        static bool seaglideEquipped = false;
        static Transform barsPanel;
        static Transform quickSlots;
        static Transform compass;
        static Transform sunbeamCountdown;
        static bool fadeBarsPanel = true;
        static float lastHealth = -1;
        static float lastOxygen = -1;
        static float lastFood = -1;
        static float lastWater = -1;

        public static void SetDynamicHUD(bool enabled)
        {
            UIFader qsFader = quickSlots.gameObject.GetComponent<UIFader>();
            UIFader barsFader = barsPanel.gameObject.GetComponent<UIFader>();
            if (qsFader && barsFader)
            {
                qsFader.SetAutoFade(enabled);
                barsFader.SetAutoFade(enabled);
            }
        }
        public static void UpdateHUDOpacity(float alpha)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
                if(VehicleHUDManager.vehicleCanvas)
                    VehicleHUDManager.vehicleCanvas.GetComponent<CanvasGroup>().alpha = alpha;
                if(sunbeamCountdown)
                    sunbeamCountdown.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0f);//make sure the background remains hidden
                //make sure the reticle isn't affected by HUD alpha setting
                if(HandReticle.main && HandReticle.main.GetComponent<CanvasGroup>())
                    HandReticle.main.GetComponent<CanvasGroup>().alpha = 0.8f;
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
                //sceneHUD.transform.position = new Vector3(sceneHUD.transform.position.x,sceneHUD.transform.position.y,distance);
            }
        }
        public static void UpdateHUDScale(float scale)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            }
            //TODO: Consider if only the HUD should be scaled or the whole screen canvas
        }

        public static void InitHUD()
        {
            if (!sceneHUD.gameObject.GetComponent<CanvasGroup>())
                sceneHUD.gameObject.AddComponent<CanvasGroup>();//add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
            UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            UpdateHUDScale(AdditionalVROptions.HUD_Scale);
            //TODO: Add option for HUD element separation by adjusting the vrSafeRect values in uGUI_SafeAreaScaler
            //vrSafeRect width value should be 1-2minx
            if (!quickSlots.GetComponent<UIFader>())
            {
                UIFader qsFader = quickSlots.gameObject.AddComponent<UIFader>();
                if (qsFader)
                    qsFader.SetAutoFade(AdditionalVROptions.dynamicHUD);
            }
            if (!barsPanel.GetComponent<UIFader>())
            {
                UIFader barsFader = barsPanel.gameObject.AddComponent<UIFader>();
                if (barsFader)
                {
                    barsFader.SetAutoFade(AdditionalVROptions.dynamicHUD);
                    barsFader.autoFadeDelay = 2;
                }
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

        [HarmonyPatch(typeof(uGUI_PlayerDeath), nameof(uGUI_PlayerDeath.Start))]
        class uGUI_PlayerDeath_Start_Patch
        {
            static void Postfix(uGUI_PlayerDeath __instance)
            {
                __instance.blackOverlay.gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 2;
            }
        }
        [HarmonyPatch(typeof(uGUI_PlayerSleep), nameof(uGUI_PlayerSleep.Start))]
        class uGUI_PlayerSleep_Start_Patch
        {
            static void Postfix(uGUI_PlayerSleep __instance)
            {
                __instance.blackOverlay.gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 2;
            }
        }

        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnDraw))]
        class Seaglide_OnDraw_Patch
        {
            static void Postfix()
            {
                seaglideEquipped = true;
            }
        }
        [HarmonyPatch(typeof(Seaglide), nameof(Seaglide.OnHolster))]
        class Seaglide_OnHolster_Patch
        {
            static void Postfix()
            {
                seaglideEquipped = false;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.Begin))]
        class uGUI_Awake_Patch
        {
            static void Postfix()
            {
                //TODO: Figure out why the Screen Canvas distance gets reset to 1 when loading a save. (Resets because the UI camera changes during loading)
                //resetting distance at the start of loading a save to make sure it doesn't get reset to 1
                UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            }
        }
        /*
        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.AnimateLoadingText))]
        class AnimatedText_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {   
                //TODO: Don't do a find here
                GameObject mainCam = GameObject.Find("UI Camera");
                if(!mainCam)
                    mainCam = GameObject.Find("MainCamera (UI)");
                GameObject loadingCanvas = GameObject.Find("VR Loading Canvas");
                if (mainCam)
                {
                    //Debug.Log("MAIN CAM NAME: " + mainCam.name);
                    loadingCanvas.transform.position = mainCam.transform.position + mainCam.transform.forward * 2;
                    loadingCanvas.transform.LookAt(mainCam.transform.position);
                }
            }
        }*/

        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.End))]
        class SceneLoading_End_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {
                VRUtil.Recenter();
                quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
                compass.rotation = Quaternion.LookRotation(compass.position);
                barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);
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
                __instance.gameObject.AddComponent<VehicleHUDManager>();
                InitHUD();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        class Player_Update_Patch
        {
            static void Postfix()
            {
                UIFader barsFader = barsPanel.GetComponent<UIFader>();
                UIFader qsFader = quickSlots.GetComponent<UIFader>();
                Player player = Player.main;
                Survival survival = player.GetComponent<Survival>();
                fadeBarsPanel = AdditionalVROptions.dynamicHUD;
                float fadeInStart = 10;
                float fadeRange = 10;//max alpha at start+range degrees

                if (AdditionalVROptions.dynamicHUD && !player.GetPDA().isInUse && survival && barsFader)
                { 
                    if(Mathf.Abs(player.liveMixin.health-lastHealth)/player.liveMixin.maxHealth > 0.05f || player.liveMixin.GetHealthFraction() < 0.33f ||
                        player.GetOxygenAvailable() < (player.GetOxygenCapacity() / 3) || player.GetOxygenAvailable() > lastOxygen ||
                        survival.food < 50 || survival.food > lastFood ||
                        survival.water < 50 || survival.water > lastWater)
                    {
                        fadeBarsPanel = false;
                    }
                    lastHealth = player.liveMixin.health;
                    lastOxygen = player.GetOxygenAvailable();
                    lastFood = survival.food;
                    lastWater = survival.water;
                    barsFader.SetAutoFade(fadeBarsPanel);
                    qsFader.SetAutoFade(!Player.main.inExosuit && !Player.main.inSeamoth);
                }
                //if the PDA is in use turn on look down for hud
                if (player.GetPDA().isInUse)
                {
                    barsFader.SetAutoFade(false);
                    qsFader.SetAutoFade(false);
                    //fades the hud in based on the view pitch. Forward is 360/0 degrees and straight down is 90 degrees.
                    if (MainCamera.camera.transform.localEulerAngles.x < 180)
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - fadeInStart) / fadeRange, 0, 1) * AdditionalVROptions.HUD_Alpha);
                    else
                        UpdateHUDOpacity(0);
                }//opacity is set back to HUDAlpha in PDA.Close Postfix
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.Close))]
        class PDA_Close_Patch
        {
            static void Postfix()
            {
                UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            }
        }

        
        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.NotifySelect))]
        class QuickSlots_NotifySelect_Patch
        {
            static void Postfix()
            {
                UIFader qsFader = quickSlots.GetComponent<UIFader>();
                qsFader.Fade(AdditionalVROptions.HUD_Alpha, 0, 0, true);//make quickslot visible as soon as the slot changes. Using Fade to cancel any running fades.
                if (!seaglideEquipped)
                    qsFader.autoFadeDelay = 2;
                else
                    qsFader.autoFadeDelay = 1; ;//fade with shorter delay if seaglide is active.
                //keep the slots visible if piloting the seamoth or suit
                qsFader.SetAutoFade((AdditionalVROptions.dynamicHUD || seaglideEquipped));
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            static void Postfix()
            {
                //add CanvasGroup to the HandReticle to be able to override the HUD CanvasGroup alpha settings to keep the Reticle always opaque.
                if (HandReticle.main)
                {
                    HandReticle.main.gameObject.AddComponent<CanvasGroup>().ignoreParentGroups = true;//not sure if this will cause issues when changes are made to the ScreenCanvas CanvasGroup;
                }
                   
            }
        }
        [HarmonyPatch(typeof(Subtitles), nameof(Subtitles.Start))]
        class SubtitlesPosition_Patch
        {
            static void Postfix(Subtitles __instance)
            {
                __instance.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);//to keep subtitles centered when scaling.
                SetSubtitleHeight(AdditionalVROptions.subtitleHeight);                
                SetSubtitleScale(AdditionalVROptions.subtitleScale);
            }
        }

        [HarmonyPatch(typeof(uGUI_SunbeamCountdown), nameof(uGUI_SunbeamCountdown.Start))]
        class SunbeamCountdown_Start_Patch
        {
            //makes the Sunbeam timer visible by moving it from the top right to bottom middle. Also hides the timer background.
            public static void Postfix(uGUI_SunbeamCountdown __instance)
            {
                sunbeamCountdown = __instance.transform;
                sunbeamCountdown.SetParent(quickSlots.parent, false);
                sunbeamCountdown.localPosition = new Vector3(0, -150, 0);
                RectTransform SunbeamRect = __instance.countdownHolder.GetComponent<RectTransform>();
                SunbeamRect.anchorMax = SunbeamRect.anchorMin = SunbeamRect.pivot = new Vector2(0.5f, 0.5f);
                SunbeamRect.anchoredPosition = new Vector2(0f, -275f);
                SunbeamRect.localScale = Vector3.one * 0.75f;
                sunbeamCountdown.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0f);//hide background
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
            static void Postfix(HandReticle __instance)
            {
                //this fixes reticle alignment in menus etc
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

        [HarmonyPatch(typeof(uGUI), nameof(uGUI.Awake))]
        class LoadingScreen_Patch
        {
            static GameObject loadingCanvas;
            static void Postfix(uGUI __instance)
            {
                Image loadingArtwork = __instance.loading.loadingBackground.transform.Find("LoadingArtwork").GetComponent<Image>();
                RectTransform textRect = __instance.loading.loadingText.gameObject.GetComponent<RectTransform>();
                RectTransform logoRect = __instance.loading.loadingBackground.transform.Find("Logo").GetComponent<RectTransform>();
                Vector2 midCenter = new Vector2(0.5f, 0.5f);
                if (loadingArtwork != null && textRect != null && logoRect != null)
                {
                    //remove background image and set background to black
                    loadingArtwork.sprite = null;
                    loadingArtwork.color = Color.black;
                    loadingArtwork.GetComponent<RectTransform>().localScale = Vector3.one * 2;//temporary fix for when hud distance is increased
                    //center the logo
                    logoRect.anchorMin = midCenter;
                    logoRect.anchorMax = midCenter;
                    logoRect.pivot = midCenter;
                    logoRect.anchoredPosition = Vector2.zero;
                    //center text and offset below logo
                    textRect.anchorMin = midCenter;
                    textRect.anchorMax = midCenter;
                    textRect.pivot = midCenter;
                    textRect.anchoredPosition = new Vector2(0f, -200f);
                    textRect.sizeDelta = new Vector2(400f, 100f);
                    textRect.gameObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                }
                /*if(!loadingCanvas)
                {
                    loadingCanvas = new GameObject("VR Loading Canvas");
                    loadingCanvas.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    loadingCanvas.layer = LayerMask.NameToLayer("UI");
                    Transform mainCam = GameObject.Find("UI Camera").transform;
                    if (mainCam)
                    {
                        Debug.Log("MAIN CAM NAME: " + mainCam.name);
                        loadingCanvas.transform.position = mainCam.position + mainCam.forward * 2;
                        loadingCanvas.transform.LookAt(mainCam.position);
                        __instance.gameObject.transform.SetParent(loadingCanvas.transform, false);
                        Object.DontDestroyOnLoad(loadingCanvas);
                    }
                    else
                        Debug.Log("UI camera not found");
                    
                }*/
            }
        }
        static Transform screenCanvas;
        static Transform overlayCanvas;
        static Transform mainMenuUICam;
        static Transform mainMenu;
        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake))]
        class MM_Awake_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                mainMenuUICam = MainCamera.camera.gameObject.transform;
                mainMenu = __instance.transform.Find("Panel/MainMenu");
                screenCanvas = GameObject.Find("ScreenCanvas").transform;
                overlayCanvas = GameObject.Find("OverlayCanvas").transform;
                VRUtil.Recenter();
            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Update))]
        class MM_Update_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                //keep the main menu tilted towards the camera.
                mainMenu.transform.root.rotation = Quaternion.LookRotation(mainMenu.position - new Vector3(mainMenuUICam.position.x, mainMenuUICam.position.y, mainMenu.position.z));
                //match screen and overlay canvas position and rotation to main menu
                screenCanvas.localPosition = __instance.transform.localPosition;
                screenCanvas.position = __instance.transform.position;
                screenCanvas.rotation = __instance.transform.rotation;
                overlayCanvas.localPosition = __instance.transform.localPosition;
                overlayCanvas.position = __instance.transform.position;
                overlayCanvas.rotation = __instance.transform.rotation;
                //try to keep the main menu visible if the HMD is moved more than 0.5 after starting the game.
                if (mainMenuUICam.localPosition.magnitude > 0.5f)
                    VRUtil.Recenter();

            }
        }
        [HarmonyPatch(typeof(uGUI_BuildWatermark), nameof(uGUI_BuildWatermark.UpdateText))]
        class BWM_UpdateText_Patch
        {
            static void Postfix(uGUI_BuildWatermark __instance)
            {
                //make the version watermark more visible
                __instance.GetComponent<Text>().color = new Vector4(1,1,1,0.5f);
                //TODO: fix this after moving the main menu back and scaling up or consider reparenting to main menu
                __instance.transform.localPosition = new Vector3(500, -670, 0);
            }
        }
    }
}
