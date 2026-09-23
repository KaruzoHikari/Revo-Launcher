using UnityEngine;

public static class MyVector2Extension
{
    public static Vector3[] ToVector3Array(this Vector2[] v2)
    {
        Vector3[] vec3Array = new Vector3[v2.Length];
        for (int i = 0; i < v2.Length; i++)
        {
            vec3Array[i] = v2[i];
        }
        return vec3Array;
    }
}