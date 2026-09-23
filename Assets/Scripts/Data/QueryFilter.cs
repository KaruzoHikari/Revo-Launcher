using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QueryFilter
{
    public string name;
    public string id;
    public FilterStatus status = FilterStatus.NEUTRAL;
    public GameObject linkedObject;
    public ContentType contentType;

    private Image cross;
    private Image neutral;
    private Image check;

    public QueryFilter(string name, string id, GameObject linkedObject, ContentType contentType)
    {
        this.name = name;
        this.id = id;
        this.linkedObject = linkedObject;
        this.contentType = contentType;

        cross = linkedObject.transform.Find("ButtonX").Find("Image").GetComponent<Image>();
        neutral = linkedObject.transform.Find("ButtonNeutral").Find("Image").GetComponent<Image>();
        check = linkedObject.transform.Find("ButtonCheck").Find("Image").GetComponent<Image>();
        
        linkedObject.transform.Find("ButtonX").GetComponent<Button>().onClick.AddListener(ClickedForbid);
        linkedObject.transform.Find("ButtonNeutral").GetComponent<Button>().onClick.AddListener(ClickedNeutral);
        linkedObject.transform.Find("ButtonCheck").GetComponent<Button>().onClick.AddListener(ClickedEnforce);

        ClickedNeutral();
        string translated = TextController.GetTranslation($"ap.filter.{name.ToLower().Replace(" ", "")}");
        linkedObject.transform.Find("TitleHolder").Find("Title").GetComponent<TextMeshProUGUI>().text = translated;
    }

    public void ClickedForbid()
    {
        SelectColor(cross);
        status = FilterStatus.FORBIDDEN;
    }

    public void ClickedNeutral()
    {
        SelectColor(neutral);
        status = FilterStatus.NEUTRAL;
    }

    public void ClickedEnforce()
    {
        SelectColor(check);
        status = FilterStatus.ENFORCED;
    }

    private void SelectColor(Image image)
    {
        SetColor(cross, image);
        SetColor(neutral, image);
        SetColor(check, image);
    }

    private void SetColor(Image image, Image desiredImage)
    {
        var tempColor = image.color;
        tempColor.a = image.Equals(desiredImage) ? 1f : 0.1f;
        image.color = tempColor;
    }

    public int GetStatus()
    {
        return (int)status;
    }

    public ContentType GetContentType()
    {
        return contentType;
    }
}


public enum FilterStatus
{
    FORBIDDEN,
    NEUTRAL,
    ENFORCED
}