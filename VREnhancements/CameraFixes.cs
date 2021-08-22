using HarmonyLib;
using UnityEngine;

namespace VREnhancements
{
    class CameraFixes
    {
        static Transform forwardRefTransform;
        static Transform headRig;
        [HarmonyPatch(typeof(MainCameraControl), nameof(MainCameraControl.Update))]
        class MCC_Update_Patch
        {
            static float yOffset;
            static void Postfix(MainCameraControl __instance)
            {
                if (!__instance.cinematicMode && Player.main.motorMode != Player.MotorMode.Vehicle)
                {
                    //offset the body/seaglide a little more to improve visibility while piloting the seaglide
                    if (Player.main.motorMode == Player.MotorMode.Seaglide)
                        yOffset = -0.18f;
                    else
                        yOffset = -0.08f;
                    //move the body so that the head bone always tracks the headset/camera position
                    __instance.viewModel.transform.localPosition = __instance.viewModel.transform.parent.worldToLocalMatrix.MultiplyPoint(forwardRefTransform.position - (headRig.position - __instance.viewModel.transform.position) + forwardRefTransform.up * yOffset + forwardRefTransform.forward * -0.02f);
                }
            }

        }
        [HarmonyPatch(typeof(MainCameraControl), nameof(MainCameraControl.Awake))]
        class MCC_Awake_Patch
        {
            static void Postfix(MainCameraControl __instance)
            {
                forwardRefTransform = MainCamera.camera.transform;
                headRig = __instance.viewModel.transform.Find("player_view/export_skeleton/head_rig");
            }
                
        }
        /*TODO: Check how difficult it is to fix the Seaglide and PDA problems with this attempt to decouple movement from head direction
         * [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.forwardReference))]
        class PlayerCon_fwdRef_Patch
        {
            static void Postfix(PlayerController __instance, ref Transform __result)
            {
                Transform temp = new GameObject().transform;
                temp.rotation = Quaternion.Euler(__result.rotation.eulerAngles.x, MainCameraControl.main.transform.rotation.eulerAngles.y, 0);
                __result = temp;
            }

        }*/

        [HarmonyPatch(typeof(MainGameController), nameof(MainGameController.StartGame))]
        class MGC_StartGame_Patch
        {
            //fix the camera position to match head position
            static void Postfix()
            {
                MainCameraControl.main.cameraOffsetTransform.localPosition = new Vector3(0f, 0f, 0.15f);
            }
        }

        [HarmonyPatch(typeof(MainGameController), nameof(MainGameController.ResetOrientation))]
        class MGC_ResetOrientation_Patch
        {
            //I'm not sure if this is necessary after doing it in StartGame above but there may be a case where 
            //it gets reset during gameplay so making sure it get fixed when VRUtil.Recenter is called. 
            //fix the camera position to match head position
            static void Postfix()
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
                //this is a quick fix to disable translucent player body but there may be a better way to fix this since the body isn't translucent when the PDA is open
                //TODO: Figure out a better way to prevent the translucent body
                foreach (SkinnedMeshRenderer renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    renderer.fadeAmount = 0;
                }
            }
        }
        [HarmonyPatch(typeof(WaterSunShaftsOnCamera), nameof(WaterSunShaftsOnCamera.Awake))]
        class SunShafts_Awake_Patch
        {
            static void Postfix(WaterSunShaftsOnCamera __instance)
            {
                __instance.reduction = 6;//default is 2. console is 4. This improves performance with little noticable difference to the sun shafts.
            }

        }
    }
}
