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
        transform.rotation = Quaternion.LookRotation(YDirection(_joints["Right Shoulder"], _joints["Right Shoulder"]));
        //set odile wrist to angle between Right shoulder, Right elbow, and Right wrist
        //_odileWrist.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Wrist"], _joints["Right Elbow"], _joints["Right Shoulder"]), 0, 0);
        //lerp Right shoulder, Right elbow, and Right wrist
        _odileJoints["VWrist"].localRotation = Quaternion.Lerp(_odileWrist.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Wrist"], _joints["Right Elbow"], _joints["Right Shoulder"]), 0, 0), 0.1f);
        //set odile arm to angle between Right elbow, Right shoulder, and Right hip
        //_odileArm.localRotation = Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Elbow"], _joints["Right Shoulder"], _joints["Right Hip"]), 0, 0);
        //lerp
        _odileJoints["VArm"].localRotation = Quaternion.Lerp(_odileArm.localRotation, Quaternion.Euler(AngleBetweenThreePoints(_joints["Right Elbow"], _joints["Right Shoulder"], _joints["Right Hip"]), 0, 0), 0.1f);
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
