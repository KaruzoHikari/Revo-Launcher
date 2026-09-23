using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using SFB;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    // yes, i know this is ugly
    // yes, i know about sprite sheets and TMP sprite assets
    // yes, i tried to fix this FOR AGES. but i kept finding 0 info on the internet on how to swap the asset texture on runtime properly
    // or how to generate the sprite sheet, and all my tests and investigation kept failing
    // so, 1 by 1 it is. sorry.
    public Image imageHour1;
    public Image imageHour2;
    public Image imageSeparator;
    public Image imagePeriod;
    public Image imageMinute1;
    public Image imageMinute2;

    public Sprite[] clockTextures;
    public List<Sprite> originalClockTextures;

    private int currentMinute = -1;

    private bool firstLoad = true;
    private static bool shouldUpdatePeriod = false;
    private bool updatedPeriod = false;
    public bool shouldDisable = false;

    private void Awake()
    {
        DOTween.Sequence()
            .Append(DOTween.ToAlpha(() => imageSeparator.color, x => imageSeparator.color = x, 0, 0.15f))
            .AppendInterval(0.6f)
            .Append(DOTween.ToAlpha(() => imageSeparator.color, x => imageSeparator.color = x, 1, 0.25f))
            .AppendInterval(0.9f)
            .SetLoops(-1);
        CheckPeriodText();
        
        // now we save the textures
        originalClockTextures = new List<Sprite>(clockTextures);
    }

    private void Start()
    {
        RefreshTexture();
        ThemeController._instance.onThemeReload.AddListener(RefreshTexture);
    }

    private void OnDestroy()
    {
        ThemeController._instance.onThemeReload.RemoveListener(RefreshTexture);
    }
    
    [Button]
    private void RefreshTexture()
    {
        // first we update our own textures
        for (int i = 0; i < originalClockTextures.Count; i++)
        {
            Sprite sprite = originalClockTextures[i];
            Sprite replacement = ThemeController.GetTexture(sprite.texture);
            if (replacement == null)
            {
                replacement = sprite;
            }

            clockTextures[i] = replacement;
        }
        
        // and now we update it on the children
        UpdateHour(true);
    }
    
    public static void CheckPeriodText()
    {
        shouldUpdatePeriod = PREFS.Is12Format.GetBool();
    }

    [Button("Update Hour")]
    void Update()
    {
        UpdateHour(false);

        if (shouldDisable)
        {
            transform.parent.gameObject.SetActive(false);
            shouldDisable = false;
        }
    }

    public void UpdateHour(bool force)
    {
        bool needsUpdate = force || firstLoad || updatedPeriod != shouldUpdatePeriod;
        if (needsUpdate)
        {
            imagePeriod.gameObject.SetActive(shouldUpdatePeriod);
            updatedPeriod = shouldUpdatePeriod;
            firstLoad = false;
        }
        
        DateTime time = DateTime.Now;
        if (time.Minute != currentMinute || needsUpdate)
        {
            currentMinute = time.Minute;
            string hourString;
            string minuteString = currentMinute.ToString().PadLeft(2, '0');

            if (shouldUpdatePeriod)
            {
                // 12h format (ew xd)
                int hour = time.Hour;
                int index = hour >= 12 ? 12 : 11;
                
                if (time.Hour > 12)
                {
                    hour -= 12;
                } else if (time.Hour == 0)
                {
                    hour = 12;
                }

                imagePeriod.sprite = clockTextures[index];
                hourString = hour.ToString();
            }
            else
            {
                // 24h format
                hourString = time.Hour.ToString();
            }

            SetNumbers(imageHour1, imageHour2, hourString, false);
            SetNumbers(imageMinute1, imageMinute2, minuteString, true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(imageMinute1.rectTransform.parent.parent.GetComponent<RectTransform>());
        }
    }

    private void SetNumbers(Image im1, Image im2, string text, bool includeZero)
    {
        if (text.Length < 2)
        {
            im1.sprite = includeZero ? clockTextures[0] : clockTextures[13];
            im2.sprite = clockTextures[int.Parse(text[0].ToString())];
        }
        else
        {
            im1.sprite = clockTextures[int.Parse(text[0].ToString())];
            im2.sprite = clockTextures[int.Parse(text[1].ToString())];
        }
    }
}
