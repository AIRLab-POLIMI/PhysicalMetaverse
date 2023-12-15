using System;
using UnityEngine;

public class AmbientManager : Monosingleton<AmbientManager>
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light directionalLightShadow;
    [SerializeField] private LightningPreset lightningPreset;
    [SerializeField] private TMPro.TextMeshProUGUI _timeTextMesh;

    private float _rotationSpeed;
    private float _previousNormalizedValue = 0.0f;
    private Quaternion _initialDirectionalLightRotation;
    private Quaternion _initialDirectionalLightShadowRotation;

    private void Start()
    {
        _initialDirectionalLightRotation = directionalLight.transform.localRotation;
        _initialDirectionalLightShadowRotation = directionalLightShadow.transform.localRotation;

        //SetInitialLightRotation();
        CalculateRotationSpeed();
    }

    private void SetInitialLightRotation()
    {
        Vector3 rotation = directionalLight.transform.localRotation.eulerAngles;
        Quaternion newRotation = Quaternion.Euler(lightningPreset.dayTimeDirectionalAngle, rotation.y, rotation.z);

        directionalLight.transform.localRotation = newRotation;
        directionalLightShadow.transform.localRotation = newRotation;
    }

    private void CalculateRotationSpeed()
    {
        float rotationRange = Mathf.Abs(lightningPreset.nightTimeDirectionalAngle - lightningPreset.dayTimeDirectionalAngle);
        _rotationSpeed = rotationRange / 1.0f; // Assuming 1.0f represents a full range
    }

    public void Restart()
    {
        directionalLight.transform.localRotation = _initialDirectionalLightRotation;
        directionalLightShadow.transform.localRotation = _initialDirectionalLightShadowRotation;
    }

    private void RotateLight(float currentNormalizedValue)
    {
        float rotationStep = (currentNormalizedValue - _previousNormalizedValue) * _rotationSpeed;
        directionalLight.transform.Rotate(rotationStep, 0, 0);
        directionalLightShadow.transform.Rotate(rotationStep, 0, 0);

        //directionalLight.transform.eulerAngles = new Vector3(
        //    directionalLight.transform.eulerAngles.x,
        //    directionalLightShadow.transform.eulerAngles.y,
        //    directionalLight.transform.eulerAngles.z);

        _previousNormalizedValue = currentNormalizedValue;
        _timeTextMesh.text = rotationStep.ToString();
    }

    public void UpdateLight(float normalisedTime)
    {
        normalisedTime = Mathf.Clamp01(normalisedTime);
        var evalPoint = lightningPreset.changeCurve.Evaluate(normalisedTime);
        var newIntensity = lightningPreset.directionalIntensity.Evaluate(evalPoint);

        RenderSettings.ambientLight = lightningPreset.ambientColor.Evaluate(evalPoint);
        RenderSettings.ambientIntensity = newIntensity;
        directionalLight.color = lightningPreset.directionalColor.Evaluate(evalPoint);

        RotateLight(evalPoint);
    }
}
