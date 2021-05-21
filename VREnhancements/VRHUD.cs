using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace VREnhancements
{
    class VRHUD
    {
        private static List<GameObject> HUDElements = new List<GameObject>();
        private static float xRotation;
        private static float zOffset;
        private static float yOffset;
        private static float scale = 1;
        public static uGUI_SceneHUD sceneHUD;

        //Add an element by name to the HUD Elements List. Should probably do extra checks to make sure the element is a child of the HUD.
        public static bool AddHUDElement(string name)
        {
            GameObject element = GameObject.Find(name);
            if (element && !HUDElements.Contains(element))
            {
                HUDElements.Add(element);
                return true;
            }
            return false;
        }
        /*public static void ShowHUD()
        {
            foreach (GameObject element in HUDElements)
            {
                if(element)
                    element.transform.localScale = Vector3.one;
            }
        }
        public static void HideHUD()
        {
            foreach (GameObject element in HUDElements)
            {
                if (element)
                    element.transform.localScale = Vector3.zero;
            }
        }*/

        //consider using this to set the opacity of elements in HUDElements instead of setting the scale in Hide and Show.
        public static void UpdateHUDOpacity(float alpha)
        {
            foreach (GameObject element in HUDElements)
            {
                foreach (CanvasRenderer renderer in element.GetComponentsInChildren<CanvasRenderer>())
                {
                    //there has to be a better way to select which renderers are affected. This is to maintain the invisible Sunbeam background set in UIElementsFixes
                    if(!(renderer.transform.parent.name=="SunbeamCountdown" && renderer.name=="Background"))
                        renderer.SetAlpha(alpha);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        class Player_Awake_Patch
        {
            static void Postfix(Player __instance)
            {
                AddHUDElement("BarsPanel");
                AddHUDElement("QuickSlots");
                AddHUDElement("SunbeamCountdown");
            }

        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;//keep a reference to the sceneHUD
                UpdateHUDOpacity(AdditionalVROptions.HUDAlpha);

            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Update))]
        class SceneHUD_Update_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if (AdditionalVROptions.DynamicHUD)
                {                    
                    if(Player.main != null)
                    {
                        //fades the hud in based on the angle that the player is looking in.
                        UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - 30) / 20, 0, 1) * AdditionalVROptions.HUDAlpha);
                    }
                }
                /*if (Input.GetKeyUp(KeyCode.Y))
                {
                    ErrorMessage.AddDebug("MainCameraTransform: " + MainCamera.camera.transform.localEulerAngles);
                }*/
            }
        }
        
    }
}
