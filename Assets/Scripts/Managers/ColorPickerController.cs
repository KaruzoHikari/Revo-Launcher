using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Extensions.ColorPicker;

public class ColorPickerController : MonoBehaviour
{

    public static ColorPickerController _instance;
    public GameObject pickerObject;
    public ColorPickerControl colorPicker;
    public RectTransform innerPickerObject;

    private void Awake()
    {
        _instance = this;
    }

    private UnityAction<Color> onColorChanged;
    public void ShowColorPicker(Color initialColor, UnityAction<Color> colorChanged)
    {
        if (pickerObject.activeSelf)
        {
            HideColorPicker();
        }
        
        onColorChanged = colorChanged;
        pickerObject.SetActive(true);
        float size = CameraController.IsPortrait() ? 4.5f : 2.5f;
        float yPos = CameraController.IsPortrait() ? -140f : -40f;
        innerPickerObject.localScale = new Vector3(size, size, size);
        innerPickerObject.anchoredPosition = new Vector2(0, yPos);
        colorPicker.CurrentColor = initialColor;
        colorPicker.onValueChanged.AddListener(onColorChanged);
    }

    public void HideColorPicker()
    {
        if (onColorChanged != null)
        {
            colorPicker.onValueChanged.RemoveListener(onColorChanged);
            onColorChanged = null;
        }
        
        pickerObject.SetActive(false);
    }

}
