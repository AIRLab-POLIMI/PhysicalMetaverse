using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvangelionPoseController : VizController
{
    private PoseManager _poseManager;
    //bool hide
    [SerializeField] private bool _HIDE = false;
    //_scale
    [SerializeField] private float _scale = 1f;
    private bool _hideStatus = true;
    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;
    
    [SerializeField] private FloatSO LookingAt;
    [SerializeField] private float _quantityOfMovement;
    [SerializeField] private float _quantityOfMovementMultiplier = 0.5f;
    [SerializeField] private float _maxQuantityOfMovement = 3.5f;
    [SerializeField] private float _unDetectedTimeMultiplier = 0.1f;
    
    [SerializeField] private FloatSO distanceFromCenter;

    [SerializeField] private GameObject _pointPrefab;
    //frequency multiplier
    [SerializeField] private float _frequencyMultiplier = 1f;

    private GameObject[] _points;
    
    private int arraySize = 64;

    private float startY;

    //TESTING
    [SerializeField] private float amplitude = 0f;
    public float _lerpSpeed = 1f;
    [SerializeField] private float offset = 0f;
    [SerializeField] private float targetOffset;
    [SerializeField] private float swarmDimension = 1f;
    //serialize _neutralDistance
    [SerializeField] private float _neutralDistance = 0.8f;
    private float colorSlider = 0f;

    private Color color1 = Color.gray;
    private Color color3 = Color.magenta;
    private Color color2 = new Color(255, 165, 0);
    private void Start()
    {
        _poseManager = PoseManager.Instance;
        _points = new GameObject[arraySize];
        SpawnPoints();
    }

    private void SpawnPoints()
    {
        float radius = 50f;
        
        
        
        startY = this.transform.localPosition.y;

        for (int i = 0; i < arraySize; ++i)
        {
            float circleposition = ((float)i / (float)360) - (4.0f/45.0f);
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius *2f;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;
            z = this.transform.position.z;
            //scale
            x *= _scale;
            amplitude *= _scale;
            GameObject obj = Instantiate(_pointPrefab, new Vector3(x, startY, z), Quaternion.identity);
            //GameObject obj = Instantiate(lidarPoint, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.LookRotation(new Vector3(x, 0.0f, z)));
            obj.transform.parent = transform;
            obj.GetComponent<Renderer>().material.color = color1;
            //scale point to _scale
            obj.transform.localScale = new Vector3(_scale, _scale, _scale);
            //obj.transform.position += obj.transform.forward*8.0f;
            _points[i] = obj;
            
        }
        
    }

    public void Hide(bool hide)
    {
        foreach (GameObject point in _points)
        {
            //hide mesh
            point.GetComponent<MeshRenderer>().enabled = !hide;
        }
        _hideStatus = !_hideStatus;
        _HIDE = false;
    }

    public override void SetHide(bool setHide)
    {
        Hide(setHide);
    }
    [SerializeField] private int poseMidpoint = 0;
    //time
    [SerializeField] private float _time = 0f;
    //_prevPersonDetectedTime
    public float _prevPersonDetectedTime = 0f;
    //_poseDecayTime
    [SerializeField] private float _poseDecayTime = 1f;
    private void Update()
    {
        _distanceFromCenterAngle = Vector3.Angle(new Vector3(_cameraTransform.forward.x, 0f, _cameraTransform.forward.z), new Vector3(-_poseManager.GetRotoTraslation().position.x, 0f, -_poseManager.GetRotoTraslation().position.z));
        //normalize
        _distanceFromCenterAngleNormalized = ((180f*180f) - _distanceFromCenterAngle*_distanceFromCenterAngle)/ (90f*90f);
        //normalized max 1
        _distanceFromCenterAngleNormalized = Mathf.Clamp01(_distanceFromCenterAngleNormalized);
        //LookingAt 
        //_odileLookAngle from _odileViz GetLookAngle
        _odileLookAngle = _poseManager.GetHeadAngleY();
        _odileLookAngle -= 270f;
        //_distanceFromCamera = _poseManager.GetDistanceFromCamera();
        if(_poseManager.GetPersonDetected()){
            _prevPersonDetectedTime = Time.time;
        }
        //min 1
        _distanceFromCamera = Mathf.Max(_poseManager.GetDistanceFromCamera() / _distanceFromCameraMultiplier, _minDistanceFromCamera);
        if(Time.time - _prevPersonDetectedTime < _poseDecayTime){
            _quantityOfMovement = _poseManager.GetQuantityOfMovement() * _quantityOfMovementMultiplier;
            //max = _maxQuantityOfMovement * _quantityOfMovementMultiplier
            _quantityOfMovement = Mathf.Min(_quantityOfMovement, _maxQuantityOfMovement * _quantityOfMovementMultiplier);
            if(_HIDE){
                Hide(_hideStatus);
            }
            //PARAMETERS:
            //-DistanceFromCenter 
            //-QtyOfMovement-> 0 to 0.3 circa
            //-LookingAt-> 0 to 1
            //-Pose Midpoint -> 0 to max converted angle (circa 60)
            /*
            float amplitude = 20f - (1f * distanceFromCenter.runtimeValue);    // the amplitude of the sine wave
            float frequency = 1f + 5f * (QtyOfMovement.runtimeValue);    // the frequency of the sine wave
            float speed = 1f + (QtyOfMovement.runtimeValue * 10f);        // the speed at which the object moves up and down
            float lookingAtElaborated = 1 - Math.Abs(LookingAt.runtimeValue - 0.5f);
            */
            //change time with timeMultiplier
            //_scaledTime = _poseManager.GetScaledTime() * _timeMultiplier;
            _time = _time + Time.deltaTime * _quantityOfMovement;
            UpdateValues();
        }
        else{
            _time = _time + Time.deltaTime * _unDetectedTimeMultiplier;
            ResetValues();

        }
        swarmDimension = Mathf.Lerp(swarmDimension, 1f / _distanceFromCamera + 0.5f, Time.deltaTime * _lerpSpeed);
        
        
        //colorSlider = LookingAt.runtimeValue;
        colorSlider = 1-_distanceFromCenterAngleNormalized;
        //if _poseManager person detected is false set lookingat to 0
        ////if(!_poseManager.GetPersonDetected())
            ////colorSlider = 0f;

        offset = Mathf.Lerp(offset, targetOffset, Time.deltaTime / 2);
        
        //float targetAmplitude = _baseAmplitude - distanceFromCenter.runtimeValue;colorSlider
        float targetAmplitude = _baseAmplitude - colorSlider;

        targetAmplitude *= _scale;
        
        
        
        amplitude = Mathf.Lerp(amplitude, targetAmplitude, Time.deltaTime * _lerpSpeed);
        
        
        if(!_SPEED_MODE)
            swarmDimension = _quantityOfMovement;

        poseMidpoint = (MaxConvertedAngle.runtimeValue + MinConvertedAngle.runtimeValue) / 2;
        targetOffset = GetDistanceRatio(poseMidpoint, 32, 32)/2;    
        for (int i = 0; i < arraySize; i++)
        {
            /*
            if (poseMidpoint != -1)
            {
                _points[i].transform.localScale = new Vector3(1, 0.5f + 8 * Mathf.Pow(GetDistanceRatio(i, poseMidpoint, 63), 10f) * (5 * lookingAtElaborated), 1);
            }
            else
            {
                _points[i].transform.localScale = new Vector3(1, 1, 1);
            }
            */
            /*
            float y = startY + amplitude * Mathf.Sin((Time.time*timeMultiplier)+((float)i/10)*2);

            _points[i].transform.position =
                new Vector3(_points[i].transform.position.x, y, _points[i].transform.position.z);
                */
            _points[i].transform.localPosition = new Vector3(_points[i].transform.localPosition.x, startY + amplitude * Mathf.Sin( _frequencyMultiplier * (_time + i * offset)), _points[i].transform.localPosition.z);
            _points[i].transform.localScale = new Vector3(_scale*2f, swarmDimension * _scale, _scale);
            _points[i].GetComponent<Renderer>().material.color = LerpColor(color1, color3, colorSlider);
        }

    }
    [SerializeField] private float _distanceFromCenterMultiplier = 1f;
    [SerializeField] private float _distanceFromCenterSpeed = 1f;
    //camera transform
    [SerializeField] private Transform _cameraTransform;
    //serialize _baseAmplitude
    [SerializeField] private float _baseAmplitude = 20f;
    //lookingatmultiplier
    [SerializeField] private float _lookingAtMultiplier = 1f;
    [SerializeField] private float _odileLookAngle;
    //angle from 0 to odile
    [SerializeField] private float _odileAngle;
    //serialize _SPEED_MODE
    [SerializeField] private bool _SPEED_MODE = false;

    //thickness mode
    //sensitivity 0.13
    //threshold 0.71
    
    public float _distanceFromCenterAngle = 0f; //zero is 180 degree * _distanceFromCenterMultiplier
    public float _distanceFromCenterAngleNormalized = 0f;
    private void UpdateValues(){
        //distanceFromCenter = angle between forward and rototrasaltion position
        //distanceFromCenter.runtimeValue = Vector3.Angle(Vector3.forward, _poseManager.GetRotoTraslation().position) * _distanceFromCenterMultiplier;
        //_distanceFromCenterAngle = only y angle between _cameraTransform.forward and -_poseManager.GetRotoTraslation().position
        //project both on xz plane
        _distanceFromCenterAngle = Vector3.Angle(new Vector3(_cameraTransform.forward.x, 0f, _cameraTransform.forward.z), new Vector3(-_poseManager.GetRotoTraslation().position.x, 0f, -_poseManager.GetRotoTraslation().position.z));

        //angle of vector from 0 to odile position
        _odileAngle = Vector3.Angle(Vector3.right, _poseManager.GetRotoTraslation().position);
        _odileAngle -= 90f;
        LookingAt.runtimeValue = 1f - Mathf.Abs(_odileLookAngle) / 180f * _lookingAtMultiplier;

        //swarmDimension = 1f / Mathf.Pow(robotPoseContoller.GetFilteredDistance() + (1f - _neutralDistance), 3f);
        //lerp
        //swarmDimension = Mathf.Lerp(swarmDimension, 1f / Mathf.Pow(_distanceFromCamera, 4f) + 0.5f, Time.deltaTime * speed);
        

    }
    public float _distanceFromCamera;
    //distance from camera multiplier
    [SerializeField] private float _distanceFromCameraMultiplier = 0.01f;
    public float _minDistanceFromCamera = 0.3f;
    private void ResetValues(){
            //_quantityOfMovement = 0f;
            //lerp
            _quantityOfMovement = Mathf.Lerp(_quantityOfMovement, 0f, Time.deltaTime * _lerpSpeed);
            //distance from center = 20
            //distanceFromCenter.runtimeValue = 20f;
            //lerp
            distanceFromCenter.runtimeValue = Mathf.Lerp(distanceFromCenter.runtimeValue, 90 * _distanceFromCenterMultiplier, Time.deltaTime * _distanceFromCenterSpeed);
            //lookingat = 0
            //LookingAt.runtimeValue = 0f;
            //lerp
            LookingAt.runtimeValue = Mathf.Lerp(LookingAt.runtimeValue, 0f, Time.deltaTime * _lerpSpeed);
    }

    //fire hide button
    public void FireHideButton(){
        _HIDE = true;
        swarmDimension = 0f;
    }

    
    public static Color LerpColor(Color color1, Color color2, float t)
    {
        t = Mathf.Clamp01(t); // ensure t is within the range of 0 to 1
        return Color.Lerp(color1, color2, t);
    }


    private float GetDistanceRatio(int value1, int value2, int maxDistance)
    {
        float distance = Math.Abs(value1 - value2);

        return 1f - (distance / maxDistance);
    }
}
