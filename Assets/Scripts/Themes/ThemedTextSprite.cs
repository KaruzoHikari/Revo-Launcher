using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedTextSprite : ThemedImage
{
    private RectTransform rectTransform;
    public int spriteIndex = 0;
    private TextMeshProUGUI text;
    
    protected override void FindElements()
    {
        base.FindElements();

        text = transform.parent.GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    [Button]
    public override void UpdateTexture()
    {
        base.UpdateTexture();
        AppController._instance.StartCoroutine(RepositionImage());
    }

    private IEnumerator RepositionImage()
    {
        yield return null; // we wait one frame with this apparently?
        
        // we need to calculate the sprite's place
        // thanks @flashvt
        TMP_TextInfo textInfo = text.textInfo;
        yield return new WaitUntil(() => text == null || text.textInfo != null);
        if (textInfo != null && textInfo.linkInfo != null && textInfo.linkInfo.Length > 0)
        {
            TMP_LinkInfo linkInfo = textInfo.linkInfo[spriteIndex];
            //Debug.Log($"Link info: {linkInfo.ToString()}");
            TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex];
            //Debug.Log($"Char info: {currentCharInfo.ToString()}");

            float x = (currentCharInfo.bottomRight.x + currentCharInfo.bottomLeft.x) / 2f;
            float y = (currentCharInfo.topLeft.y + currentCharInfo.bottomLeft.y) / 2f;
            //Debug.Log($"Coords: {x}, {y}");

            Vector2 centerPosition = new Vector2(x, y);
        
            // now we position our image and set the proper width/height
            rectTransform.localPosition = centerPosition;
        }
        else
        {
            Debug.Log($"Links not found, hiding text sprite: {textInfo != null} - {(textInfo != null ? textInfo.linkInfo != null : 0)} - {(textInfo != null && textInfo.linkInfo != null ? textInfo.linkInfo.Length : 0)}");
            // we hide it temporarily
            if (rawImage is not null)
            {
                rawImage.color = new Color(0, 0, 0, 0);
            } else if (image is not null)
            {
                image.color = new Color(0, 0, 0, 0);
            }
        }
    }
}
