using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class ThemedElement : MonoBehaviour
{
    public ThemeColor themeColor;
    [HideInInspector] public bool updateElement = true;

    private void Awake()
    {
        FindElements();
    }

    void Start()
    {
        StartCoroutine(TriggerUpdate());
    }

    private IEnumerator TriggerUpdate()
    {
        while (!ThemeController._instance.HasLoadedTheme())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        UpdateElement();
    }
    
    [Button]
    public void UpdateElement()
    {
        if (!updateElement)
        {
            return;
        }
        
        _UpdateElement();
    }

    protected abstract void _UpdateElement();
    protected abstract void FindElements();

    public virtual ThemeColor[] GetAllColors()
    {
        return new ThemeColor[] { themeColor };
    }
}
