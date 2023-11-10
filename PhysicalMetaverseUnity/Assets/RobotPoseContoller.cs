using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPoseContoller : VizController
{
    //bool serialize HIDE
    [SerializeField] private bool _HIDE = true;
    [SerializeField] private bool _HIDE_BUTTON = true;
    //PersonManagerV2
    [SerializeField] private PoseReceiver _poseReceiver;
    [SerializeField] private PoseManager _poseManager;
    private Transform _rotoTraslation;
    public Transform _odileWrist;
    public Transform _odileArm;

    //dictionary of all joints
    private Dictionary<string, Transform> _joints;

    //dictionary of odile joints
    private Dictionary<string, Transform> _odileJoints = new Dictionary<string, Transform>();
    //odilejoints are VRotate, VArm, VWrist, VCamArm, VCamAlign, VCamPan, VCamTilt

    public List<string> _odileJointNames = new List<string>();
    public List<Transform> _odileJointTransforms = new List<Transform>();
    //public inverse kinematic gameobject
    public GameObject _leftHandTracker;
    public GameObject _rightHandTracker;
    [Range(0.01f, 2f)]
    public float _odileScale = 0.516f;
    //private list of meshes
    private List<MeshRenderer> _meshes = new List<MeshRenderer>();
    [SerializeField] private GameObject _pose;
    //transform _offsetWithPose
    [SerializeField] private Vector3 _offsetWithPose = new Vector3(0, 0, 0);
    [SerializeField] private float _xTraslationMultiplier = 2f;
    [SerializeField] private float _yTraslationMultiplier = 0f;
    [SerializeField] private float _zTraslationMultiplier = 1f;
    //variables to keep median of zdistance
    //list
    
    // Start is called before the first frame update
    void Start()
    {
        _poseManager = PoseManager.Instance;
        _rotoTraslation = _poseManager.GetRotoTraslation();
        _joints = _poseManager.GetPoseJoints();
        /*foreach (Transform child in transform)
        {
            if (child.name.Contains("Joint"))
            {
                _odileJoints.Add(child.GetChild(0).name, child.GetChild(0));
            }
            //log
            Debug.Log(child);
            Debug.Log(child.parent.name);
        }
        //log count
        Debug.Log(_odileJoints.Count);*/
        //among all children of this gameobject and children of children find the ones called >Joint and add them to _odileJoints, with string key as the corresponding first child
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("Joint"))
            {
                _odileJoints.Add(child.GetChild(0).name, child);
                _odileJointNames.Add(child.GetChild(0).name);
                _odileJointTransforms.Add(child);
            }
        }

        //initialize hand tracker to a 0.2 sphere
        _leftHandTracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _leftHandTracker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //parent to odile's VRotate
        _leftHandTracker.transform.parent = transform;
        _leftHandTracker.transform.localPosition = new Vector3(0, 0, 0);
        //name
        _leftHandTracker.name = "Left Hand Tracker";
        //red
        _leftHandTracker.GetComponent<Renderer>().material.color = Color.red;
        _leftHandTracker.GetComponent<MeshRenderer>().enabled = _handTrackerMeshEnabled;

        //initialize hand tracker to a 0.2 sphere
        _rightHandTracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _rightHandTracker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //parent to odile's VRotate
        _rightHandTracker.transform.parent = transform;
        _rightHandTracker.transform.localPosition = new Vector3(0, 0, 0);
        //name
        _rightHandTracker.name = "Right Hand Tracker";
        //blue
        _rightHandTracker.GetComponent<Renderer>().material.color = Color.blue;
        _rightHandTracker.GetComponent<MeshRenderer>().enabled = _handTrackerMeshEnabled;

        //fill mesh list with all meshes of object and children and children of children
        foreach (MeshRenderer mesh in transform.GetComponentsInChildren<MeshRenderer>())
        {
            _meshes.Add(mesh);
        }
        //set all meshes
        foreach (MeshRenderer mesh in _meshes)
        {
            mesh.enabled = false;
        }
        //disable all sprite mesh renderes
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.enabled = false;
        }
    
        
    }
    public bool _handTrackerMeshEnabled = false;

    public float _perspectiveCorrection = 1f;
    public bool _NO_PERSON = false;
    public bool GetPoseDetected(){
        return _poseReceiver.GetPersonDetected();
    }
    // Update is called once per frame
    void Update()
    {
        _zDistance = _poseManager.GetDistanceFromCamera();
        if(_HIDE_BUTTON){
            _HIDE = !_HIDE;
            Hide(_HIDE);
            _HIDE_BUTTON = false;
        }
        //orient parent like YDirection("Left Shoulder", "Left Shoulder")
        //////transform.rotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Left Shoulder"]));
        //set odile wrist to angle between Left shoulder, Left elbow, and Left wrist
        //_odileWrist.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Left Wrist"], _joints["Left Elbow"], _joints["Left Shoulder"]), 0, 0);
        //lerp Left shoulder, Left elbow, and Left wrist
        //////_odileJoints["VWrist"].localRotation = Quaternion.Lerp(_odileWrist.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Left Wrist"], _joints["Left Elbow"], _joints["Left Shoulder"]), 0, 0), 0.1f);
        //set odile arm to angle between Left elbow, Left shoulder, and Left hip
        //_odileArm.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Left Elbow"], _joints["Left Shoulder"], _joints["Left Hip"]), 0, 0);
        //lerp
        //////_odileJoints["VArm"].localRotation = Quaternion.Lerp(_odileArm.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Left Elbow"], _joints["Left Shoulder"], _joints["Left Hip"]), 0, 0), 0.1f);
        //get the position difference between Left wrist and Left hip, the place the hand tracker there relative odile's VRotate
        Vector3 handTrackerPos = _joints["Left Wrist"].position - _joints["Left Hip"].position;
        //normalize with difference of left shoulder minus left hip
        handTrackerPos = handTrackerPos / (_joints["Left Shoulder"].position - _joints["Left Hip"].position).magnitude;
        _leftHandTracker.transform.localPosition = handTrackerPos * _odileScale + _odileJoints["VRotate"].localPosition + new Vector3(0, _heightOffset, 0);

        handTrackerPos = _joints["Right Wrist"].position - _joints["Right Hip"].position;
        //normalize with difference of left shoulder minus left hip
        handTrackerPos = handTrackerPos / (_joints["Right Shoulder"].position - _joints["Right Hip"].position).magnitude;
        _rightHandTracker.transform.localPosition = handTrackerPos * _odileScale + _odileJoints["VRotate"].localPosition + new Vector3(0, _heightOffset, 0);
        //inverse kinematics of odile joints to get as close as possible to hand tracker
        InverseKinematics();
        NeckKinematics();
        //InverseKinematics2();
        //pose location = (_joints["Left Shoulder"].localPosition + _joints["Right Shoulder"].localPosition) / 2;
        Vector3 poseLocation = Vector3.zero;
        Vector3 poseXLocation = (_joints["Left Shoulder"].localPosition + _joints["Right Shoulder"].localPosition) / 2;
        //get distance from personmanager to set z
        float zDistance = _poseManager.GetDistanceFromCamera();
        
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
        poseLocation = new Vector3(xPosition, 0, zDistance);
        //target = pose transform plus middle between left shoulder and right shoulder
        Vector3 target = _pose.transform.localPosition + new Vector3(_xTraslationMultiplier*poseLocation.x + _offsetWithPose.x, _yTraslationMultiplier*poseLocation.y + _offsetWithPose.y, _zTraslationMultiplier*poseLocation.z + _offsetWithPose.z);
        target.y = 0f;
        //lerp position to left foot
        if(!_manualMovement)
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, _lerpSpeed);



        //


        //HeadAngles(); //TODO
        //HeadAngles2D();
        HeadAngles3();
        
        if(_NO_PERSON){
            _odileJoints["VCamArm"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamAlign"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamPan"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamTilt"].GetComponent<DOFController>().ResetDof();
        }
        

        transform.localRotation = _rotoTraslation.localRotation;
        _NO_PERSON = !_poseManager.GetPersonDetected();
    }
    public float GetFilteredDistance(){
        return _zDistance;
    }
    public bool _manualMovement = false;
    public float _lerpSpeed = 0.5f;
    public float yOffset = 0f;
    public float zOffset = 0f;
    public Vector3 _tilt = new Vector3(0, 0, 0);

    /*void HeadAngles(){
        Quaternion bodyRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) * Quaternion.Euler(0, 0, 0);
        //final angle should be x=0, y=direction, z=-90
        Quaternion headPan = Quaternion.LookRotation(YDirection(_joints["Left Ear"], _joints["Right Ear"]));
        //set VCamPan to eyeDirection
        //_odileJoints["VCamPan"].localRotation = headPan;
        _odileJoints["VCamPan"].GetComponent<DOFController>().SetAngle(headPan.eulerAngles.y - bodyRotation.eulerAngles.y);
        //Quaternion headTilt = Quaternion.LookRotation(ZDirection(_joints["Left Ear"], _joints["Nose"])) * Quaternion.Euler(90, 0, 0);
        Quaternion headTilt = Quaternion.LookRotation(ZDirection(_joints["Left Ear"], _joints["Nose"]));
        _tilt = headTilt.eulerAngles;
        //set VCamTilt to headTilt
        _odileJoints["VCamTilt"].GetComponent<DOFController>().SetAngle(headTilt.eulerAngles.x - xOffset);


    }*/

    //get _lookAngle
    /*public float GetLookAngle(){
        return _odileJoints["VCamPan"].GetComponent<DOFController>().GetAngle();
    }
    public float GetTiltAngle(){
        return _odileJoints["VCamTilt"].GetComponent<DOFController>().GetAngle();
    }*/

    //get handtrackers localpositions
    public Vector3 GetLeftHandTrackerLocalPosition(){
        return _leftHandTracker.transform.localPosition;
    }
    public Vector3 GetRightHandTrackerLocalPosition(){
        return _rightHandTracker.transform.localPosition;
    }

    public Vector3 GetNoseTrackerLocalPosition(){
        return _joints["Nose"].localPosition;
    }

    //right wrist
    public Vector3 GetRightWristLocalPosition(){
        return _joints["Right Wrist"].localPosition;
    }

    //left wrist
    public Vector3 GetLeftWristLocalPosition(){
        return _joints["Left Wrist"].localPosition;
    }

    //right shoulder
    public Vector3 GetRightShoulderLocalPosition(){
        return _joints["Right Shoulder"].localPosition;
    }

    //left shoulder
    public Vector3 GetLeftShoulderLocalPosition(){
        return _joints["Left Shoulder"].localPosition;
    }

    //_leftDiff _rightDiff
    [SerializeField] private float _leftDiff = 0f;
    [SerializeField] private float _rightDiff = 0f;
    void HeadAngles2D(){ //TODO use a more refined formula for angle
        //difference between left ear and left eye's x
        _leftDiff = _joints["Left Ear"].position.x - _joints["Left Eye"].position.x;
        //difference between right ear and right eye's x
        _rightDiff = _joints["Right Ear"].position.x - _joints["Right Eye"].position.x;
        
        float headAngle = 0f;

        //if leftDiff is positive and rightDiff is negative
        if(_leftDiff > 0 && _rightDiff < 0){
            //headAngle is 0
            headAngle = 180f;
        }
        //if leftDiff is negative and rightDiff is positive
        else if(_leftDiff < 0 && _rightDiff > 0){
            //headAngle is 30
            headAngle = 0f;
        }
        //if leftDiff is positive and rightDiff is positive
        else if(_leftDiff > 0 && _rightDiff > 0){
            //headAngle is 15
            headAngle = 45f;
        }
        //if leftDiff is negative and rightDiff is negative
        else if(_leftDiff < 0 && _rightDiff < 0){
            //headAngle is -15
            headAngle = -45f;
        }
        //set VCamPan to headAngle
        _odileJoints["VCamPan"].GetComponent<DOFController>().SetAngle(headAngle);
    }
    //serialize tilt zero distance
    [SerializeField] private float _tiltZeroDistance = 0.8f;
    [SerializeField] private float _tiltZeroHeight = 0.8f;
    //nose height
    [SerializeField] private float _noseHeightMultiplier = 0.1f;
    
    private void HeadAngles3(){
        _odileJoints["VCamPan"].GetComponent<DOFController>().SetAngle(_poseManager.GetHeadAngleY() + 90f);
        _odileJoints["VCamTilt"].GetComponent<DOFController>().SetAngle(_poseManager.GetHeadAngleX());
    }

    //if distance is 1 VArm = 90, VWrist = 0
    //if distance is 0.5 VArm = 30 (sin-1  0.5), VWrist = 120
    //if distance is 0 VArm = 0, VWrist = 180
    
    //public _heightOffset slider with range
    [Range(-1, 1)]
    public float _heightOffset = 0;
    public GameObject _currentHandTracker;

    public bool _prevHide = false;
    public void HideOld(bool setHide){
        if(!_manualMovement || _HIDE){
            if(setHide){
                _prevHide = true;
                //set all meshes
                foreach (MeshRenderer mesh in _meshes)
                {
                    mesh.enabled = false;
                }
                //disable all sprite mesh renderes
                foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                {
                    spriteRenderer.enabled = false;
                }
            }
            else{
                if((_prevHide && !_HIDE) || (_HIDE_BUTTON && !_HIDE)){
                    //Vector3 target = _joints["Left Foot 29"].position;
                    //target.y = 0f;
                    //no lerp
                    //transform.position = target;
                    //set all meshes
                    foreach (MeshRenderer mesh in _meshes)
                    {
                        mesh.enabled = true;
                    }
                    //disable all sprite mesh renderes
                    foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        spriteRenderer.enabled = true;
                    }
                }
                _prevHide = false;
            }
        }
    }

    //list of gameobjects
    [SerializeField] private List<GameObject> _fedeOdileParts;
    //list _fedeOdilePartsStrings
    public List<string> _fedeOdilePartsStrings = new List<string>();
    public void Hide(bool setHide){
        if(!_manualMovement || _HIDE){
            if(setHide){
                _prevHide = true;
                //disable all obejcts in _fedOdileParts
                foreach (GameObject obj in _fedeOdileParts)
                {
                    obj.SetActive(false);
                }
                    
                _odileJoints["VCamArm"].GetComponent<DOFController>().ResetDof();
                _odileJoints["VCamAlign"].GetComponent<DOFController>().ResetDof();
                _odileJoints["VCamPan"].GetComponent<DOFController>().ResetDof();
                _odileJoints["VCamTilt"].GetComponent<DOFController>().ResetDof();
            }
            else{
                if((_prevHide && !_HIDE) || (_HIDE_BUTTON && !_HIDE)){
                    //enable all obejcts in _fedOdileParts
                    foreach (GameObject obj in _fedeOdileParts)
                    {
                        obj.SetActive(true);
                        //enable all meshes in the children
                        foreach (MeshRenderer mesh in obj.GetComponentsInChildren<MeshRenderer>())
                        {
                            mesh.enabled = true;
                        }
                    }
                    //ResetDof on all dof
                    
                    _odileJoints["VCamArm"].GetComponent<DOFController>().ResetDof();
                    _odileJoints["VCamAlign"].GetComponent<DOFController>().ResetDof();
                    _odileJoints["VCamPan"].GetComponent<DOFController>().ResetDof();
                    _odileJoints["VCamTilt"].GetComponent<DOFController>().ResetDof();

                }
                _prevHide = false;
            }
        }
    }

    
    //fire hide button
    public void FireHideButton(){
        _HIDE_BUTTON = true;
    }

    public override void SetHide(bool setHide)
    {
        if(setHide){
            _HIDE = true;
            //disable all obejcts in _fedOdileParts
            foreach (GameObject obj in _fedeOdileParts)
            {
                obj.SetActive(false);
            }
                
            _odileJoints["VCamArm"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamAlign"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamPan"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamTilt"].GetComponent<DOFController>().ResetDof();
            _prevHide = true;
        }
        else{
            _HIDE = false;
            //enable all obejcts in _fedOdileParts
            foreach (GameObject obj in _fedeOdileParts)
            {
                obj.SetActive(true);
                //enable all meshes in the children
                foreach (MeshRenderer mesh in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh.enabled = true;
                }
            }
            //ResetDof on all dof
            
            _odileJoints["VCamArm"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamAlign"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamPan"].GetComponent<DOFController>().ResetDof();
            _odileJoints["VCamTilt"].GetComponent<DOFController>().ResetDof();
            _prevHide = false;
        }
    }

    //inverse kinematics of odile joints to get as close as possible to hand tracker, joints to move are VRotate, VArm, VWrist
    void InverseKinematics(){
        _currentHandTracker = _leftHandTracker;
        //get y angle between up vector and hand tracker
        ////float yAngle = Vector3.Angle(Vector3.up, _handTracker.transform.position - _odileJoints["VRotate"].position);
        //set y angle of VRotate to yAngle
        ////_odileJoints["VRotate"].localRotation = Quaternion.Euler(0, yAngle, -90);
        //print distance from VRotate to hand tracker without z component
        //float distance = Vector3.Distance(_odileJoints["VRotate"].position, _handTracker.transform.position);
        float distance = Vector3.Distance(new Vector3(_odileJoints["VRotate"].position.x, _odileJoints["VRotate"].position.y, 0), new Vector3(_currentHandTracker.transform.position.x, _currentHandTracker.transform.position.y, 0));
        //Debug.Log("Distance " + distance);
        float height = _odileJoints["VRotate"].position.y - _currentHandTracker.transform.position.y;

        //clamp
        height = Mathf.Clamp(height, -1, 1);
        //log height
        //Debug.Log("Height " + height);
        //distance clamp to 0 1
        distance = Mathf.Clamp(distance, 0, 1);
        //height angle, subract 0.5y from distance and get angle using sin-1 on it
        float heightAngle = Mathf.Asin(height) * Mathf.Rad2Deg;
        //clamp
        heightAngle = Mathf.Clamp(heightAngle, -90, 90);
        //set VArm x angle to sin-1 distance plus distanceAngle
        //_odileJoints["VArm"].localRotation = Quaternion.Euler(Mathf.Asin(distance) * Mathf.Rad2Deg, 0, 0) * Quaternion.Euler(heightAngle, 0, 0);
        //lerp
        _odileJoints["VArm"].localRotation = Quaternion.Lerp(_odileJoints["VArm"].localRotation, Quaternion.Euler(Mathf.Asin(distance) * Mathf.Rad2Deg, 0, 0) * Quaternion.Euler(heightAngle, 0, 0), 0.1f);
        //set VWrist x angle to 180 - VArm x angle
        //_odileJoints["VWrist"].localRotation = Quaternion.Euler(180 - (Mathf.Asin(distance) * Mathf.Rad2Deg)*2, 0, 0);
        //lerp
        _odileJoints["VWrist"].localRotation = Quaternion.Lerp(_odileJoints["VWrist"].localRotation, Quaternion.Euler(180 - (Mathf.Asin(distance) * Mathf.Rad2Deg)*2, 0, 0), 0.1f);
        Vector3 yDirection;
        //VRotate YDirection shoulder and wrist, rotate also -90 on z
        if(_currentHandTracker == _leftHandTracker)
            yDirection = YDirection(_joints["Left Shoulder"], _joints["Left Wrist"]);
        else
            yDirection = YDirection(_joints["Right Shoulder"], _joints["Right Wrist"]);
        //only positive y
        //yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //invert
        yDirection = -yDirection;
        //only positive y
        yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //_odileJoints["VRotate"].localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Left Wrist"])) * Quaternion.Euler(0, -90, -90);
        //lerp plus transform.localRotation
        _odileJoints["VRotate"].localRotation = Quaternion.Lerp(_odileJoints["VRotate"].localRotation, Quaternion.LookRotation(yDirection) * Quaternion.Euler(0, -90, -90), 0.1f);
        //TODO lerp only through positive y, even if it is longer path
        
        
    }

    
    void InverseKinematics2(){
        _currentHandTracker = _rightHandTracker;
        //get y angle between up vector and hand tracker
        ////float yAngle = Vector3.Angle(Vector3.up, _handTracker.transform.position - _odileJoints["VRotate"].position);
        //set y angle of VRotate to yAngle
        ////_odileJoints["VRotate"].localRotation = Quaternion.Euler(0, yAngle, -90);
        //print distance from VRotate to hand tracker without z component
        //float distance = Vector3.Distance(_odileJoints["VRotate"].position, _handTracker.transform.position);
        float distance = Vector3.Distance(_odileJoints["VRotate"].position, _currentHandTracker.transform.position);
        //draw line
        Debug.DrawLine(_odileJoints["VRotate"].position, _currentHandTracker.transform.position, Color.red);
        //Debug.Log("Distance " + distance);
        float height = _odileJoints["VRotate"].position.y - _currentHandTracker.transform.position.y;
        //draw blue line
        Debug.DrawLine(_odileJoints["VRotate"].position, new Vector3(_odileJoints["VRotate"].position.x, _currentHandTracker.transform.position.y, _odileJoints["VRotate"].position.z), Color.blue);
        //arm and forearm are long both 1.6
        //clamp distance between 0 and 3.2
        distance = Mathf.Clamp(distance, 0, 3.2f);
        //from height and distance find angle using arcsin
        float heightAngle = Mathf.Asin(height / distance) * Mathf.Rad2Deg;
        //set 90 - heightangle to VArm
        _odileJoints["VArm"].localRotation = Quaternion.Euler(90 - heightAngle, 0, 0);
/*
        //log height
        //Debug.Log("Height " + height);
        //distance clamp to 0 1
        distance = Mathf.Clamp(distance, 0, 1);
        //height angle, subract 0.5y from distance and get angle using sin-1 on it
        float heightAngle = Mathf.Asin(height) * Mathf.Rad2Deg;
        //clamp
        heightAngle = Mathf.Clamp(heightAngle, -90, 90);
        //set VArm x angle to sin-1 distance plus distanceAngle
        //_odileJoints["VArm"].localRotation = Quaternion.Euler(Mathf.Asin(distance) * Mathf.Rad2Deg, 0, 0) * Quaternion.Euler(heightAngle, 0, 0);
        //lerp
        _odileJoints["VArm"].localRotation = Quaternion.Lerp(_odileJoints["VArm"].localRotation, Quaternion.Euler(Mathf.Asin(distance) * Mathf.Rad2Deg, 0, 0) * Quaternion.Euler(heightAngle, 0, 0), 0.1f);
        //set VWrist x angle to 180 - VArm x angle
        //_odileJoints["VWrist"].localRotation = Quaternion.Euler(180 - (Mathf.Asin(distance) * Mathf.Rad2Deg)*2, 0, 0);
        //lerp
        _odileJoints["VWrist"].localRotation = Quaternion.Lerp(_odileJoints["VWrist"].localRotation, Quaternion.Euler(180 - (Mathf.Asin(distance) * Mathf.Rad2Deg)*2, 0, 0), 0.1f);
        Vector3 yDirection;
        //VRotate YDirection shoulder and wrist, rotate also -90 on z
        if(_currentHandTracker == _leftHandTracker)
            yDirection = YDirection(_joints["Left Shoulder"], _joints["Left Wrist"]);
        else
            yDirection = YDirection(_joints["Right Shoulder"], _joints["Right Wrist"]);
        //only positive y
        //yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //invert
        yDirection = -yDirection;
        //only positive y
        yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //_odileJoints["VRotate"].localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Left Wrist"])) * Quaternion.Euler(0, -90, -90);
        //lerp plus transform.localRotation
        _odileJoints["VRotate"].localRotation = Quaternion.Lerp(_odileJoints["VRotate"].localRotation, Quaternion.LookRotation(yDirection) * Quaternion.Euler(0, -90, -90), 0.1f);
        //TODO lerp only through positive y, even if it is longer path
        */
        
    }
    public float _leftDistance;
    public float _neckSensitivity = 50f;
    public float _neckOffset = 25f;
    public float _neckOffset2 = 40f;
    public float _neckSensitivity2 = 0.3f;
    private float _zDistance = 1f;

    private void NeckKinematics(){
        _currentHandTracker = _rightHandTracker;
        //get y angle between up vector and hand tracker
        ////float yAngle = Vector3.Angle(Vector3.up, _handTracker.transform.position - _odileJoints["VRotate"].position);
        //set y angle of VRotate to yAngle
        ////_odileJoints["VRotate"].localRotation = Quaternion.Euler(0, yAngle, -90);
        //print distance from VRotate to hand tracker without z component
        //float distance = Vector3.Distance(_odileJoints["VRotate"].position, _handTracker.transform.position);
        float distance = Vector3.Distance(new Vector3(_odileJoints["VRotate"].position.x, _odileJoints["VRotate"].position.y, 0), new Vector3(_currentHandTracker.transform.position.x, _currentHandTracker.transform.position.y, 0));
        //Debug.Log("Distance " + distance);
        float height = _odileJoints["VRotate"].position.y - _currentHandTracker.transform.position.y;
        //clamp
        height = Mathf.Clamp(height, -1, 1);
        //log height
        //Debug.Log("Height " + height);
        //distance clamp to 0 1
        distance = Mathf.Clamp(distance, 0, 1);
        //height angle, subract 0.5y from distance and get angle using sin-1 on it
        float heightAngle = Mathf.Asin(height) * Mathf.Rad2Deg;
        //clamp
        heightAngle = Mathf.Clamp(heightAngle, -90, 90);
        heightAngle = heightAngle * _neckSensitivity2;
        //set VArm x angle to sin-1 distance plus distanceAngle
        //_odileJoints["VArm"].localRotation = Quaternion.Euler(Mathf.Asin(distance) * Mathf.Rad2Deg, 0, 0) * Quaternion.Euler(heightAngle, 0, 0);
        //lerp
        heightAngle = heightAngle - _neckOffset2;
        _odileJoints["VCamArm"].GetComponent<DOFController>().SetAngle(-heightAngle);
        _odileJoints["VCamAlign"].GetComponent<DOFController>().SetAngle(heightAngle);
        //TODO lerp only through positive y, even if it is longer path
        
    }
    private void NeckKinematicsOLD(){
        //get distance of left hand tracker from left hip and set VCamArm dofcontroller
        //_leftDistance = Vector3.Distance(_joints["Right Wrist"].position, _joints["Right Hip"].position) * _oldZDistance;
        //_leftDistance = distance from right wrist to vertical line passing by right hip
        _leftDistance = Vector3.Distance(_joints["Right Wrist"].position, new Vector3(_joints["Right Hip"].position.x, _joints["Right Wrist"].position.y, _joints["Right Hip"].position.z)) * _zDistance;
        //map angle
        _leftDistance = _leftDistance * _neckSensitivity - _neckOffset;
        _odileJoints["VCamArm"].GetComponent<DOFController>().SetAngle(_leftDistance);
        //set VCamAlign to complementary
        _odileJoints["VCamAlign"].GetComponent<DOFController>().SetAngle(-_leftDistance);
    }
    
    //return the vector ortogonal to two vectors on the xz plane
    Vector3 YDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(aPos.x, 0, aPos.z);
        Vector3 bPosXZ = new Vector3(bPos.x, 0, bPos.z);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(-aToB.z, 0, aToB.x);
        return aToBOrtho;
    }
    //return the vector ortogonal to two vectors on the xy plane
    Vector3 ZDirection(Transform a, Transform b)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 aPosXZ = new Vector3(aPos.x, aPos.y, 0);
        Vector3 bPosXZ = new Vector3(bPos.x, bPos.y, 0);
        Vector3 aToB = bPosXZ - aPosXZ;
        Vector3 aToBOrtho = new Vector3(-aToB.y, aToB.x, 0);
        return aToBOrtho;
    }

    //function to return vector orthogonal to plane passing by three transform.positions
    Vector3 OrthogonalToPlane(Transform a, Transform b, Transform c)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 cPos = c.position;
        Vector3 aToB = bPos - aPos;
        Vector3 aToC = cPos - aPos;
        Vector3 aToBOrtho = Vector3.Cross(aToB, aToC);
        return aToBOrtho;
    }

    //return the angle between two points with pivot on a third point
    float AngleBetweenThreePoints(Transform a, Transform b, Transform c)
    {
        Vector3 aPos = a.position;
        Vector3 bPos = b.position;
        Vector3 cPos = c.position;
        Vector3 aToB = bPos - aPos;
        Vector3 aToC = cPos - aPos;
        float angle = Vector3.Angle(aToB, aToC);
        return angle;
    }
}
