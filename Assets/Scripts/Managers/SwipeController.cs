using System;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    // more like the GesturesController at this point
    
    public static SwipeController _instance;
    
    public bool shouldCheckSwipe = true;
    public bool shouldCheckHoldChannel = true;
    public bool shouldCheckSwipeDown = true;
    public bool shouldCheckDoubleClick = true;
    public bool shouldCheckDrag = true;
    public bool shouldCheckHoldEmptyChannel = true;
    public bool shouldCheckHoldSdCard = true;
    
    public float swipeThreshold = 50f;
    public float verticalSwipeThreshold = 250f;
    public float timeThreshold = 0.3f;

    private Vector2 fingerDown;
    private DateTime fingerDownTime;
    private Vector2 fingerUp;
    private DateTime fingerUpTime;

    private bool cancelMultipleTouches;

    private List<KeyCode> konami = new List<KeyCode>() { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };
    private int currentCode = 0;

    private void Awake()
    {
        _instance = this;
        RefreshSetting();
    }

    private void Update()
    {
        // we check the konami code
        if (Input.GetKeyDown(konami[currentCode]))
        {
            currentCode++;
            if (currentCode >= konami.Count)
            {
                // executed Konami code!
                currentCode = 0;
                SettingsController._instance.ExecutedKonami();
            }
        }
        
        if (!shouldCheckSwipe && !shouldCheckSwipeDown)
        {
            return;
        }

        int touchCount = Input.touches.Length;
        if (cancelMultipleTouches && touchCount < 1)
        {
            cancelMultipleTouches = false;
        } else if (!cancelMultipleTouches && touchCount > 1)
        {
            cancelMultipleTouches = true;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            fingerDown = Input.mousePosition;
            fingerUp = Input.mousePosition;
            fingerDownTime = DateTime.Now;
        }

        if (Input.GetMouseButtonUp(0))
        {
            fingerDown = Input.mousePosition;
            fingerUpTime = DateTime.Now;
            CheckSwipe();
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerDown = touch.position;
                fingerUp = touch.position;
                fingerDownTime = DateTime.Now;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                fingerDown = touch.position;
                fingerUpTime = DateTime.Now;
                CheckSwipe();
            }
        }
    }

    private void CheckSwipe()
    {
        if (cancelMultipleTouches)
        {
            return;
        }
        
        float duration = (float) fingerUpTime.Subtract(fingerDownTime).TotalSeconds;
        if (duration > timeThreshold) return;

        if (!GridController._instance.IsLiftingChannel() && AppController._instance.IsInMainMenu() && !ThemeController._instance.IsPanelOpen() && !PopupController.IsBlockerOpen())
        {
            bool doneVertical = false;
            if (shouldCheckSwipeDown)
            {
                doneVertical = CheckSwapVertical();
            }
            
            if (!doneVertical && shouldCheckSwipe)
            {
                CheckSwapHorizontal();
            }

        }

        fingerUp = fingerDown;
    }

    private void CheckSwapHorizontal()
    {
        float deltaX = fingerDown.x - fingerUp.x;
        if (Mathf.Abs(deltaX) > swipeThreshold)
        {
            if (deltaX < 0)
            {
                if (ChannelController._instance.currentChannel != null)
                {
                    ChannelController._instance.channelRightArrow.ClickedArrow();
                }
                else
                {
                    GridController._instance.rightArrow.ClickedArrow();
                }
            }
            else if (deltaX > 0)
            {
                if (ChannelController._instance.currentChannel != null)
                {
                    ChannelController._instance.channelLeftArrow.ClickedArrow();
                }
                else
                {
                    GridController._instance.leftArrow.ClickedArrow();
                }
            }
        }
    }

    private bool CheckSwapVertical()
    {
        float deltaY = fingerDown.y - fingerUp.y;
        if (Mathf.Abs(deltaY) > verticalSwipeThreshold)
        {
            // swipe channel vertically to close it
            if (ChannelController._instance.currentChannel != null)
            {
                ChannelController._instance.CloseChannel();
                return true;
            }
        }

        return false;
    }
    
    public void RefreshSetting()
    {
        shouldCheckSwipe = PREFS.AllowSwipe.GetBool();
        shouldCheckDrag = PREFS.AllowDrag.GetBool();
        shouldCheckDoubleClick = PREFS.AllowDoubleClick.GetBool();
        shouldCheckSwipeDown = PREFS.AllowSwipeDown.GetBool();
        shouldCheckHoldChannel = PREFS.EnableHoldChannel.GetBool();
        shouldCheckHoldEmptyChannel = PREFS.HoldEmptyChannel.GetBool();
        shouldCheckHoldSdCard = PREFS.HoldSdCard.GetBool();
    }
}