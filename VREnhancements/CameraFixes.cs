using HarmonyLib;
using UnityEngine;

namespace VREnhancements
{
    class CameraFixes
    {
        [HarmonyPatch(typeof(MainCameraControl), nameof(MainCameraControl.Update))]
        class MCC_Update_Patch
        {
            private static float defaultZOffset = 0.17f;//default player model Z offset from camera
            private static float defaultYOffset = 0.0f;
            private static float seaglideZOffset = 0.1f;//player model Z offset when piloting the seaglide
            private static float seaglideYOffset = -0.15f;
            private static float swimZOffset = 0.08f;//player model Z offset when swimming
            private static float swimYOffset = -0.02f;
            private static float pdaCloseTimer = 0;
            private static bool pdaIsClosing = false;
            private static float pdaCloseDelay = 1f;
            static void Postfix(MainCameraControl __instance)
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
                //when the pda is opened the viewmodel is moved forward but even when the state is closed, it is kept foward for a short while which was causing the neck to 
                //show if I also moved the model forward at the same time. So I maintain my own closing state with pdaIsClosing and only move the model forward after pdaCloseDelay.
                //There may be a better way to solve this so someone please fix it.
                //I think the shifting of the model happens because of local position changes in the actual 3D model since the offset values didn't change during the model shift.
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
        }

        [HarmonyPatch(typeof(MainGameController), nameof(MainGameController.ResetOrientation))]
        class MGC_ResetOrientation_Patch
        {
            //fix the camera position to match head position after recentering VR
            static void Postfix(MainGameController __instance)
            {
                MainCameraControl.main.cameraOffsetTransform.localPosition = new Vector3(0f, 0f, 0.15f);
            }
        }

        [HarmonyPatch(typeof(PlayerCinematicController), nameof(PlayerCinematicController.SkipCinematic))]
        class SkipCinematic_Patch
        {
            //replace skipcinematic method to disable VR recenter after a cinematic. Only the player should initiate a recenter.
            static bool Prefix(PlayerCinematicController __instance, Player player)
            {
                Traverse.Create(__instance).Field("player").SetValue(player);//set private player field
                if (player)
                {
                    Transform playerTransform = player.GetComponent<Transform>();
                    Transform MCCTransform = MainCameraControl.main.GetComponent<Transform>();
                    if (Traverse.Create(__instance).Method("UseEndTransform").GetValue<bool>())//execute private bool method
                    {
                        player.playerController.SetEnabled(false);
                        //the following is what was removed from the original method
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
        }

        [HarmonyPatch(typeof(SNCameraRoot), nameof(SNCameraRoot.SetFov))]
        class SNCamSetFov_Patch
        {
            //Class: SNCameraRoot
            //Prevent this method from filling the log with errors when trying to set Fov while in VR mode.
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsExternalCams), nameof(CyclopsExternalCams.EnterCameraView))]
        class EnterCameraView_Patch
        {
            //removed the VRUtil.Recenter call from the original method
            static bool Prefix(CyclopsExternalCams __instance)
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
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
        class ArmsController_Start__Patch
        {

            static void Postfix(ArmsController __instance)
            {
                //the player model materials have the shader keyword UWE_VR_FADEOUT which seems to cause the top part of the model to go translucent
                //this is a quick fix to disable this fading out.
                //TODO: Figure out how opening the PDA usually disables this.
                foreach (SkinnedMeshRenderer renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    renderer.fadeAmount = 0;
                }
            }
        }
    }
}
