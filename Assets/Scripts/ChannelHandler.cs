using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ChannelHandler : MonoBehaviour
{
    public CanvasGroup blueBorder;
    protected Tween blueBorderTween;
    public RectTransform buttonRectTransform;
    
    public bool isBeingHovered = false;

    public void Resize(float size)
    {
        transform.localScale = new Vector3(size, size, 0);
    }

    public void ShowBlueBorder()
    {
        if (blueBorderTween != null && blueBorderTween.IsActive())
        {
            blueBorderTween.Kill();
            blueBorderTween = null;
        }

        Transform borderTransform = blueBorder.transform;
        borderTransform.SetAsFirstSibling();
        borderTransform.localScale = new Vector3(1, 1, 1);
        blueBorder.alpha = 1f;
        
        WiimoteController.RumbleMain(WiimoteController._instance.buttonRumbleTime);
    }

    public void HideBlueBorder()
    {
        blueBorder.transform.SetAsLastSibling();

        blueBorderTween = DOTween.Sequence()
            .Append(blueBorder.transform.DOScale(new Vector3(0.85f, 0.85f, 1f), 1f))
            .Join(blueBorder.DOFade(0f, 1f))
            .AppendCallback(() => blueBorderTween = null);
    }
    
    public bool ButtonContainsPosition(Vector2 xPos)
    {
        return StaticUtils.IsBeingHovered(buttonRectTransform, xPos);
    }
}
