using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class TranslatedText : MonoBehaviour
{
    public string id;
    [HideInInspector] public bool canBeRefreshed = true;
    private void Start()
    {
        Refresh();
    }

    [Button]
    public void Refresh()
    {
        if (!string.IsNullOrEmpty(id) && canBeRefreshed)
        {
            GetComponent<TextMeshProUGUI>().text = TextController.GetTranslation(id);
        }
    }
}
