using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using QModManager.API.ModLoading;
using HarmonyLib;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;

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
                }
                catch (Exception ex)
                {
                    Debug.Log("Error with VREnhancements patching: " + ex.Message);
                }
            }
        }
    }
}
