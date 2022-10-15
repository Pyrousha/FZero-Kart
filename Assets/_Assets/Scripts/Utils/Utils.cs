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
}
