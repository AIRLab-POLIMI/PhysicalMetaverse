using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPoseContoller : MonoBehaviour
{
    //PersonManagerV2
    public PersonManagerV2 _personManager;
    public Transform _odileWrist;
    public Transform _odileArm;

    //dictionary of all joints
    private Dictionary<string, Transform> _joints = new Dictionary<string, Transform>();

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
    //serialize field pose invalidated
    [SerializeField] private bool _poseInvalidated = false;
    //float pose invalidated decay time
    [SerializeField] private float _poseInvalidatedDecayTime = 1f;
    //private invalidation start time
    private float _invalidationStartTime = 0f;
    //private list of meshes
    private List<MeshRenderer> _meshes = new List<MeshRenderer>();
    
    // Start is called before the first frame update
    void Start()
    {
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

    }
    public bool _handTrackerMeshEnabled = false;
    private bool _notPopulated = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        //if spawned
        if (_notPopulated)
        {
            if (_personManager._spawned){
                _joints.Clear();
                //print names of all spheres
                foreach (GameObject sphere in _personManager._spheres)
                {
                    //print(sphere.name);
                    _joints.Add(sphere.name, sphere.transform);
                }
                _notPopulated = false;
            }
            else
                return;
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
        //target = _joints["Left Foot 29"].position + new Vector3(0, 1f, 0);
        Vector3 target = _joints["Left Foot 29"].position;
        target.y = 0f;
        //lerp position to left foot
        if(!_manualMovement)
            transform.position = Vector3.Lerp(transform.position, target, _lerpSpeed);

        //set transform.localrotation y angle from YDirection of left shoulder and right shoulder
        //transform.localRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"]));
        //quaternion destRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) + 180 on y
        Quaternion destRotation = Quaternion.LookRotation(YDirection(_joints["Left Shoulder"], _joints["Right Shoulder"])) * Quaternion.Euler(0, 180, 0);
        //lerp
        transform.localRotation = Quaternion.Lerp(transform.localRotation, destRotation, 0.1f);
        
        //HeadAngles(); //TODO

    }
    public bool _manualMovement = false;
    public float _lerpSpeed = 0.5f;

    public GameObject _debugCylinder;
    void HeadAngles(){
        //store YDirection(_joints["Left Eye"], _joints["Right Eye"])
        Vector3 eyeDirection = YDirection(_joints["Left Eye"], _joints["Right Eye"]);
        //switch y with x
        eyeDirection = new Vector3(eyeDirection.y, eyeDirection.x, eyeDirection.z);
        //get y angle between left eye and right eye
        Quaternion destRotation = Quaternion.LookRotation(eyeDirection);
        //swap x angle and y angle
        destRotation = Quaternion.Euler(destRotation.eulerAngles.y, destRotation.eulerAngles.x, destRotation.eulerAngles.z);
        //rotate 90 on x
        destRotation = destRotation * Quaternion.Euler(90, 0, 0);
        //set rotation of _debugCylinder to destRotation
        _debugCylinder.transform.localRotation = destRotation;
        //subtract transform.localRotation.y to the z angle of cylinder local rotation
        _debugCylinder.transform.localRotation = Quaternion.Euler(_debugCylinder.transform.localRotation.eulerAngles.x, _debugCylinder.transform.localRotation.eulerAngles.y,  (_debugCylinder.transform.localRotation.eulerAngles.z + transform.localRotation.eulerAngles.y) * 2);
        //hide mesh of _debugCylinder
        _debugCylinder.GetComponent<MeshRenderer>().enabled = false;
        //set x angle of VCamPan to z angle of debug cylinder
        _odileJoints["VCamPan"].localRotation = _debugCylinder.transform.localRotation;
        float yan = _debugCylinder.transform.localRotation.eulerAngles.y;
        //print angles
        Debug.Log("X: " + destRotation.eulerAngles.x + " Y: " + yan + " Z: " + destRotation.eulerAngles.z);

    }

    //if distance is 1 VArm = 90, VWrist = 0
    //if distance is 0.5 VArm = 30 (sin-1  0.5), VWrist = 120
    //if distance is 0 VArm = 0, VWrist = 180
    
    //public _heightOffset slider with range
    [Range(-1, 1)]
    public float _heightOffset = 0;
    public GameObject _currentHandTracker;

    public bool _prevHide = false;
    public void Hide(bool setHide){
        if(!_manualMovement){
            if(setHide){
                _prevHide = true;
                //set all meshes
                foreach (MeshRenderer mesh in _meshes)
                {
                    mesh.enabled = false;
                }
            }
            else{
                if(_prevHide){
                    Vector3 target = _joints["Left Foot 29"].position;
                    target.y = 0f;
                    //no lerp
                    transform.position = target;
                }
                _prevHide = false;
            }
        }
    }
    //inverse kinematics of odile joints to get as close as possible to hand tracker, joints to move are VRotate, VArm, VWrist
    void InverseKinematics(){
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

    void SetPoseInvalidated(bool poseInvalidated){
        _poseInvalidated = poseInvalidated;
        _invalidationStartTime = Time.time;
    }
}
