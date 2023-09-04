using System;
using UnityEngine;

public static class LidarToCameraRange
{
    private static float rDistance = 250f; //In centimeters
    private static float cameraDegrees = 64f; //In degrees

    
    //PUT POSITIVE DISTANCE IF THE CAMERA IS IN THE FRONT AND THE LIDAR IN THE BACK, OTHERWISE THE OPPOSITE
    public static int LidarDegreesBasedOnDistance(float distanceLidarCamera)
    {
        float degrees = (rDistance * cameraDegrees) / (rDistance + distanceLidarCamera);
        return (int)Math.Round(degrees, 0);
    }
}
