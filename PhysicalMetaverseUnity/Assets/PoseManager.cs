using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : Monosingleton<PoseManager>
{
    [SerializeField] private PoseReceiver _poseReceiver;
    [SerializeField] private Transform _rotoTraslation;
    //_camera
    [SerializeField] private Transform _camera;
    [SerializeField] private GameObject _pose;
    //serialize field: _quantityOfMovement, _headAngleX, _headAngleY, _poseJoints scriptableobject, _distanceFromCamera
    //dictionary of all joints
    private Dictionary<string, Transform> _poseJoints = new Dictionary<string, Transform>();
    [SerializeField] private float _distanceFromCamera;
    [SerializeField] private float _headAngleX;
    //headTilt dof
    [SerializeField] private DOFController _headTilt;
    [SerializeField] private float _headAngleY;
    //headPan dof
    [SerializeField] private DOFController _headPan;
    [SerializeField] private float _quantityOfMovement;
    [SerializeField] private bool _personDetected = false;

    //
    [SerializeField] private  bool _MANUAL_MOVEMENT = false;

    //vizcontroller list
    [SerializeField] private List<VizController> _vizControllerList;
    //dictionary with name and vizcontroller
    [SerializeField] private Dictionary<string, VizController> _vizControllerDict;
    //current viz string
    [SerializeField] private VizController _currentVizController;


    [ContextMenu("Next Viz")]
    public void NextViz(){
        //find current in _vizControllerList
        int index = _vizControllerList.IndexOf(_currentVizController);
        //increment index
        index++;
        //if index is out of range set to 0
        if(index >= _vizControllerList.Count)
            index = 0;
        //set current viz to index
        _currentVizController = _vizControllerList[index];
        //set all hides in list
        foreach (VizController viz in _vizControllerList)
        {
            //set hide to true
            viz.SetHide(true);
        }
        //set hide to false
        _currentVizController.SetHide(false);
    }
    

    public Dictionary<string, Transform> GetPoseJoints()
    {
        return _poseJoints;
    }

    public float GetDistanceFromCamera()
    {
        //TODO if manual movement return case (no distance sensors, distance is measured directly in unity)
        return _distanceFromCamera;
    }

    public float GetHeadAngleX()
    {
        return _headTilt.GetAngle();
    }

    public float GetHeadAngleY()
    {
        return _headPan.GetAngle();
    }

    public float GetQuantityOfMovement()
    {
        return _quantityOfMovement;
    }

    public bool GetPersonDetected()
    {
        return _personDetected;
    }

    public Transform GetRotoTraslation()
    {
        return _rotoTraslation;
    }

    public bool GetManualMovement()
    {
        return _MANUAL_MOVEMENT;
    }

    // Start is called before the first frame update
    void Start()
    {
        _poseReceiver = PoseReceiver.Instance;
        //_vizNamesList
        _vizControllerDict = new Dictionary<string, VizController>();
        //among children and children of children find viz controllers
        _vizControllerList = new List<VizController>();
        foreach (Transform child in transform)
        {
            //if child has vizcontroller
            if(child.GetComponent<VizController>() != null){
                //add to list
                _vizControllerList.Add(child.GetComponent<VizController>());
                //add vizcontroller name to _vizControllerDict
                _vizControllerDict.Add(child.name, child.GetComponent<VizController>());
            }
            //if child has children
            if(child.childCount > 0){
                //for each child of child
                foreach (Transform childOfChild in child)
                {
                    //if child of child has vizcontroller
                    if(childOfChild.GetComponent<VizController>() != null){
                        //add to list
                        _vizControllerList.Add(childOfChild.GetComponent<VizController>());
                        //add vizcontroller name to _vizControllerDict
                        _vizControllerDict.Add(childOfChild.name, childOfChild.GetComponent<VizController>());
                    }
                }
            }
        }
        NextViz();
        NextViz();
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePoseJoints();
        CalculateDistanceFromCamera();
        CalculateHeadAngles();
        CalculateQuantityOfMovement();
        CalculatePersonDetected();
        CalculateRotoTraslation();
        CalculateParallax();
    }

    private bool _notPopulated = true;
    private void CalculatePoseJoints()
    {
        //if spawned
        if (_notPopulated)
        {
            if (_poseReceiver.GetSpawned()){
                _poseJoints.Clear();
                //print names of all spheres
                foreach (GameObject sphere in _poseReceiver.GetSpheres())
                {
                    //print(sphere.name);
                    _poseJoints.Add(sphere.name, sphere.transform);
                }
                _notPopulated = false;
            }
            else
                return;
        }
    }

    private float _oldZDistance = 0.7f;
    private float _distanceDeltaTolerance = 1f;
    private List<float> _zDistanceList = new List<float>();
    //static _list_length
    private int _list_length = 20;
    private void CalculateDistanceFromCamera()
    {
        float zDistance = _poseReceiver.GetDistance();
        //if zDistance changed more than 100% of oldZDistance use oldZDistance
        if(Mathf.Abs(zDistance - _oldZDistance) > _distanceDeltaTolerance)
            zDistance = _oldZDistance;
        else
            _zDistanceList.Add(zDistance);
            if(_zDistanceList.Count > _list_length)
                _zDistanceList.RemoveAt(0);
                //sort list
                //sorted list
                List<float> sortedList = new List<float>();
                //for each value in _zDistanceList
                foreach (float value in _zDistanceList)
                {
                    //add value to sorted list
                    sortedList.Add(value);
                }
                //sort sorted list
                sortedList.Sort();
                //choose middle value
                if(_zDistanceList.Count > _list_length) //check here if distance broke
                    zDistance = sortedList[_list_length/2];
                //zDistance = min of list
                /*for(int i = 0; i < _zDistanceList.Count; i++){
                    if(_zDistanceList[i] < zDistance)
                        zDistance = _zDistanceList[i];
                }*/
            _oldZDistance = zDistance;

        if(!_personDetected){
            _oldZDistance = 0.6f;
        }

        _distanceFromCamera = _oldZDistance;
    }

    //float nose height
    private float _noseHeight = 0.8f;
    [SerializeField] private float _headXOffset = -45f;
    private void CalculateHeadAngles()
    {
        Quaternion bodyRotation = Quaternion.LookRotation(YDirection(_poseJoints["Left Shoulder"], _poseJoints["Right Shoulder"])) * Quaternion.Euler(0, 0, 0);
        //final angle should be x=0, y=direction, z=-90
        Quaternion leftAngle = Quaternion.LookRotation(YDirection(_poseJoints["Left Ear"], _poseJoints["Left Eye"]));
        Quaternion rightAngle = Quaternion.LookRotation(YDirection(_poseJoints["Right Ear"], _poseJoints["Right Eye"]));
        float avgAngle = (leftAngle.eulerAngles.y + rightAngle.eulerAngles.y) / 2 - bodyRotation.eulerAngles.y;
        _headAngleY = avgAngle;
        _headPan.SetAngle(_headAngleY);
        //set VCamPan to eyeDirection
        //_odileJoints["VCamPan"].localRotation = headPan;
        //Quaternion headTilt = Quaternion.LookRotation(ZDirection(_poseJoints["Left Ear"], _poseJoints["Nose"])) * Quaternion.Euler(90, 0, 0);
        Quaternion leftTilt = Quaternion.LookRotation(XDirection(_poseJoints["Left Ear"], _poseJoints["Nose"]));
        Quaternion rightTilt = Quaternion.LookRotation(XDirection(_poseJoints["Right Ear"], _poseJoints["Nose"]));
        avgAngle = (leftTilt.eulerAngles.x + rightTilt.eulerAngles.x) / 2;
        _noseHeight = _poseJoints["Nose"].localPosition.y;
        //avg angle * tiltzerp / filtered distance
        //avgAngle = avgAngle * _tiltZeroDistance / _oldZDistance;
        //avgAngle = avgAngle + (_noseHeight - _tiltZeroHeight) * _noseHeightMultiplier; //TODO
        //set VCamTilt to headTilt
        _headAngleX = -avgAngle - _headXOffset;
        _headTilt.SetAngle(_headAngleX);
    }
    
    //return the vector ortogonal to two vectors on the yz plane
    Vector3 XDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(0, aPos.y, aPos.z);
        Vector3 bPosXZ = new Vector3(0, bPos.y, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(0, -aToB.z, aToB.y);
        return aToBOrtho;
    }

    [SerializeField] private float _deltaTime = 0f;
    private float _prevTime = 0f;
    [SerializeField] private float _mesurementDeltaTime = 0.06f;
    private Vector3 _leftHandTrackerPrevPosition = Vector3.zero;
    private Vector3 _rightHandTrackerPrevPosition = Vector3.zero;
    //prev horizontal_headHorizontalAngle
    private float _prevHorizontalHeadAngle = 0f;
    //list of delta measurements
    private List<float> _leftHandDeltaMeasurements = new List<float>();
    private List<float> _rightHandDeltaMeasurements = new List<float>();
    
    //list of head horizontal angle delta measurements
    private List<float> _headHorizontalAngleDeltaMeasurements = new List<float>();
    private float _leftHandDelta = 0f;
    private float _rightHandDelta = 0f;
    //head delta
    private float _headDelta = 0f;
    //_measurementsTaken
    private int _measurementsTaken = 0;
    private int _totalMeasurements = 6;
    [SerializeField] private float _leftHandSpeed = 0f;
    [SerializeField] private float _rightHandSpeed = 0f;
    [SerializeField] private float _headSpeed = 0f;
    //_headSpeedSensitivity
    [SerializeField] private float _headSpeedSensitivity = 0.1f;
    //serialize new quantity of movement
    [SerializeField] private float _newQtyOfMovement = 0f;
    //serialize _qtyOfMovement threshold
    [SerializeField] private float _qtyOfMovementThreshold = 1.34f;
    //serialize _qtyOfMovementSensitivity
    [SerializeField] private float _qtyOfMovementSensitivity = 0.5f;
    //_qtyOfMovementLerp
    [SerializeField] private float _qtyOfMovementLerp = 0.1f;

    [SerializeField] private float _timeMultiplier = 1f;
    [SerializeField] private float _scaledTime = 0f;
    //getscaledtime
    public float GetScaledTime(){
        return _scaledTime;
    }
    private void CalculateQuantityOfMovement()
    {
        //left and right handtrackers
        //left handtracker
        Vector3 leftHandTracker = _poseJoints["Left Wrist"].position - _poseJoints["Left Hip"].position;
        //right handtracker
        Vector3 rightHandTracker = _poseJoints["Right Wrist"].position - _poseJoints["Right Hip"].position;
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

            //head horizontal angle
            float headHorizontalAngle = _headPan.GetAngle();
            //delta of head horizontal angle
            delta = Mathf.Abs(headHorizontalAngle - _prevHorizontalHeadAngle);
            _prevHorizontalHeadAngle = headHorizontalAngle;
            //add delta to list
            _headHorizontalAngleDeltaMeasurements.Add(delta);
            _headDelta += delta;


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
                //head speed
                _headSpeed = _headDelta / (_mesurementDeltaTime * _totalMeasurements) * _headSpeedSensitivity;
                _headDelta -= _headHorizontalAngleDeltaMeasurements[0];
                _headHorizontalAngleDeltaMeasurements.RemoveAt(0);
                _measurementsTaken -= 1;
                //QtyOfMovement.runtimeValue = _leftHandSpeed + _rightHandSpeed;
                _newQtyOfMovement = _leftHandSpeed + _rightHandSpeed + _headSpeed;
                //if less than _qtyOfMovementThreshold set to
                if(_newQtyOfMovement < _qtyOfMovementThreshold){
                    //lerp _newQtyOfMovement to _qtyOfMovementThreshold
                    _newQtyOfMovement = Mathf.Lerp(_newQtyOfMovement, _qtyOfMovementThreshold, _qtyOfMovementLerp);
                    _quantityOfMovement = Mathf.Lerp(_quantityOfMovement, _qtyOfMovementThreshold, _qtyOfMovementLerp);
                }
                //timeMultiplier = _newQtyOfMovement * _qtyOfMovementSensitivity;
                //lerp
                _timeMultiplier = Mathf.Lerp(_timeMultiplier, _newQtyOfMovement * _qtyOfMovementSensitivity, _qtyOfMovementLerp);
                _timeMultiplier /= 1.3f;
                //lerp QtyOfMovement.runtimeValue
                _quantityOfMovement = Mathf.Lerp(_quantityOfMovement, _newQtyOfMovement * _qtyOfMovementSensitivity, _qtyOfMovementLerp);
            }
            //reset _prevTime
            _prevTime = Time.time;
        }
        _scaledTime = _scaledTime + Time.deltaTime * _timeMultiplier;
    }

    [SerializeField] private float _exitX = 5.9f;
    private void CalculatePersonDetected()
    {
        //if abs x less than _exitX
        if(Mathf.Abs(_rotoTraslation.localPosition.x) < _exitX){
            _personDetected = true;
        }
        else{
            _personDetected = false;
        }
    }

    [SerializeField] private float _rotationOffset = 0f;
    public void SetRotationOffset(float rotationOffset){
        _rotationOffset = rotationOffset;
    }
    [SerializeField] private float _lerpSpeed = 0.1f;
    [SerializeField] private float _xTraslationMultiplier = 3.1f;
    [SerializeField] private float _yTraslationMultiplier = 0f;
    [SerializeField] private float _zTraslationMultiplier = 8.8f;
    //transform _offsetWithPose
    [SerializeField] private Vector3 _offsetWithPose = new Vector3(18.12f, 0, -9.9f);
    private void CalculateRotoTraslation()
    {
        //ROTATION
        //set transform.localrotation y angle from YDirection of left shoulder and right shoulder
        //transform.localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"]));
        //quaternion destRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) + 180 on y
        Quaternion destRotation = Quaternion.LookRotation(YDirection(_poseJoints["Left Shoulder"], _poseJoints["Right Shoulder"])) * Quaternion.Euler(0, -_rotationOffset, 0);
        //rotate 180
        destRotation *= Quaternion.Euler(0, 180, 0);
        //lerp
        _rotoTraslation.localRotation = Quaternion.Lerp(_rotoTraslation.localRotation, destRotation, _lerpSpeed);


        //TRASLATION
        //pose location = (_joints["Left Shoulder"].localPosition + _joints["Right Shoulder"].localPosition) / 2;
        Vector3 poseLocation = Vector3.zero;
        Vector3 poseXLocation = (_poseJoints["Left Shoulder"].localPosition + _poseJoints["Right Shoulder"].localPosition) / 2;
        
        /*Vector3 poseZLocation = _joints["Left Shoulder"].localPosition - _joints["Left Ankle"].localPosition;

        float length_to_measure = poseZLocation.y;

        // Focal length of the camera in millimeters
        float focal_length_mm = 4.81f;
        float actual_square_size_meters = 1f;  // Replace this with the actual measurement

        // Calculate the angular size in radians
        float angular_size_rad = 2 * Mathf.Atan(length_to_measure / (2 * focal_length_mm));

        //# Calculate the distance using the formula
        float distance_meters = (actual_square_size_meters / 2) / Mathf.Tan(angular_size_rad / 2);
        distance_meters = distance_meters*100* 33/13;
        //#print("Distance from camera to square: {:.2f} meters".format(distance_meters))
        distance_meters *= 100;*/
        float xPosition = poseXLocation.x;
        //correct with zDistance
        //xPosition = (xPosition - 7f) * (zDistance/_perspectiveCorrection);
        poseLocation = new Vector3(xPosition, 0, _distanceFromCamera);
        //target = pose transform plus middle between left shoulder and right shoulder
        Vector3 target = _pose.transform.localPosition + new Vector3(_xTraslationMultiplier*poseLocation.x + _offsetWithPose.x, _yTraslationMultiplier*poseLocation.y + _offsetWithPose.y, _zTraslationMultiplier*poseLocation.z + _offsetWithPose.z);
        target.y = 0f;
        //lerp position to left foot
        if(!_MANUAL_MOVEMENT)
            _rotoTraslation.localPosition = Vector3.Lerp(_rotoTraslation.localPosition, target, _lerpSpeed);
    }

    //return the vector ortogonal to two vectors on the xz plane
    private Vector3 YDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(aPos.x, 0, aPos.z);
        Vector3 bPosXZ = new Vector3(bPos.x, 0, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(-aToB.z, 0, aToB.x);
        return aToBOrtho;
    }

    public float _camAngleSensitivity = 10f;
    public float _camXAngleSensitivity = 10f;
    [SerializeField] private float _lerpNose;
    private void CalculateParallax(){
        //Parallax stuff

        //map localtransform x from -5,5 to 90,270 and set rotation of _camera
        //clamp from -5 to 5
        float camYAngle = Mathf.Clamp(_rotoTraslation.localPosition.x, -5f, 5f);
        camYAngle = -(_rotoTraslation.localPosition.x/5f) * _camAngleSensitivity;// + 180f;
        //lerp _lerpNose to nose
        _lerpNose = Mathf.Lerp(_lerpNose, _poseJoints["Nose"].localPosition.y, _lerpSpeed);
        //camXAngle add 1 and clamp between -2 and 2
        float camXAngle = Mathf.Clamp(_lerpNose + 1f, -2f, 2f);
        
        camXAngle = (camXAngle/2f) * _camXAngleSensitivity;

        if(_personDetected){
            _camera.localRotation = Quaternion.Lerp(_camera.localRotation, Quaternion.Euler(camXAngle, camYAngle, 0), _lerpSpeed);
        }
        else{
            _camera.localRotation = Quaternion.Lerp(_camera.localRotation, Quaternion.Euler(0, 0, 0), _lerpSpeed);
        }
    }
}
