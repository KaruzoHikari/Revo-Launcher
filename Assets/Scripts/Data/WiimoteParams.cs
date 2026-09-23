using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiimoteParams
{
    private Wiimote wiimote;
    private int index;
    public RectTransform cursor;
    private Vector2 lastScreenPosition = new Vector2();

    private bool isLeftClicking;
    private bool isRightClicking;
    private bool isPlusClicking;
    private bool isMinusClicking;
    private bool isHomeClicking;
    private bool isNunchuckCalibrated;
    private Vector2 defaultNunchuckValues = new Vector2(128, 128);
    private bool isInfraredCalibrated;
    private int defaultInfraredIndex = 1;
    private bool canRumble = true;

    public static WiimoteParams Create(Wiimote wiimote, int index)
    {
        WiimoteParams wiimoteParams = new WiimoteParams();
        wiimoteParams.wiimote = wiimote;
        wiimoteParams.index = index;

        if (wiimoteParams.IsMain())
        {
            wiimoteParams.cursor = InputController._instance.mainCursor;
        }
        else
        {
            GameObject cursorGameObject = GameObject.Instantiate(InputController._instance.cursorPrefab, InputController._instance.cursorHolder);
            TextMeshProUGUI text = cursorGameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            
            Color textureColor = InputController.GetCursorColor(index);
            Color.RGBToHSV(textureColor, out float h, out float s, out float v);
            Color indexColor = Color.HSVToRGB(h, 1, v);
            
            text.text = (index + 1).ToString();
            text.color = indexColor;
            cursorGameObject.transform.Find("Color").GetComponent<Image>().color = textureColor;
            wiimoteParams.cursor = cursorGameObject.GetComponent<RectTransform>();
            wiimoteParams.cursor.SetAsFirstSibling();
            wiimoteParams.cursor.position = new Vector3(0,0,0);
        }

        wiimoteParams.SetIndexLeds();
        wiimoteParams.Rumble(WiimoteController._instance.connectRumbleTime);
        return wiimoteParams;
    }

    public void Rumble(float time)
    {
        if (canRumble)
        {
            WiimoteController._instance.StartCoroutine(_Rumble(time));
        }
    }

    public bool IsMain()
    {
        return index == 0;
    }

    private IEnumerator _Rumble(float time)
    {
        canRumble = false;
        wiimote.RumbleOn = true; // Enabled Rumble
        wiimote.SendStatusInfoRequest(); // Requests Status Report, encodes Rumble into input report
        yield return new WaitForSeconds(time);
        wiimote.RumbleOn = false; // Disabled Rumble
        wiimote.SendStatusInfoRequest(); // Requests Status Report, encodes Rumble into input report
        yield return new WaitForSeconds(WiimoteController._instance.rumbleDelay);
        canRumble = true;
    }

    private void CalibrateNunchuck()
    {
        if (!isNunchuckCalibrated && wiimote.current_ext == ExtensionController.NUNCHUCK)
        {
            NunchuckData data = wiimote.Nunchuck;
            defaultNunchuckValues = new Vector2(data.stick[0], data.stick[1]);
            if (defaultNunchuckValues.x != 0 && defaultNunchuckValues.y != 0)
            {
                isNunchuckCalibrated = true;
            }
            Debug.Log($"Calibrating! {defaultNunchuckValues.x} || {defaultNunchuckValues.y}");
        }
    }

    private void CalibrateInfrared()
    {
        if (!isInfraredCalibrated)
        {
            float[,] ir = wiimote.Ir.GetProbableSensorBarIR();
            Vector2 vector0 = new Vector2(ir[0, 0], ir[0, 1]);
            Vector2 vector1 = new Vector2(ir[1, 0], ir[1, 1]);
            if (!vector0.Equals(vector1))
            {
                isInfraredCalibrated = true;
                defaultInfraredIndex = vector1.x > vector0.x ? 1 : 0;
            }
        }
    }

    public void UpdatePointer()
    {
        if(wiimote.current_ext == ExtensionController.NUNCHUCK) {
            // A) Nunchunk to move the cursor
            CalibrateNunchuck();
            MoveThroughNunchuck();
            isInfraredCalibrated = false;
        }
        else
        {
            isNunchuckCalibrated = false;
            // B) Pointer to the screen
            float[] pointer = wiimote.Ir.GetPointingPosition();
            if (pointer[0] >= 0 || pointer[1] >= 0)
            {
                CalibrateInfrared();
                MoveThroughIR();
            }
            else
            {
                // C) Horizontal Wiimote (2 = A, 1 = B)
                MoveThroughButtons();
                isInfraredCalibrated = false;
            }
        }
    }

    private void MovePointer(Vector2 newScreenPosition)
    {
        if (IsMain())
        {
            MouseOperations.SetCursorPosition((int)newScreenPosition.x,(int)newScreenPosition.y);
        }
        else
        {
            cursor.position = newScreenPosition;
        }
    }

    private void StraightenCursor()
    {
        if (cursor.eulerAngles != Vector3.zero)
        {
            cursor.eulerAngles = Vector3.zero;
        }
    }

    private void MoveThroughNunchuck()
    {
        if (!isNunchuckCalibrated)
        {
            return;
        }

        StraightenCursor();
        NunchuckData data = wiimote.Nunchuck;
        float[] delta = {data.stick[0] - defaultNunchuckValues.x, data.stick[1] - defaultNunchuckValues.y};
        float factor = IsMain()
            ? WiimoteController._instance.nunchuckFactorMain
            : WiimoteController._instance.nunchuckFactorExtra;
        if (delta[0] != 0 || delta[1] != 0)
        {
            Vector2 cursorPos;
            if (IsMain())
            {
                MouseOperations.MousePoint mousePoint = MouseOperations.GetCursorPosition();
                cursorPos = new Vector2(mousePoint.X, mousePoint.Y);
                delta[1] = -delta[1];
            }
            else
            {
                cursorPos = cursor.position;
            }
            Vector2 newPosition = new Vector2(cursorPos.x + delta[0] * factor, cursorPos.y + delta[1] * factor);
            MovePointer(newPosition);
        }
    }

    private void MoveThroughButtons()
    {
        StraightenCursor();
        float factor = IsMain()
            ? WiimoteController._instance.arrowsFactorMain
            : WiimoteController._instance.arrowsFactorExtra;
        float up = wiimote.Button.d_right ? factor : 0;
        float down = wiimote.Button.d_left ? -factor : 0;
        float left = wiimote.Button.d_up ? -factor : 0;
        float right = wiimote.Button.d_down ? factor : 0;
        if (up != 0 || down != 0 || left != 0 || right != 0)
        {
            Vector2 cursorPos;
            if (IsMain())
            {
                MouseOperations.MousePoint mousePoint = MouseOperations.GetCursorPosition();
                cursorPos = new Vector2(mousePoint.X, mousePoint.Y);
                up = -up;
                down = -down;
            }
            else
            {
                cursorPos = cursor.position;
            }
            Vector2 newPosition = new Vector2(cursorPos.x + left + right, cursorPos.y + up + down);
            MovePointer(newPosition);
        }
    }

    private void MoveThroughIR()
    {
        // First we retrieve the raw position that the Wiimote is getting
        Vector2 screenVector = GetIRScreenPosition();
            
        // Now we invert its Y
        // (In Windows, the cursor is set from the top left, instead of the bottom left)
        screenVector.y = Screen.currentResolution.height - screenVector.y;

        // We start building the final vector
        Vector2 finalVector;
            
        // We obtain the current screen space pos of the mouse
        if (!IsMain())
        {
            finalVector = lastScreenPosition;
        }
        else
        {
            MouseOperations.MousePoint mousePoint = MouseOperations.GetCursorPosition();
            finalVector = new Vector2(mousePoint.X,mousePoint.Y);
        }
        
        // Now we ease the movement (cause it's unstable as fuck)
        finalVector += new Vector2(
            (screenVector.x - finalVector.x) / WiimoteController._instance.stabilizationFactor,
            (screenVector.y - finalVector.y) / WiimoteController._instance.stabilizationFactor);
        
        // We set the position
        // And then we move the cursor texture
        if (IsMain())
        {
            MouseOperations.SetCursorPosition((int)finalVector.x,(int)finalVector.y);
        }
        else
        {
            lastScreenPosition = finalVector;
            cursor.position = CameraController._instanceOverlay.GetWorldPosition(new Vector2(
                lastScreenPosition.x * WiimoteController._instance.extraControllerMultiplier
                - Screen.currentResolution.width/2f,
                (Screen.currentResolution.height - lastScreenPosition.y) * WiimoteController._instance.extraControllerMultiplier
                - Screen.currentResolution.height/2f));
        }
        
        // And finally we rotate the cursor
        float[,] ir = wiimote.Ir.GetProbableSensorBarIR();
        int initialIndex = defaultInfraredIndex;
        int finalIndex = initialIndex == 1 ? 0 : 1;
        Vector2 initialPos = new Vector2(ir[initialIndex, 0], ir[initialIndex, 1]);
        Vector2 finalPos = new Vector2(ir[finalIndex, 0], ir[finalIndex, 1]);
        Vector2 vector = finalPos - initialPos;
        vector.Normalize();
        vector = Quaternion.Euler(0, 0, -90) * vector;
        float angle = Vector2.SignedAngle(Vector2.up, vector);
        cursor.eulerAngles = new Vector3(0, 0, -angle);
    }

    /*public Vector2 GetScreenPosition()
    {
        return CameraController.GetScreenPosition(cursor.position);
    }
    
    public Vector2 GetWindowsScreenPosition()
    {
        Vector2 screenPos = CameraController.GetScreenPosition(cursor.position);
        screenPos.y = Screen.currentResolution.height - screenPos.y;
        return screenPos;
    }*/
    
    public void CleanUp()
    {
        WiimoteManager.Cleanup(wiimote);
        if (!IsMain())
        {
            GameObject.Destroy(cursor.gameObject);
        }
    }

    public void Refresh()
    {
        RetrieveWiimoteData();
        UpdatePointer();
        CheckClicks();
        //float[] acel = wiimote.Accel.GetCalibratedAccelData();
        //Debug.Log($"Accel: {acel[0]} - {acel[1]} - {acel[2]}");
    }

    private void RetrieveWiimoteData()
    {
        // ReadWiimoteData() returns 0 when nothing is left to read, so we make sure it's up to date
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);
    }

    public Vector2 GetIRScreenPosition()
    {
        // We read the IR data
        float[] pointer = wiimote.Ir.GetPointingPosition();
        
        // We retrieve the canvas size
        var sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        
        // We calculate the height we're pointing to, and the width (since the pointer returns a [0,1] array)
        float width = (sizeDelta.x * pointer[0]);
        float height = (sizeDelta.y * pointer[1]);

        return new Vector2(width, height);
    }

    private void CheckClicks()
    {
        bool left = wiimote.Button.a || wiimote.Button.two;
        bool right = wiimote.Button.b || wiimote.Button.one;
        bool plus = wiimote.Button.plus;
        bool minus = wiimote.Button.minus;
        bool home = wiimote.Button.home;

        if (IsMain())
        {
            if (!isLeftClicking && left)
            {
                SimulateLeftClick();
            }
            else if (isLeftClicking && !left)
            {
                ReleaseLeftClick();
            }

            if (!isRightClicking && right)
            {
                SimulateRightClick();
            }
            else if (isRightClicking && !right)
            {
                ReleaseRightClick();
            }
        }

        if (!isPlusClicking && plus)
        {
            SimulatePlusClick();
        }
        else if (isPlusClicking && !plus)
        {
            ReleasePlusClick();
        }

        if (!isMinusClicking && minus)
        {
            SimulateMinusClick();
        }
        else if (isMinusClicking && !minus)
        {
            ReleaseMinusClick();
        }
        
        if (!isHomeClicking && home)
        {
            SimulateHomeClick();
        }
        else if (isHomeClicking && !home)
        {
            ReleaseHomeClick();
        }
    }

    private void SimulateHomeClick()
    {
        isHomeClicking = true;
        EditorController._instance.ClickedPause();
    }
    
    private void ReleaseHomeClick()
    {
        isHomeClicking = false;
    }
    
    private void SimulatePlusClick()
    {
        isPlusClicking = true;
        SimulateArrowClick(true);
    }

    private void SimulateMinusClick()
    {
        isMinusClicking = true;
        SimulateArrowClick(false);
    }

    private void SimulateArrowClick(bool higher)
    {
        if (AppController._instance.IsInMainMenu())
        {
            ArrowController arrow;
            if (ChannelController._instance.currentChannel != null)
            {
                arrow = higher
                    ? ChannelController._instance.channelRightArrow
                    : ChannelController._instance.channelLeftArrow;
            }
            else
            {
                arrow = higher
                    ? GridController._instance.rightArrow
                    : GridController._instance.leftArrow;
            }

            arrow.ClickedArrow();
        }
    }

    private void ReleasePlusClick()
    {
        isPlusClicking = false;
    }

    private void ReleaseMinusClick()
    {
        isMinusClicking = false;
    }

    private void SimulateLeftClick()
    {
        isLeftClicking = true;
        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
    }

    private void ReleaseLeftClick()
    {
        isLeftClicking = false;
        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
    }

    private void SimulateRightClick()
    {
        isRightClicking = true;
        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown);
    }
    
    private void ReleaseRightClick()
    {
        isRightClicking = false;
        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp);
    }
    
    private void SetIndexLeds()
    {
        int playerPos = index + 1;
        switch (playerPos)
        {
            case 1: { wiimote.SendPlayerLED(true, false, false, false); break; }
            case 2: { wiimote.SendPlayerLED(false, true, false, false); break; }
            case 3: { wiimote.SendPlayerLED(false, false, true, false); break; }
            case 4: { wiimote.SendPlayerLED(false, false, false, true); break; }
            case 5: { wiimote.SendPlayerLED(true, true, false, false); break; }
            case 6: { wiimote.SendPlayerLED(true, false, true, false); break; }
            case 7: { wiimote.SendPlayerLED(true, false, false, true); break; }
            case 8: { wiimote.SendPlayerLED(false, true, true, false); break; }
            case 9: { wiimote.SendPlayerLED(false, true, false, true); break; }
            case 10: { wiimote.SendPlayerLED(false, false, true, true); break; }
            case 11: { wiimote.SendPlayerLED(true, true, true, false); break; }
            case 12: { wiimote.SendPlayerLED(true, true, false, true); break; }
            case 13: { wiimote.SendPlayerLED(false, true, true, true); break; }
            default: { wiimote.SendPlayerLED(true, true, true, true); break; }
        }
    }
}