
using UnityEngine;


public static class TrigonometryHelper
{
    public static Vector3 RotateVector3ByEuler(Vector3 originalVector, float xRot = 0, float yRot = 0, float zRot = 0)
    {
        Quaternion rotation = Quaternion.Euler(xRot,yRot,zRot);
        return rotation * originalVector;
    }

    public static float QuaternionDifference(Quaternion a, Quaternion b)
    {
        float difX = SquaredDiff(a.x, b.x);
        float difY = SquaredDiff(a.y, b.y);
        float difZ = SquaredDiff(a.z, b.z);
        float difW = SquaredDiff(a.w, b.w);

        return Mathf.Sqrt(difX + difY + difZ + difW);
    }
    
    public static float SquaredDiff(float a, float b) => Mathf.Pow(a - b, 2);
}
