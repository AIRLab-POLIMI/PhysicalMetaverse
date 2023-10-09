
using System;
using UnityEngine;


public class AmbientManager : Monosingleton<AmbientManager>
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light directionalLightShadow;
    [SerializeField] private LightningPreset lightningPreset;

    [SerializeField] private Color a;

    private float _rotationSpeed; 
    private float _previousNormalizedValue = 0.0f;
    
    private void Start()
    {
        Vector3 rotation = directionalLight.transform.localRotation.eulerAngles;
        directionalLight.transform.localRotation = Quaternion.Euler(
            lightningPreset.dayTimeDirectionalAngle,
            rotation.y,
            rotation.z);
        directionalLightShadow.transform.localRotation = Quaternion.Euler(
            lightningPreset.dayTimeDirectionalAngle,
            rotation.y,
            rotation.z);
        directionalLight.transform.localRotation = Quaternion.Euler(
            lightningPreset.dayTimeDirectionalAngle,
            directionalLightShadow.transform.localRotation.y,
            rotation.z);
        
        float rotationRange = Mathf.Abs(lightningPreset.nightTimeDirectionalAngle - lightningPreset.dayTimeDirectionalAngle);
        _rotationSpeed = rotationRange / 1.0f; // Assuming 1.0f represents a full range

        _previousNormalizedValue = 0.0f;
    }

    private void RotateLight(float currentNormalizedValue)
    {
        // Calculate the rotation step based on the difference between current and previous normalizedValue
        float rotationStep = (currentNormalizedValue - _previousNormalizedValue) * _rotationSpeed;

        // Rotate the object around the X-axis
        directionalLight.transform.Rotate(rotationStep, 0, 0);
        directionalLightShadow.transform.Rotate(rotationStep, 0, 0);
        //log y angle of directionalLightShadow
        //Debug.Log("Directional light shadow y angle: " + directionalLightShadow.transform.eulerAngles.y);
        //set y eulerangles of directional light to y eulerangles of directional light shadow
        directionalLight.transform.eulerAngles = new Vector3(directionalLight.transform.eulerAngles.x, directionalLightShadow.transform.eulerAngles.y, directionalLight.transform.eulerAngles.z);


        // Update the previousNormalizedValue for the next frame
        _previousNormalizedValue = currentNormalizedValue;
    }
    
    public void UpdateLight(float normalisedTime)
    {   
        normalisedTime = Mathf.Clamp(normalisedTime, 0, 1);

        var evalPoint = Mathf.Clamp01(lightningPreset.changeCurve.Evaluate(normalisedTime));
        var newIntensity = Mathf.Clamp01(lightningPreset.directionalIntensity.Evaluate(evalPoint));
        
        // a = lightningPreset.directionalColor.Evaluate(evalPoint);
        RenderSettings.ambientLight = lightningPreset.ambientColor.Evaluate(evalPoint);
        RenderSettings.ambientIntensity = newIntensity;

        directionalLight.color = lightningPreset.directionalColor.Evaluate(evalPoint);
        
        RotateLight(evalPoint);
        
        // change the intensity of the directional light from 0 to 1 when normalised time is between 0 and 1 respectively. Use Lerp method
        //directionalLight.intensity = newIntensity;
    }
}
