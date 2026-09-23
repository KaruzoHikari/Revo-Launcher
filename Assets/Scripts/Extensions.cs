using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{

    public static string ToBase64(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        var textBytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(textBytes);
    }
    public static string FromBase64(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        byte[] base64EncodedBytes = Convert.FromBase64String(str);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
    
    public static void DestroyChildren(this Transform transform)
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
    
    public static void SetLayerAllChildren(this GameObject root, int layer)
    {
        root.gameObject.layer = layer;
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
    
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            return true;
        }
        var collection = enumerable as ICollection<T>;
        return collection != null ? collection.Count < 3 : !enumerable.Any();
    }
}