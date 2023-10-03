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
    public GameObject _handTracker;
    [Range(0.01f, 1f)]
    public float _odileScale = 0.516f;
    
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
        _handTracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _handTracker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //parent to odile's VRotate
        _handTracker.transform.parent = transform;
        _handTracker.transform.localPosition = new Vector3(0, 0, 0);
    }

    private bool _notPopulated = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        //if spawned
        if (_notPopulated && _personManager._spawned)
        {
            //print names of all spheres
            foreach (GameObject sphere in _personManager._spheres)
            {
                //print(sphere.name);
                _joints.Add(sphere.name, sphere.transform);
            }
            _notPopulated = false;
            
            return;
        }
        //orient parent like YDirection("Right Shoulder", "Right Shoulder")
        //////transform.rotation = Quaternion.LookRotation(YDirection(_joints["Right Shoulder"], _joints["Right Shoulder"]));
        //set odile wrist to angle between Right shoulder, Right elbow, and Right wrist
        //_odileWrist.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Wrist"], _joints["Right Elbow"], _joints["Right Shoulder"]), 0, 0);
        //lerp Right shoulder, Right elbow, and Right wrist
        //////_odileJoints["VWrist"].localRotation = Quaternion.Lerp(_odileWrist.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Wrist"], _joints["Right Elbow"], _joints["Right Shoulder"]), 0, 0), 0.1f);
        //set odile arm to angle between Right elbow, Right shoulder, and Right hip
        //_odileArm.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Elbow"], _joints["Right Shoulder"], _joints["Right Hip"]), 0, 0);
        //lerp
        //////_odileJoints["VArm"].localRotation = Quaternion.Lerp(_odileArm.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Elbow"], _joints["Right Shoulder"], _joints["Right Hip"]), 0, 0), 0.1f);
        //get the position difference between right wrist and right hip, the place the hand tracker there relative odile's VRotate
        Vector3 handTrackerPos = _joints["Right Wrist"].position - _joints["Right Hip"].position;
        _handTracker.transform.localPosition = handTrackerPos * _odileScale + _odileJoints["VRotate"].localPosition + new Vector3(0, _heightOffset, 0);
        //inverse kinematics of odile joints to get as close as possible to hand tracker
        InverseKinematics();
    }

    //if distance is 1 VArm = 90, VWrist = 0
    //if distance is 0.5 VArm = 30 (sin-1  0.5), VWrist = 120
    //if distance is 0 VArm = 0, VWrist = 180
    
    //public _heightOffset slider with range
    [Range(-1, 1)]
    public float _heightOffset = 0;

    //inverse kinematics of odile joints to get as close as possible to hand tracker, joints to move are VRotate, VArm, VWrist
    void InverseKinematics(){
        //get y angle between up vector and hand tracker
        ////float yAngle = Vector3.Angle(Vector3.up, _handTracker.transform.position - _odileJoints["VRotate"].position);
        //set y angle of VRotate to yAngle
        ////_odileJoints["VRotate"].localRotation = Quaternion.Euler(0, yAngle, -90);
        //print distance from VRotate to hand tracker without z component
        //float distance = Vector3.Distance(_odileJoints["VRotate"].position, _handTracker.transform.position);
        float distance = Vector3.Distance(new Vector3(_odileJoints["VRotate"].position.x, _odileJoints["VRotate"].position.y, 0), new Vector3(_handTracker.transform.position.x, _handTracker.transform.position.y, 0));
        Debug.Log("Distance " + distance);
        float height = _odileJoints["VRotate"].position.y - _handTracker.transform.position.y;
        //clamp
        height = Mathf.Clamp(height, -1, 1);
        //log height
        Debug.Log("Height " + height);
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

        //VRotate YDirection shoulder and wrist, rotate also -90 on z
        Vector3 yDirection = YDirection(_joints["Right Shoulder"], _joints["Right Wrist"]);
        //only positive y
        //yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //invert
        yDirection = -yDirection;
        //only positive y
        yDirection = new Vector3(yDirection.x, Mathf.Abs(yDirection.y), yDirection.z);
        //_odileJoints["VRotate"].localRotation = Quaternion.LookRotation(YDirection(_joints["Right Shoulder"], _joints["Right Wrist"])) * Quaternion.Euler(0, -90, -90);
        //lerp
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
