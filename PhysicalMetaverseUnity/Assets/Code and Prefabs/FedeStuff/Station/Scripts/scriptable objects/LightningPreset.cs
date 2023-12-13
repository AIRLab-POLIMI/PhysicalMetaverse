
using UnityEngine;


[CreateAssetMenu(fileName = "LightningPreset", menuName = "ScriptableObjects/LightningPreset", order = 1)]
public class LightningPreset : ScriptableObject
{
    public Gradient directionalColor;
    public Gradient ambientColor;
    
    public AnimationCurve changeCurve;
    
    public AnimationCurve directionalIntensity;

    public float dayTimeDirectionalAngle;
    public float nightTimeDirectionalAngle;
}
