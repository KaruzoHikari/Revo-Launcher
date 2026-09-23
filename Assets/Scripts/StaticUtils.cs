using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SFB;
using SimpleFileBrowser;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public static class StaticUtils
{
    public static bool IsWithinRange(int number, int bound1, int bound2)
    {
        return number >= Math.Min(bound1, bound2) && number <= Math.Max(bound1, bound2);
    }

    public static bool IsBeingHovered(RectTransform button, Vector2 pos)
    {
        Vector2 sizeDelta = button.sizeDelta;
        Vector3 position = button.transform.position;
        Vector3 localScale = button.lossyScale;
        float fMinX = position.x - (sizeDelta.x * 0.5f * localScale.x);
        float fMaxX = position.x + (sizeDelta.x * 0.5f * localScale.x);
        float fMinY = position.y - (sizeDelta.y * 0.5f * localScale.y);
        float fMaxY = position.y + (sizeDelta.y * 0.5f * localScale.y);

        if(pos.x <= fMaxX && pos.x >= fMinX)
        {
            if(pos.y <= fMaxY && pos.y >= fMinY)
            {
                return true;
            }
        }

        return false;
    }
    
    public static void ChangeBooleanImage(GameObject gameObject, bool newBoolean)
    {
        GameObject imageHolder = gameObject.transform.Find("Elements").Find("ImageHolder").gameObject;

        ThemeColor backThemeColor = newBoolean ? ThemeColor.BooleanTrueBackground : ThemeColor.BooleanFalseBackground;
        Color backColor = ThemeController.GetColor(backThemeColor);
        imageHolder.transform.Find("Color").GetComponent<Image>().color = backColor;
        imageHolder.GetComponent<UICornerCut>().color = backColor;
        ThemedImage backImage = imageHolder.transform.Find("Color").GetComponent<ThemedImage>();
        backImage.themeColor = backThemeColor;
        backImage.FindElements(true);

        Texture newTexture = newBoolean ? EditorController._instance.checkmark : EditorController._instance.cross;
        Sprite replacement = ThemeController.GetTexture(newTexture);
        newTexture = replacement is null ? newTexture : replacement.texture;

        ThemeColor iconThemeColor = newBoolean ? ThemeColor.BooleanTrueIcon : ThemeColor.BooleanFalseIcon;
        Color iconColor = ThemeController.GetColor(iconThemeColor);
        RawImage icon = imageHolder.transform.Find("Image").gameObject.GetComponent<RawImage>();
        icon.texture = newTexture;
        icon.color = iconColor;
        ThemedImage iconImage = icon.transform.GetComponent<ThemedImage>();
        iconImage.themeColor = iconThemeColor;
        iconImage.FindElements(true);
    }

    private static char[] urlCharacters = {'&', '+', '$', ',', '/', ';', ':', '=', '?', '@'};
    public static string SanitizeString(string text)
    {
        string sanitized = string.Join("z", text.Split(Path.GetInvalidFileNameChars()));
        sanitized = string.Join("z", sanitized.Split(Path.GetInvalidPathChars()));
        foreach (char character in urlCharacters)
        {
            sanitized = sanitized.Replace(character, 'z');
        }
        return sanitized;
    }

    public static bool IsInsideTransform(RectTransform trans, Vector2 vector, float offset)
    {
        Vector3[] corners = new Vector3[4];
        trans.GetWorldCorners(corners);
        Vector2[] surrounded = GetSurroundedPositions(vector, offset);

        // now we calculate the proper corners cause it's not always right
        float minX = corners[0].x;
        float maxX = corners[0].x;
        float minY = corners[0].y;
        float maxY = corners[0].y;
        foreach (Vector3 v in corners)
        {
            if (v.x < minX)
            {
                minX = v.x;
            }
            if (v.x > maxX)
            {
                maxX = v.x;
            }
            if (v.y < minY)
            {
                minY = v.y;
            }
            if (v.y > maxY)
            {
                maxY = v.y;
            }
        }

        Vector2 topLeft = new Vector2(minX, maxY);
        Vector2 bottomRight = new Vector2(maxX, minY);
        
        return IsOverlaping(topLeft, bottomRight, surrounded[0], surrounded[2]);
    }

    private static Vector2[] GetSurroundedPositions(Vector2 v, float offset)
    {
        return new Vector2[]
        {
            new Vector2(v.x - offset, v.y + offset), new Vector2(v.x + offset, v.y + offset),
            new Vector2(v.x + offset, v.y - offset), new Vector2(v.x - offset, v.y - offset),
        };
    }

    private static bool IsOverlaping(Vector2 topLeft1, Vector2 bottomRight1, Vector2 topLeft2, Vector2 bottomRight2)
    {
        // thanks stackoverflow xd
        
        // if rectangle has area 0, no overlap
        if (topLeft1.x == bottomRight1.x || topLeft1.y == bottomRight1.y || bottomRight2.x == topLeft2.x || topLeft2.y == bottomRight2.y)
        {
            return false;
        }
       
        // If one rectangle is on left side of other
        if (topLeft1.x > bottomRight2.x || topLeft2.x > bottomRight1.x)
        {
            return false;
        }
 
        // If one rectangle is above other
        if (bottomRight1.y > topLeft2.y || bottomRight2.y > topLeft1.y)
        {
            return false;
        }
        return true;
    }
    
    public static Texture2D DuplicateTexture(this Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    
    public static void MarkAsNonReadable(this Texture2D source)
    {
        source.Apply(false,true);
    }

    public static void LoadGif(string filePath, List<Texture2D> gifFrames, List<float> gifFrameDelays)
    {
        try
        {
            gifFrames.Clear();
            gifFrameDelays.Clear();
            using (var gifStream = new GifStream(filePath))
            {
                while (gifStream.HasMoreData)
                {
                    switch (gifStream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                        {
                            var image = gifStream.ReadImage();
                            var frame = new Texture2D(gifStream.Header.width, gifStream.Header.height, TextureFormat.ARGB32, false);

                            frame.SetPixels32(image.colors);
                            frame.Apply();

                            gifFrames.Add(frame);
                            gifFrameDelays.Add(image.SafeDelaySeconds);
                            break;
                        } 
                        
                        default:
                        {
                            gifStream.SkipToken(); // Other tokens
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error while loading gif!\n" + e);
        }
    }
    
    // from https://discussions.unity.com/t/force-immediate-layout-update/608126/56
    // this kept driving me nuts, the documentation is wrong. thanks a lot
    public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
    {
        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
    }

    public static void SetViewAtTop(this ScrollRect scrollRect)
    {
        scrollRect.SetViewAt(1f);
    }
    
    public static void SetViewAtBottom(this ScrollRect scrollRect)
    {
        scrollRect.SetViewAt(0f);
    }

    public static void SetViewAt(this ScrollRect scrollRect, float value)
    {
        scrollRect.verticalNormalizedPosition = value;
        AppController._instance.StartCoroutine(ApplyScrollPosition(scrollRect, value));
    }
    
    private static IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
    {
        // thanks StackOverflow
        yield return new WaitForEndOfFrame();
        sr.verticalNormalizedPosition = verticalPos;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
    }

    public static string GetGameFileName(string path)
    {
        return Path.GetFileNameWithoutExtension(GetGameFileNameWithExtension(path));
    }
    
    public static string GetGameFileNameWithExtension(string path)
    {
        return Application.isMobilePlatform && FileBrowserHelpers.FileExists(path) ? FileBrowserHelpers.GetFilename(path) : FileManager.GetFileName(path.Trim());
    }
    
    public static string GetExtension(string path)
    {
        return Path.GetExtension(Application.isMobilePlatform && FileBrowserHelpers.FileExists(path) ? FileBrowserHelpers.GetFilename(path) : path).Replace(".","").ToLowerInvariant().Trim();
    }

    public static string GetOnlyAlphanumeric(string str)
    {
        // we need to remove "the" from the games because it keeps messing up the titles like zelda. wtf
        return string.IsNullOrEmpty(str) ? "" : Regex.Replace(str.ToLowerInvariant().Replace(" the ", ""), "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
    }
    
    public static double GetJaccardDistance(string source, string target)
    {
        return 1 - source.JaccardIndex(target);
    }

    private static double JaccardIndex(this string source, string target)
    {
        return (Convert.ToDouble(source.Intersect(target).Count())) / (Convert.ToDouble(source.Union(target).Count()));
    }
    
    public static int GetLongestCommonSubstring(string source, string target)
    {
        if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target)) { return 0; }

        int[,] L = new int[source.Length, target.Length];
        int maximumLength = 0;
        int lastSubsBegin = 0;
        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < source.Length; i++)
        {
            for (int j = 0; j < target.Length; j++)
            {
                if (source[i] != target[j])
                {
                    L[i, j] = 0;
                }
                else
                {
                    if ((i == 0) || (j == 0))
                        L[i, j] = 1;
                    else
                        L[i, j] = 1 + L[i - 1, j - 1];

                    if (L[i, j] > maximumLength)
                    {
                        maximumLength = L[i, j];
                        int thisSubsBegin = i - L[i, j] + 1;
                        if (lastSubsBegin == thisSubsBegin)
                        {
                            // if the current LCS is the same as the last time this block ran
                            stringBuilder.Append(source[i]);
                        }
                        else
                        {
                            // this block resets the string builder if a different LCS is found
                            lastSubsBegin = thisSubsBegin;
                            stringBuilder.Length = 0; //clear it
                            stringBuilder.Append(source.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                        }
                    }
                }
            }
        }

        return stringBuilder.ToString().Length;
    }

    public static int GetLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }
}