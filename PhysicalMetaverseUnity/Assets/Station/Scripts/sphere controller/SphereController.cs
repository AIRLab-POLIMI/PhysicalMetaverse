
using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class SphereController : Monosingleton<SphereController>
{
    [Space]
    [Header("References")] 
        [SerializeField] private SphereMeshController sphereMeshController;
        [SerializeField] private InputActionProperty controllerInput;
        
    [Space]
    [Header("Sphere Parameters")]
    
        [Range(0, 1)]
        [SerializeField] float maxScale;
        [Range(0, 1)]
        [SerializeField] float minScale;
        [SerializeField] private float minBrightness;
        [SerializeField] private float maxBrightness;    
        
        [Tooltip("input values below this threshold set scale and brightness to 0. Until that values, they reach the MIN value")]
        [Range(0, 1)]
        [SerializeField] private float minThreshold;


    private float _curInput;
    
    private void Start()
    {
        OnGameStarted();
    }

    private void OnEnable()
    {
        controllerInput.action.Enable();
    }
    
    private void OnDisable()
    {
        controllerInput.action.Disable();
    }

    public void OnGameStarted()
    {
        sphereMeshController.gameObject.SetActive(true);
        sphereMeshController.Init(minScale, minBrightness);
    }
     
    private void Update()
    {
        OnNewInputRcv(controllerInput.action.ReadValue<float>());
    }
    
    
    public void OnNewInputRcv(float newInputVal)
    {
        // get the input from the controller
        _curInput = newInputVal;
        
        // update the sphere size and emission
        SetSphere();
    }

    private void SetSphere()
    {
        // set the sphere EMISSION and SIZE based on the INPUT, in range [0, 1]
        var belowThresh = _curInput < minThreshold;

        // scale is 0 if input < minThresh, otherwise in range [minScale, 1]
        // var newScale = belowThresh
        //     ? 0
        //     : MapRange(_curInput, 0, 1, minScale, maxScale);
        // // brightness is 0 if input < minThresh, otherwise in range [minBrightness, maxBrightness]
        // var newBrightness = belowThresh
        //     ? 0
        //     : MapRange(_curInput, 0, 1, minBrightness, maxBrightness);
        
        var newScale = MapRange(_curInput, 0, 1, minScale, maxScale);
        // brightness is 0 if input < minThresh, otherwise in range [minBrightness, maxBrightness]
        var newBrightness = MapRange(_curInput, 0, 1, minBrightness, maxBrightness);
        
        sphereMeshController.OnInputChanged(newBrightness, newScale);
    }
    
    private static float MapRange(float x, float minIn, float maxIn, float minOut, float maxOut) =>
        minOut + ((maxOut - minOut) / (maxIn - minIn)) * (x - minIn);
}
