using HarmonyLib;
using UnityEngine;
using FMODUnity;

namespace VREnhancements
{
    class _3DAudioFix
    {
        [HarmonyPatch(typeof(SNCameraRoot), nameof(SNCameraRoot.Awake))]
        class Awake_Patch
        {
            //Surround sound was not working right since there were two audio listener components attached to the main camera
            //Can't remember why I had to remove both before adding back just the StudioListener.
            //Should probably check if using Destroy instead of DestroyImmediate would work since it is supposed to be safer.
            static void Postfix(SNCameraRoot __instance)
            {
                if (SNCameraRoot.main.mainCam)
                {
                    //remove the audio listeners from the PlayerCameras object that does not rotate with the VR headset
                    Object.DestroyImmediate(__instance.gameObject.GetComponent<AudioListener>());
                    Object.DestroyImmediate(__instance.gameObject.GetComponent<StudioListener>());
                    //add new listener to the main camera that does rotate with the VR headset
                    SNCameraRoot.main.mainCam.gameObject.AddComponent<StudioListener>();
                }
            }
        }
    }
}
