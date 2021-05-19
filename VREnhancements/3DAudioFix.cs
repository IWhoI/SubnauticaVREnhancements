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
                GameObject mainCamera = __instance.transform.Find("MainCamera").gameObject;
                if (mainCamera != null)
                {
                    //UnityEngine.Object.DestroyImmediate(__instance.gameObject.GetComponent<AudioListener>());
                    //UnityEngine.Object.DestroyImmediate(__instance.gameObject.GetComponent<StudioListener>());
                    UnityEngine.Object.DestroyImmediate(mainCamera.GetComponent<AudioListener>());
                    UnityEngine.Object.DestroyImmediate(mainCamera.GetComponent<StudioListener>());
                    mainCamera.AddComponent<StudioListener>();
                }
            }
        }
    }
}
