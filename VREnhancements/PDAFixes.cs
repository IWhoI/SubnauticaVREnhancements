using HarmonyLib;
using UnityEngine;
using RootMotion.FinalIK;
using System;
using static VFXParticlesPool;

namespace VREnhancements
{
    //TODO: Consider squashing the PDA model in the y axis and placing the PDA screen above the PDA to make it much larger
    class PDAFixes
    {
        static readonly float pdaScale = 1.45f;
        static readonly float screenScale = 0.0003f;
        static readonly float pdaXOffset = -0.35f;
        public static float pdaDistance=0.4f;
        static readonly float pdaXRot = 220f;
        static readonly float pdaYRot = 30f;
        static readonly float pdaZRot = 75f;
        static GameObject leftHandTarget;
        static FullBodyBipedIK myIK;

        public static void SetPDADistance(float distance)
        {
            pdaDistance = distance;
        }
        /*
         * Do this to make sure the PDA opens in front the player if allowing pitch control with input.
        [HarmonyPatch(typeof(PDA), nameof(PDA.Open))]
        class Prefix_PDA_Open_Patch
        {
            static bool Prefix(PDA __instance)
            {
                MainCameraControl.main.cameraOffsetTransform.localRotation = Quaternion.identity;
                MainCameraControl.main.cameraUPTransform.localRotation = Quaternion.identity;
                MainCameraControl.main.transform.localRotation = Quaternion.Euler(0, MainCameraControl.main.transform.localRotation.eulerAngles.y, MainCameraControl.main.transform.localRotation.eulerAngles.z);
                return true;
            }
        }*/

        [HarmonyPatch(typeof(PDA), nameof(PDA.Open))]
        class PDA_Open_Patch
        {
            static void Postfix(PDA __instance, bool __result)
            {
                //if the PDA was opened
                if (__result)
                {
                    //set the PDA and PDA Screen scale
                    uGUI_CanvasScaler contentScreen = __instance.ui.GetComponent<uGUI_CanvasScaler>();
                    __instance.transform.localScale = new Vector3(pdaScale, pdaScale, 1f);
                    contentScreen.transform.localScale = Vector3.one * screenScale;
                    contentScreen.SetAnchor(__instance.screenAnchor);
                    if (!leftHandTarget)
                        leftHandTarget = new GameObject();
                    leftHandTarget.transform.parent = Player.main.camRoot.transform;
                    //TODO: This is probably needlessly complicated and could be done in a simpler way
                    Transform armsTransform = Player.main.armsController.transform;
                    Transform leftHTParentTf = leftHandTarget.transform.parent.transform;
                    if (Player.main.motorMode != Player.MotorMode.Vehicle)
                        leftHandTarget.transform.localPosition = leftHTParentTf.InverseTransformPoint(Player.main.playerController.forwardReference.position + armsTransform.right * pdaXOffset + Vector3.up * -0.15f + new Vector3(armsTransform.forward.x, 0f, armsTransform.forward.z).normalized * pdaDistance);
                    else
                        leftHandTarget.transform.localPosition = leftHTParentTf.InverseTransformPoint(leftHTParentTf.position + leftHTParentTf.right * pdaXOffset + leftHTParentTf.forward * pdaDistance + leftHTParentTf.up * -0.15f);
                    leftHandTarget.transform.rotation = armsTransform.rotation * Quaternion.Euler(pdaXRot, pdaYRot, pdaZRot);
                }
            }
        }

        //this stops the model from snapping to 0 y rotation and turning back to head rotation when closing the PDA
        [HarmonyPatch(typeof(MainCameraControl), nameof(MainCameraControl.ResetLockedVRViewModelAngle))]
        class MainCameraControl_ResetVRViewModelAngle_Patch
        {
            static bool Prefix()
            {
                //do the reset in PDA.Deactivated which runs after the closing animation to prevent the player model rotation from snapping to y=0 while closing.
                if(Player.main.GetPDA().isInUse)
                    return false;
                else
                    return true;
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.Deactivated))]
        class PDA_Deactivated_Patch
        {
            static void Postfix()
            {
                //this was being done before the closing animation in PDA.Close and caused the player model rotation to snap to y=0 while closing
                MainCameraControl.main.ResetLockedVRViewModelAngle();
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.Close))]
        class PDA_Close_Patch
        {
            static void Postfix()
            {
                if (leftHandTarget)
                {
                    GameObject.Destroy(leftHandTarget);
                }
            }
        }

        [HarmonyPatch(typeof(PDA), nameof(PDA.ManagedUpdate))]
        class PDA_Update_Patch
        {
            static bool Prefix()
            {
                if (leftHandTarget)
                {
                    myIK.solver.leftHandEffector.target = leftHandTarget.transform;
                }  
                return true;
            }
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Reconfigure))]
        class ArmsCon_Reconfigure_Patch
        {
            static void Postfix(ArmsController __instance, ref bool ___reconfigureWorldTarget)
            {
                //This fixes a bug in the original game code where reconfigureWorldTarget was set to true in SetWorldIKTarget but never reset to false after running Reconfigure
                //This caused the PDA ik target to work until piloting a vehicle which called SetWorldIKTarget and continuously called Reconfigure every frame after.
                ___reconfigureWorldTarget = false;
            }
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
        class ArmsCon_Start_Patch
        {
            static void Postfix(ArmsController __instance, FullBodyBipedIK ___ik)
            {
                //get the private ik field from ArmsController to use it in PDA Update method
                myIK = ___ik;
            }
        }
    }
}
