using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class InputController : MonoBehaviour
{
    public static InputController _instance;
    public RectTransform mainCursor;
    public Transform cursorHolder;
    public GameObject cursorPrefab;
    public Color[] indexColors;
    public float regularCursorScale = 1f;
    public float zoomedCursorScale = 0.1f;
    public float androidCursorScale = 1.125f;

    private void Awake()
    {
        _instance = this;
        Cursor.visible = false;
        RefreshPointerVisibility();
        if (Application.isMobilePlatform)
        {
            regularCursorScale *= androidCursorScale;
            zoomedCursorScale *= androidCursorScale;
            mainCursor.localScale = new Vector2(regularCursorScale, regularCursorScale);
        }
    }

    public void RefreshPointerVisibility()
    {
        mainCursor.gameObject.SetActive(!Application.isMobilePlatform || PREFS.ShowCursor.GetBool());
    }

    private void Update()
    {
        mainCursor.position = CameraController._instanceOverlay.GetWorldPosition(Input.mousePosition);
    }

    public static Color GetCursorColor(int index)
    {
        if (index < _instance.indexColors.Length)
        {
            return _instance.indexColors[index];
        }

        return Random.ColorHSV(0f,1f,0.23f,0.23f, 1f, 1f);
    }

    public static void RefreshCursorSize()
    {
        float size = CameraController._instanceOverlay.GetOrtoSize();
        float min = Math.Min(CameraController._instanceOverlay.zoomedOrtoSize, CameraController._instanceOverlay.regularOrtoSize);
        float range = Math.Abs(CameraController._instanceOverlay.zoomedOrtoSize - CameraController._instanceOverlay.regularOrtoSize);

        size -= min;
        float percentage = (size / range);
        float value = Mathf.Lerp(_instance.zoomedCursorScale, _instance.regularCursorScale, percentage);

        foreach (WiimoteParams wiimoteParams in WiimoteController._instance.wiimoteData)
        {
            wiimoteParams.cursor.localScale = new Vector2(value, value);
        }
        _instance.mainCursor.localScale = new Vector2(value, value);
    }
}
