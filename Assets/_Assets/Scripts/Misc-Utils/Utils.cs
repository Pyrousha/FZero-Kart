using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    /// <summary>
    /// Remaps a value from one range to another, keeping constant percentage of range
    /// </summary>
    /// <param name="value"></param> value to interpolate based on
    /// <param name="minIn"></param> lower bound of input range
    /// <param name="maxIn"></param> upper bound of input range
    /// <param name="minOut"></param> lower bound of output range
    /// <param name="maxOut"></param> upper bound of output range
    /// <returns></returns>
    public static float RemapRange(float value, float minIn, float maxIn, float minOut, float maxOut)
    {
        if (maxIn < minIn || maxOut < minOut)
            Debug.LogError("min must be less than max!");

        float percent = (value - minIn) / (maxIn - minIn);

        return RemapPercent(percent, minOut, maxOut);
    }

    /// <summary>
    /// Remaps a percentage to a value based on a given range
    /// </summary>
    /// <param name="percent"></param> percentage from min to max of range
    /// <param name="minOut"></param> lower bound of output range
    /// <param name="maxOut"></param> upper bound of output range
    /// <returns></returns>
    public static float RemapPercent(float percent, float minOut, float maxOut)
    {
        //if  (maxOut < minOut)
        //    Debug.LogError("min must be less than max!");

        float outRange = maxOut - minOut;
        float outValue = outRange * percent + minOut;

        return outValue;
    }

    /// <summary>
    /// modifies a specified vector's x,y, and/or z parameter
    /// </summary>
    /// <param name="vector"></param> vector to modify
    /// <param name="newX"></param> new x-value of vector (or null to not modify)
    /// <param name="newY"></param> new y-value of vector (or null to not modify)
    /// <param name="newZ"></param> new z-value of vector (or null to not modify)
    /// <returns></returns>
    public static Vector3 ModifyVector(Vector3 vector, float? newX, float? newY, float? newZ)
    {
        Vector3 toReturn = vector;

        if (newX != null)
            toReturn.x = (float)newX;

        if (newY != null)
            toReturn.y = (float)newY;

        if (newZ != null)
            toReturn.z = (float)newZ;

        return toReturn;
    }

    /// <summary>
    /// Returns a list of transforms, which are the children of the specified transform
    /// </summary>
    /// <param name="parent"></param> parent transform to get children of
    /// <returns></returns>
    public static List<Transform> GetChildrenOfTransform(Transform parent)
    {
        List<Transform> children = new List<Transform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }

        return children;
    }

    /// <summary>
    /// Adds enough leading zeroes to an int to have a string of specified size
    /// </summary>
    /// <param name="number">number to add zeroes in front of</param>
    /// <param name="numCharactersTotal"></param> number of characters to have total in returned string
    /// <returns> given number as a string, with enough leading zeroes to make string size equal to numCharactersTotal. </returns>
    public static string AddLeadingZeroes(int number, int numCharactersTotal)
    {
        string toReturn = number.ToString();

        while (toReturn.Length < numCharactersTotal)
        {
            toReturn = "0" + toReturn;
        }

        return toReturn;
    }

    public static Color MultiplySaturation(Color col, float saturationMultiplier)
    {
        float H, S, V;

        Color.RGBToHSV(col, out H, out S, out V);

        S *= saturationMultiplier;

        return Color.HSVToRGB(H, S, V);
    }

    public static Color MultiplyValue(Color col, float valueMultiplierBG, float valueMax)
    {
        float H, S, V;

        Color.RGBToHSV(col, out H, out S, out V);

        V = Mathf.Min(V * valueMultiplierBG, valueMax);

        return Color.HSVToRGB(H, S, V);
    }
}
