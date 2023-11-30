
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


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
        //_blinkIntensity
        [Range(0, 5)]
        [SerializeField] private float _blinkIntensity;
        //_blinkfrequency
        [Range(0, 100)]
        [SerializeField] private float _blinkFrequency;
        //_blinkScale
        [Range(1, 4)]
        [SerializeField] private float _blinkScale;
        //scaleScale
        [Range(1, 3)]
        [SerializeField] private float _scaleScale;
        //blink sphere bool
        [SerializeField] private bool _blinkSphere;
        //_blinkTime
        [Range(0, 5)]
        [SerializeField] private float _blinkTime = 2f;


    private float _curInput;
    
    private void Start()
    {
        //get _rightHandController from father
        _rightHandController = GetComponentInParent<XRBaseController>();
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
        //if _blinkSphere is true, blink the sphere
        if (_blinkSphere)
        {
            BlinkSphere();
            _blinkSphere = false;
        }
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

    private bool _blinking;

    //blink sphere public coroutine, use a sinusoid to blink the sphere between min and max values
    public void BlinkSphere()
    {
        _blinking = true;
        StartCoroutine(BlinkSphereCoroutine(_blinkTime));
    }

    [SerializeField] private GameObject _qRInvalidationAreaVeryClose;
    private System.Collections.IEnumerator BlinkSphereCoroutine(float duration)
    {
        float t = 0;
        //disable _qRInvalidationAreaVeryClose
        try
        {
            _qRInvalidationAreaVeryClose.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log("No qr invalidation area to disable: " + e);
        }
        while (t < duration && _blinking)
        {
            t += Time.deltaTime;
            var newScale = Mathf.Lerp(minScale, _scaleScale * maxScale, _blinkIntensity*Mathf.Sin(t * _blinkFrequency));
            var newBrightness = Mathf.Lerp(minBrightness, _blinkScale * maxBrightness, _blinkIntensity*Mathf.Sin(t * _blinkFrequency));
            sphereMeshController.OnInputChanged(newBrightness, newScale);
            yield return null;
            //vibrate _rightHandController
            _rightHandController.SendHapticImpulse(0.5f, 0.1f);
        }
        //enable _qRInvalidationAreaVeryClose
        _qRInvalidationAreaVeryClose.SetActive(true);
    }

    public void StopBlink()
    {
        //set sphere to zero
        SetSphere();
        _blinking = false;
    }
    
    public XRBaseController _rightHandController;
    private static float MapRange(float x, float minIn, float maxIn, float minOut, float maxOut) =>
        minOut + ((maxOut - minOut) / (maxIn - minIn)) * (x - minIn);
}
