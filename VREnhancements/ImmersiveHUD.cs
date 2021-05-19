using HarmonyLib;
using UnityEngine;

namespace VREnhancements
{
    class ImmersiveHUD
    {
        private static GameObject quickSlots;
        private static GameObject barsPanel;
        public static bool Disable()
        {
            if (quickSlots != null && barsPanel != null)
            {
                //disable immersive hud by making the original hud always visible
                quickSlots.transform.localScale = Vector3.one;
                barsPanel.transform.localScale = Vector3.one;
                AdditionalVROptions.immersiveHUD = false;
                return true;
            }
            else
                return false;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        class Player_Awake_Patch
        {
            static void Postfix(Player __instance)
            {
                barsPanel = GameObject.Find("BarsPanel");
                quickSlots = GameObject.Find("QuickSlots");
            }

        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Update))]
        class HUD_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if (AdditionalVROptions.immersiveHUD && quickSlots != null && barsPanel != null)
                    if (Player.main != null && Vector3.Angle(MainCamera.camera.transform.forward, Player.main.transform.up) < 120f)
                    {
                        quickSlots.transform.localScale = Vector3.zero;
                        barsPanel.transform.localScale = Vector3.zero;
                    }
                    else
                    {
                        quickSlots.transform.localScale = Vector3.one;
                        barsPanel.transform.localScale = Vector3.one;
                    }
            }
        }
        
    }
}
