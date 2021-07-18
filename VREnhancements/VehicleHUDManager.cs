using UnityEngine;

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
        //float canvasDistance = 1.15f;
        /*Vector3 seamothHUDPos = new Vector3(250,-100,0);
        Vector3 seamothCompassPos = new Vector3(0, 450, 0);
        Vector3 seamothQuickSlotsPos = new Vector3(0, -250, 50);
        Vector3 seamothBarsPanelPos = new Vector3(-250, -60, 0);
        Vector3 exosuitHUDPos = new Vector3(450, 230, 0);
        Vector3 exosuitCompassPos = new Vector3(0, 500, 0);
        Vector3 exosuitQuickSlotsPos = new Vector3(0, -600, 0);
        Vector3 exosuitBarsPanelPos = new Vector3(-450, 230, 0);*/
        Vector3 seamothHUDPos = new Vector3(300,-150, 800);
        Vector3 seamothCompassPos = new Vector3(0, 350, 650);
        Vector3 seamothQuickSlotsPos = new Vector3(0, -275, 800);
        Vector3 seamothBarsPanelPos = new Vector3(0, 0, 0);
        Vector3 exosuitHUDPos = new Vector3(0, 0, 0);
        Vector3 exosuitCompassPos = new Vector3(0, 0, 0);
        Vector3 exosuitQuickSlotsPos = new Vector3(0, 0, 0);
        Vector3 exosuitBarsPanelPos = new Vector3(0, 0, 0);
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
            canvasScaler.vrMode = uGUI_CanvasScaler.Mode.Inversed;//the canvasScaler moves elements in front of the UI camera based on the mode. Inversed moves the elements so that they look like they are attached to the set canvasScaler anchor.
            //TODO:Not that important but figure out how to get raycasts working on the new canvas so drag and drop will work on the quickslots while PDA is open in the vehicle
            //vehicleCanvas.AddComponent<uGUI_GraphicRaycaster>();
            vehicleCanvas.layer = LayerMask.NameToLayer("UI");
            vehicleCanvas.transform.localScale = Vector3.one * 0.0015f;//set scale to the original ScreenCanvas scale
            //vehicleCanvas.transform.position = new Vector3(0,0, canvasDistance);
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
                        canvasScaler.SetAnchor(SNCameraRoot.main.mainCam.transform.parent);//TODO:Make the anchor be the vehicle instead if I get the upper body ik working while piloting
                        vehicleCanvas.SetActive(true);
                    }
                    if (player.inSeamoth)
                        seamothHUD.rotation = Quaternion.LookRotation(seamothHUD.position);
                    else
                        exosuitHUD.rotation = Quaternion.LookRotation(exosuitHUD.position);
                    quickSlots.rotation = Quaternion.LookRotation(quickSlots.position);
                    compass.rotation = Quaternion.LookRotation(compass.position);
                    barsPanel.rotation = Quaternion.LookRotation(barsPanel.position);
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
