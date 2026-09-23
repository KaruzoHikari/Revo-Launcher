using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler/*, IDragHandler*/
{
    private bool pointerDown;
    private bool executedCommand;
    private float pointerDownTimer;

    [SerializeField]
    private float requiredHoldTime;
    public UnityEvent onLongClick;
    public UnityEvent onRelease;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (executedCommand)
        {
            onRelease?.Invoke();
        }
        Reset(true);
    }
    
    /*public void OnDrag(PointerEventData eventData)
    {
        // We block the drag event from a certain delta because it causes issues with Scroll rects
        // This will cause hold buttons to work even if you -start- holding them, but stop holding somewhere else
        // But that's fine.
    }*/

    private void Update()
    {
        if (!executedCommand && pointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                onLongClick?.Invoke();
                executedCommand = true;
                Reset(false);
            }
        }
    }

    private void Reset(bool includeCommand)
    {
        pointerDown = false;
        if (includeCommand)
        {
            executedCommand = false;
        }
        pointerDownTimer = 0;
    }

    private void OnEnable()
    {
        Reset(true);
    }
}