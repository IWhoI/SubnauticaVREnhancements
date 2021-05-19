using System;
using UnityEngine;
using UnityEngine.XR;
using QModManager.API.ModLoading;
using HarmonyLib;

namespace VREnhancements
{
    [QModCore]
    public static class MainPatcher
    {
        [QModPatch]
        public static void Patch()
        {
            if (XRSettings.enabled)
            {                
                try
                {
                    Harmony harmony = new Harmony("com.whotnt.subnautica.vrenhancements.mod");
                    harmony.PatchAll();
                    Console.WriteLine("[VR Enhancements] Patched");
                }
                catch (Exception ex)
                {
                    Debug.Log("Error with VREnhancements patching: " + ex.Message);
                }
            }
        }
    }
}
