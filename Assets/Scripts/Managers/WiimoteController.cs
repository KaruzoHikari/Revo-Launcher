using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using WiimoteApi;

public class WiimoteController : MonoBehaviour
{
    public static WiimoteController _instance;
    public List<WiimoteParams> wiimoteData = new List<WiimoteParams>();
    public RectTransform testCircle;
    [Title("IR Values")]
    public int pixelDragThreshold = 50;
    public float stabilizationFactor = 2f;
    public float extraControllerMultiplier = 2f;
    [Title("Rumble values")]
    public float rumbleDelay = 0.5f;
    public float connectRumbleTime = 0.4f;
    public float buttonRumbleTime = 0.05f;
    [Title("Movement factors")]
    public float nunchuckFactorMain = 0.035f;
    public float nunchuckFactorExtra = 0.05f;
    public float arrowsFactorMain = 3.0f;
    public float arrowsFactorExtra = 4.5f;
    [Title("Debug values")]
    public float debugRumbleTime = 0.01f;
    public Vector2 debugMousePos = new Vector2(500,500);
    [Button]
    public void DebugRumble() { wiimoteData[0].Rumble(debugRumbleTime); }
    [Button]
    public void DebugMouse() { MouseOperations.SetCursorPosition((int)debugMousePos.x, (int)debugMousePos.y); }
    
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        InitWiimotes();
    }

    [Button]
    public void InitWiimotes() {
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        ClearWiimotes();

        try
        {
            WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        }
        catch (Exception e)
        {
            Debug.Log("Failed to fetch Wiimotes! (Is Bluetooth even available in this computer?");
        }
        int index = 0;
        foreach(Wiimote remote in WiimoteManager.Wiimotes)
        {
            Debug.Log($"Adding remote nº{index+1}!");
            //remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8);
            remote.SetupIRCamera(IRDataType.BASIC);
            wiimoteData.Add(WiimoteParams.Create(remote,index));
            index++;
        }

        if (wiimoteData.Count > 0)
        {
            // We reduce the drag threshold because the Wiimotes IR pos aren't stable at all
            EventSystem.current.pixelDragThreshold = this.pixelDragThreshold;
        }
        #endif
    }

    public void RefreshSensitivity()
    {
        nunchuckFactorMain = PREFS.NunchuckSensitivity.GetFloat();
        arrowsFactorMain = PREFS.DPadSensitivity.GetFloat();
    }

    private void Update()
    {
        RefreshWiimoteData();
    }

    private void RefreshWiimoteData()
    {
        foreach (WiimoteParams wiimoteParams in wiimoteData)
        {
            wiimoteParams.Refresh();
        }
    }

    public static void RumbleMain(float time)
    {
        foreach (WiimoteParams wiimoteParams in _instance.wiimoteData)
        {
            if (wiimoteParams.IsMain())
            {
                wiimoteParams.Rumble(time);
                return;
            }
        }
    }

    private void OnApplicationQuit()
    {
        ClearWiimotes();
    }

    private void ClearWiimotes()
    {
        foreach (WiimoteParams parameters in wiimoteData)
        {
            parameters.CleanUp();
        }
        wiimoteData.Clear();
    }
}
