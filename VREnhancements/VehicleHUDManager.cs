using UnityEngine;

namespace VREnhancements
{
    class VehicleHUDManager : MonoBehaviour
    {
        GameObject vehicleHUD;
        CanvasGroup canvasGroup;
        Transform barsPanel;
        Transform quickSlots;
        Transform seamothHUD;
        Transform exosuitHUD;
        Transform compass;
        Transform HUDContent;
        float canvasDistance = 1.15f;
        Vector3 seamothHUDPos = new Vector3(250,-100,0);
        Vector3 seamothCompassPos = new Vector3(0, 450, 0);
        Vector3 seamothQuickSlotsPos = new Vector3(0, -250, 0);
        Vector3 seamothBarsPanelPos = new Vector3(-250, 0, 0);
        Vector3 exosuitHUDPos = new Vector3(450, 230, 0);
        Vector3 exosuitCompassPos = new Vector3(0, 500, 0);
        Vector3 exosuitQuickSlotsPos = new Vector3(0, -600, 0);
        Vector3 exosuitBarsPanelPos = new Vector3(-450, 230, 0);
        Vector3 originalCompassPos;
        Vector3 originalQuickSlotsPos;
        Vector3 originalBarsPanelPos;
        bool vehicleHUDAttached = false;

        void Awake()
        {
            //create a new canvas worldspace canvas for vehicles
            vehicleHUD = new GameObject("VRVehicleHUD");
            DontDestroyOnLoad(vehicleHUD);
            vehicleHUD.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasGroup = vehicleHUD.AddComponent<CanvasGroup>();
            //TODO:Not that important but figure out how to get raycasts working on the new canvas so drag and drop will work on the quickslots while PDA is open in the vehicle
            //vehicleHUD.AddComponent<uGUI_GraphicRaycaster>();
            vehicleHUD.layer = LayerMask.NameToLayer("Default");
            vehicleHUD.transform.localScale = Vector3.one * 0.0015f;//set scale to the original ScreenCanvas scale
            vehicleHUD.transform.localPosition = new Vector3(0,0, canvasDistance);
            canvasGroup.alpha = AdditionalVROptions.HUD_Alpha;
            HUDContent = GameObject.Find("HUD/Content").transform;
            seamothHUD = HUDContent.Find("Seamoth").transform;
            exosuitHUD = HUDContent.Find("Exosuit").transform;
            quickSlots = HUDContent.Find("QuickSlots").transform;
            compass = HUDContent.Find("DepthCompass").transform;
            barsPanel = HUDContent.Find("BarsPanel").transform;
            seamothHUD.SetParent(vehicleHUD.transform, false);//move the vehicle specific HUD elements to the new vehicle HUD
            seamothHUD.localPosition = seamothHUDPos;
            exosuitHUD.SetParent(vehicleHUD.transform, false);
            exosuitHUD.localPosition = exosuitHUDPos;
            vehicleHUD.SetActive(false);
        }

        void Update()
        {
            Player player = Player.main;
            if (player)
            {
                //TODO: Probably better to just do this attachment in the Player.Awake
                if (!vehicleHUDAttached)
                {
                    vehicleHUD.transform.SetParent(MainCameraControl.main.viewModel, false);//attach vehicle hud to the player body instead of camera. Didn't attach to vehicle since hud moved around too much when operating the exosuit.
                    vehicleHUDAttached = true;
                }
                PDA pda = player.GetPDA();
                if (player.inSeamoth || player.inExosuit)
                {
                    if(!vehicleHUD.activeInHierarchy)
                    {
                        originalCompassPos = compass.localPosition;
                        originalQuickSlotsPos = quickSlots.localPosition;
                        originalBarsPanelPos = barsPanel.localPosition;
                        compass.SetParent(vehicleHUD.transform, false);
                        quickSlots.SetParent(vehicleHUD.transform, false);
                        barsPanel.SetParent(vehicleHUD.transform, false);
                        if (player.inSeamoth)
                        {
                            compass.localPosition = seamothCompassPos;
                            quickSlots.localPosition = seamothQuickSlotsPos;
                            barsPanel.localPosition = seamothBarsPanelPos;
                        }
                        else
                        {
                            compass.localPosition = exosuitCompassPos;
                            quickSlots.localPosition = exosuitQuickSlotsPos;
                            barsPanel.localPosition = exosuitBarsPanelPos;
                        }
                        vehicleHUD.SetActive(true);
                    }
                }
                else if(vehicleHUD.activeInHierarchy)
                {
                    compass.SetParent(HUDContent, false);
                    compass.localPosition = originalCompassPos;
                    quickSlots.SetParent(HUDContent, false);
                    quickSlots.localPosition = originalQuickSlotsPos;
                    barsPanel.SetParent(HUDContent, false);
                    barsPanel.localPosition = originalBarsPanelPos;
                    vehicleHUD.SetActive(false);
                }
            }
        }
    }
}
