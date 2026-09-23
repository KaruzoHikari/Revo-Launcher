using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    public List<EmptyAppHandler> emptyChannels = new List<EmptyAppHandler>();
    public int number;
    public RectTransform innerGrid;
    public GameObject backMask;
    public RawImage backgroundImage;
    public ThemedTransform clockTransform;
    public Clock clockScript;
    public GridLayoutGroup gridLayout;
    private Vector2 originalCellSize;
    private Coroutine lockedImage;
    private RectTransform rectTransform;
    private bool lockImage;

    public void FindElements()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        innerGrid = transform.Find("Grid").GetComponent<RectTransform>();
        gridLayout = innerGrid.GetComponent<GridLayoutGroup>();
        originalCellSize = gridLayout.cellSize;
        backMask = transform.Find("BackMask").gameObject;
        backgroundImage = backMask.transform.Find("Background").gameObject.GetComponent<RawImage>();
        clockTransform = transform.Find("BottomClip").Find("ClockHolder").GetComponent<ThemedTransform>();
        clockScript = transform.Find("BottomClip").Find("ClockHolder").Find("Clock").GetComponent<Clock>();
        backMask.SetActive(false);
        ResizeGrid();
    }

    private void Start()
    {
        FixPositions(true);
    }

    public void ResizeGrid()
    {
        rectTransform.sizeDelta = new Vector2(CameraController.GetFixedWidth(), CameraController.GetFixedHeight());
        //innerGrid.sizeDelta = new Vector2(CameraController.GetFixedWidth(), CameraController.GetFixedHeight());
        //innerGrid.offsetMax = new Vector2(-290, 0);
        innerGrid.localScale = CameraController.IsPortrait() ? Vector2.one : new Vector2(0.9f, 0.9f);
        gridLayout.constraintCount = GridController._instance.GetAppsPerX();

        // we setup the new app sizes
        float size = GridController._instance.GetChannelSizeMultiplier();
        gridLayout.cellSize = new Vector2(originalCellSize.x * size, originalCellSize.y * size);
        gridLayout.spacing = new Vector2();
        innerGrid.transform.GetComponentsInChildren<AppHandler>().ToList().ForEach(appHandler => appHandler.Resize(size));
        innerGrid.transform.GetComponentsInChildren<EmptyAppHandler>().ToList().ForEach(appHandler => appHandler.Resize(size));
    }

    private void Update()
    {
        if (lockImage)
        {
            FixPositions();
        }
    }

    public void EnableBackground()
    {
        backMask.SetActive(true);
        clockTransform.gameObject.SetActive(true);
        clockScript.shouldDisable = false;
    }

    public void DisableBackground()
    {
        backMask.SetActive(false);
        clockScript.shouldDisable = true;
    }

    public void FixPositions(bool refreshClock = false)
    {
        backgroundImage.rectTransform.position = new Vector3();
        backgroundImage.rectTransform.sizeDelta = AppController._instance.gameCanvas.sizeDelta;
        
        clockTransform.UpdateElement();
        if (refreshClock)
        {
            clockScript.UpdateHour(true);
        }
    }

    public void LockImage(float duration)
    {
        lockImage = true;
        if (lockedImage != null)
        {
            StopCoroutine(lockedImage);
        }
        lockedImage = StartCoroutine(_LockImage(duration+0.1f));
    }

    private IEnumerator _LockImage(float duration)
    {
        yield return new WaitForSeconds(duration);
        lockImage = false;
    }
}