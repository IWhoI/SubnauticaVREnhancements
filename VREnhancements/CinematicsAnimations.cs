using HarmonyLib;

namespace VREnhancements
{
    class CinematicsAnimations
    {
        [HarmonyPatch(typeof(GameOptions), nameof(GameOptions.GetVrAnimationMode))]
        class GetVRAnimationMode_Patch
        {
            static bool Prefix(ref bool __result)
            {
                //if VR animations are enabled in the menu then we need to return false to get animations to play.
                __result = !GameOptions.enableVrAnimations;
                return false;//dont execute the original method since we are only using the menu option to determine if animations play.
            }
        }

        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnPilotModeBegin))]
        class OnPilotModeBegin_Patch
        {
           
            static bool Prefix(Vehicle __instance)
            {
                if (__instance.mainAnimator)
                {
                    __instance.mainAnimator.SetBool("vr_active", GameOptions.GetVrAnimationMode());
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnPilotModeEnd))]
        class OnPilotModeEnd_Patch
        {            
             static bool Prefix(Vehicle __instance)
            {
                if (__instance.mainAnimator)
                {
                    __instance.mainAnimator.SetBool("vr_active", GameOptions.GetVrAnimationMode());
                }
                return true;
            }
        }
    }

}
