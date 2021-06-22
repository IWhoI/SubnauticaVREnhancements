using HarmonyLib;
using UnityEngine;
using RootMotion.FinalIK;

namespace VREnhancements
{
    class PDAFixes
    {
        static float pdaScale = 1.45f;//1.75f;
        static float screenScale = 0.0003f;//0.00035f;
        static float pdaXOffset = -0.35f;
        public static float pdaDistance=0.4f;
        static float pdaXRot = 220f;
        static float pdaYRot = 30f;
        static float pdaZRot = 75f;
        static GameObject leftHandTarget;
        static FullBodyBipedIK myIK;

        public void SetPDADistance(float distance)
        {
            pdaDistance = distance;
        }


        [HarmonyPatch(typeof(PDA), nameof(PDA.Open))]
        class PDA_Open_Patch
        {
            static void Postfix(PDA __instance, bool __result)
            {
                //if the PDA was opened
                if (__result)
                {
                    //set the PDA and PDA Screen scale
                    GameObject screen = Traverse.Create(__instance).Field("screen").GetValue<GameObject>();//get private variable screen
                    uGUI_CanvasScaler component = screen.GetComponent<uGUI_CanvasScaler>();
                    __instance.transform.localScale = new Vector3(pdaScale, pdaScale, 1f);
                    component.transform.localScale = Vector3.one * screenScale;
                    component.SetAnchor(__instance.screenAnchor);
                    if (!leftHandTarget)
                        leftHandTarget = new GameObject();
                    leftHandTarget.transform.parent = Player.main.camRoot.transform;
                    //TODO: This is probably needlessly complicated and could be done in a simpler way
                    if (Player.main.motorMode != Player.MotorMode.Vehicle)
                        leftHandTarget.transform.localPosition = leftHandTarget.transform.parent.transform.InverseTransformPoint(Player.main.playerController.forwardReference.position + Player.main.armsController.transform.right * pdaXOffset + Vector3.up * -0.15f + new Vector3(Player.main.armsController.transform.forward.x, 0f, Player.main.armsController.transform.forward.z).normalized * pdaDistance);
                    else
                        leftHandTarget.transform.localPosition = leftHandTarget.transform.parent.transform.InverseTransformPoint(leftHandTarget.transform.parent.transform.position + leftHandTarget.transform.parent.transform.right * pdaXOffset + leftHandTarget.transform.parent.transform.forward * pdaDistance + leftHandTarget.transform.parent.transform.up * -0.15f);
                    leftHandTarget.transform.rotation = Player.main.armsController.transform.rotation * Quaternion.Euler(pdaXRot, pdaYRot, pdaZRot);
                }
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

        [HarmonyPatch(typeof(PDA), nameof(PDA.Update))]
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
            static void Postfix(ArmsController __instance)
            {
                //This fixes a bug in the original game code where reconfigureWorldTarget was set to true in SetWorldIKTarget but never reset to false after running Reconfigure
                //This caused the PDA ik target to work until piloting a vehicle which called SetWorldIKTarget and continuously called Reconfigure every frame after.
                Traverse.Create(__instance).Field("reconfigureWorldTarget").SetValue(false);
            }
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
        class ArmsCon_Start_Patch
        {
            static void Postfix(ArmsController __instance)
            {
                //get the private ik field from ArmsController to use it in PDA Update method
                myIK = Traverse.Create(__instance).Field("ik").GetValue<FullBodyBipedIK>();
            }
        }
    }
}
