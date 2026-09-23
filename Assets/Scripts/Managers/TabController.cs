using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabController : MonoBehaviour
{
    // This is here to allow TAB when on Desktop
    public static TabController _instance;
    private EventSystem system;
    private List<TMP_InputField> currentFields = new List<TMP_InputField>();

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        system = EventSystem.current;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TMP_InputField nextField = GetNextField();
            if (nextField != null)
            {
                nextField.OnPointerClick(new PointerEventData(system)); // if it's an input field, also set the text caret
                system.SetSelectedGameObject(nextField.gameObject, new BaseEventData(system));
            }
        }
    }

    private TMP_InputField GetNextField()
    {
        GameObject currentObject = system.currentSelectedGameObject;
        TMP_InputField inputField = currentObject.GetComponent<TMP_InputField>();
        bool reached = false;
        if (inputField != null)
        {
            for (int i = 0; i < currentFields.Count; i++)
            {
                if (currentObject.Equals(currentFields[i].gameObject))
                {
                    reached = true;
                }

                if (reached)
                {
                    if (i == currentFields.Count - 1)
                    {
                        return currentFields[0];
                    }

                    if (currentFields[i + 1] != null)
                    {
                        return currentFields[i + 1];
                    }
                }
            }
        }

        return null;
    }

    public static void AddInputField(TMP_InputField field)
    {
        _instance.currentFields.Add(field);
    }

    public static void ClearFields()
    {
        _instance.currentFields.Clear();
    }

}
