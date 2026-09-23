using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public static PopupController _instance;
    public GameObject popupView;
    public Image popupBackground;
    public GameObject popupPrefab;
    public GameObject unthemedPopupPrefab;

    private GameObject currentPopup;
    
    private void Awake()
    {
        _instance = this;
    }

    public static bool IsPopupOpen()
    {
        return _instance.currentPopup != null;
    }

    public static bool IsBlockerOpen()
    {
        return _instance.popupView.activeSelf;
    }

    public static void ClosePopup()
    {
        GameObject popup = _instance.currentPopup;
        _instance.StartCoroutine(_ClosePopup(popup));
    }

    public static void CloseBlocker()
    {
        ClosePopup();
    }

    private static IEnumerator _ClosePopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.transform.DOLocalMoveY(CameraController.GetFixedHeight(), 0.4f).SetEase(Ease.InQuad);
            DOTween.ToAlpha(() => _instance.popupBackground.color, x => _instance.popupBackground.color = x, 0, 0.4f).SetEase(Ease.InQuad);

            yield return new WaitForSeconds(0.4f);
            if (popup.Equals(_instance.currentPopup))
            {
                DestroyCurrentPopup();
                _instance.popupView.SetActive(false);
            }
            else
            {
                Destroy(popup); // another popup is open atm
            }
        }
        else
        {
            _instance.popupView.SetActive(false);
        }
    }

    public static void DestroyCurrentPopup()
    {
        if (_instance.currentPopup != null)
        {
            Destroy(_instance.currentPopup);
            _instance.currentPopup = null;
        }
    }

    public static void ResetBackgroundTransparency()
    {
        _instance.popupBackground.color = Color.clear;
    }

    public static void ShowBlocker()
    {
        _instance.popupView.SetActive(true);
    }

    public static GameObject ShowPopup(string id, bool addOkButton = true, string[] replacementArray = null, float extraSize = 300, bool closePopupAfter = true, GameObject customPrefab = null)
    {
        Debug.Log($"Showing popup! {id}");
        DestroyCurrentPopup();
        _instance.popupView.SetActive(true);

        GameObject popup = Instantiate(customPrefab is null ? _instance.popupPrefab : customPrefab, _instance.popupView.transform);
        _instance.currentPopup = popup;
        _instance.currentPopup.transform.localPosition = new Vector3(0, -CameraController.GetFixedHeight(), 0);
        TextMeshProUGUI textComponent = _instance.currentPopup.transform.Find("Middle").Find("Elements").Find("Text").GetComponent<TextMeshProUGUI>();

        string text = TextController.GetTranslation(id, replacementArray);
        textComponent.text = text;
        
        // we need to update the font now or it won't fit properly in some new fonts
        ThemedText themedText = textComponent.GetComponent<ThemedText>();
        if (themedText != null)
        {
            themedText.UpdateElement();
        }
        
        // now we add the ID so the themes editor can pick it up
        TranslatedText trans = textComponent.GetComponent<TranslatedText>();
        if (trans != null)
        {
            trans.canBeRefreshed = false;
            trans.id = id;
        }
        
        // now we setup the height
        float maxHeight = CameraController.GetFixedHeight() - 80;
        float textHeight = textComponent.preferredHeight;
        // We can't let popups get out of bounds
        if (textHeight > maxHeight - extraSize)
        {
            textHeight = maxHeight - extraSize;
        }
        textComponent.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(855, textHeight);

        float preferredHeight = textComponent.preferredHeight + extraSize;
        // We can't let popups get out of bounds
        if (preferredHeight > maxHeight)
        {
            preferredHeight = maxHeight;
        }
        _instance.currentPopup.transform.Find("Middle").GetComponent<RectTransform>().sizeDelta = new Vector2(935, preferredHeight);
        _instance.currentPopup.transform.Find("Middle").Find("Elements").GetComponent<RectTransform>().sizeDelta = new Vector2(855, preferredHeight);

        if (addOkButton)
        {
            GameObject okButton = _instance.currentPopup.transform.Find("Middle").Find("Elements").Find("Button_OK").gameObject;
            okButton.SetActive(true);
            
            if (closePopupAfter)
            {
                okButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (_instance.currentPopup.Equals(popup))
                    {
                        ClosePopup();
                    }
                });
            }
        }
        
        // Animations
        _instance.currentPopup.transform.DOLocalMoveY(0f, 0.4f);
        DOTween.ToAlpha(() => _instance.popupBackground.color, x => _instance.popupBackground.color = x, 200f / 255f, 0.4f);
        return popup;
    }
    
    public static void ShowPopup(string id, UnityAction okCallback, bool closePopup = true, string[] replacementArray = null, GameObject customPrefab = null)
    {
        GameObject popup = ShowPopup(id, okCallback != null, replacementArray, okCallback != null ? 300 : 150, false, customPrefab);
        Button okButton = _instance.currentPopup.transform.Find("Middle").Find("Elements").Find("Button_OK").GetComponent<Button>();

        if (okCallback != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(okCallback);
            
            if (closePopup)
            {
                okButton.onClick.AddListener(() =>
                {
                    if (_instance.currentPopup.Equals(popup))
                    {
                        ClosePopup();
                    }
                });
            }
        }
    }
    
    public static void ShowPopup(string id, UnityAction yesCallback, UnityAction noCallback, string[] replacementArray = null, GameObject customPrefab = null)
    {
        ShowPopup(id, false, replacementArray, customPrefab: customPrefab);
        
        GameObject buttonsHolder = _instance.currentPopup.transform.Find("Middle").Find("Elements").Find("ButtonsYesNo").gameObject;
        buttonsHolder.SetActive(true);
        buttonsHolder.transform.Find("Button_Yes").GetComponent<Button>().onClick.AddListener(yesCallback);
        buttonsHolder.transform.Find("Button_No").GetComponent<Button>().onClick.AddListener(noCallback);
    }
}
