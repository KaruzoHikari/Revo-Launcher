using System;
using System.Collections;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Vector2 = UnityEngine.Vector2;

public class ThemeTransform
{
    public ThemeOffsets category;
    public Vector2 offset;
    public Vector2 scale;
    public int rotation;
    public ThemePositions position = ThemePositions.Neutral;

    public ThemeTransform(ThemeOffsets category)
    {
        this.category = category;
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        offset = Vector2.zero;
        scale = Vector2.one;
        rotation = 0;
        position = ThemePositions.Neutral;
    }

    public void RetrieveValues(ThemeTransform another)
    {
        this.offset = another.offset;
        this.scale = another.scale;
        this.rotation = another.rotation;
        this.position = another.position;
    }

    public bool HasDefaultValues()
    {
        return offset == Vector2.zero && scale == Vector2.one && rotation == 0 && position == ThemePositions.Neutral;
    }
}