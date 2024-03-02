using System;
using UnityEngine;
using UnityEngine.XR;
using BepInEx;
using HarmonyLib;

namespace VREnhancements
{
    [BepInPlugin("com.whotnt.subnautica.vrenhancements.mod", "VREnhancements", "3.2.0")]
    public class MainPatcher:BaseUnityPlugin
    {
       private void Awake()
        {
            if (XRSettings.enabled)
            {                
                try
                {
                    Harmony harmony = new Harmony("com.whotnt.subnautica.vrenhancements.mod");
                    harmony.PatchAll();
                    Console.WriteLine("[VR Enhancements] Patched. Unity Version: "+Application.unityVersion);
                    //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error with VREnhancements patching: " + ex.Message);
                }
            }
        }
    }
}
