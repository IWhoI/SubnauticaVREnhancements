using UnityEngine;
using UnityEngine.UI;

namespace VREnhancements
{
    class VehicleHUDManager : MonoBehaviour
    {
        public static GameObject vehicleCanvas;
        uGUI_CanvasScaler canvasScaler;
        Transform barsPanel;
        Transform quickSlots;
        public static Transform seamothHUD;
        public static Transform exosuitHUD;
        Transform compass;
        Transform HUDContent;
        Vector3 seamothHUDPos = new Vector3(350, -300, 1000);
        Vector3 seamothCompassPos = new Vector3(0, 450, 950);
        Vector3 seamothQuickSlotsPos = new Vector3(0, -400, 1100);
        Vector3 seamothBarsPanelPos = new Vector3(-350, -250, 1000);
        Vector3 exosuitHUDPos = new Vector3(700, -200, 600);
        Vector3 exosuitCompassPos = new Vector3(0, 400, 700);
        Vector3 exosuitQuickSlotsPos = new Vector3(0, -700, 700);
        Vector3 exosuitBarsPanelPos = new Vector3(-700, -200, 600);
        Vector3 originalCompassPos;
        Vector3 originalQuickSlotsPos;
        Vector3 originalBarsPanelPos;

        void Awake()
        {
            //create a new worldspace canvas for vehicles
            vehicleCanvas = new GameObject("VRVehicleCanvas");
            DontDestroyOnLoad(vehicleCanvas);
            Canvas canvas = vehicleCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "HUD";
            vehicleCanvas.AddComponent<CanvasGroup>();
            canvasScaler = vehicleCanvas.AddComponent<uGUI_CanvasScaler>();
            //the canvasScaler moves elements in front of the UI camera based on the mode.
            //In inversed mode it moves the elements so that they look like they are attached to the canvasScaler anchor from the main camera's perspective.
            canvasScaler.vrMode = uGUI_CanvasScaler.Mode.Inversed;
            //TODO:Not that important but figure out how to get raycasts working on the new canvas so drag and drop will work on the quickslots while PDA is open in the vehicle
            //vehicleCanvas.AddComponent<uGUI_GraphicRaycaster>();
            vehicleCanvas.layer = LayerMask.NameToLayer("UI");
            vehicleCanvas.transform.localScale = Vector3.one * 0.001f;
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
                if (player.inSeamoth || player.inExosuit)
                {
                    if(!vehicleCanvas.activeInHierarchy)
                    {
                        //save the original HUD element positions before moving them
                        originalCompassPos = compass.localPosition;
                        originalQuickSlotsPos = quickSlots.localPosition;
                        originalBarsPanelPos = barsPanel.localPosition;
                        //move the elements
                        compass.SetParent(vehicleCanvas.transform, false);
                        quickSlots.SetParent(vehicleCanvas.transform, false);
                        barsPanel.SetParent(vehicleCanvas.transform, false);
                        //set custom element positions based on vehicle
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
                        //TODO:Make the anchor be the vehicle instead so the hud will not move with the head if I get the upper body ik working while piloting
                        canvasScaler.SetAnchor(SNCameraRoot.main.mainCam.transform.parent);
                        vehicleCanvas.SetActive(true);
                    }
                    //using vehicleCanvas.transform.up to compensate for the rotation done by the canvas scaler in inversed mode.
                    if (player.inSeamoth)
                        seamothHUD.rotation = Quaternion.LookRotation(seamothHUD.position,vehicleCanvas.transform.up);
                    else
                        exosuitHUD.rotation = Quaternion.LookRotation(exosuitHUD.position, vehicleCanvas.transform.up);
                    quickSlots.rotation = Quaternion.LookRotation(quickSlots.position, vehicleCanvas.transform.up);
                    compass.rotation = Quaternion.LookRotation(compass.position, vehicleCanvas.transform.up);
                    barsPanel.rotation = Quaternion.LookRotation(barsPanel.position, vehicleCanvas.transform.up);
                }
                else if(vehicleCanvas.activeInHierarchy)
                {
                    //if not in seamoth or exosuit but vehicleCanvas is active then move elements back to the normal HUD and disable vehicleCanvas
                    compass.SetParent(HUDContent, false);
                    compass.localPosition = originalCompassPos;
                    quickSlots.SetParent(HUDContent, false);
                    quickSlots.localPosition = originalQuickSlotsPos;
                    barsPanel.SetParent(HUDContent, false);
                    barsPanel.localPosition = originalBarsPanelPos;
                    vehicleCanvas.SetActive(false);
                    //reset the rotation to look at the UI camera at (0,0,0);
                    quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
                    compass.rotation = Quaternion.LookRotation(compass.position);
                    barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);
                }
            }
        }
    }
}
