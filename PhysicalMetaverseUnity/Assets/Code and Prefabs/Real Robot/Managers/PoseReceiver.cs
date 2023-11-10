//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography;

//replace back all .localPosition to .position if there are problems

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
//this manager receives the 34 body landmarks detected by the depthai camera and positions a sphere at each landmark
public class PoseReceiver : Monosingleton<PoseReceiver>
{
    public float _poseInvalidationTime = 0.4f;
    private PoseManager _poseManager;
    //dictionary of all joints
    private Dictionary<string, Transform> _joints = new Dictionary<string, Transform>();

    //make a slider to adjust the rotation angle
    //public float rotationAngle = -20f;
    [Range(-90f, 90f)]
    public float rotationAngle = -20f;

    //listen for udp messages on port 5005
    static int port = 5005;
    //udpclient object
    private UdpClient client;
    //udp packet storage
    private byte[] data;
    private int[][] parsedData;

    //spawned
    [SerializeField] private bool _spawned = false;
    //get
    public bool GetSpawned(){
        return _spawned;
    }

    [SerializeField] private GameObject[] _spheres;
    //public getspheres
    public GameObject[] GetSpheres(){
        return _spheres;
    }

    //prev rcv time
    private float prevRcvTime = 0f;
    //serialize person detected
    [SerializeField] private bool _personDetected = false;
    //poseInvalidated
    [SerializeField] private bool _poseInvalidated = false;
    //serialize pose confirmation area
    [SerializeField] private GameObject _poseConfirmationArea;
    
    public bool ENABLE_LOG = false;
    public bool MESH_ENABLED = true;
    public bool _mesh_status = true;
    public bool _CENTER_TO_VIZ = true;
    public bool _GET_SPHERES = true;

    //serializefied center viz offsets
    [SerializeField] private float _centerVizXOffset = 0f;
    [SerializeField] private float _centerVizYOffset = 0f;
    [SerializeField] private float _centerVizZOffset = 0f;
    //serializefield transform offset with root
    [SerializeField] private Vector3 _rootOffset;
    public GameObject _pose;
    public void OnMsgRcv(byte[] msg)
    {
        //disable Debug.Log for this object
        Debug.unityLogger.logEnabled = ENABLE_LOG;
        data = msg;
        char[] bytesAsChars = new char[msg.Length];
        for (int i = 0; i < msg.Length; i++)
        {
            bytesAsChars[i] = (char)msg[i];
        }
        string message = new string(bytesAsChars);
        Debug.Log("Person Manager received message: " + message);
        parsedData = ParseData(message);
        //log difference between times
        Debug.Log(Time.time - prevRcvTime);
        
        //set prevRcvTime only if lenght of parsedData is >= 33
        if(parsedData.Length >= 33){
            prevRcvTime = Time.time;
        }
        Debug.unityLogger.logEnabled = true;
    }
    //get _personDetected
    public bool GetPersonDetected(){
        return _personDetected;
    }

    
    //SetPoseInvalidated
    public void SetPoseInvalidated(){
        _poseInvalidated = true;
    }

    //struct containing a string and a gameobject
    [System.Serializable]
    public class PoseJointsDict
    {
        public string name;
        public GameObject joint;
    }

    //list of RobotJointsArmsDict
    public List<PoseJointsDict> _poseJoints = new List<PoseJointsDict>();
    void Start()
    {
        _poseManager = PoseManager.Instance;
        newObject = new GameObject("Transformer");
        newTransform = newObject.AddComponent<Transform>();
        //create empty gameobjects Head, Left Arm, Right Arm, Left Leg, Right Leg with parent this
        GameObject head = new GameObject("Head");
        head.transform.parent = this.transform;
        GameObject leftArm = new GameObject("Left Arm");
        leftArm.transform.parent = this.transform;
        GameObject rightArm = new GameObject("Right Arm");
        rightArm.transform.parent = this.transform;
        GameObject leftLeg = new GameObject("Left Leg");
        leftLeg.transform.parent = this.transform;
        GameObject rightLeg = new GameObject("Right Leg");
        rightLeg.transform.parent = this.transform;
        //rotate pose 180
        _pose.transform.rotation = Quaternion.Euler(0, 180, 0);

        _originalEarsOffset = _earsOffset;
    }

    //a receive looks like
/*
[[ 226   63  -22]
[ 239   46  -17]
[ 244   46  -17]
[ 249   47  -17]
[ 224   46  -20]
[ 219   45  -20]
[ 215   45  -20]
[ 257   62    7]
[ 212   59    0]
[ 236   89  -13]
[ 217   88  -15]
[ 259  214   27]
[ 164  147  -20]
[ 198  360   10]
[  87  181  -65]
[ 125  447  -18]
[  46  129 -116]
[ 109  478  -22]
[  30  108 -133]
[ 102  467  -33]
[  34  109 -132]
[ 105  456  -23]
[  40  121 -118]
[  61  453   19]
[   4  417  -20]
[ 128  529    0]
[  74  514  -47]
[  59  614   69]
[  20  583   35]
[  48  625   75]
[  13  586   44]
[  50  650   75]
[   5  628   52]
[  29  437    0]
[ 331   10    0]]
 */
    public float _zDistance = 0.1f;
    [Range (0.1f, 100f)]
    public float _zDistanceMultiplier = 1f;
    [Range (0.1f, 100f)]
    public float _yDistanceMultiplier = 1f;
    [Range (0.1f, 100f)]
    public float _distanceScaleMultiplier = 1f;
    //function to parse it into an array of arrays of 3 integers
    private int[][] ParseData(string data)
    {
        //string ending float, read chars from length - 5 to length - 1
        string endingFloat = data.Substring(data.Length - 6, 6);
        //replace dot with comma
        endingFloat = endingFloat.Replace(".", ",");
        //remove final comma
        endingFloat = endingFloat.Substring(0, endingFloat.Length - 1);
        _zDistance = float.Parse(endingFloat);
        _zDistance /= 100;
        Debug.Log(endingFloat);
        string dataWithoutEndingFloat = data.Substring(0, data.Length - 6);
        //split the data into lines
        string[] lines = dataWithoutEndingFloat.Split('\n');
        //create an array of arrays of 3 integers
        int[][] parsedData = new int[lines.Length][];
        //for each line
        for (int i = 0; i < lines.Length; i++)
        {
            //log
            //Debug.Log(lines[i]);
            //split the line into integers, spit on variable number of spaces
            string[] integers = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < integers.Length; j++)
            {
                //remove any space, [, or ]
                integers[j] = integers[j].Replace(" ", "").Replace("[", "").Replace("]", "");
                //if the integer is empty discard it
                if (integers[j] == "")
                {
                    //remove the integer
                    integers[j] = null;
                }
            }

            //store non empy elements into a new integers array
            int[] newIntegers = new int[3];
            //index for newIntegers
            int index = 0;
            //for each integer
            for (int j = 0; j < integers.Length; j++)
            {
                //if the integer is not null
                if (integers[j] != null)
                {
                    //parse the integer
                    int integer = int.Parse(integers[j]);
                    //store the integer in newIntegers
                    newIntegers[index] = integer;
                    //increment index
                    index++;
                }
            }
            //Debug.Log(newIntegers);
            parsedData[i] = newIntegers;
        }
        /*string parsed = "";
        //log all elements of parsedData
        for (int i = 0; i < parsedData.Length; i++)
        {
            for (int j = 0; j < parsedData[i].Length; j++)
            {   
                parsed += parsedData[i][j] + " ";
            }
            parsed += "\n";
        }
        Debug.Log(parsed);
        */
        //return the array of arrays
        return parsedData;
    }

    //public get distance
    public float GetDistance(){
        return _zDistance;
    }
    
    //range 0.1 1 _poseDecayTime
    [Range(0.1f, 1f)]
    public float _poseDecayTime = 0.4f;
    public GameObject _odileViz;

    private float _prevInvalidatedTime = 0f;

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void Update()
    {   
        //if first receive
        if (data != null)
        {
            if(!_spawned){
                //call function to spawn spheres
                if(_GET_SPHERES){
                    UseExistingSpheres();
                    CheckMeshEnabled();
                }
                else{
                    SpawnSpheres();
                }
                _joints.Clear();
                //print names of all spheres
                foreach (GameObject sphere in _spheres)
                {
                    //print(sphere.name);
                    _joints.Add(sphere.name, sphere.transform);
                }
            }
            
            //move spheres to position
            MoveSpheres();
            data = null;//if mesh enable, else disable mesh

            CheckMeshEnabled();
        }

        //if time since last receive is more than pose decay time move spheres to y = -100
        if (Time.time - prevRcvTime > _poseDecayTime)
        {
            _personDetected = false;
            //move transform down 100y
            //transform.localPosition = new Vector3(transform.localPosition.x, -100f, transform.localPosition.z);
            //disable _odileViz
            ////_odileViz.GetComponent<RobotPoseContoller>().Hide(true);

            //disable _poseConfirmationArea
            _poseConfirmationArea.SetActive(false);
        }
        else{
            _personDetected = true;
            _poseInvalidated = false;
            //enable _poseConfirmationArea
            _poseConfirmationArea.SetActive(true);

        }

        //if not poseinvalidated _prevInvalidatedTime = Time.time
        if(!_poseInvalidated){
            _prevInvalidatedTime = Time.time;
        }

        //if time since _prevInvalidatedTime is more than poseInvalidationTime _poseManager ShowViz false
        if(Time.time - _prevInvalidatedTime > _poseInvalidationTime){
            _poseManager.ShowViz(false);
        }
        else{
            _poseManager.ShowViz(true);
        }
        

        //set odileviz rotation to orientation of vector from zero to odileviz
        Vector3 direction = _poseManager.GetRotoTraslation().position - Vector3.zero;
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg + 90f;
        _poseManager.SetRotationOffset(angle);
    }

    //get _joints
    public Dictionary<string, Transform> GetJoints(){
        return _joints;
    }

    private void CheckMeshEnabled(){
        if (MESH_ENABLED)
        {
            if(_mesh_status){
                //disable mesh of all spheres
                foreach (GameObject sphere in _spheres)
                {
                    sphere.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else{
                //enable mesh of all spheres
                foreach (GameObject sphere in _spheres)
                {
                    sphere.GetComponent<MeshRenderer>().enabled = true;
                }
            }
            _mesh_status = !_mesh_status;
            MESH_ENABLED = false;
        }
    }

    //spawn spheres
    private void SpawnSpheres()
    {
        //if parsedData.Length shorter than 30 return
        if (parsedData.Length < 30)
            return;
        _spheres = new GameObject[parsedData.Length];
        //list of strings containing Shoulder, Elbow, Wrist, Hip, Knee, Ankle
        List<string> joints = new List<string> { "Shoulder", "Elbow", "Wrist", "Hip", "Knee", "Ankle" };

        //spawn a sphere for each element in the array
        for (int i = 0; i < parsedData.Length; i++)
        {
            //spawn a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(60f/_scale, 60f/_scale, 60f/_scale);
            //name the sphere
            sphere.name = "Sphere (" + i + ")";
            //scale 10
            sphere.transform.localScale = new Vector3(80f/_scale, 80f/_scale, 80f/_scale);
            //from 0 to 10 make spheres red, from 11 to 21 make spheres yellow only on odd numbers, from 10 to 20 make spheres orange only on even numbers,
            //from 24 to 32 make spheres green only on even numbers, from 23 to 31 make spheres blue only on odd numbers
            if (i >= 0 && i <= 11)
            {
                //localscale to 0.1
                sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                sphere.GetComponent<Renderer>().material.color = Color.cyan;
                //name face + i
                //if 3 or 6 call left or right eye
                if (i == 3){
                    sphere.name = "Left Eye";
                    //color black
                    sphere.GetComponent<Renderer>().material.color = Color.black;
                }
                else if (i == 6){
                    sphere.name = "Right Eye";
                    //color black
                    sphere.GetComponent<Renderer>().material.color = Color.black;
                }
                else
                {
                    //if i 0 call it Nose
                    if (i == 0)
                        sphere.name = "Nose";
                    else
                        sphere.name = "Face (" + i + ")";
                }
            }
            if (i >= 11 && i <= 21)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.yellow;
                    //name left + current joint
                    if(i > 15 )
                        sphere.name = "Left Hand " + i;
                    else
                        sphere.name = "Left " + joints[(i - 11)/2];
                }
            }
            if (i >= 12 && i <= 20)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.red;
                    //name right + current joint
                    if(i > 16)
                        sphere.name = "Right Hand " + i;
                    else
                        sphere.name = "Right " + joints[(i - 12)/2];
                }
            }
            if (i >= 24 && i <= 32)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.green;
                    //name right + current joint
                    if(i > 28)
                        sphere.name = "Right Foot " + i;
                    else
                        sphere.name = "Right " + joints[(i - 24)/2 + 3];
                }
            }
            if (i >= 23 && i <= 31)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.blue;
                    //name left + current joint
                    if(i > 27)
                        sphere.name = "Left Foot " + i;
                    else
                        sphere.name = "Left " + joints[(i - 23)/2 + 3];
                }
            }
            //set the position of the sphere
            sphere.transform.localPosition = new Vector3(i, 0, 0);
            //add the sphere to the spheres array
            _spheres[i] = sphere;
        }
        //set spawned to true
        _spawned = true;
        //set sphere34 if sphere name is sphere 34
        sphere34 = _spheres[33];
        //set all spheres as children of this gameobject
        foreach (GameObject sphere in _spheres)
        {
            sphere.transform.parent = _pose.transform;
        }
    }

    private void UseExistingSpheres(){
        _spheres = new GameObject[parsedData.Length];
        int i = 0;
        //add each child of _pose to _spheres
        foreach(Transform sphere in _pose.gameObject.GetComponentsInChildren<Transform>()){
            //skip _pose
            if(sphere.gameObject == _pose.gameObject)
                continue;
            _spheres[i] = sphere.gameObject;
            //set the position of the sphere
            sphere.localPosition = new Vector3(i, 0, 0);
            i++;
        }
        //set spawned to true
        _spawned = true;
    }
    //gameobject sphere 34
    private GameObject sphere34;
    [Range(1f, 5000f)]
    public float _scale = 169f;

    [Range(-100f, 100f)]
    public float zOffset = 5.5f;

    [Range(-100f, 100f)]
    public float yOffset = -10f;
    [Range(-100f, 100f)]
    public float xOffset = -10f;

    [Range(0.1f, 10f)]
    public float _xScale = 1f;
    [Range(0.1f, 10f)]
    public float _perspectiveCorrection = 1f;
    [Range(0.1f, 10f)]
    public float _yScale = 1f;
    [Range(0.1f, 10f)]
    public float _zScale = 1f;

    [Range(0f, 100f)]
    public float zMultiplier = 1f;
    private GameObject newObject;
    private Transform newTransform = null;

[Range(0.01f, 1f)]
    public float _speed = 0.05f;

    //move spheres
    private void MoveSpheresNEW()
    {
        try{
            

            // Attach a Transform component to the GameObject
            
            //for each sphere
            for (int i = 0; i < parsedData.Length; i++)
            {
                //get the sphere
                GameObject sphere = _spheres[i];
                //create new transform// Create a new GameObject
                //set positions
                //sphere.transform.localPosition = new Vector3(parsedData[i][1], parsedData[i][0], parsedData[i][2]);
                //should rotate z by 45 degrees. if a point has y = 0 z is unchanged, if a point has y = 100, z is brought closer
                newTransform.localPosition = new Vector3(parsedData[i][1]/_scale, parsedData[i][0]/_scale, parsedData[i][2]/_scale);
                newTransform.localScale = new Vector3(80f/_scale, 80f/_scale, 80f/_scale);
                //rotate spheres position by 45 degrees with fulcrum at 
                Vector3 rotationAxis = Vector3.right; // You can adjust the axis according to your requirements

                // Specify the rotation angle in degrees
                //float rotationAngle = -20f; // You can adjust the angle as desired
                //rotation center in 0 0
                ////Vector3 rotationCenter = new Vector3(224.1144f/_scale, -26f/_scale, -359.3866f/_scale); // You can adjust the center of rotation as desired
                // Rotate the sphere around the center of rotation
                ////newTransform.RotateAround(rotationCenter, rotationAxis, rotationAngle);
                //move sphere by Offset
                newTransform.localPosition = new Vector3(sphere.transform.localPosition.x + xOffset, sphere.transform.localPosition.y + yOffset, sphere.transform.localPosition.z + zOffset);// + 1/sphere34.transform.localPosition.y * zMultiplier);
                //sphere lerp to newTransform
                sphere.transform.localPosition = Vector3.Lerp(sphere.transform.localPosition, newTransform.localPosition, _speed);
                //move gradually
                //sphere.transform.localPosition = Vector3.Lerp(sphere.transform.localPosition, new Vector3(parsedData[i][0], parsedData[i][1], parsedData[i][2]), 0.05f);

            }
        }
        catch(Exception e)
        {
            Debug.Log(":-)");
            //log size of data
            Debug.Log(parsedData.Length);
            //log size of first element
            Debug.Log(parsedData[0].Length);
            //log spheres size
            Debug.Log(_spheres.Length);
        }
    }

    public bool _rotate90 = false;
    [SerializeField] private float _earsOffset = 0.1f;
    private float _originalEarsOffset = 0.1f;
    private void MoveSpheres()
    {
        try{
            //for each sphere
            for (int i = 0; i < parsedData.Length; i++)
            {
                //get the sphere
                GameObject sphere = _spheres[i];
                //set positions
                //sphere.transform.localPosition = new Vector3(parsedData[i][1], parsedData[i][0], parsedData[i][2]);
                //should rotate z by 45 degrees. if a point has y = 0 z is unchanged, if a point has y = 100, z is brought closer
                if (_rotate90)
                    sphere.transform.localPosition = new Vector3(-parsedData[i][1]/_scale, parsedData[i][0]/_scale, parsedData[i][2]/_scale);
                else
                    sphere.transform.localPosition = new Vector3(-parsedData[i][0]/_scale, parsedData[i][1]/_scale, parsedData[i][2]/_scale);
                sphere.transform.localScale = new Vector3(80f/_scale, 80f/_scale, 80f/_scale);
                //rotate spheres position by 45 degrees with fulcrum at 
                Vector3 rotationAxis = Vector3.right; // You can adjust the axis according to your requirements

                // Specify the rotation angle in degrees
                //float rotationAngle = -20f; // You can adjust the angle as desired
                //rotation center in 0 0
                Vector3 rotationCenter = new Vector3(224.1144f/_scale, -26f/_scale, -359.3866f/_scale); // You can adjust the center of rotation as desired
                // Rotate the sphere around the center of rotation
                sphere.transform.RotateAround(rotationCenter, rotationAxis, rotationAngle);
                //move sphere by Offset
                sphere.transform.localPosition = new Vector3(((sphere.transform.localPosition.x * _xScale) + xOffset), (sphere.transform.localPosition.y * _yScale) + yOffset, (sphere.transform.localPosition.z * _zScale) + zOffset);// + 1/sphere34.transform.localPosition.y * zMultiplier);
                //move gradually
                //sphere.transform.localPosition = Vector3.Lerp(sphere.transform.localPosition, new Vector3(parsedData[i][0], parsedData[i][1], parsedData[i][2]), 0.05f);

            }
        }
        catch(Exception e)
        {
            Debug.Log(":-)");
            //log size of data
            Debug.Log(parsedData.Length);
            //log size of first element
            Debug.Log(parsedData[0].Length);
            //log spheres size
            Debug.Log(_spheres.Length);
        }
        //find y of bottom left foot
        float footY = _spheres[23].transform.localPosition.y;
        float footX = _spheres[23].transform.localPosition.x;
        //move father z like _zDistance * _zDistanceMultiplier
        _personCollider.transform.localPosition = new Vector3(_personCollider.transform.localPosition.x, _personCollider.transform.localPosition.y, _zDistance * _zDistanceMultiplier);
        //move father y to make it so _spheres[23].transform.localPosition.y; goes to absolute 0
        _personCollider.transform.localPosition = new Vector3(_personCollider.transform.localPosition.x, -footY, _personCollider.transform.localPosition.z);
        //use footX and perspective correction to move father x
        _personCollider.transform.localPosition = new Vector3(footX*(_zDistance/_perspectiveCorrection), _personCollider.transform.localPosition.y, _personCollider.transform.localPosition.z);
        _personCollider.transform.localPosition = new Vector3(_personCollider.transform.localPosition.x * _poseMultiplier, _personCollider.transform.localPosition.y, _personCollider.transform.localPosition.z * _poseMultiplier);
        _personCollider.transform.position = new Vector3(-_personCollider.transform.position.x, _personCollider.transform.position.y, -_personCollider.transform.position.z);
        //y angle = _cameraTransform.eulerAngles.y clamped between -90 and 90
        float yAngle = _cameraTransform.eulerAngles.y - 180;
        yAngle = Mathf.Clamp(yAngle, -90f, 90f);
        _yAngle = yAngle;
        //rotate around 0 by _cameraTransform y angle
        _personCollider.transform.RotateAround(Vector3.zero, Vector3.up, yAngle);
        //transform.localPosition = new Vector3((_zDistance/_perspectiveCorrection), transform.localPosition.y, transform.localPosition.z);
        
    }
    public float _poseMultiplier = 200f;
    public GameObject _personCollider;
    //public camera transform
    public Transform _cameraTransform;
    public float _yAngle;
}

