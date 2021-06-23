using UnityEngine;

namespace VREnhancements
{
    class VehicleHUDManager : MonoBehaviour
    {
        public static GameObject vehicleCanvas;
        Transform barsPanel;
        Transform quickSlots;
        public static Transform seamothHUD;
        public static Transform exosuitHUD;
        Transform compass;
        Transform HUDContent;
        float canvasDistance = 1.15f;
        Vector3 seamothHUDPos = new Vector3(250,-100,0);
        Vector3 seamothCompassPos = new Vector3(0, 450, 0);
        Vector3 seamothQuickSlotsPos = new Vector3(0, -250, 50);
        Vector3 seamothBarsPanelPos = new Vector3(-250, -60, 0);
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
            vehicleCanvas = new GameObject("VRVehicleCanvas");
            DontDestroyOnLoad(vehicleCanvas);
            vehicleCanvas.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            vehicleCanvas.AddComponent<CanvasGroup>();
            //TODO:Not that important but figure out how to get raycasts working on the new canvas so drag and drop will work on the quickslots while PDA is open in the vehicle
            //vehicleCanvas.AddComponent<uGUI_GraphicRaycaster>();
            vehicleCanvas.layer = LayerMask.NameToLayer("Default");
            vehicleCanvas.transform.localScale = Vector3.one * 0.0015f;//set scale to the original ScreenCanvas scale
            vehicleCanvas.transform.localPosition = new Vector3(0,0, canvasDistance);
            HUDContent = GameObject.Find("HUD/Content").transform;
            seamothHUD = HUDContent.Find("Seamoth").transform;
            exosuitHUD = HUDContent.Find("Exosuit").transform;
            quickSlots = HUDContent.Find("QuickSlots").transform;
            compass = HUDContent.Find("DepthCompass").transform;
            barsPanel = HUDContent.Find("BarsPanel").transform;
            seamothHUD.SetParent(vehicleCanvas.transform, false);//move the vehicle specific HUD elements to the new vehicle Canvas
            seamothHUD.localPosition = seamothHUDPos;
            exosuitHUD.SetParent(vehicleCanvas.transform, false);
            exosuitHUD.localPosition = exosuitHUDPos;
            vehicleCanvas.SetActive(false);
        }

        void Update()
        {
            Player player = Player.main;
            if (player)
            {
                //TODO: Probably better to just do this attachment in the Player.Start
                if (!vehicleHUDAttached)
                {
                    //attach vehicle canvas to the player body since it doesn't move when piloting. Didn't attach to vehicle since hud moved around too much when operating the exosuit.
                    vehicleCanvas.transform.SetParent(MainCameraControl.main.viewModel, false);
                    vehicleHUDAttached = true;
                }
                if (player.inSeamoth || player.inExosuit)
                {
                    if(!vehicleCanvas.activeInHierarchy)
                    {
                        originalCompassPos = compass.localPosition;
                        originalQuickSlotsPos = quickSlots.localPosition;
                        originalBarsPanelPos = barsPanel.localPosition;
                        compass.SetParent(vehicleCanvas.transform, false);
                        quickSlots.SetParent(vehicleCanvas.transform, false);
                        barsPanel.SetParent(vehicleCanvas.transform, false);
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
                        vehicleCanvas.SetActive(true);
                    }
                }
                else if(vehicleCanvas.activeInHierarchy)//if not in seamoth or exosuit but vehicleCanvas is active then move elements back to the normal HUD and disable vehicleCanvas
                {
                    compass.SetParent(HUDContent, false);
                    compass.localPosition = originalCompassPos;
                    quickSlots.SetParent(HUDContent, false);
                    quickSlots.localPosition = originalQuickSlotsPos;
                    barsPanel.SetParent(HUDContent, false);
                    barsPanel.localPosition = originalBarsPanelPos;
                    vehicleCanvas.SetActive(false);
                }
            }
        }
    }
}
