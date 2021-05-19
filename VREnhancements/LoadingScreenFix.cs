using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace VREnhancements
{
    class LoadingScreenFix
    {
        [HarmonyPatch(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.Init))]
        class SceneLoading_Init_Patch
        {
        /*
        Loading--[RectTransform | uGUI_SceneLoading | CanvasGroup | ]
        |	LoadingScreen--[RectTransform | CanvasRenderer | Image | uGUI_Fader | ]
        |	|	LoadingArtwork--[RectTransform | CanvasRenderer | Image | AspectRatioFitter | ]
        |	|	LoadingText--[RectTransform | CanvasRenderer | Text | uGUI_TextFade | ]
        |	|	Logo--[RectTransform | CanvasRenderer | uGUI_Logo | ]
        */
            static void Postfix(uGUI_SceneLoading __instance)
            {
                Image loadingArtwork = null;
                RectTransform textRect = null;
                RectTransform logoRect = null;
                try
                {
                    loadingArtwork = __instance.loadingBackground.transform.Find("LoadingArtwork").GetComponent<Image>();
                    textRect = __instance.loadingText.gameObject.GetComponent<RectTransform>();
                    logoRect = __instance.loadingBackground.transform.Find("Logo").GetComponent<RectTransform>();
                }
                catch (Exception ex)
                {
                    Debug.Log("VR Enhancements Mod: Error finding Loading Screen Elements: " + ex.Message);
                    return;
                }
                Vector2 midCenter = new Vector2(0.5f, 0.5f);
                if (loadingArtwork != null && textRect != null && logoRect != null)
                {
                    //remove background image and set background to black
                    loadingArtwork.sprite = null;
                    loadingArtwork.color = Color.black;
                    //center the logo
                    logoRect.anchorMin = midCenter;
                    logoRect.anchorMax = midCenter;
                    logoRect.pivot = midCenter;
                    logoRect.anchoredPosition = Vector2.zero;
                    //center text and offset below logo
                    textRect.anchorMin = midCenter;
                    textRect.anchorMax = midCenter;
                    textRect.pivot = midCenter;
                    textRect.anchoredPosition = new Vector2(0f, -200f);
                    textRect.sizeDelta = new Vector2(400f, 100f);
                    textRect.gameObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                }
            }
        }
    }
}
