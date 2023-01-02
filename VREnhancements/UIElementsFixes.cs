using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

namespace VREnhancements
{
    //TODO: Break this up if possible
    class UIElementsFixes
    {
        static RectTransform CameraCyclopsHUD;
        static RectTransform CameraDroneHUD;
        static readonly float CameraHUDScaleFactor = 0.75f;
        static uGUI_SceneHUD sceneHUD;
        static CanvasGroup resTrackerCG;
        static CanvasGroup pingsCG;
        static bool seaglideEquipped = false;
        static Transform barsPanel;
        static Transform quickSlots;
        static Transform compass;
        static bool fadeBarsPanel = true;
        static float lastHealth = -1;
        static float lastOxygen = -1;
        static float lastFood = -1;
        static float lastWater = -1;
        static Rect defaultSafeRect;
        static readonly float menuDistance = 2f;
        static readonly float menuScale = 0.002f;
        static bool showDefaultReticle = false;

        public static void SetDynamicHUD(bool enabled)
        {
            //TODO: Decide if checking if faders are not null is necessary
            UIFader qsFader = quickSlots.gameObject.GetComponent<UIFader>();
            UIFader barsFader = barsPanel.gameObject.GetComponent<UIFader>();
            if (qsFader && barsFader)
            {
                qsFader.SetAutoFade(enabled);
                barsFader.SetAutoFade(enabled);
                uGUI_SunbeamCountdown.main.GetComponent<UIFader>().SetAutoFade(enabled);
            }
        }
        public static void UpdateHUDOpacity(float alpha)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
                if (VehicleHUDManager.vehicleCanvas)
                    VehicleHUDManager.vehicleCanvas.GetComponent<CanvasGroup>().alpha = alpha;
            }
            uGUI_SunbeamCountdown.main.transform.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0);//make sure the background remains hidden
            //make the blips and pings more translucent than other hud elements
            if (resTrackerCG)
                resTrackerCG.alpha = Mathf.Clamp(alpha - 0.2f,0.1f,1);
            if (pingsCG)
                pingsCG.alpha = Mathf.Clamp(alpha - 0.2f, 0.1f, 1);
        }
        public static void UpdateHUDDistance(float distance)
        {
            if (sceneHUD)
            {
                Transform screenCanvas = sceneHUD.transform.parent;
                Camera uicamera = ManagedCanvasUpdate.GetUICamera();
                if (uicamera != null)
                {
                    Transform transform = uicamera.transform;
                    //move the screen canvas instead of just the HUD so all on screen elements like blips etc are also affect by the distance update.
                    screenCanvas.transform.localPosition = screenCanvas.transform.parent.transform.InverseTransformPoint(transform.position + transform.forward * distance);
                    //make sure the elements are still facing the camera after changing position
                    UpdateHUDLookAt();
                }
            }
        }
        public static void UpdateHUDScale(float scale)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            }
        }
        public static void UpdateHUDSeparation(float separation)
        {
            if (sceneHUD)
            {
                Rect safeAreaRect;
                //to make sure that the Rect is centered the width should be 1 - 2x
                switch (separation)
                {
                    case 0:
                        safeAreaRect = defaultSafeRect;
                        break;
                    case 1:
                        safeAreaRect = new Rect(0.3f,0.3f,0.4f,0.3f);
                        break;
                    case 2:
                        safeAreaRect = new Rect(0.2f, 0.2f, 0.6f, 0.5f);
                        break;
                    case 3:
                        safeAreaRect = new Rect(0.15f, 0.15f, 0.7f, 0.6f);
                        break;
                    default:
                        safeAreaRect = defaultSafeRect;
                        break;
                }
                sceneHUD.GetComponent<uGUI_SafeAreaScaler>().vrSafeRect = safeAreaRect;
                //the position of element in front the UI Camera would change if the Rect size changes so making sure the elements still face the camera.
                UpdateHUDLookAt();

            }
        }

        public static void InitHUD()
        {
            sceneHUD.transform.localPosition = Vector3.zero;//not sure why this isn't zero by default.
            
            uGUI_SunbeamCountdown.main.transform.SetParent(quickSlots.parent, false);//changes parent from ScreenCanvas to HUD/Content
            RectTransform holderRect = uGUI_SunbeamCountdown.main.countdownHolder.GetComponent<RectTransform>();
            holderRect.anchorMax = holderRect.anchorMin = holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.localPosition = Vector3.zero;
            RectTransform sbRect = uGUI_SunbeamCountdown.main.GetComponent<RectTransform>();
            sbRect.anchorMin = sbRect.anchorMax = new Vector2(0.5f, 0);
            uGUI_SunbeamCountdown.main.transform.localPosition = new Vector3(0, -450, 0);

            UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            UpdateHUDScale(AdditionalVROptions.HUD_Scale);
            //UpdateHUDSeparation done in uGUI_SceneLoading.End instead

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

            if (!uGUI_SunbeamCountdown.main.GetComponent<UIFader>())
            {
                UIFader sbcFader = uGUI_SunbeamCountdown.main.gameObject.AddComponent<UIFader>();
                if (sbcFader)
                {
                    sbcFader.SetAutoFade(AdditionalVROptions.dynamicHUD);
                    sbcFader.autoFadeDelay = 5;
                }
            }
        }
        /*public static void SetSubtitleHeight(float percentage)
        {
            Subtitles.main.popup.oy = Subtitles.main.GetComponent<RectTransform>().rect.height * percentage / 100;
        }
        public static void SetSubtitleScale(float scale)
        {
            Subtitles.main.popup.GetComponent<RectTransform>().localScale = Vector3.one * scale;
        }*/

        public static void UpdateHUDLookAt()
        {
            quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
            compass.rotation = Quaternion.LookRotation(compass.position);
            barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);
            uGUI_SunbeamCountdown.main.transform.rotation = Quaternion.LookRotation(uGUI_SunbeamCountdown.main.transform.position);
        }

        [HarmonyPatch(typeof(Hint), nameof(Hint.Awake))]
        class Hint_Awake_Patch
        {
            static void Postfix(Hint __instance)
            {
                __instance.message.oy = 800;
                __instance.warning.oy = 800;
            }
        }
        ;

        [HarmonyPatch(typeof(uGUI_ResourceTracker), nameof(uGUI_ResourceTracker.Start))]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(uGUI_ResourceTracker __instance)
            {
                if (!resTrackerCG)
                {
                    resTrackerCG = __instance.gameObject.AddComponent<CanvasGroup>();
                    resTrackerCG.alpha = Mathf.Clamp(AdditionalVROptions.HUD_Alpha - 0.2f, 0.1f, 1);
                }
                    
            }
        }
        [HarmonyPatch(typeof(uGUI_Pings), nameof(uGUI_Pings.OnEnable))]
        class Pings_Enable_Patch
        {
            static void Postfix(uGUI_Pings __instance)
            {
                if (!pingsCG)
                {
                    pingsCG = __instance.canvasGroup;
                    pingsCG.alpha = Mathf.Clamp(AdditionalVROptions.HUD_Alpha - 0.2f, 0.1f, 1);
                }
                    
            }
        }
        /* TODO: This has to be updated if I still want the subtitle height to be customizable but the default subtitles in VR now seem fine.
         * the update will involve uGUI_MessageQueue
        [HarmonyPatch(typeof(Subtitles), nameof(Subtitles.Show))]
        class SubtitlesPosition_Patch
        {
            static bool Prefix(Subtitles __instance)
            {
                __instance.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);//to keep subtitles centered when scaling.
                __instance.popup.text.alignment = TextAnchor.MiddleLeft;
                SetSubtitleScale(AdditionalVROptions.subtitleScale);
                SetSubtitleHeight(AdditionalVROptions.subtitleHeight);
                return true;
            }
        }*/

        //make sure the black overlays always hides the background for all HUD distances by scaling them up
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

        [HarmonyPatch(typeof(uGUI_SceneIntro), nameof(uGUI_SceneIntro.Start))]
        class uGUI_uGUI_SceneIntro_Start_Patch
        {
            static void Postfix(uGUI_SceneIntro __instance)
            {
                __instance.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.one * 2000;
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

        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.Awake))]
        class LoadingScreen_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {
                Image loadingArtwork = __instance.loadingBackground.transform.Find("LoadingArtwork").GetComponent<Image>();
                Vector2 midCenter = new Vector2(0.5f, 0.5f);
                uGUI_Logo logo = __instance.loadingBackground.GetComponentInChildren<uGUI_Logo>();
                if (loadingArtwork != null)
                {
                    //remove background image and set background to black
                    loadingArtwork.sprite = null;
                    loadingArtwork.color = Color.black;
                    loadingArtwork.GetComponent<RectTransform>().localScale = Vector3.one * 2;//temporary fix for when hud distance is increased

                }
                if (logo != null)
                {
                    //center the logo and loading bar
                    RectTransform logoRect = logo.GetComponent<RectTransform>();
                    logoRect.anchoredPosition = new Vector2(0, 120f);
                    logoRect.anchorMax = logoRect.anchorMin = midCenter;
                    RectTransform parentCanvasRect = logo.transform.parent.GetComponent<RectTransform>();
                    parentCanvasRect.anchoredPosition = new Vector2(0, -25f);
                    parentCanvasRect.anchorMin = Vector2.zero;
                    parentCanvasRect.anchorMax = Vector2.one;
                }
            }
        }

        //EnsureCreated is called at the end of PAXTerrainController.LoadAsync() which is just before the player takes control
        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.EnsureCreatedAsync))]
        class Loading_End_Patch
        {
            static void Postfix()
            {
                ManagedCanvasUpdate.GetUICamera().clearFlags = CameraClearFlags.Depth;//fixes problem with right hand tools preventing some blips from showing
                InitHUD();
                UpdateHUDLookAt();
                UpdateHUDSeparation(AdditionalVROptions.HUD_Separation);//wasn't working in HUD awake so put it here instead
                VRUtil.Recenter();
            }
        }
        
        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;
                if (!sceneHUD.gameObject.GetComponent<CanvasGroup>())
                    sceneHUD.gameObject.AddComponent<CanvasGroup>();//add CanvasGroup to the HUD to be able to set the alpha of all HUD elements
                barsPanel = __instance.transform.Find("Content/BarsPanel");
                quickSlots = __instance.transform.Find("Content/QuickSlots");
                compass = __instance.transform.Find("Content/DepthCompass");
                defaultSafeRect = sceneHUD.GetComponent<uGUI_SafeAreaScaler>().vrSafeRect;
                __instance.gameObject.AddComponent<VehicleHUDManager>();
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
                    uGUI_SunbeamCountdown.main.GetComponent<UIFader>().SetAutoFade(false);
                    //fades the hud in based on the view pitch. Forward is 360/0 degrees and straight down is 90 degrees.
                    if (MainCamera.camera.transform.localEulerAngles.x < 180)
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - fadeInStart) / fadeRange, 0, 1) * AdditionalVROptions.HUD_Alpha);
                    else
                        UpdateHUDOpacity(0);
                }//opacity is set back to HUDAlpha in PDA.Deactivated Postfix
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.Deactivated))]
        class PDA_Deactivated_Patch
        {
            static void Postfix()
            {
                UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
                uGUI_SunbeamCountdown.main?.transform.GetComponent<UIFader>()?.SetAutoFade(AdditionalVROptions.dynamicHUD);
            }
        }

        
        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.NotifySelect))]
        class QuickSlots_NotifySelect_Patch
        {
            static void Postfix()
            {
                UIFader qsFader = quickSlots.GetComponent<UIFader>();
                if(qsFader)
                {
                    qsFader.Fade(AdditionalVROptions.HUD_Alpha, 0, 0, true);//make quickslot visible as soon as the slot changes. Using Fade to cancel any running fades.
                    if (!seaglideEquipped)
                        qsFader.autoFadeDelay = 2;
                    else
                        qsFader.autoFadeDelay = 1;//fade with shorter delay if seaglide is active.
                    qsFader.SetAutoFade((AdditionalVROptions.dynamicHUD || seaglideEquipped));
                }                
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            //TODO: Make the tool power text appear and fade away after selecting a tool instead of when pointing at surfaces(weird that they did it this way)
            public static Sprite defaultReticle;
            static void Postfix()
            {
                if (HandReticle.main)
                    defaultReticle = HandReticle.main.transform.Find("IconCanvas/Default").gameObject.GetComponent<Image>().sprite;
            }
        }

        [HarmonyPatch(typeof(uGUI_MapRoomScanner), nameof(uGUI_MapRoomScanner.OnTriggerEnter))]
        class MapRoomScanner_TriggerEnter_Patch
        {
            static void Postfix(uGUI_MapRoomScanner __instance)
            {
                if (__instance.raycaster.enabled)
                    showDefaultReticle = true;
            }
        }
        [HarmonyPatch(typeof(uGUI_MapRoomScanner), nameof(uGUI_MapRoomScanner.OnTriggerExit))]
        class MapRoomScanner_TriggerExit_Patch
        {
            static void Postfix(uGUI_MapRoomScanner __instance)
            {
                if (!__instance.raycaster.enabled)
                    showDefaultReticle = false;
            }
        }

        /*[HarmonyPatch(typeof(CyclopsHelmHUDManager), nameof(CyclopsHelmHUDManager.StartPiloting))]
        class CyclopsStartPiloting_Patch
        {
            static void Postfix()
            {
                    showDefaultReticle = true;
            }
        }
        [HarmonyPatch(typeof(CyclopsHelmHUDManager), nameof(CyclopsHelmHUDManager.StopPiloting))]
        class CyclopsStopPiloting_Patch
        {
            static void Postfix()
            {
                showDefaultReticle = false;
            }
        }*/

        [HarmonyPatch(typeof(CyclopsVehicleStorageTerminalButton), nameof(CyclopsVehicleStorageTerminalButton.OnPointerEnter))]
        class CyclopsVehicleTerminalButtonEnter_Patch
        {
            static void Postfix()
            {
                showDefaultReticle = true;
            }
        }
        [HarmonyPatch(typeof(CyclopsVehicleStorageTerminalButton), nameof(CyclopsVehicleStorageTerminalButton.OnPointerExit))]
        class CyclopsVehicleTerminalButtonExit_Patch
        {
            static void Postfix()
            {
                showDefaultReticle = false;
            }
        }

        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.SetIconInternal))]
        class HandReticle_SetIconInt_Patch
        {
            static bool Prefix(ref HandReticle.IconType newIconType)
            {
                //only show the reticle on interactive elements
                if (newIconType == HandReticle.IconType.Default && !showDefaultReticle && !Player.main.isPiloting)
                {
                        newIconType = HandReticle.IconType.None;
                }
                return true;

            }
        }
        
        //fixes the reticle distance being locked to the interaction distance after interaction. eg Entering Seamoth and piloting Cyclops
        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
        class HR_LateUpdate_Patch
        {
            static PointerEventData pointerEventData;
            static RaycastResult currentRayCastResult;
            static bool Prefix(HandReticle __instance)
            {
                if (Player.main)
                {                    
                    FPSInputModule.current.GetPointerDataFromInputModule(out pointerEventData);
                    currentRayCastResult = pointerEventData.pointerCurrentRaycast;
                    SubRoot currSub = Player.main.GetCurrentSub();
                    //if piloting the cyclops and not using cyclops cameras
                    if (Player.main.isPiloting && currSub && currSub.isCyclops && !CameraCyclopsHUD.gameObject.activeInHierarchy)
                    {   
                        //if the cursor is over an interactive element set the cursor distance to the distance of th element, otherwise set it to HUD_Distance.
                        if(currentRayCastResult.gameObject)
                            __instance.SetTargetDistance(currentRayCastResult.distance - 0.05f);//-0.05 since it was sometimes rendering a little behind the target
                        else
                            __instance.SetTargetDistance(AdditionalVROptions.HUD_Distance);
                    }
                    else if (Player.main.GetMode() == Player.Mode.LockedPiloting || CameraCyclopsHUD.gameObject.activeInHierarchy)
                        __instance.SetTargetDistance(AdditionalVROptions.HUD_Distance);
                }
                return true;
            }
            static void Postfix(HandReticle __instance)
            {
                float distance = __instance.transform.position.z;
                distance = distance < 0.6f ? 0.6f : distance;//prevent the reticle from getting too close to the player
                //this fixes reticle alignment in menus etc
                __instance.transform.position = new Vector3(0f, 0f, distance);
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
            //TODO: Check is this is still needed
            static void Postfix()
            {
                if (sceneHUD)
                    sceneHUD.GetComponent<CanvasGroup>().alpha = AdditionalVROptions.HUD_Alpha;
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
            static void Postfix()
            {
                if (sceneHUD)
                    sceneHUD.GetComponent<CanvasGroup>().alpha = AdditionalVROptions.HUD_Alpha;
            }
        }
        static bool actualGazedBasedCursor;
        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
        class GetCursorScreenPosition_Patch
        {
            static void Postfix(ref Vector2 __result)
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
                Canvas cursorCanvas = Traverse.Create(__instance).Field("_cursorCanvas").GetValue<Canvas>();
                RaycastResult lastRaycastResult = Traverse.Create(__instance).Field("lastRaycastResult").GetValue<RaycastResult>();
                if (cursorCanvas && lastRaycastResult.isValid)
                {
                    cursorCanvas.sortingLayerID = lastRaycastResult.sortingLayer;//put the cursor on the same layer as whatever was hit by the cursor raycast.
                }
                //change the VR cursor to look like the default hand reticle cursor for better accuracy when selecting smaller ui elements
                //TODO: Find a way to not do this every frame.
                if (cursorCanvas && HandReticle_Start_Patch.defaultReticle)
                {
                    cursorCanvas.GetComponentInChildren<Image>().overrideSprite = HandReticle_Start_Patch.defaultReticle;
                    if (cursorCanvas.transform.localScale.x > 0.002f)
                        cursorCanvas.transform.localScale = Vector3.one * 0.002f;
                }

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
                //GameObject mainCam = GameObject.Find("Main Camera");
                mainMenuUICam = ManagedCanvasUpdate.GetUICamera().transform;
                mainMenu = __instance.transform.Find("Panel/MainMenu");
                screenCanvas = GameObject.Find("ScreenCanvas").transform;
                overlayCanvas = GameObject.Find("OverlayCanvas").transform;
                __instance.gameObject.GetComponent<uGUI_CanvasScaler>().enabled = false;//disabling the canvas scaler to prevent it from messing up the custom distance and scale
                __instance.transform.position = new Vector3(mainMenuUICam.transform.position.x + menuDistance,-0.3f,0);
                __instance.transform.localScale = Vector3.one * menuScale * 1.5f;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.25f;//sharpen text
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
                screenCanvas.localPosition = overlayCanvas.localPosition = __instance.transform.localPosition;
                screenCanvas.position = overlayCanvas.position = __instance.transform.position;
                screenCanvas.rotation = overlayCanvas.rotation = __instance.transform.rotation;
                //try to keep the main menu visible if the HMD is moved more than 0.5 after starting the game.
                if (mainMenuUICam.localPosition.magnitude > 0.5f)
                    VRUtil.Recenter();
                //make sure the cursor remains visible after clicking outside the menu area
                if (!FPSInputModule.current.lastGroup)
                    __instance.Select(false);
            }
        }

        [HarmonyPatch(typeof(MainMenuLoadButton), nameof(MainMenuLoadButton.Start))]
        class MMLoadBtn_Start_Patch
        {
            static void Postfix(MainMenuLoadButton __instance)
            {
                //saved game buttons are not on the UI layer so the cursor disappears when it gets set to default layer after raycasting on those elements
                Transform[] children = __instance.GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("UI");
                }
            }
        }

        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.Open))]
        class InGameMenu_Open_Patch
        {
            static bool Prefix(IngameMenu __instance)
            {
                uGUI_CanvasScaler canvasScaler = __instance.gameObject.GetComponent<uGUI_CanvasScaler>();
                canvasScaler.distance = menuDistance;
                //__instance.transform.localScale = Vector3.one * menuScale;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.5f;//sharpen text
                return true;
            }
        }
        
        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.Open))]
        class uGUI_BuilderMenu_Open_Patch
        {
            static bool Prefix(uGUI_BuilderMenu __instance)
            {
                uGUI_CanvasScaler canvasScaler = __instance.gameObject.GetComponent<uGUI_CanvasScaler>();
                canvasScaler.distance = menuDistance;
               //__instance.transform.localScale = Vector3.one * menuScale;
                __instance.gameObject.GetComponent<Canvas>().scaleFactor = 1.5f;//sharpen text
                return true;
            }
        }
        
        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.SetScaleFactor))]
        class Canvas_ScaleFactor_Patch
        {
            static bool Prefix(ref float scaleFactor)
            {
                //any scale factor less than 1 reduces the quality of UI elements.
                if (scaleFactor < 1)
                    scaleFactor = 1;
                return true;
            }
        }
        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.UpdateTransform))]
        class Canvas_UpdateTransform_Patch
        {
            static bool Prefix(uGUI_CanvasScaler __instance)
            {
                if (__instance.gameObject.name == "ScreenCanvas")
                    __instance.distance = AdditionalVROptions.HUD_Distance;
                return true;
            }
        }
        [HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.UpdateFrustum))]
        class Canvas_UpdateFrustum_Patch
        {
            //doing this to maintain the original canvas scale after changing the canvas scaler distance
            static float customDistance=1;
            static bool Prefix(uGUI_CanvasScaler __instance)
            {
                if (__instance.gameObject.name == "ScreenCanvas")
                {
                    //save the modified canvas distance
                    customDistance = __instance.distance;
                    //set the canvas distance back to the default 1 before UpdateFrustum calculates the scale
                    __instance.distance = 1;
                }
                return true;
            }
            static void Postfix(uGUI_CanvasScaler __instance)
            {
                //restore the modified canvas distance after the scale has been calculated.
                if (__instance.gameObject.name == "ScreenCanvas")
                    __instance.distance = customDistance;
            }
        }
    }
}
