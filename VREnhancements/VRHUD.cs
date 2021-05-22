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
        public static void UpdateHUDDistance(float distance)
        {
            if(sceneHUD)
            sceneHUD.GetComponentInParent<Canvas>().transform.position = 
                new Vector3(sceneHUD.GetComponentInParent<Canvas>().transform.position.x,
                sceneHUD.GetComponentInParent<Canvas>().transform.position.y,
                distance);
        }
        public static void UpdateHUDScale(float scale)
        {
            if(sceneHUD)
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            Debug.Log("HUD Scale: " + scale);
        }

        /*[HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        class Player_Awake_Patch
        {
            static void Postfix(Player __instance)
            {
                AddHUDElement("BarsPanel");
                AddHUDElement("QuickSlots");
                AddHUDElement("SunbeamCountdown");
            }

        }*/

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;//keep a reference to the sceneHUD
                AddHUDElement("BarsPanel");
                AddHUDElement("QuickSlots");
                AddHUDElement("SunbeamCountdown");
                UpdateHUDOpacity(AdditionalVROptions.HUDAlpha);
                UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
                UpdateHUDScale(AdditionalVROptions.HUD_Scale);
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
                        //fades the hud in based on the angle that the player is looking in. Straight up is 270 and forward is 360/0
                        if (MainCamera.camera.transform.localEulerAngles.x < 180)
                            UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - 30) / 20, 0, 1) * AdditionalVROptions.HUDAlpha);
                        else
                            UpdateHUDOpacity(0);
                    }
                }
                if (Input.GetKeyUp(KeyCode.Y))
                {
                    ErrorMessage.AddDebug("HUD RectT Scale: " + sceneHUD.GetComponent<RectTransform>().localScale);
                }
            }
        }
        
    }
}
