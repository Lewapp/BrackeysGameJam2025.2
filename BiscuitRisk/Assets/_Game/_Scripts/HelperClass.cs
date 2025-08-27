using System;
using UnityEngine;

public static class HelperClass 
{
    public static T GetRandomEnumValue<T>()
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}
