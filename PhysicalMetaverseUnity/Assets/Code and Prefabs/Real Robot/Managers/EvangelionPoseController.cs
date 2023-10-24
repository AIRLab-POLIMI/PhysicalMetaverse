using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvangelionPoseController : MonoBehaviour
{
    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;
    
    [SerializeField] private FloatSO LookingAt;

    [SerializeField] private FloatSO QtyOfMovement;
    
    [SerializeField] private FloatSO distanceFromCenter;

    [SerializeField] private GameObject _pointPrefab;

    [SerializeField] private float timeMultiplier = 1f;

    private GameObject[] _points;
    
    private int arraySize = 64;

    private float startY;

    private Vector3[] initialPositions;

    //TESTING
    private float amplitude = 0f;
    private float frequency = 0f;
    private float speed = 1f;
    [SerializeField] private float offset = 0f;
    private float targetOffset;
    private float swarmDimension = 1f;
    private float colorSlider = 0f;

    private Color color1 = Color.gray;
    private Color color3 = Color.magenta;
    private Color color2 = new Color(255, 165, 0);
    [SerializeField] private Transform _odileViz;
    private void Start()
    {
        _points = new GameObject[arraySize];
        SpawnPoints();
    }

    private void SpawnPoints()
    {
        float radius = 50f;
        
        
        
        startY = this.transform.position.y;

        for (int i = 0; i < arraySize; ++i)
        {
            float circleposition = ((float)i / (float)360) - (4.0f/45.0f);
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;
            z = this.transform.position.z;
            GameObject obj = Instantiate(_pointPrefab, new Vector3(x, startY, z), Quaternion.identity);
            //GameObject obj = Instantiate(lidarPoint, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.LookRotation(new Vector3(x, 0.0f, z)));
            obj.transform.parent = transform;
            obj.GetComponent<Renderer>().material.color = color1;
            //obj.transform.position += obj.transform.forward*8.0f;
            _points[i] = obj;
            
        }
        
    }

    [SerializeField] private float _scaledTime = 0f;
    private void Update()
    {
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
        UpdateValues();
        int poseMidpoint = -1;
        
        if(MinConvertedAngle.runtimeValue != -1)
            poseMidpoint = (MaxConvertedAngle.runtimeValue + MinConvertedAngle.runtimeValue) / 2;


        if (poseMidpoint == -1)
        {
            targetOffset = 0f;
            colorSlider = 0.0f;
        }
        else
        {
            targetOffset = GetDistanceRatio(poseMidpoint, 32, 32)/2;
            colorSlider = LookingAt.runtimeValue;
        }

        offset = Mathf.Lerp(offset, targetOffset, Time.deltaTime / 2);
        

        float targetAmplitude = 20f - distanceFromCenter.runtimeValue;
        
        
        
        amplitude = Mathf.Lerp(amplitude, targetAmplitude, Time.deltaTime * speed);
        
        swarmDimension = QtyOfMovement.runtimeValue * 5;



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
            //change time with timeMultiplier
            _scaledTime = _scaledTime + Time.deltaTime / 100f * timeMultiplier;
            _points[i].transform.position = new Vector3(_points[i].transform.position.x, startY + amplitude * Mathf.Sin( _scaledTime + i * offset), _points[i].transform.position.z);
            _points[i].transform.localScale = new Vector3(1, 1 + swarmDimension, 1);
            _points[i].GetComponent<Renderer>().material.color = LerpColor(color1, color3, colorSlider);

        }
    }
    [SerializeField] private float _distanceFromCenterMultiplier = 1f;
    //lookingatmultiplier
    [SerializeField] private float _lookingAtMultiplier = 1f;
    [SerializeField] private float _odileLookAngle;
    //angle from 0 to odile
    [SerializeField] private float _odileAngle;
    private Vector3 _leftHandTrackerPrevPosition = Vector3.zero;
    private Vector3 _rightHandTrackerPrevPosition = Vector3.zero;
    [SerializeField] private float _leftHandSpeed = 0f;
    [SerializeField] private float _rightHandSpeed = 0f;
    //variables to have a sliding window of 10 measurements on lefthand tracker position
    //list of 10 delta measurements
    private List<float> _leftHandDeltaMeasurements = new List<float>();
    private List<float> _rightHandDeltaMeasurements = new List<float>();
    [SerializeField] private float _mesurementDeltaTime = 0.06f;
    //_measurementsTaken
    private int _measurementsTaken = 0;
    private float _prevTime = 0f;
    private float _leftHandDelta = 0f;
    private float _rightHandDelta = 0f;
    public float _realDeltaTime = 0f;
    private float _deltaTime = 0f;
    private int _totalMeasurements = 6;
    //serialize _qtyOfMovementSensitivity
    [SerializeField] private float _qtyOfMovementSensitivity = 1f;
    //serialize new quantity of movement
    [SerializeField] private float _newQtyOfMovement = 0f;
    //serialize _qtyOfMovement threshold
    [SerializeField] private float _qtyOfMovementThreshold = 2f;
    //_qtyOfMovementLerp
    [SerializeField] private float _qtyOfMovementLerp = 0.1f;
    
    private void UpdateValues(){
        RobotPoseContoller robotPoseContoller = _odileViz.GetComponent<RobotPoseContoller>();
        // get distanceFromCenter.runtimeValue from odileviz x position abs
        distanceFromCenter.runtimeValue = Mathf.Abs(_odileViz.position.x) * _distanceFromCenterMultiplier;
        //LookingAt 
        //_odileLookAngle from _odileViz GetLookAngle
        _odileLookAngle = robotPoseContoller.GetLookAngle();
        _odileLookAngle -= 270f;
        //angle of vector from 0 to odile position
        _odileAngle = Vector3.Angle(Vector3.right, _odileViz.position);
        _odileAngle -= 90f;
        LookingAt.runtimeValue = 1f - Mathf.Abs(_odileLookAngle) / 180f * _lookingAtMultiplier;

        //left and right handtrackers
        //left handtracker
        Vector3 leftHandTracker = robotPoseContoller.GetLeftHandTrackerLocalPosition();
        //right handtracker
        Vector3 rightHandTracker = robotPoseContoller.GetRightHandTrackerLocalPosition();
        /*
        //left hand speed, use deltatime
        _leftHandSpeed = Vector3.Distance(_leftHandTrackerPrevPosition, leftHandTracker) / Time.deltaTime; //TODO sum distances in intervals of one second so speed is actually measurable
        //right hand speed, use deltatime
        _rightHandSpeed = Vector3.Distance(_rightHandTrackerPrevPosition, rightHandTracker) / Time.deltaTime;
        _leftHandTrackerPrevPosition = leftHandTracker;
        _rightHandTrackerPrevPosition = rightHandTracker;
        */
        _deltaTime = Time.time - _prevTime;
        //take a measurement every _mesurementDeltaTime
        if(_deltaTime > _mesurementDeltaTime){
            _realDeltaTime = _deltaTime;
            //module of Vector3.Distance(_leftHandTrackerPrevPosition, leftHandTracker)
            float delta = Vector3.Distance(_leftHandTrackerPrevPosition, leftHandTracker);
            _leftHandTrackerPrevPosition = leftHandTracker;
            //add delta to list
            _leftHandDeltaMeasurements.Add(delta);
            _leftHandDelta += delta;

            delta = Vector3.Distance(_rightHandTrackerPrevPosition, rightHandTracker);
            _rightHandTrackerPrevPosition = rightHandTracker;
            //add delta to list
            _rightHandDeltaMeasurements.Add(delta);
            _rightHandDelta += delta;

            //increment _measurementsTaken
            _measurementsTaken++;
            //if _measurementsTaken is 10, remove first element
            if(_measurementsTaken == _totalMeasurements){
                //measure speed
                _leftHandSpeed = _leftHandDelta / (_mesurementDeltaTime * _totalMeasurements);
                _leftHandDelta -= _leftHandDeltaMeasurements[0];
                _leftHandDeltaMeasurements.RemoveAt(0);
                _rightHandSpeed = _rightHandDelta / (_mesurementDeltaTime * _totalMeasurements);
                _rightHandDelta -= _rightHandDeltaMeasurements[0];
                _rightHandDeltaMeasurements.RemoveAt(0);
                _measurementsTaken -= 1;
                //QtyOfMovement.runtimeValue = _leftHandSpeed + _rightHandSpeed;
                _newQtyOfMovement = _leftHandSpeed + _rightHandSpeed;
                //if less than _qtyOfMovementThreshold set to
                if(_newQtyOfMovement < _qtyOfMovementThreshold){
                    //lerp _newQtyOfMovement to _qtyOfMovementThreshold
                    _newQtyOfMovement = Mathf.Lerp(_newQtyOfMovement, _qtyOfMovementThreshold, _qtyOfMovementLerp);
                }
                //timeMultiplier = _newQtyOfMovement * _qtyOfMovementSensitivity;
                //lerp
                timeMultiplier = Mathf.Lerp(timeMultiplier, _newQtyOfMovement * _qtyOfMovementSensitivity, _qtyOfMovementLerp);
            }
            //reset _prevTime
            _prevTime = Time.time;
        }

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
