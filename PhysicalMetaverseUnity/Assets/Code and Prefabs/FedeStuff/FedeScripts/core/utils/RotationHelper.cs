
using UnityEngine;


public static class RotationHelper
{
    public static float ClampAngle(float lfAngle, float lfMin = float.MinValue, float lfMax = float.MaxValue)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    //function that uses unity methods to subtract 180 from all angles and returns new vector3;
    public static Vector3 SubtractAllAngles(Vector3 angles, float subtractAngle)
    {
        return new Vector3(
            Mathf.DeltaAngle(angles.x, subtractAngle), 
            Mathf.DeltaAngle(angles.y, subtractAngle), 
            Mathf.DeltaAngle(angles.z, subtractAngle));
    }

}
