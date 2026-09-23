using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Data;
using DG.Tweening;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GridController : MonoBehaviour
{

    public static GridController _instance;

    [Title("Grid prefabs")]
    public GameObject appPrefab;
    public GameObject emptyAppPrefab;

    public int appsPerX;
    public int appsPerXHorizontal;
    public int appsPerY;
    public int appsPerYHorizontal;
    public GameObject gridHolder;
    public GameObject gridPrefab;
    public GameObject liftChannelPrefab;
    public Transform liftChannelHolder;

    private List<Grid> grids = new List<Grid>();
    [Title("Grid Settings")]
    public Grid currentGrid;
    public ScrollRect scrollRect;
    public ArrowController leftArrow;
    public ArrowController rightArrow;
    public bool isChangingGrid = false;
    public GameObject sdCard;
    public ThemedImage sdCardCover;

    [Title("Lifting Items")]
    public GameObject liftingItemsHolder;
    public Image deleteButtonImage;
    public Image editButtonImage;
    private RectTransform deleteButtonTransform;
    private RectTransform editButtonTransform;
    private bool isOverDelete;
    private bool isOverEdit;
    private bool isOverArrow;
    private ArrowController holdingArrow;
    private float arrowTimer = 0f;

    public Channel currentLiftingChannel = null;
    private GameObject liftingImage;

    private void Awake()
    {
        _instance = this;
        deleteButtonTransform = deleteButtonImage.gameObject.GetComponent<RectTransform>();
        editButtonTransform = editButtonImage.gameObject.GetComponent<RectTransform>();
        RefreshAppNumbers();
    }

    private void Start()
    {
        RefreshSdCard();
        RefreshGrids();
    }

    public void RefreshAppNumbers()
    {
        GRIDSIZE size = GetChosenGridSize();
        if (size != GRIDSIZE.CUSTOM)
        {
            int vertical = CalculateVerticalYApps(size);
            int horizontal = CalculateVerticalXApps(size);
            SetAppNumbers(horizontal, vertical);
        }
        else
        {
            // we load them from the preferences
            SetAppNumbersFromPrefs();
        }
    }

    private int GetDefaultAppsOnVerticalY()
    {
        // This formula is ancient from the code, no idea why I did it like this :/
        return CameraController.IsPortrait()
            ? (int) Math.Floor(CameraController.GetFixedHeight() / 435f)
            : (int) Math.Floor(CameraController.GetFixedWidth() / 435f);
    }

    public void SetAppNumbersFromPrefs()
    {
        bool vertical = !PREFS.AllowHorizontal.GetBool();
        int chosenX = vertical ? PREFS.NumberAppsX.GetInt() : PREFS.NumberAppsXHorizontal.GetInt();
        int chosenY = vertical ? PREFS.NumberAppsY.GetInt() : PREFS.NumberAppsYHorizontal.GetInt();
        SetAppNumbers(vertical ? chosenX : chosenY, vertical ? chosenY : chosenX);
    }

    public void SetAppNumbers(int verticalAppsPerX, int verticalAppsPerY)
    {
        appsPerX = verticalAppsPerX;
        PREFS.NumberAppsX.SetInt(verticalAppsPerX);
        appsPerYHorizontal = verticalAppsPerX;
        PREFS.NumberAppsYHorizontal.SetInt(verticalAppsPerX);
        appsPerY = verticalAppsPerY;
        PREFS.NumberAppsY.SetInt(verticalAppsPerY);
        appsPerXHorizontal = verticalAppsPerY;
        PREFS.NumberAppsXHorizontal.SetInt(verticalAppsPerY);
        
        RefreshGrids();
    }

    public GRIDSIZE GetChosenGridSize()
    {
        return Enum.Parse<GRIDSIZE>(PREFS.GridSize.GetString());
    }

    public void RefreshGridSizes()
    {
        //RefreshApps();
        foreach (Grid grid in grids)
        {
            grid.ResizeGrid();
        }
        // And we refresh the grid
        AppController._instance.gameCanvas.ForceUpdateRectTransforms();
        AppController._instance.overlayCanvas.ForceUpdateRectTransforms();
        StaticUtils.RefreshLayoutGroupsImmediateAndRecursive(gridHolder);
        if (currentGrid != null)
        {
            ChangeGrid(currentGrid.number, true, 0.1f);
        }
    }

    public void RefreshSdCard()
    {
        bool shown = PREFS.ShowSdCard.GetBool();
        sdCard.SetActive(shown);
        bool enabled = PREFS.SdCardColor.GetBool();
        SetSdCardColor(enabled);
        RectTransform dateTransform = TextController._instance.date.dateText.rectTransform;
        dateTransform.sizeDelta = new Vector2(shown ? 320 : 440, dateTransform.sizeDelta.y);
    }

    public void SetSdCardColor(bool colored)
    {
        sdCardCover.themeColor = colored ? ThemeColor.SdCardCover : ThemeColor.SdCardCoverDisabled;
        sdCardCover.UpdateElement();
        PREFS.SdCardColor.SetBool(colored);
    }

    public void RefreshClocks()
    {
        foreach (Grid grid in grids)
        {
            grid.FixPositions(true);
        }
    }

    [Button]
    public void RefreshGrids(bool goToFirstGrid = true)
    {
        // First we remove the extra grids
        List<Grid> removeList = new List<Grid>();
        for (int i = GetNumberOfGrids(); i < grids.Count; i++)
        {
            Grid grid = grids[i];
            Destroy(grid.gameObject);
            removeList.Add(grid);
        }
        removeList.ForEach(grid => grids.Remove(grid));

        // Then we add missing grids
        for (int i = grids.Count; i < GetNumberOfGrids(); i++)
        {
            GameObject obj = Instantiate(gridPrefab, gridHolder.transform);
            Grid grid = obj.AddComponent<Grid>();
            grid.number = i;
            grid.FindElements();
            grids.Add(grid);
        }
        
        // We refresh all the channel and grid sizes
        HandleEmptySpaces();
        RefreshGridSizes();
        if (goToFirstGrid)
        {
            ChangeGrid(0, true, 0f);
        }
        
        // And we also refresh the default channel viewer size
        ChannelController._instance.RefreshChannelObjectsSize();
    }

    public void ResetGridSize()
    {
        // used in case of the user abusing the system
        SetAppNumbers(2, GetDefaultAppsOnVerticalY());
    }

    public int GetNumberOfGrids()
    {
        return PREFS.NumberOfPages.GetInt();
    }

    public int GetAppsOnShortSide()
    {
        return CameraController.IsPortrait() ? appsPerX : appsPerYHorizontal;
    }

    public int GetAppsOnLongSide()
    {
        return CameraController.IsPortrait() ? appsPerY : appsPerXHorizontal;
    }

    public int GetAppsPerX()
    {
        // return 1; // animshowcase toremove
        return CameraController.IsPortrait() ? appsPerX : appsPerXHorizontal;
    }

    public int GetAppsPerY()
    {
        // return 1; // animshowcase toremove
        return CameraController.IsPortrait() ? appsPerY : appsPerYHorizontal;
    }

    public int GetMaxAppsNumber()
    {
        return GetAppsPerX() * GetAppsPerY();
    }

    public float GetChannelSizeMultiplier()
    {
        // we return the smallest, either on X or Y
        int shortApps = GetAppsOnShortSide();
        float horizontalSize = 2f / shortApps;
        
        int longApps = GetAppsOnLongSide();
        float verticalSize = GetDefaultAppsOnVerticalY() / (float) longApps;

        return Math.Min(horizontalSize, verticalSize);
    }

    public int CalculateVerticalYApps(GRIDSIZE size)
    {
        int defaultHeight = GetDefaultAppsOnVerticalY();
        switch (size)
        {
            case GRIDSIZE.BIG:
            {
                return defaultHeight;
            }
            case GRIDSIZE.MEDIUM:
            {
                return (int) (defaultHeight * 1.5f);
            }
            case GRIDSIZE.SMALL:
            {
                return defaultHeight * 2;
            }
        }
        return defaultHeight;
    }
    
    public int CalculateVerticalXApps(GRIDSIZE size)
    {
        switch (size)
        {
            case GRIDSIZE.BIG:
            {
                return 2;
            }
            case GRIDSIZE.MEDIUM:
            {
                return 3;
            }
            case GRIDSIZE.SMALL:
            {
                return 4;
            }
        }
        return 2;
    }

    public void ChangeGrid(int grid, bool force = false, float duration = 0.5f)
    {
        if (!force && (grid < 0 || grid >= grids.Count || grid == currentGrid.number))
        {
            return;
        }

        StartCoroutine(_ChangeGrid(grid, duration));
    }

    private IEnumerator _ChangeGrid(int grid, float duration)
    {
        Debug.Log($"Changing to grid {grid} in {duration} seconds.");
        // First we clear the blue border of the handlers (if there's any)
        if (currentGrid is not null)
        {
            foreach (EmptyAppHandler handler in currentGrid.emptyChannels)
            {
                handler.isBeingHovered = false;
                handler.HideBlueBorder();
            }
        }


        // Then we proceed
        Grid previousGrid = currentGrid;
        currentGrid = grids[grid];
        isChangingGrid = true;

        float endValue;
        if (grids.Count <= 1)
        {
            endValue = 0;
        }
        else
        {
            endValue = (float)currentGrid.number / (grids.Count - 1);
        }

        if (duration > 0)
        {
            scrollRect.DOHorizontalNormalizedPos(endValue, duration);
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = endValue;
        }
        
        // Now we have to lock the background of all the grids we're gonna go through
        if (previousGrid is not null)
        {
            foreach (Grid checkGrid in grids)
            {
                if (checkGrid != null && StaticUtils.IsWithinRange(checkGrid.number, previousGrid.number, currentGrid.number))
                {
                    checkGrid.EnableBackground();
                    if (checkGrid.isActiveAndEnabled)
                    {
                        checkGrid.LockImage(duration);
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(duration);

        // Here we check which arrows should be enabled
        if (currentGrid.number <= 0)
        {
            leftArrow.Hide();
        }
        else
        {
            leftArrow.Show();
        }

        if (currentGrid.number < grids.Count - 1)
        {
            rightArrow.Show();
        }
        else
        {
            rightArrow.Hide();
        }
        
        // And finally we clear the backgrounds
        foreach (Grid checkGrid in grids)
        {
            checkGrid.FixPositions(true);
            if (checkGrid.number != currentGrid.number)
            {
                checkGrid.DisableBackground();
            }
            else
            {
                checkGrid.EnableBackground();
            }
        }
        
        isChangingGrid = false;
    }

    public void HideArrows()
    {
        leftArrow.Hide();
        rightArrow.Hide();
    }

    public void ShowArrows()
    {
        if (currentGrid.number > 0)
        {
            leftArrow.Show();
        }

        if (currentGrid.number < grids.Count - 1)
        {
            rightArrow.Show();
        }
    }

    public bool InsertChannel(Channel channel)
    {
        // if it gets out of the app limits, we don't actually draw it
        if (channel.position >= GetMaxAppsNumber())
        {
            return true;
        }
        
        GameObject prefab = null;
        try
        {
            Transform grid = grids[channel.gridNumber].innerGrid;
            prefab = Instantiate(appPrefab, grid);
            int childIndex = channel.position;

            // Since things get weird when loading, we first swap their child index and then destroy it
            if (grid.childCount > childIndex)
            {
                DestroyEmptyChannel(channel.gridNumber, channel.position);
                prefab.transform.SetSiblingIndex(childIndex);
            }

            AppHandler drawer = prefab.GetComponent<AppHandler>();
            drawer.Draw(channel);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            if (prefab != null)
            {
                Destroy(prefab);
            }

            return false;
        }
    }

    private void HandleEmptySpaces()
    {
        int max = GetMaxAppsNumber();
        for (int gridNumber = 0; gridNumber < GetNumberOfGrids(); gridNumber++)
        {
            Transform grid = grids[gridNumber].innerGrid;
            int previousCount = grid.childCount;
            // first we add the missing empty ones
            for (int child = grid.childCount; child < max; child++)
            {
                int number = gridNumber;
                SpawnEmptyChannel(number, child);
            }
            
            // now we check if we were mising any channels - if so, we add them
            foreach (Channel channel in ChannelController._instance.loadedChannels)
            {
                if (channel.gridNumber == gridNumber && channel.position >= previousCount)
                {
                    InsertChannel(channel);
                    channel.LinkAnimations();
                }
            }
            
            // on the other hand, we remove the remaining channels if the size became smaller than previous one
            foreach (Channel channel in ChannelController._instance.loadedChannels)
            {
                if (channel.gridNumber == gridNumber && channel.position >= max && channel.appHandler != null)
                {
                    RemoveChannelFromGrid(channel.appHandler);
                }
            }
            
            // finally we remove extra empty ones if they exist
            for (int child = grid.childCount; child > max; child--)
            {
                int number = gridNumber;
                DestroyEmptyChannel(number, child-1);
            }
        }
    }

    public void LiftChannel(AppHandler appHandler)
    {
        if (AppController.IsTransitioning() || isChangingGrid || ChannelController._instance.currentChannel != null ||
            PopupController.IsPopupOpen())
        {
            return;
        }

        currentLiftingChannel = appHandler.channel;
        RemoveChannelFromGrid(appHandler);

        liftingImage = Instantiate(liftChannelPrefab, liftChannelHolder);
        liftingImage.transform.localPosition = CameraController._instance.GetMiddleCursor();
        //liftingImage.transform.SetSiblingIndex(AppController._instance.mainView.transform.childCount - 3);
        liftingImage.transform.localScale = Vector3.one * GetChannelSizeMultiplier();

        liftingItemsHolder.SetActive(true);
        DOTween.ToAlpha(() => deleteButtonImage.color, x => deleteButtonImage.color = x, 1f, 0.3f);
        DOTween.ToAlpha(() => editButtonImage.color, x => editButtonImage.color = x, 1f, 0.3f);
        ChannelController._instance.CoverChannels();
        PopupController.ShowBlocker();
    }

    public void RemoveChannelFromGrid(AppHandler appHandler)
    {
        // We unlink the channel animations so that they won't try to update themselves here
        appHandler.channel.UnlinkAnimations();
        
        // We remove the channel object
        Destroy(appHandler.gameObject);
        
        // And we spawn an empty channel in its place
        SpawnEmptyChannel(appHandler.channel.gridNumber, appHandler.channel.position);
    }

    public void DeleteChannel(AppHandler appHandler)
    {
        // First we remove from grid
        RemoveChannelFromGrid(appHandler);
        
        // Now we delete from files
        ChannelController._instance.DeleteChannel(appHandler.channel);
    }

    private void Update()
    {
        if (IsLiftingChannel())
        {
            bool shouldStop;
            if (Application.isMobilePlatform)
            {
                shouldStop = Input.touchCount == 0;
            }
            else
            {
                shouldStop = !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
            }

            if (!shouldStop)
            {
                // If we're still holding the channel, we update the position and check for blue borders
                liftingImage.transform.localPosition = CameraController._instance.GetMiddleCursor();

                foreach (EmptyAppHandler handler in currentGrid.emptyChannels)
                {
                    // First we check for the channels
                    bool cursorIsOver = handler.ButtonContainsPosition(liftingImage.transform.localPosition);
                    if (!cursorIsOver && handler.isBeingHovered)
                    {
                        handler.isBeingHovered = false;
                        handler.HideBlueBorder();
                    }
                    else if (cursorIsOver && !handler.isBeingHovered)
                    {
                        handler.isBeingHovered = true;
                        handler.ShowBlueBorder();
                    }

                    // Then we check for the delete button
                    bool cursorDelete = StaticUtils.IsBeingHovered(deleteButtonTransform, liftingImage.transform.localPosition);
                    if (cursorDelete && !isOverDelete)
                    {
                        isOverDelete = true;
                        deleteButtonTransform.DOScale(new Vector3(1.75f, 1.75f, 1.75f), 0.25f);
                    }
                    else if (!cursorDelete && isOverDelete)
                    {
                        isOverDelete = false;
                        deleteButtonTransform.DOScale(new Vector3(1f, 1f, 1f), 0.25f);
                    }
                    
                    // Finally we check the edit button
                    bool cursorEdit = StaticUtils.IsBeingHovered(editButtonTransform, liftingImage.transform.localPosition);
                    if (cursorEdit && !isOverEdit)
                    {
                        isOverEdit = true;
                        editButtonTransform.DOScale(new Vector3(1.75f, 1.75f, 1.75f), 0.25f);
                    }
                    else if (!cursorEdit && isOverEdit)
                    {
                        isOverEdit = false;
                        editButtonTransform.DOScale(new Vector3(1f, 1f, 1f), 0.25f);
                    }
                }

                // Then we check for the arrows
                CheckChannelOverArrow();
            }
            else
            {
                LayDownChannel();
            }
        }
        else
        {
            // we check for the back button
            if (!AppController.IsTransitioning() && !isChangingGrid && scrollRect.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape)) {
                // we move to the first grid
                AudioController.PlaySoundEffect(AudioLibrary._instance.CLICK_PLUSLESS);
                ChangeGrid(0);
            }
        }
    }

    private void CheckChannelOverArrow()
    {
        if (arrowTimer > 1f)
        {
            holdingArrow.ClickedArrow();
            arrowTimer -= 0.5f;
        }

        if (isChangingGrid)
        {
            return;
        }

        Vector3 localPosition = liftingImage.transform.localPosition;
        bool overLeft = StaticUtils.IsBeingHovered(leftArrow.channelHitboxTransform, localPosition);
        bool overRight = StaticUtils.IsBeingHovered(rightArrow.channelHitboxTransform, localPosition);

        if (isOverArrow)
        {
            // Here we either keep the timer going, reset it to another arrow, or cancel it
            if (overLeft)
            {
                if (holdingArrow.Equals(leftArrow))
                {
                    arrowTimer += Time.deltaTime;
                }
                else
                {
                    holdingArrow.MouseLeave();
                    holdingArrow = leftArrow;
                    holdingArrow.MouseEnter();
                    arrowTimer = 0;
                }
            }
            else if (overRight)
            {
                if (holdingArrow.Equals(rightArrow))
                {
                    arrowTimer += Time.deltaTime;
                }
                else
                {
                    holdingArrow.MouseLeave();
                    holdingArrow = rightArrow;
                    holdingArrow.MouseEnter();
                    arrowTimer = 0;
                }
            }
            else
            {
                isOverArrow = false;
                holdingArrow.MouseLeave();
                holdingArrow = null;
                arrowTimer = 0;
            }
        }
        else
        {
            if (overLeft)
            {
                holdingArrow = leftArrow;
                isOverArrow = true;
                holdingArrow.MouseEnter();
            }
            else if (overRight)
            {
                holdingArrow = rightArrow;
                isOverArrow = true;
                holdingArrow.MouseEnter();
            }
            else
            {
                isOverArrow = false;
                holdingArrow = null;
                arrowTimer = 0;
            }
        }
    }

    private void LayDownChannel()
    {
        if (currentLiftingChannel == null)
        {
            return;
        }

        if (isOverDelete)
        {
            ChannelController._instance.DeleteChannel(currentLiftingChannel);
        }
        else
        {
            if (isOverEdit)
            {
                // we enter the edit menu
                SettingsController._instance.EditChannel(currentLiftingChannel);
            }
            else
            {
                // Now we check if there's any empty channel underneath. Otherwise we just reinsert the channel in its new position
                EmptyAppHandler emptyChannel = null;
                foreach (EmptyAppHandler handler in currentGrid.emptyChannels)
                {
                    if (handler.ButtonContainsPosition(liftingImage.transform.localPosition))
                    {
                        emptyChannel = handler;
                        break;
                    }
                }

                if (emptyChannel != null)
                {
                    currentLiftingChannel.position = emptyChannel.position;
                    currentLiftingChannel.gridNumber = emptyChannel.gridNumber;
                }
            }
            
            // We put the channels back to wherever they should be
            InsertChannel(currentLiftingChannel);
            currentLiftingChannel.LinkAnimations();
            currentLiftingChannel.Save();

            if (!isOverEdit)
            {
                AnimationController._instance.RestartIconAnimations();
            }
        }

        // We reset all the lifting channel status
        Destroy(liftingImage.gameObject);
        currentLiftingChannel = null;
        liftingImage = null;

        isOverDelete = false;
        deleteButtonTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f);
        editButtonTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f);
        DOTween.ToAlpha(() => deleteButtonImage.color, x => deleteButtonImage.color = x, 0f, 0.3f);
        DOTween.ToAlpha(() => editButtonImage.color, x => editButtonImage.color = x, 0f, 0.3f);

        ChannelController._instance.UncoverChannels();
        PopupController.CloseBlocker();
    }

    public void SpawnEmptyChannel(int gridNumber, int position)
    {
        Transform grid = grids[gridNumber].innerGrid;
        GameObject prefab = Instantiate(emptyAppPrefab, grid);
        int childIndex = position;
        prefab.transform.SetSiblingIndex(childIndex);
        prefab.transform.Find("Mask").Find("Click").GetComponent<LongClickButton>()
            .onLongClick.AddListener(() => ChannelController._instance.OnHoldEmptyChannel(gridNumber, position));
        EmptyAppHandler emptyAppHandler = prefab.GetComponent<EmptyAppHandler>();
        emptyAppHandler.gridNumber = gridNumber;
        emptyAppHandler.position = position;


        // Then we add it to our list of empty channels
        grids[gridNumber].emptyChannels.Add(emptyAppHandler);
    }

    public void DestroyEmptyChannel(int gridNumber, int pos)
    {
        Transform grid = grids[gridNumber].innerGrid;
        GameObject emptyChannel = grid.GetChild(pos).gameObject;
        emptyChannel.transform.SetAsLastSibling();

        EmptyAppHandler handler = emptyChannel.GetComponent<EmptyAppHandler>();
        if (handler != null)
        {
            handler.StopAnimations();
            grids[gridNumber].emptyChannels.Remove(handler);
            Destroy(emptyChannel.gameObject);
        }
    }

    public bool IsLiftingChannel()
    {
        return currentLiftingChannel != null;
    }

    public List<EmptyAppHandler> GetAllEmptyAppHandlers()
    {
        List<EmptyAppHandler> list = new List<EmptyAppHandler>();
        foreach (Grid grid in grids)
        {
            list.AddRange(grid.emptyChannels);
        }

        return list;
    }

}
