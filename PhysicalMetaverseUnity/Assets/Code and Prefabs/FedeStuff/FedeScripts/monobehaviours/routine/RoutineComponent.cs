
using UnityEngine;


[System.Serializable]
public class RoutineComponent
{
    [SerializeField] private string dofName;
    
    [SerializeField] private int numCycles;
    [SerializeField] private float _defaultPosition;

    [SerializeField, Range(-1, 1)] private float movementCenter;
    [SerializeField, Range(0, 1)] private float movementAmplitude;

    [SerializeField] private float minMotorValue;
    [SerializeField] private float maxMotorValue;
    
    [SerializeField] private string motorKey;

    // Calculate the angular frequency based on the desired number of cycles and total time
    private float _angularFrequency;
    private float _motorRange;
    private float _byteRange;
    
    [Space]
    
    [Range(0, 255)]
    public byte _currentValue;
    
    
    public void Start(float totalDuration)
    {
        _angularFrequency = 2 * Mathf.PI * numCycles / totalDuration;
        _motorRange = maxMotorValue - minMotorValue;
        _byteRange = 255;
    }

    public string GetMsg(float time)
    {
        float value = Mathf.Clamp(
            movementCenter + movementAmplitude * Mathf.Sin(_angularFrequency * time), 
            minMotorValue, maxMotorValue);
        
        // map value from minMotorValue - maxMotorValue to 0 - 255
        _currentValue = (byte) Mathf.RoundToInt((value - minMotorValue) * _byteRange / _motorRange);

        return motorKey + Constants.KeyValDelimiter + _currentValue;
    }

    //get default value msg
    public string GetDefaultMsg()
    {
        return motorKey + Constants.KeyValDelimiter + _defaultPosition;
    }
}
