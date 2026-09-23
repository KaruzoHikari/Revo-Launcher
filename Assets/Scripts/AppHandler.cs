using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppHandler : ChannelHandler
{
    [JsonIgnore] public Channel channel;
    [Title("Prefab Elements")]
    public RawImage border;
    public Image cover;
    public GameObject nameTag;
    public TextMeshProUGUI nameTagText;
    private CanvasGroup nameTagCanvas;
    public RectTransform nameTagMiddle;
    public RectTransform nameTagButton;
    public Transform iconImageHolder;
    public RectTransform rectTransform;
    public GameObject options;

    public void Draw(Channel channel)
    {
        this.channel = channel;
        channel.appHandler = this;
        rectTransform = GetComponent<RectTransform>();
        nameTagCanvas = nameTag.GetComponent<CanvasGroup>();
        nameTagText = nameTag.GetComponentInChildren<TextMeshProUGUI>();
        Resize(GridController._instance.GetChannelSizeMultiplier());
    }

    public void EditChannel()
    {
        SettingsController._instance.EditChannel(channel);
        SetOptions(false);
    }

    public void RemoveChannel()
    {
        SetOptions(false);
        GridController._instance.DeleteChannel(this);
    }

    private void SetOptions(bool isEnabled)
    {
        options.SetActive(isEnabled);
        /*if (isEnabled)
        {
            channel.Cover();
        }
        else
        {
            channel.Uncover();
        }*/
    }

    private void Update()
    {
        if (GridController._instance.IsLiftingChannel() || !AnimationController._instance.shouldTimeGlobal)
        {
            return;
        }
        
        // first we check for whether we should close this app's options
        if (options.activeSelf && IsClickingSomewhereElse())
        {
            SetOptions(false);
        }

        // then we check for the lift
        if (!SwipeController._instance.shouldCheckDrag)
        {
            return;
        }
        
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (ButtonContainsPosition(CameraController._instance.GetWorldPosition(touch0.position))
                    && ButtonContainsPosition(CameraController._instance.GetWorldPosition(touch1.position)))
                {
                    GridController._instance.LiftChannel(this);
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                if (ButtonContainsPosition(CameraController._instance.GetWorldMousePosition()))
                {
                    GridController._instance.LiftChannel(this);
                }
            }
        }
    }

    private bool IsClickingSomewhereElse()
    {
        Vector2 vec;
        if (Application.isMobilePlatform && Input.touchCount > 0)
        {
            vec = CameraController._instance.GetWorldPosition(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButton(0))
        {
            vec = CameraController._instance.GetWorldMousePosition();
        }
        else
        {
            return false;
        }

        return !ButtonContainsPosition(vec);
    }

    public void ClickedChannel()
    {
        if (Input.touchCount > 1)
        {
            return;
        }

        blueBorder.alpha = 0f;
        HideTag(0f);
        ChannelController._instance.OpenChannel(this);
    }

    public void HeldChannel()
    {
        // we show the options if we hold the channel
        if (SwipeController._instance.shouldCheckHoldChannel && !IsClickingSomewhereElse() && !AppController.IsTransitioning() && !PopupController.IsPopupOpen())
        {
            SetOptions(true);
        }
    }

    public Vector2 GetCenter()
    {
        Vector3 position = transform.position;
        return new Vector2(position.x, position.y);
    }

    public void MouseEnter()
    {
        if (GridController._instance.IsLiftingChannel() || !AnimationController._instance.shouldTimeGlobal)
        {
            return;
        }
        
        StartCoroutine(_MouseEnter());
    }

    private IEnumerator _MouseEnter()
    {
        ShowBlueBorder();
        isBeingHovered = true;
        AudioController.PlaySoundEffect(AudioLibrary._instance.HOVER_BUTTON);
        
        yield return new WaitForSeconds(0.4f);
        if (isBeingHovered)
        {
            if (Application.isMobilePlatform && !PREFS.HasTriedHolding.GetBool())
            {
                PREFS.HasTriedHolding.SetBool(true);
                PopupController.ShowPopup("popup.reminderhold");
            }
            AudioController.PlaySoundEffect(AudioLibrary._instance.HOVER_CHANNEL);
            ShowTag();
        }
    }

    public void MouseLeave()
    {
        isBeingHovered = false;
        HideBlueBorder();

        HideTag(0.12f);
    }

    private void ShowTag()
    {
        // we limit the text to a certain characters
        string finalText = channel.GetTag();
        int limit = 48;
        if (finalText.Length > limit)
        {
            finalText = finalText.Substring(0, limit) + "...";
        }

        // we assign the text
        nameTag.SetActive(true);
        nameTagText.text = finalText;
        nameTagText.ForceMeshUpdate(true, true);

        // we first limit its width, in case the text is short enough to not take more than 1 line
        float width = Math.Min(nameTagText.preferredWidth, 450);
        nameTagMiddle.sizeDelta = new Vector2(width, 100);
        nameTagButton.sizeDelta = new Vector2(width, 100);
        nameTagText.ForceMeshUpdate(true, true);
        
        // now we limit by height (took some time to figure out an issue here: https://forum.unity.com/threads/text-metrics-sometimes-incorrect-when-the-text-contains-spaces.1136176/#post-7331752)
        float height = nameTagText.preferredHeight + 65;
        nameTagMiddle.sizeDelta = new Vector2(width, height);
        nameTagButton.sizeDelta = new Vector2(width, height);
        nameTagText.ForceMeshUpdate(true, true);
        
        // refresh the layouts
        RebuildLayout();
        
        // finally we readjust the width in case there's spaces around the tags with the new line height
        float minX = 0, maxX = 0;
        foreach (TMP_CharacterInfo info in nameTagText.textInfo.characterInfo)
        {
            float min = Math.Min(info.bottomLeft.x, info.topLeft.x);
            if (min < minX)
            {
                minX = min;
            }
            float max = Math.Max(info.bottomRight.x, info.topRight.x);
            if (max > maxX)
            {
                maxX = max;
            }
        }
        float maxWidth = (maxX - minX) + 7.5f;
        nameTagMiddle.sizeDelta = new Vector2(maxWidth, height);
        nameTagButton.sizeDelta = new Vector2(maxWidth, height);
        nameTagText.ForceMeshUpdate(true, true);
        
        // refresh the layouts
        RebuildLayout();

        // and now perform its anim
        DOTween.Sequence()
            .Append(nameTag.transform.DOScale(new Vector3(1f, 1f, 1f), 0.12f))
            .Join(nameTagCanvas.DOFade(1f, 0.12f));
    }

    private void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameTagText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameTagButton);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameTagButton);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameTag.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameTag.transform as RectTransform);
    }

    private void HideTag(float duration)
    {
        DOTween.Sequence()
            .Append(nameTag.transform.DOScale(new Vector3(0.9f, 0.9f, 1f), duration))
            .Join(nameTagCanvas.DOFade(0f, duration))
            .AppendCallback(() => nameTag.SetActive(false));
    }
}
