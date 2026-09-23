using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyAppHandler : ChannelHandler
{
    private RectTransform rectTransform;
    public Image screen;
    public RawImage lines;
    public Sprite[] textures;
    private List<Sprite> originalTextures;
    private int currentTexture = 0;

    public int gridNumber;
    public int position;

    public float screenDelay = 0.2f;
    public float linesDelay = 0.1f;
    public float linesValue = 0.01f;

    private Coroutine screenAnim;
    private Coroutine linesAnim;

    protected void Awake()
    {
        originalTextures = new List<Sprite>(textures);
        rectTransform = GetComponent<RectTransform>();
        Resize(GridController._instance.GetChannelSizeMultiplier());
    }

    void Start()
    {
        ThemeController._instance.onThemeReload.AddListener(ReloadTextures);
        ReloadTextures();
        RestartAnimation();
    }

    private void OnDestroy()
    {
        ThemeController._instance.onThemeReload.RemoveListener(ReloadTextures);
    }

    private void ReloadTextures()
    {
        for (int i = 0; i < originalTextures.Count; i++)
        {
            Sprite newSprite = ThemeController.GetTexture(originalTextures[i].texture) ?? originalTextures[i];
            textures[i] = newSprite;
        }
    }
    
    public void Resize(float size)
    {
        transform.localScale = new Vector3(size, size, 0);
    }

    public void RestartAnimation()
    {
        StopAnimations();
        screenAnim = StartCoroutine(StartScreenAnimation());
        linesAnim = StartCoroutine(StartScreenLines());
    }

    public void StopAnimations()
    {
        try
        {
            if (screenAnim != null)
            {
                StopCoroutine(screenAnim);
            }

            if (linesAnim != null)
            {
                StopCoroutine(linesAnim);
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    private IEnumerator StartScreenAnimation()
    {
        while(true)
        {
            yield return new WaitForSeconds(screenDelay);
            currentTexture++;
            if (currentTexture >= textures.Length)
            {
                currentTexture = 0;
            }
            screen.sprite = textures[currentTexture];
        }
    }
    
    private IEnumerator StartScreenLines()
    {
        while(true)
        {
            yield return new WaitForSeconds(linesDelay);
            Rect currentRect = lines.uvRect;
            currentRect.y += Time.deltaTime * linesValue;
            lines.uvRect = currentRect;
        }
    }
}
