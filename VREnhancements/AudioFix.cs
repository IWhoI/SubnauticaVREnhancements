using HarmonyLib;
using UnityEngine;
using FMODUnity;

namespace VREnhancements
{
    class AudioFix
    {
        [HarmonyPatch(typeof(SNCameraRoot), nameof(SNCameraRoot.Awake))]
        class Awake_Patch
        {
            static void Postfix(SNCameraRoot __instance)
            {
                if (SNCameraRoot.main.mainCam)
                {
                    //remove the audio listeners from the PlayerCameras object that does not rotate with the VR headset
                    Object.Destroy(__instance.gameObject.GetComponent<AudioListener>());
                    Object.Destroy(__instance.gameObject.GetComponent<StudioListener>());
                    //add new listener to the main camera that does rotate with the VR headset
                    SNCameraRoot.main.mainCam.gameObject.AddComponent<StudioListener>();
                }
            }
        }
    }
}
