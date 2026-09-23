using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    private Vector3 defaultPosition = new Vector3(0, 0, -1000);
    public float regularSize = 960f;
    public float regularSizeHorizontal = 540f;
    public float zoomedSize = 172f; // this used to say 60f?
    public float zoomedSizeHorizontal = 60f;
    [HideInInspector] public float regularOrtoSize;
    [HideInInspector] public float zoomedOrtoSize;
    public RectTransform cameraArea;
    public RectTransform menusArea;
    private Vector2 canvasCenter;
    public float finalSize;

    private float resolutionChangeTimer = 1f;
    private int previousHeight;
    private int previousWidth;
    public float debugWidth = -1f;
    public float debugHeight = -1f;

    public bool shouldDebugOrientation;
    public ScreenOrientation debugOrientation;
    private ScreenOrientation currentOrientation;

    public static CameraController _instance;
    public static CameraController _instanceOverlay;
    public bool isOverlay;
    public bool isHorizontalAllowed;
    private Camera _camera;

    private Tween fovTween;
    
    void Awake()
    {
        if (isOverlay)
        {
            _instanceOverlay = this;
        }
        else
        {
            _instance = this;
            AssignFirstOrientation();
            UnlockHorizontalMode();
        }
        _camera = GetComponent<Camera>();
        
        FixCameraFOV(true);
    }

    private void AssignFirstOrientation()
    {
        bool allow = PREFS.AllowHorizontal.GetBool();
        if (!allow)
        {
            currentOrientation = ScreenOrientation.Portrait;
        }
        else
        {
            if (Application.isMobilePlatform)
            {
                // in Android we let them rotate the screen
                currentOrientation = Screen.orientation;
                Debug.Log($"Setting current orientation to {currentOrientation}");
            }
            else
            {
                // in PC it's set by the horizontal mode
                currentOrientation = allow ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
            }
        }
    }

    private void Update()
    {
        if (!isOverlay && Application.isMobilePlatform && CanRotateScreen())
        {
            if (Screen.orientation != currentOrientation)
            {
                AssignFirstOrientation();
                StartCoroutine(_RefreshCanvas());
            }
            else if (resolutionChangeTimer <= 0)
            {
                resolutionChangeTimer = 1f;
                if (Screen.height != previousHeight || Screen.width != previousWidth)
                {
                    // used to reset grid in foldables when their screen size changes
                    Debug.Log("Refreshing canvas because of resolution changes!");
                    StartCoroutine(_RefreshCanvas());
                }
            }

            resolutionChangeTimer -= Time.deltaTime;
            previousHeight = Screen.height;
            previousWidth = Screen.width;
        }
    }

    public static void UnlockHorizontalMode()
    {
        bool allowed = PREFS.AllowHorizontal.GetBool();
        _instance.AllowHorizontalMode(allowed);

        // we rotate the screen if we can
        _instance.AssignFirstOrientation();
        _instance.StartCoroutine(_instance._RefreshCanvas());
    }
    
    public static void LockHorizontalMode()
    {
        _instance.AllowHorizontalMode(false);
    }

    public static void SetAutorotationStatus(bool allowed)
    {
        Screen.autorotateToPortrait = allowed;
        Screen.autorotateToLandscapeLeft = allowed;
        Screen.autorotateToPortraitUpsideDown = allowed;
        Screen.autorotateToLandscapeRight = allowed;
    }

    private void AllowHorizontalMode(bool allowed)
    {
        Debug.Log($"Setting horizontal allowed state: {allowed}");
        SetAutorotationStatus(allowed);
        this.isHorizontalAllowed = allowed;

        if (!allowed)
        {
            _instance.ForcePortrait();
        }
        else
        {
            if (Application.isMobilePlatform)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            else
            {
                // PCs can't rotate so we just enable by default
                _instance.currentOrientation = ScreenOrientation.LandscapeLeft;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
        }
    }

    public void ForcePortrait()
    {
        bool refreshCanvas = GetOrientation() != ScreenOrientation.Portrait;
        Debug.Log($"Should refresh? {refreshCanvas} - {_instance.currentOrientation}");
        _instance.currentOrientation = ScreenOrientation.Portrait;
        Screen.orientation = ScreenOrientation.Portrait;
        if (refreshCanvas)
        {
            StartCoroutine(_RefreshCanvas());
        }
    }

    private IEnumerator _RefreshCanvas()
    {
        yield return new WaitForSeconds(0.1f);
        _instance.RefreshCanvas();
    }

    private bool CanRotateScreen()
    {
        return Math.Abs(_camera.orthographicSize - regularOrtoSize) < 0.1f;
    }

    public float GetOrtoSize()
    {
        return _camera.orthographicSize;
    }

    private float GetRegularSize()
    {
        return IsPortrait() ? regularSize : regularSizeHorizontal;
    }

    private float GetZoomedSize()
    {
        return IsPortrait() ? zoomedSize : zoomedSizeHorizontal;
    }

    private void CalculateOrtoSizes()
    {
        regularOrtoSize = GetOrtoSize(GetRegularSize());
        zoomedOrtoSize = GetOrtoSize(GetZoomedSize());
    }

    private float GetOrtoSize(float regularSizeIn1920)
    {
        if (!IsPortrait())
        {
            // in landscape it's always the same!
            return regularSizeIn1920;
        }
        
        // we fit the app in the screen dimensions
        float height = debugHeight < 0 ? GetScreenHeight() : debugHeight;
        float width = debugWidth < 0 ? GetScreenWidth() : debugWidth;
        
        // Debug.Log("Screen height (regular) is: " + height);
        // Debug.Log("Screen height (display) is: " + Display.main.renderingHeight);
        // Debug.Log($"Fixed size (from {regularSizeIn1920} is: " + (regularSizeIn1920 * (height / 1920f)) + ")");
        
        return IsPortrait() 
            ? regularSizeIn1920 * (height / 1920f) * (1080f / width)
            : regularSizeIn1920 * (width / 1920f) * (1080f / height);
    }

    [Button("Fix Camera's FOV")]
    private void FixCameraFOV(bool forceToRegular = false)
    {
        CalculateOrtoSizes();
        float currentSize = forceToRegular || ChannelController._instance.currentChannel == null
            ? GetRegularSize()
            : GetZoomedSize();
        _camera.orthographicSize = GetOrtoSize(currentSize);
    }

    [Button("Zoom Out")]
    public void ZoomOut(/*AppHandler app, */float duration)
    {
        /*Vector2 currentPivot = menusArea.pivot;
        Vector2 currentPos = menusArea.position;
        menusArea.pivot = new Vector2(0.5f, 0.5f);
        Vector2 delta = -menusArea.position;
        menusArea.pivot = currentPivot;
        
            
        menusArea.DOMove(currentPos + delta, duration);
        menusArea.DOScale(new Vector3(1f, 1f, 1f), duration);*/
        
        
        transform.DOMove(defaultPosition, duration);
        fovTween?.Complete();
        regularOrtoSize = GetOrtoSize(GetRegularSize());
        fovTween = _camera.DOOrthoSize(regularOrtoSize, duration);
        SetCursorUpdateTween();
        
    }

    public void ZoomIn(AppHandler app, float duration)
    {
        /*menusArea.pivot = new Vector2(0.5f, 0.5f);
        Vector2 center = app.GetCenter();
        Rect rect = menusArea.rect;
        menusArea.pivot = new Vector2((center.x/rect.width) + 0.5f, (center.y/rect.height) + 0.5f);
        canvasCenter = menusArea.position;
        menusArea.DOMove(canvasCenter-center, duration).SetEase(Ease.InQuad);
        menusArea.DOScale(new Vector3(finalSize, finalSize, 1f), duration).SetEase(Ease.InQuad).OnUpdate(() =>
        {
            return;
            menusArea.pivot = new Vector2(0.5f, 0.5f);
            Vector2 newCenter = app.GetCenter();
            menusArea.pivot = new Vector2((newCenter.x/rect.width) + 0.5f, (newCenter.y/rect.height) + 0.5f);
            Debug.Log($"New center! {newCenter}\nNew pivot! {menusArea.pivot}");
        });*/


        Vector2 center = app.GetCenter();
        transform.DOMove(new Vector3(center.x, center.y, _camera.transform.position.z), duration).SetEase(Ease.InQuad);
        
        fovTween?.Complete();
        zoomedOrtoSize = GetOrtoSize(GetZoomedSize()) * GridController._instance.GetChannelSizeMultiplier();
        fovTween = _camera.DOOrthoSize(zoomedOrtoSize, duration).SetEase(Ease.InQuad);
        SetCursorUpdateTween();
    }

    private void SetCursorUpdateTween()
    {
        if (false && fovTween != null)
        {
            fovTween.OnUpdate(UpdateCameraElements);
            fovTween.OnComplete(UpdateCameraElements);
        }
    }

    private void UpdateCameraElements()
    {
        InputController.RefreshCursorSize();
        AdjustCameraArea();
    }

    private void AdjustCameraArea()
    {
        Vector3 position = _camera.transform.position;
        cameraArea.position = new Vector3(position.x, position.y, cameraArea.position.z);
        float delta = _camera.orthographicSize / regularOrtoSize;
        cameraArea.sizeDelta = new Vector2(delta, delta);
    }

    public Vector2 GetWorldPosition(Vector2 pos)
    {
        Vector3 screenPosition = new Vector3(pos.x, pos.y, -_camera.transform.position.z);
        return _camera.ScreenToWorldPoint(screenPosition);
    }
    
    public Vector2 GetScreenPosition(Vector2 pos)
    {
        Vector3 screenPosition = new Vector3(pos.x, pos.y, -_camera.transform.position.z);
        return _camera.WorldToScreenPoint(screenPosition);
    }

    public Vector2 GetWorldMousePosition()
    {
        return GetWorldPosition(Input.mousePosition);
    }

    public Vector2 GetMiddleCursor()
    {
        if (Application.isMobilePlatform)
        {
            Vector2 positionVector = new Vector2();
            int touchCount = Input.touchCount;
            if (touchCount <= 0)
            {
                return positionVector;
            }

            for (int i = 0; i < touchCount; i++)
            {
                positionVector += GetWorldPosition(Input.GetTouch(i).position);
            }
            return new Vector2(positionVector.x / touchCount, positionVector.y / touchCount);
        }
        else
        {
            return GetWorldMousePosition();
        }
    }

    public static ScreenOrientation GetOrientation()
    {
        return _instance.shouldDebugOrientation
            ? _instance.debugOrientation
            : _instance.currentOrientation;
    }

    public static bool IsPortrait()
    {
        bool portrait = GetOrientation() == ScreenOrientation.Portrait || GetOrientation() == ScreenOrientation.PortraitUpsideDown;
        return portrait;
    }

    public static int GetMonitorWidth()
    {
        // we need to fix them because of fullscreen shenanigans
        return PREFS.FullScreen.GetBool() && !Application.isMobilePlatform ? Display.main.systemWidth : Display.main.renderingWidth;
    }

    public static int GetMonitorHeight()
    {
        return PREFS.FullScreen.GetBool() && !Application.isMobilePlatform ? Display.main.systemHeight : Display.main.renderingHeight;
    }

    public static int GetScreenWidth()
    {
        return IsPortrait()
            ? Math.Min(GetMonitorWidth(), GetMonitorHeight())
            : Math.Max(GetMonitorWidth(), GetMonitorHeight());
    }

    public static int GetScreenHeight()
    {
        return IsPortrait()
            ? Math.Max(GetMonitorWidth(), GetMonitorHeight())
            : Math.Min(GetMonitorWidth(), GetMonitorHeight());
    }

    public static float GetFixedWidth()
    {
        // Debug.Log($"Rendering width is {GetScreenWidth()}");
        int width = (int) (IsPortrait() ? 1080 : GetFixedHeight() * GetScreenWidth() / (float) GetScreenHeight());
        return width;
    }

    public static float GetFixedHeight()
    {
        // Debug.Log($"Rendering height is {GetScreenHeight()}");
        int height = (int) (IsPortrait() ? GetFixedWidth() * GetScreenHeight() / (float) GetScreenWidth() : 1080);
        return height;
    }

    public static float GetChannelSize(float originalSizeIn1080)
    {
        // aspect ratio is just an illusion in horizontal. lol.
        if (!IsPortrait())
        {
            return originalSizeIn1080;
        }
        
        // we need to make sure the channels keep their aspect ratio, regardless of the screen size and dimensions
        // so we compare in order to know whether it should be limited on the width or on the height
        float width = GetFixedWidth();
        float height = GetFixedHeight();
        
        // now we check whether we limit by height or width
        // if we limit by width (ratio > reference), we just return the original size
        // if we limit by height (ratio < reference), we need to reduce the size even more
        double ratio = height / width;
        double reference = 16f / 9f;

        float size = ratio >= reference
            ? originalSizeIn1080
            : originalSizeIn1080 * (height / 1920f);
        return size;
    }

    private void RefreshCanvas()
    {
        Debug.Log("Refreshing all canvas sizes!");
        _instance.FixCameraFOV();
        _instanceOverlay.FixCameraFOV();
        AppController._instance.RefreshCanvasSize();
        GridController._instance.RefreshAppNumbers(); // for foldables, perhaps now it doesn't fit as many channels
        GridController._instance.RefreshGrids(false);
        ThemeController._instance.UpdateAllTransforms();
        AppController._instance.gameCanvas.ForceUpdateRectTransforms();
    }

    [Button("Refresh canvas")]
    public void BUTTON_Canvas()
    {
        RefreshCanvas();
    }

    [Button("Change orientation to PORTRAIT")]
    public void BUTTON_Portrait()
    {
        debugOrientation = ScreenOrientation.Portrait;
        Screen.orientation = ScreenOrientation.Portrait;
        RefreshCanvas();
    }

    [Button("Change orientation to LANDSCAPE")]
    public void BUTTON_Landscape()
    {
        debugOrientation = ScreenOrientation.LandscapeLeft;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        RefreshCanvas();
    }
}
