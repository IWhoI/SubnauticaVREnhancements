﻿using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace VREnhancements
{
    class VRHUD
    {
        //TODO: Alot of this is messy and could probably be done better. Also consider merging this with UIElementFixes.
        private static List<GameObject> DynamicHUDElements = new List<GameObject>();
        public static uGUI_SceneHUD sceneHUD;
       
        //Add an element by name to the HUD Elements List.
        public static bool AddHUDElement(string name)
        {
            GameObject element = GameObject.Find(name);
            if (element && !DynamicHUDElements.Contains(element))
            {
                DynamicHUDElements.Add(element);
                return true;
            }
            return false;
        }

        //consider using this to set the opacity of elements in HUDElements instead of setting the scale in Hide and Show.
        public static void UpdateHUDOpacity(float alpha)
        {
            //this sets the alpha for elements that are not part of the CanvasGroup that was added to the HUD object
            foreach (GameObject element in DynamicHUDElements)
            {
                if(element)
                    foreach (CanvasRenderer renderer in element.GetComponentsInChildren<CanvasRenderer>())
                    {
                        //there has to be a better way to do this. This is to maintain the invisible Sunbeam background set in UIElementsFixes
                        if(!(renderer.transform.parent.name=="SunbeamCountdown" && renderer.name=="Background"))
                            renderer.SetAlpha(alpha);
                    }
            } 
            if (sceneHUD.GetComponent<CanvasGroup>())
                sceneHUD.GetComponent<CanvasGroup>().alpha = alpha;
            CanvasGroup HandReticleCG = HandReticle.main.GetComponent<CanvasGroup>();
            if(HandReticleCG)
            {
                HandReticleCG.ignoreParentGroups = true;//not sure if this will cause issues when changes are made to the ScreenCanvas CanvasGroup
                HandReticleCG.alpha = 1;                
            }

        }
        public static void UpdateHUDDistance(float distance)
        {
            if (sceneHUD)
            {
                sceneHUD.GetComponentInParent<Canvas>().transform.position =
                    new Vector3(sceneHUD.GetComponentInParent<Canvas>().transform.position.x,
                    sceneHUD.GetComponentInParent<Canvas>().transform.position.y,
                    distance);
            }
        }
        public static void UpdateHUDScale(float scale)
        {
            if(sceneHUD)
                sceneHUD.GetComponent<RectTransform>().localScale = Vector3.one * scale;
        }

        static void InitHUD()
        {
            AddHUDElement("SunbeamCountdown");
            UpdateHUDOpacity(AdditionalVROptions.HUD_Alpha);
            UpdateHUDDistance(AdditionalVROptions.HUD_Distance);
            UpdateHUDScale(AdditionalVROptions.HUD_Scale);
        }


        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.End))]
        class SceneLoading_End_Patch
        {
            static void Postfix(uGUI_SceneLoading __instance)
            {
                //only Initialize HUD after loading to make sure AdditionalVROptions are loaded first. Might have a better way to do this.
                InitHUD();
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Awake))]
        class SceneHUD_Awake_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                sceneHUD = __instance;//keep a reference to the sceneHUD
                sceneHUD.gameObject.AddComponent<CanvasGroup>();
            }
        }
        [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.Start))]
        class HandReticle_Start_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if (HandReticle.main)
                    HandReticle.main.gameObject.AddComponent<CanvasGroup>();
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneHUD), nameof(uGUI_SceneHUD.Update))]
        class SceneHUD_Update_Patch
        {
            static void Postfix(uGUI_SceneHUD __instance)
            {
                if (AdditionalVROptions.DynamicHUD && MainCamera.camera)
                {
                        //fades the hud in based on the angle that the player is looking in. Straight up is 270 and forward is 360/0
                        if (MainCamera.camera.transform.localEulerAngles.x < 180)
                            UpdateHUDOpacity(Mathf.Clamp((MainCamera.camera.transform.localEulerAngles.x - 30) / 15, 0, 1) * AdditionalVROptions.HUD_Alpha);
                        else
                            UpdateHUDOpacity(0);
                }
            }
        }
    }
}
