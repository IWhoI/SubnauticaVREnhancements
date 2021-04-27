using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using HarmonyLib;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;

namespace VREnhancements
{
    public class MainPatcher
    {
        private static Harmony harmony;
        private static int tabIndex;
        private static float pdaScale = 1.45f;//1.75f;
        private static float screenScale = 0.0003f;//0.00035f;
        private static float defaultZOffset = 0.17f;//forward/back
        private static float defaultYOffset = 0.0f;//vertical
        private static float pdaXOffset = -0.35f;
        private static float pdaZOffset = 0.28f;
        private static float seaglideZOffset = 0.1f;//forward/back
        private static float seaglideYOffset = -0.15f;//vertical
        private static float swimZOffset = 0.08f;//forward/back
        private static float swimYOffset = -0.02f;//vertical
        private static float pdaXRot = 220f;
        private static float pdaYRot = 30f;
        private static float pdaZRot = 75f;
        private static GameObject quickSlots;
        private static GameObject barsPanel;
        private static Button recenterVRButton;
        private static GameObject leftHandTarget;
        private static FullBodyBipedIK myIK;
        public static bool lookDownHUD = false;
        private static float pdaCloseTimer = 0;
        private static bool pdaIsClosing = false;
        private static float pdaCloseDelay = 1f;

        private static bool actualGazedBasedCursor;

        public static void Patch()
        {
            if (XRSettings.enabled)
            {
                try
                {
                    harmony = new Harmony("com.whotnt.subnautica.vrenhancements.mod");
                    harmony.Patch(AccessTools.Method(typeof(GameOptions), "GetVrAnimationMode", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("GetVrAnimationMode_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_OptionsPanel), "AddGeneralTab", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("AddGeneralTab_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), "AddTab", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("AddTab_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(GameSettings), "SerializeVRSettings", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("SerializeVRSettings_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(MainCameraControl), "Update", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("MCC_Update_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(Vehicle), "OnPilotModeBegin", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("OnPilotModeBegin_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(Vehicle), "OnPilotModeEnd", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("OnPilotModeEnd_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(Subtitles), "Start", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("SubtitlesStart_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(PDA), "get_ui", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("PDA_getui_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(SNCameraRoot), "SetFov", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("SNCamSetFov_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(FPSInputModule), "GetCursorScreenPosition", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("GetCursorScreenPosition_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(FPSInputModule), "UpdateCursor", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("UpdateCursor_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(FPSInputModule), "UpdateCursor", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("UpdateCursor_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(HandReticle), "LateUpdate", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("HandRLateUpdate_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(Player), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("Player_Awake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_SceneHUD), "Update", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("SceneHUD_Update_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(SNCameraRoot), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("SNCam_Awake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_SceneLoading), "Init", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("Init_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(CyclopsExternalCams), "EnterCameraView", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("EnterCameraView_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_BuilderMenu), "Close", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("BuilderMenuClose_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_CameraDrone), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("CameraDroneAwake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_CameraCyclops), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("CameraCyclopsAwake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("MainMenuAwake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(PlayerCinematicController), "SkipCinematic", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("SkipCinematic_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(IngameMenu), "Awake", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("IGM_Awake_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(MainGameController), "ResetOrientation", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("ResetOrientation_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(PDA), "Open", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("PDA_Open_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(PDA), "Close", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("PDA_Close_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(PDA), "Update", null, null), new HarmonyMethod(typeof(MainPatcher).GetMethod("PDA_Update_Prefix")), null, null);
                    harmony.Patch(AccessTools.Method(typeof(ArmsController), "Reconfigure", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("Reconfigure_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(ArmsController), "Start", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("ArmsCon_Start_Postfix")), null);
                    harmony.Patch(AccessTools.Method(typeof(uGUI_SunbeamCountdown), "Start", null, null), null, new HarmonyMethod(typeof(MainPatcher).GetMethod("uGUI_SunbeamCountdown_Start_Postfix")), null);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error with VREnhancements patching: " + ex.Message);
                }
            }
        }
        //Class: GameOptions
        //take the returned value from the original method and modify it
        public static bool GetVrAnimationMode_Prefix(ref bool __result)
        {
            //if VR animations are enabled in the menu then we need to return false to get animations to play.
            __result = !GameOptions.enableVrAnimations;
            return false;
        }

        //Class: uGUI_OptionsPanel
        public static void AddGeneralTab_Postfix(uGUI_OptionsPanel __instance)
        {
            __instance.AddHeading(tabIndex, "Additional VR Options");//add new heading under the General Tab
            __instance.AddToggleOption(tabIndex, "Enable VR Animations", GameOptions.enableVrAnimations, delegate (bool v)                
            {
                GameOptions.enableVrAnimations = v;
                //playerAnimator vr_active is normally set in the Start function of Player so we need to update it if option changed during gameplay
                if (Player.main != null)
                    Player.main.playerAnimator.SetBool("vr_active", !v);
            });
            __instance.AddToggleOption(tabIndex, "Look Down for HUD", lookDownHUD, delegate (bool v)
            {
                lookDownHUD = v;
                //immediated reenable the HUD if option toggled off
                if (!v && quickSlots != null && barsPanel != null)
                {
                    quickSlots.transform.localScale = Vector3.one;
                    barsPanel.transform.localScale = Vector3.one;
                }
            });
            __instance.AddSliderOption(tabIndex, "Walk Speed(Default: 60%)", VROptions.groundMoveScale * 100, 50, 100, 60, delegate (float v)
            {
                VROptions.groundMoveScale = v/100f;
            });
           /* Weird stuff was happening if the PDA was opened and when entering the Seamoth when pitch was changed with the mouse or controller
            * __instance.AddToggleOption(tabIndex, "Enable Pitch Control", !VROptions.disableInputPitch, delegate (bool v)
            {
                VROptions.disableInputPitch = !v;
                //reset pitch to zero if pitch control is disabled.
                if (VROptions.disableInputPitch)
                {
                    Player.main.GetComponentInChildren<MainCameraControl>().ResetCamera();
                }
            });*/
        }

        //Class: uGUI_TabbedControlsPanel
        public static void AddTab_Postfix(int __result, string label)
        {
            //get the tabIndex of the general tab to be able to use it in  AddGeneralTab_Postfix
            if (label.Equals("General"))
                tabIndex = __result;
        }

        //Class: GameSettings
        public static void SerializeVRSettings_Postfix(GameSettings.ISerializer serializer)
        {
            //for saving the VR animation setting
            GameOptions.enableVrAnimations = serializer.Serialize("VR/EnableVRAnimations", GameOptions.enableVrAnimations);
            VROptions.groundMoveScale = serializer.Serialize("VR/GroundMoveScale", VROptions.groundMoveScale);
            lookDownHUD = serializer.Serialize("VR/LookDownHUD", lookDownHUD);
            //VROptions.disableInputPitch = serializer.Serialize("VR/disableInputPitch", VROptions.disableInputPitch);
        }

        //Class: MainCameraControl
        public static void MCC_Update_Postfix(MainCameraControl __instance)
        {
            Transform forwardRefTransform = __instance.GetComponentInParent<PlayerController>().forwardReference;//forwardReference is the main camera transform
            if (pdaIsClosing && pdaCloseTimer < pdaCloseDelay)
            {
                pdaCloseTimer += Time.deltaTime;
            }
            else if (pdaCloseTimer >= pdaCloseDelay || (pdaIsClosing && Player.main.GetPDA().state == PDA.State.Opened))
            {
                pdaIsClosing = false;
                pdaCloseTimer = 0;
            }
            if (Player.main.GetPDA().state == PDA.State.Closing)
            {
                pdaIsClosing = true;
            }
            //when the pda is opened the viewmodel is moved forward but even when the state is closed, it is kept foward for a short while which was causing the neck to show if
            //I also moved the model forward at the same time. So I maintain my own closing state with pdaIsClosing and only move the model forward after pdaCloseDelay
            if (Player.main.GetPDA().state == PDA.State.Closed && !pdaIsClosing)
            {
                if (Player.main.motorMode == Player.MotorMode.Seaglide)
                {
                    __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position + (forwardRefTransform.up * seaglideYOffset) + forwardRefTransform.forward * seaglideZOffset);
                }
                else if (Player.main.transform.position.y < Ocean.main.GetOceanLevel() + 1f && !Player.main.IsInside() && !Player.main.precursorOutOfWater)
                {
                    //use the viewModel transform instead of forwardRef since the player body pitches while swimming.
                    string clipName = Player.main.playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                    if (clipName == "Back_lean" || clipName == "view_surface_swim_forward")
                        __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position + (__instance.viewModel.transform.up * (swimYOffset - 0.1f)) + __instance.viewModel.transform.forward * (swimZOffset - 0.1f));
                    else
                        __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position + (__instance.viewModel.transform.up * swimYOffset) + __instance.viewModel.transform.forward * swimZOffset);
                }
                else if (!__instance.cinematicMode && Player.main.motorMode != Player.MotorMode.Vehicle && Player.main.motorMode != Player.MotorMode.Seaglide)
                {
                    if (Player.main.movementSpeed == 0)
                        __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position + Vector3.up * defaultYOffset + new Vector3(forwardRefTransform.forward.x, 0f, forwardRefTransform.forward.z).normalized * defaultZOffset);
                    else
                        __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position + Vector3.up * defaultYOffset + new Vector3(forwardRefTransform.forward.x, 0f, forwardRefTransform.forward.z).normalized * (defaultZOffset - 0.1f));
                }
            }
        }

        //Class: Vehicle
        public static bool OnPilotModeBegin_Prefix(Vehicle __instance)
        {
            if (__instance.mainAnimator)
            {
                __instance.mainAnimator.SetBool("vr_active", GameOptions.GetVrAnimationMode());
            }
            return true;
        }
        //Class: Vehicle
        public static bool OnPilotModeEnd_Prefix(Vehicle __instance)
        {
            if (__instance.mainAnimator)
            {
                __instance.mainAnimator.SetBool("vr_active", GameOptions.GetVrAnimationMode());
            }
            return true;
        }

        //Class: Subtitles
        //Bring up the subtitles into view while in VR
        public static void SubtitlesStart_Postfix(Subtitles __instance)
        {
            __instance.popup.oy = 800f;//higher values means higher on the screen
        }

        //Class: PDA
        public static void PDA_getui_Postfix(PDA __instance)
        {
            GameObject screen = Traverse.Create(__instance).Field("screen").GetValue<GameObject>();//get private variable screen
            uGUI_CanvasScaler component = screen.GetComponent<uGUI_CanvasScaler>();
                /*component.transform.localScale = Vector3.one * 0.00032f;
                __instance.transform.localScale = new Vector3(0.15f, 1f, 1f);
                __instance.screenAnchor.transform.localPosition = new Vector3(-1.45f, -0.045f, 0f);*/
                __instance.transform.localScale = new Vector3(pdaScale, pdaScale, 1f);
                component.transform.localScale = Vector3.one * screenScale;
                component.SetAnchor(__instance.screenAnchor);
        }
        //Class: SNCameraRoot
        //Prevent this method from filling the log with errors when trying to set Fov while in VR mode.
        public static bool SNCamSetFov_Prefix()
        {
            return false;
        }
        //Class: FPSInputModule
        //Set the screen position of the cursor to be the middle of the screen if mouse movement of the cursor is disabled.
        //Fixes alignment problem with menu in the scanner room.
        public static void GetCursorScreenPosition_Postfix(FPSInputModule __instance, ref Vector2 __result)
        {
            if (XRSettings.enabled)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    __result = GraphicsUtil.GetScreenSize() * 0.5f;
                }
                else if(!actualGazedBasedCursor)//fix cursor snapping to middle of view when cursor goes off canvas due to hack in UpdateCursor
                    //Screen.width gives monitor width and GraphicsUtil.GetScreenSize().x will give either monitor or VR eye texture width
                    __result = new Vector2(Input.mousePosition.x/Screen.width * GraphicsUtil.GetScreenSize().x, Input.mousePosition.y/Screen.height * GraphicsUtil.GetScreenSize().y);
            }
        }
        public static void UpdateCursor_Prefix()
        {
            //save the original value so we can set it back in the postfix
            actualGazedBasedCursor = VROptions.gazeBasedCursor;
            //trying make flag in UpdateCursor be true if Cursor.lockState != CursorLockMode.Locked)
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                VROptions.gazeBasedCursor = true;
            }
               
        }
        public static void UpdateCursor_Postfix(FPSInputModule __instance)
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

        //Class: HandReticle
        //Fix the cursor offset from the center of the view.
        public static void HandRLateUpdate_Postfix(HandReticle __instance)
        {
                __instance.transform.position = new Vector3(0f, 0f, __instance.transform.position.z);
        }

        public static void Player_Awake_Postfix(uGUI_SceneHUD __instance)
        {
            barsPanel = GameObject.Find("BarsPanel");
            quickSlots = GameObject.Find("QuickSlots");
        }

        public static void SceneHUD_Update_Postfix(uGUI_SceneHUD __instance)
        {
            if(lookDownHUD && quickSlots != null && barsPanel != null)
                if (Player.main != null && Vector3.Angle(MainCamera.camera.transform.forward, Player.main.transform.up) < 120f)
                {
                    quickSlots.transform.localScale = Vector3.zero;
                    barsPanel.transform.localScale = Vector3.zero;
                }
                else
                {
                    quickSlots.transform.localScale = Vector3.one;
                    barsPanel.transform.localScale = Vector3.one;
                }
        }

        //Class: SNCameraRoot
        //Surround sound was not working right since there were two audio listener components attached to the main camera
        //Can't remember why I had to remove both before adding back just the StudioListener.
        //Should probably check if using Destroy instead of DestroyImmediate would work since it is supposed to be safer.
        public static void SNCam_Awake_Postfix(SNCameraRoot __instance)
        {
            GameObject mainCamera = __instance.transform.Find("MainCamera").gameObject;
            if (mainCamera != null)
            {
                UnityEngine.Object.DestroyImmediate(__instance.gameObject.GetComponent<AudioListener>());
                UnityEngine.Object.DestroyImmediate(__instance.gameObject.GetComponent<StudioListener>());
                mainCamera.AddComponent<StudioListener>();
            }
            
        }


        //Class: uGUI_SceneLoading
        /*
        Loading--[RectTransform | uGUI_SceneLoading | CanvasGroup | ]
        |	LoadingScreen--[RectTransform | CanvasRenderer | Image | uGUI_Fader | ]
        |	|	LoadingArtwork--[RectTransform | CanvasRenderer | Image | AspectRatioFitter | ]
        |	|	LoadingText--[RectTransform | CanvasRenderer | Text | uGUI_TextFade | ]
        |	|	Logo--[RectTransform | CanvasRenderer | uGUI_Logo | ]
        */
        public static void Init_Postfix(uGUI_SceneLoading __instance)
        {
            Image loadingArtwork = null;
            RectTransform textRect = null;
            RectTransform logoRect = null;
            try
            {
                loadingArtwork = __instance.loadingBackground.transform.Find("LoadingArtwork").GetComponent<Image>();
                textRect = __instance.loadingText.gameObject.GetComponent<RectTransform>();
                logoRect = __instance.loadingBackground.transform.Find("Logo").GetComponent<RectTransform>();
            }
            catch(Exception ex)
            {
                Debug.Log("VR Enhancements Mod: Error finding Loading Screen Elements: " + ex.Message);
                return;
            }
            Vector2 midCenter = new Vector2(0.5f, 0.5f);
            if(loadingArtwork != null && textRect != null && logoRect != null)
            {
                //remove background image and set background to black
                loadingArtwork.sprite = null;
                loadingArtwork.color = Color.black;
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
        }
        public static bool EnterCameraView_Prefix(CyclopsExternalCams __instance)
        {
            Traverse.Create(__instance).Field("usingCamera").SetValue(true);//Using Harmony Reflection Helper to set private variable usingCamera.
            InputHandlerStack.main.Push(__instance);
            Player main = Player.main;
            MainCameraControl.main.enabled = false;
            Player.main.SetHeadVisible(true);
            __instance.cameraLight.enabled = true;
            Traverse.Create(__instance).Method("ChangeCamera", 0).GetValue();//Call the private method ChangeCamera(0) using Harmony Reflection Helper
            if (__instance.lightingPanel)
            {
                __instance.lightingPanel.TempTurnOffFloodlights();
            }
            return false;
        }
        //VRViewModelAngle was being locked in the Open method but not reset in the close method
        //This fixes the wrong orientation of the the player model during animations after using the builder tool.
        public static void BuilderMenuClose_Postfix()
        {
            MainCameraControl.main.ResetLockedVRViewModelAngle();
        }

        //Reduce the size of the HUD in the Drone Camera to make the health and energy bars visible
        //Look into moving the HUD further back instead of scaling it down.
        public static void CameraDroneAwake_Postfix(uGUI_CameraDrone __instance)
        {
            GameObject droneCamera = __instance.transform.Find("Content").Find("CameraScannerRoom").gameObject;
            if (droneCamera != null)
            {
                droneCamera.GetComponent<RectTransform>().localScale = new Vector3(0.6f, 0.6f, 1f);
                return;
            }
            Debug.Log("VR Enhancements Mod: Cannot set Drone UI scale. Drone Camera Not Found");
        }

        public static void CameraCyclopsAwake_Postfix(uGUI_CameraCyclops __instance)
        {
            GameObject cyclopsCamera = __instance.transform.Find("Content").Find("CameraCyclops").gameObject;
            if (cyclopsCamera != null)
            {
                cyclopsCamera.GetComponent<RectTransform>().localScale = new Vector3(0.7f, 0.7f, 1f);
                return;
            }
            Debug.Log("VR Enhancements Mod: Cannot set CyclopsCamera UI scale. Cyclops Camera Not Found");
        }

        public static void MainMenuAwake_Postfix(uGUI_MainMenu __instance)
        {
            GameObject mainMenu = __instance.transform.Find("Panel").Find("MainMenu").gameObject;
            if (mainMenu != null)
            {
                mainMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 385);
                return;
            }
            Debug.Log("VR Enhancements Mod: Cannot set Main Menu Postions. MainMenu Not Found");
        }

        //replace skipcinematic method to disable VR recenter after a cinematic. Only the player should initiate a recenter.
        public static bool SkipCinematic_Prefix(PlayerCinematicController __instance, Player player)
        {
            Traverse.Create(__instance).Field("player").SetValue(player);//set private player field
            if (player)
            {
                Transform playerTransform = player.GetComponent<Transform>();
                Transform MCCTransform = MainCameraControl.main.GetComponent<Transform>();
                if (Traverse.Create(__instance).Method("UseEndTransform").GetValue<bool>())//execute private bool method
                {
                    player.playerController.SetEnabled(false);
                    /*if (XRSettings.enabled)
                    {
                        MainCameraControl.main.ResetCamera();
                        VRUtil.Recenter();
                    }*/
                    playerTransform.position = __instance.endTransform.position;
                    playerTransform.rotation = __instance.endTransform.rotation;
                    MCCTransform.rotation = playerTransform.rotation;
                }
                player.playerController.SetEnabled(true);
                player.cinematicModeActive = false;
            }
            if (__instance.informGameObject != null)
            {
                __instance.informGameObject.SendMessage("OnPlayerCinematicModeEnd", __instance, SendMessageOptions.DontRequireReceiver);
            }
            return false;//don't execute original SkipCinematic method
        }

        //code copied from the quit to desktop mod and modified
        public static void IGM_Awake_Postfix(IngameMenu __instance)
        {
            if (__instance != null && recenterVRButton == null)
            {
                //I think this is copying an existing button
                Button menuButton = __instance.quitToMainMenuButton.transform.parent.GetChild(0).gameObject.GetComponent<Button>();                
                recenterVRButton = UnityEngine.Object.Instantiate<Button>(menuButton, __instance.quitToMainMenuButton.transform.parent);
                recenterVRButton.transform.SetSiblingIndex(1);//put the button in the second position in the menu
                recenterVRButton.name = "RecenterVR";
                recenterVRButton.onClick.RemoveAllListeners();//this seems to be removing listeners that would have been copied from the original button
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
        //fix the camera position to match head position after recentering VR(F2 key)
        public static void ResetOrientation_Postfix(MainGameController __instance)
        {
            MainCameraControl.main.cameraOffsetTransform.localPosition = new Vector3(0f, 0f, 0.15f);
        }

        public static void PDA_Open_Postfix(PDA __instance, bool __result)
        {
            //if the PDA was opened
            if (__result)
            {
                if (!leftHandTarget)
                    leftHandTarget = new GameObject();
                //leftHandTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //leftHandTarget.GetComponent<Collider>().enabled = false;
                // leftHandTarget.transform.localScale *= 0.05f;
                leftHandTarget.transform.parent = Player.main.camRoot.transform;
                if (Player.main.motorMode != Player.MotorMode.Vehicle)
                    leftHandTarget.transform.localPosition = leftHandTarget.transform.parent.transform.InverseTransformPoint(Player.main.playerController.forwardReference.position + Player.main.armsController.transform.right * pdaXOffset + Vector3.up * -0.15f + new Vector3(Player.main.armsController.transform.forward.x, 0f, Player.main.armsController.transform.forward.z).normalized * pdaZOffset);
                else
                    leftHandTarget.transform.localPosition = leftHandTarget.transform.parent.transform.InverseTransformPoint(leftHandTarget.transform.parent.transform.position + leftHandTarget.transform.parent.transform.right * pdaXOffset + leftHandTarget.transform.parent.transform.forward * pdaZOffset + leftHandTarget.transform.parent.transform.up * -0.15f);
                leftHandTarget.transform.rotation = Player.main.armsController.transform.rotation * Quaternion.Euler(pdaXRot, pdaYRot, pdaZRot);
            }
        }
        public static void PDA_Close_Postfix()
        {
            if (leftHandTarget)
            {
                GameObject.Destroy(leftHandTarget);
            }
        }
        public static bool PDA_Update_Prefix()
        {
            if(leftHandTarget)
                myIK.solver.leftHandEffector.target = leftHandTarget.transform;
            return true;
        }

        public static void Reconfigure_Postfix(ArmsController __instance)
        {
            //This fixes a bug in the original code where reconfigureWorldTarget was set to true in SetWorldIKTarget but never reset to false after running Reconfigure
            //This caused the PDA ik target to work until piloting a vehicle which called SetWorldIKTarget and continuously called Reconfigure every frame after.
            Traverse.Create(__instance).Field("reconfigureWorldTarget").SetValue(false);
        }
        public static void ArmsCon_Start_Postfix(ArmsController __instance)
        {
            //get the private ik field from ArmsController to use it in PDA Update method
            myIK = Traverse.Create(__instance).Field("ik").GetValue<FullBodyBipedIK>();
        }

        public static void uGUI_SunbeamCountdown_Start_Postfix(uGUI_SunbeamCountdown __instance)
        {
            Vector2 midCenter = new Vector2(0.5f, 0.5f);
            __instance.countdownHolder.GetComponent<RectTransform>().anchorMax = midCenter;
            __instance.countdownHolder.GetComponent<RectTransform>().anchorMin = midCenter;
            __instance.countdownHolder.GetComponent<RectTransform>().pivot = midCenter;
            __instance.countdownHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -275f);
            __instance.countdownHolder.GetComponent<RectTransform>().localScale = Vector3.one * 0.75f;
            __instance.transform.Find("Background").GetComponent<CanvasRenderer>().SetAlpha(0f);
        }
    }//main patcher class
}
