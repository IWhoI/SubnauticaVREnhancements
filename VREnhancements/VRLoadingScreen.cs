using UnityEngine;
using UnityEngine.UI;

namespace VREnhancements
{
    class VRLoadingScreen : MonoBehaviour
    {
        //Transform UICamera;
        Camera mainCamera;
        public static VRLoadingScreen main;
        void Awake()
        {
                main = this;
        }
        void Start()
        { 
            //UICamera = ManagedCanvasUpdate.GetUICamera().transform;
            GameObject go = GameObject.Find("Main Camera");
            if (go)
                mainCamera = go.GetComponent<Camera>();
            //transform.GetComponent<uGUI_CanvasScaler>().distance = AdditionalVROptions.HUD_Distance;
            Image loadingArtwork = uGUI.main.loading.loadingBackground.transform.Find("LoadingArtwork").GetComponent<Image>();
            RectTransform textRect = uGUI.main.loading.loadingText.gameObject.GetComponent<RectTransform>();
            RectTransform logoRect = uGUI.main.loading.loadingBackground.transform.Find("Logo").GetComponent<RectTransform>();
            Vector2 midCenter = new Vector2(0.5f, 0.5f);
            if (loadingArtwork != null && textRect != null && logoRect != null)
            {
                //remove background image and set background to black
                loadingArtwork.sprite = null;
                loadingArtwork.color = Color.black;
                loadingArtwork.GetComponent<RectTransform>().localScale = Vector3.one * 2;//temporary fix for when hud distance is increased
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
        public void StartLoading()
        {
            GameObject go = GameObject.Find("Main Camera");
            if (go)
            {
                mainCamera = go.GetComponent<Camera>();
                mainCamera.enabled = false;//make sure only the loading screen is visible
                //don't need to reenable it since the main camera is replaced during the loading process
            }
        }
    }
}
