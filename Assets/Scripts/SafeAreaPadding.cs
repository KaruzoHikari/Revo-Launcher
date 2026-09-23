using System;
using UnityEngine;

public class SafeAreaPadding : MonoBehaviour
{

    private RectTransform rectTransform;
	public float percentageTop = 1;
	public float percentageBottom = 1;
    public float debugBottomPadding;
    public float debugTopPadding;
    
    void Start()
    {
        float top = debugTopPadding != 0 ? debugTopPadding : (CameraController.GetScreenHeight() - Screen.safeArea.yMax) * percentageTop;
        float bottom = debugBottomPadding != 0 ? debugBottomPadding : (Screen.safeArea.y) * percentageBottom;
        
        rectTransform = GetComponent<RectTransform>();
        // we'll ignore bottom padding for now, I'm more worried about the iPhone notch
        //rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
        StaticUtils.RefreshLayoutGroupsImmediateAndRecursive(gameObject);

        Debug.Log("The paddings applied are: " + bottom + ", " + top);
        Debug.Log("The new offsets are: " + rectTransform.offsetMin + ", " + rectTransform.offsetMax);
    }

    private void Update()
    {
        /*Debug.Log("the rect transform size is: " + rectTransform.sizeDelta.x + ", " + rectTransform.sizeDelta.y);
        Debug.Log("the rect transform rect is: " + rectTransform.rect.x + ", " + rectTransform.rect.y);
        Debug.Log("the rect transform position is: " + rectTransform.position.x + ", " + rectTransform.position.y);*/
    }
}
