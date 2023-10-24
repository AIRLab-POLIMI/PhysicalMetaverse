using System;
using System.Linq;
using Core;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class LidarManager : Monosingleton<LidarManager>
{
    [Header("ACTIONS")]
    [Space]
    [SerializeField] private bool ENABLE_LOG = false;
    [SerializeField] private bool _SMOOTH_WITH_ODOMETRY = true;
    [SerializeField] private bool _MERGE_WALLS = false;
    [SerializeField] private bool _LIDAR_TRACKING = true;
    [SerializeField] private bool _PERSON_TRACKING = true;
    [SerializeField] private bool _UPDATE_PILLAR_BEHAVIOUR = true;
    [SerializeField] private bool _STATION_TO_CLOSEST = true;
    [SerializeField] private bool _DISABLE_LIDAR = false;
    [Space]
    [Space]

    [Header("PILLAR GENERATION")]
    [Space]
    [SerializeField] private float _lidarScale = 1f;
    [SerializeField] private bool _disableBackPillars = true;
    [SerializeField] private int _disablePillarsRange = 60;
    [SerializeField] private int _savePillarsRange = 20;
    [SerializeField] private GameObject lidarPoint;
    [SerializeField] private GameObject lidarPoint2;
    [SerializeField] private IntSO lidarMode;
    private GameObject[] _points;
    private int[] _measurements;
    private  int arraySize = 360;
    private float minDistValue = 1.0f;
    private float maxDistValue = 20.0f;
    private int minMeasure = 0;
    private int maxMeasure = 5000;
    private Vector3 defaultHidePosition = new Vector3(-0.5f, -10f, -12.0f);
    [SerializeField] private FloatSO distanceFromCamera; //In centimeters, positive if lidar is behind camera, negative if lidar is in frontm of the camera
    [SerializeField] private FloatSO poseDistance;
    private float defaultPoseDistance = 10.0f;
    private int nOfLidarDegrees = 0;
    private int[] trackedCameraDegrees;
    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;

    [Space]
    [Space]
    [Header("DENOISING")]
    [Space]
    [SerializeField] private OdometryManager _odometryManager;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _odometrySpeed = 0.23f;
    [SerializeField] private float newTolerance = 1.2f; //1 is no tolerance
    private int[] currentPositions;
    [SerializeField] float fadeDuration = 0.05f; // Duration of the fade-out effect in seconds

    [Space]
    [Space]
    [Header("LIDAR TRACKING")]
    [Space]
    //array of blob booleans
    [SerializeField] private int[] _blobs = new int[360];
    [SerializeField] private int[] _personBlobs = new int[360];
    [SerializeField] private int _middle = 0;
    [SerializeField] private int count = 0;
    [SerializeField] private int _skippableBlobPoints = 2;
    [SerializeField] private int _personSkippableBlobPoints = 6;
    [SerializeField] private List<int> _blobSizes = new List<int>();
    [SerializeField] private List<int> _blobStarts = new List<int>();
    [SerializeField] private List<int> _personBlobSizes = new List<int>();
    [SerializeField] private List<int> _personBlobStarts = new List<int>();
    [SerializeField] private List<int> _blobIds = new List<int>();
    //dictionary of string gameobject blobtrackers
    [SerializeField] private Dictionary<int, GameObject> _blobTrackers = new Dictionary<int, GameObject>();
    [SerializeField] private float _maxJumpDistance = 3f;
    [SerializeField] private List<GameObject> _stationList;
    //range 0 1 public float lidar tracking lerp
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _lidarTrackingLerp = 0.5f;
    ////private GameObject _personTracker;
    [SerializeField] private GameObject _humanViz;
    [SerializeField] private float _humanVizOffset = 1f;
    [SerializeField] private float _maxPersonJumpDistance = 8f;
    [SerializeField] private GameObject personCollider;
    [SerializeField] private float _personPillarDown = -3f;
    [SerializeField] private float _pillarLerpSpeed = 0.1f;
    [SerializeField] private float _backUpReducer = 3f;
    [SerializeField] private int _PERSON_ID = 9;

    [Space]
    [Space]
    [Header("MERGE WALLS")]
    [Space]
    [SerializeField] private float _wallCylinderThreshold = 3.0f;
    [SerializeField] private float _wallSizeMultiplier = 1.2f;
    [SerializeField] private float _wallHeight = 4.5f;
    //range _aggregatePointsTolerance from 0 to 100
    [Range(1, 2)] [SerializeField] private float _aggregatePointsTolerance = 1.1f; //1 is no tolerance
    [SerializeField] private int _skippablePoints = 0;
    private List<GameObject> _walls = new List<GameObject>();
    private List<GameObject> _cylinders = new List<GameObject>();
    private int _wallCount = 0;


    public void Setup()
    {
        Debug.Log("[Lidar Manager setup]");

        nOfLidarDegrees = LidarToCameraRange.LidarDegreesBasedOnDistance(distanceFromCamera.runtimeValue);

        _points = new GameObject[arraySize];

        _measurements = new int[arraySize];

        //Points are spawned as inactive, because prefab is inactive. 
        SpawnPoints();
    }

    public void ActivatePoints()
    {
        for (int i = 0; i < arraySize; i++)
        {
            _points[i].SetActive(true);
        }
    }

    public void SetBlobAt(int id, int value){
        _blobs [id] = value;
    }

    public void SetPersonBlobAt(int id, int value){
        _personBlobs [id] = value;
    }


    private void Start()
    {
        //if _DISABLE_LIDAR disable this
        if(_DISABLE_LIDAR)
            this.gameObject.SetActive(false);
        //instantiate blob tracker
        ////_personTracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //set tag to Station
        ////_personTracker.tag = "Person";
        //blobtracker name "Person"
        ////_personTracker.name = "Person";
        //scale 3
        ////_personTracker.transform.localScale = new Vector3(3, 3, 3);
        //add rigidbody
        ////_personTracker.AddComponent<Rigidbody>();
        //add trigger mesh collider
        ////_personTracker.AddComponent<MeshCollider>().convex = true;
        ////_personTracker.GetComponent<MeshCollider>().isTrigger = true;
        //set rigidbody to kinematic
        ////_personTracker.GetComponent<Rigidbody>().isKinematic = true;
        //add object Walls as children and populate it with 150 cube meshes
        GameObject walls = new GameObject("Walls");
        walls.transform.parent = transform;
        for(int i = 0; i < 150; i++){
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.parent = walls.transform;
            wall.transform.position = new Vector3(0,0,0);
            wall.transform.localScale = new Vector3(0.1f, _wallHeight, 0.1f);
            wall.SetActive(false);
            //remove collider
            Destroy(wall.GetComponent<BoxCollider>());
            _walls.Add(wall);
        }
        //add 150 cylinders
        for(int i = 0; i < 150; i++){
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wall.transform.parent = walls.transform;
            wall.transform.position = new Vector3(0,0,0);
            wall.transform.localScale = new Vector3(0.1f, _wallHeight, 0.1f);
            //disable wall
            wall.SetActive(false);
            //remove collider
            Destroy(wall.GetComponent<CapsuleCollider>());
            _cylinders.Add(wall);
        }
        //init _blobs to -1
        _blobs = new int[arraySize];
        for(int i = 0; i < arraySize; i++){
            _blobs[i] = -1;
        }

        //init person blobs to -1
        _personBlobs = new int[arraySize];
        for(int i = 0; i < arraySize; i++){
            _personBlobs[i] = -1;
        }
        
        DisableBackPillars();
    }

    private void DisableBackPillars(){
        if(_disableBackPillars){
            //disable pillars from 0 to 30 and from 330 to 360
            for(int j = 0; j < _disablePillarsRange; j++){
                //set station id to -1
                _points[j].GetComponent<PillarManager>().SetStationId(-1);
                _blobs[j] = -1;
                _personBlobs[j] = -1;
                _points[j].SetActive(false);
            }
            for(int j = 360-_disablePillarsRange; j < 360; j++){
                //set station id to -1
                _blobs[j] = -1;
                _personBlobs[j] = -1;
                _points[j].GetComponent<PillarManager>().SetStationId(-1);
                _points[j].SetActive(false);
            }
            //enable pillars from 0 to _savePillarsRange
            for(int j = 0; j < _savePillarsRange; j++){
                _points[j].SetActive(true);
            }
            for(int j = 360-_savePillarsRange; j < 360; j++){
                _points[j].SetActive(true);
            }

        }
    }
    public void OnMsgRcv(byte[] msg)
    {
        
        DisableBackPillars();
        
        //disable Debug.Log for this object
        Debug.unityLogger.logEnabled = ENABLE_LOG;

        int[] bytesAsInts = new int[arraySize];
        //print size
        Debug.Log(msg.Length);
        Buffer.BlockCopy(msg, 0, bytesAsInts, 0, msg.Length);
        //log bytesAsInts
        //Debug.Log(bytesAsInts);

        trackedCameraDegrees = CopyTrackedDegrees(bytesAsInts, nOfLidarDegrees);

        //var sb = new StringBuilder("new int[] { ");

        int i = 0;

        if(currentPositions == null){
            currentPositions = new int[arraySize];
            foreach (int n in bytesAsInts)
            {
                UpdatePosition(i, n);
                i++;
            }
        }
        else
        {
            foreach (int n in bytesAsInts)
            {
                //if max between current and new divided by min between current and new is greater than tolerance then update
                float relativeDistance = (float)Math.Max(currentPositions[i], n) / (float)Math.Min(currentPositions[i], n);
                if(!(currentPositions[i] == 0 && n == 0) && relativeDistance > newTolerance)
                {
                    UpdatePosition(i, n);
                    currentPositions[i] = n;
                }
                //sb.Append(n + ", ");
                //UpdatePosition(i, n);
                i++;
            }
        }

        if(_MERGE_WALLS)
            WallsUsingDistancesSkippableGroups();
        if(!_MERGE_WALLS){
            if(_wallCount > 0)
            {
                //disable all elements in walls
                foreach(GameObject wall in _walls){
                    wall.GetComponent<MeshRenderer>().enabled = false;
                }
                foreach(GameObject cylinder in _cylinders){
                    cylinder.GetComponent<MeshRenderer>().enabled = false;
                }
                _wallCount = 0;
            }
        }
        Debug.unityLogger.logEnabled = true;
        //reset transform to 0
        transform.position = new Vector3(0,0,0);


    }
    void FixedUpdate()
    {
        //disable mesh of _blobTracker
        ////_personTracker.GetComponent<MeshRenderer>().enabled = false;
        //disable _blobTrackers meshes
        foreach(GameObject blob in _blobTrackers.Values){
            blob.GetComponent<MeshRenderer>().enabled = false;
        }
        //check blob array and enable mesh of corresponding true values
        /*
        for(int i = 0; i < 360; i++){
            if(_blobs[i] >= 0){
                _points[i].GetComponent<MeshRenderer>().enabled = true;
            }
            else{
                _points[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
        */
        
        if(_UPDATE_PILLAR_BEHAVIOUR)
            UpdatePillarBehaviour(_personPillarDown, _pillarLerpSpeed, _backUpReducer);

        if (_SMOOTH_WITH_ODOMETRY)
            Odometry();
    }

    void Update()
    {
        if(_LIDAR_TRACKING)
            LidarTracking();
        if(_PERSON_TRACKING)
            PersonTracking();
    }
    
    void UpdatePillarBehaviour(float personPillarDown, float pillarLerpSpeed, float backUpReducer){
        _UPDATE_PILLAR_BEHAVIOUR = false;
        foreach(GameObject pillar in _points){
            pillar.GetComponent<PillarManager>().UpdateBehaviour(personPillarDown, pillarLerpSpeed, backUpReducer);
        }
    }

    void Odometry()
    {
        if (_odometryManager._forward)
        {
            transform.position -= Vector3.forward * _odometrySpeed * Time.deltaTime;
            //fade material alpha a bit, no lerp
            
        }
        if (_odometryManager._backward)
        {
            transform.position += Vector3.forward * _odometrySpeed * Time.deltaTime;
            
        }
        if (_odometryManager._left)
        {
            transform.position += Vector3.right * _odometrySpeed * Time.deltaTime;
            
        }
        if (_odometryManager._right)
        {
            transform.position -= Vector3.right * _odometrySpeed * Time.deltaTime;
            
        }
        if (_odometryManager._rotateLeft)
        {
            transform.Rotate(Vector3.up * _odometrySpeed * Time.deltaTime);
            
        }
        if (_odometryManager._rotateRight)
        {
            transform.Rotate(Vector3.down * _odometrySpeed * Time.deltaTime);
            
        }
    }

    
    public void SpawnLidarBlobs(){
        StationManager stationManager = FindObjectOfType<StationManager>();
        foreach(GameObject station in stationManager.GetStations()){
            //spawn blob at station position
            GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //name it like corresponding station
            blob.name = station.name;
            blob.transform.position = station.transform.position;
            blob.GetComponent<MeshRenderer>().enabled = false;
            blob.tag = "Station";
            //add rigidbody
            blob.AddComponent<Rigidbody>();
            //add trigger mesh collider
            blob.AddComponent<MeshCollider>().convex = true;
            blob.GetComponent<MeshCollider>().isTrigger = true;
            //set rigidbody to kinematic
            blob.GetComponent<Rigidbody>().isKinematic = true;
            //add to list, get key from last digit of station name
            _blobTrackers.Add(int.Parse(station.name.Substring(station.name.Length - 1)), blob);
            //red if 0 blue if 1
            if(station.name.EndsWith("0")){
                blob.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else{
                blob.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
    }

    private void PersonTracking(){
        //clear lists
        _personBlobSizes.Clear();
        _personBlobStarts.Clear();
        //track a blob of pillars whose station id is 9, the tag is "Person"
        int skippableBlobPoints = _personSkippableBlobPoints;
        //look at _personBlobs array
        for(int i = 0; i < 360; i++){
            skippableBlobPoints = _personSkippableBlobPoints;
            //if true
            if(_personBlobs[i] == _PERSON_ID){
                //count until false
                count = 0;
                int j = i;
                bool seamPassed = false;
                while(skippableBlobPoints>0){
                    if(_personBlobs[j] != _PERSON_ID){
                        skippableBlobPoints--;
                    }
                    else{
                        skippableBlobPoints = _personSkippableBlobPoints;
                    }
                    count++;
                    j++;
                    if(j >= 360){
                        skippableBlobPoints = -1;
                    }
                    /*if(!seamPassed){
                        //if j is out of range
                        if(j >= 360){
                            j -= 360;
                            seamPassed = true;
                        }
                    }
                    else{
                        //if j is out of range
                        if(j >= 180){
                            _skippableBlobPoints = 1;

                        }
                    }*/
                }
                _personBlobSizes.Add(count);
                _personBlobStarts.Add(i);
                i = j;
            }
        }
        //find the index of the max element in list _personBlobSizes using Math.max
        int found = 0;
        int max = 0;
        int trueMiddle = 0;
        for(int k = 0; k < _personBlobSizes.Count; k++){
            if(_personBlobSizes[k] > max){
                max = _personBlobSizes[k];
                found = _personBlobStarts[k];
                trueMiddle = found + max/2;
                if(found >= 360){
                    found -= 360;
                }
            }
        }
        //spawn cylinder at middle of maxIndex
        //int middle = i + count/2;
        //middle = index of the pillar closest to world center
        //for each point in the blob, check distance from world center and keep the smallest
        int closestIndex = found;
        float minDistance = 1000f;
        for(int k = found; k < found + count; k++){
            if(k >= 360){
                k -= 360;
            }
            //get distance from world center
            float distance = Vector3.Distance(_points[k].transform.position, new Vector3(0,0,0));
            if(distance < minDistance){
                minDistance = distance;
                closestIndex = k;
            }
        }
        if(closestIndex >= 360){
            closestIndex -= 360;
        }
        Transform point = _points[closestIndex].transform;
        Transform trueMiddleTransform = _points[trueMiddle].transform;
        //lerp corresponding blobtracker at point
        ////_personTracker.transform.position = Vector3.Lerp(////_personTracker.transform.position, point.position, _lidarTrackingLerp);
        //destination equal to point minus 1 in direction of vector zero
        Vector3 destination = point.position - point.position.normalized * _humanVizOffset;
        //if distance is less than maxjumpdistance lerp
        if(Vector3.Distance(_humanViz.transform.position, destination) < _maxPersonJumpDistance){
            _humanViz.transform.position = Vector3.Lerp(_humanViz.transform.position, destination, _lidarTrackingLerp);
        }
        else{
            //else teleport
            _humanViz.transform.position = destination;
        }

        //lerp with constant speed
        //_humanViz.transform.position = Vector3.MoveTowards(_humanViz.transform.position, point.position, _lidarTrackingLerp);

        //no lerp
        //////_personTracker.transform.position = point.position;
        
        //TODO if actual person is not being tracked by camera take control of it, otherwise don't move it

            //lerp gameobject "PersonCollider" there
            //GameObject personCollider = GameObject.Find("PersonCollider");
            //personCollider.transform.position = Vector3.Lerp(personCollider.transform.position, point.position, _lidarTrackingLerp);
            //no lerp
            personCollider.transform.position = point.position;
            //position personCollider at point distance in direction of trueMiddleTransform
            //personCollider.transform.position = trueMiddleTransform.position.normalized * point.position.magnitude;
            
        //enable mesh
        ////_personTracker.GetComponent<MeshRenderer>().enabled = true;
        _humanViz.GetComponent<MeshRenderer>().enabled = true;
    }

    //Snaps station to a group of pillars, and manages to track it if pillars change reasonably slowly
    void LidarTracking(){
        //clear lists
        _blobSizes.Clear();
        _blobStarts.Clear();
        _blobIds.Clear();

        int skippableBlobPoints = _skippableBlobPoints;
        //disable _stationList meshes
        foreach(GameObject station in _stationList){
            ////station.GetComponent<MeshRenderer>().enabled = false;
            //station.GetComponent<StationController>().Hide();
        }
        //find start of groups of consecutive points and count their size
        for(int i = 0; i < 360; i++){
            skippableBlobPoints = _skippableBlobPoints;
            //if true
            if(_blobs[i] >= 0 && _blobs[i] != _PERSON_ID){
                //count until false
                count = 0;
                int j = i;
                bool seamPassed = false;
                while(skippableBlobPoints>0){
                    if(_blobs[j] < 0){
                        skippableBlobPoints--;
                    }
                    else{
                        skippableBlobPoints = _skippableBlobPoints;
                    }
                    count++;
                    j++;
                    if(j >= 360){
                        skippableBlobPoints = -1;
                    }
                    /*if(!seamPassed){
                        //if j is out of range
                        if(j >= 360){
                            j -= 360;
                            seamPassed = true;
                        }
                    }
                    else{
                        //if j is out of range
                        if(j >= 180){
                            _skippableBlobPoints = 1;

                        }
                    }*/
                }
                _blobSizes.Add(count);
                _blobStarts.Add(i);
                _blobIds.Add(_blobs[i]);
                //skip to j
                i = j;
            }
        }
        //choose the biggest blob and spawn cylinder at its middle
        //int max = 0;
        //int maxIndex = 0;
        //for(int i = 0; i < _blobSizes.Count; i++){
        //    if(_blobSizes[i] > max){
        //        max = _blobSizes[i];
        //        maxIndex = i;
        //    }
        //}
        //if(_blobStarts.Count == 0){
        //    return;
        //}
        
        //TODO try selection of bigger blob when more than one have the same id
        /*
        //check blobids, for each duplicate value check size in blobsizes and delete the smaller one, also in blobstarts
        for(int i = 0; i < _blobIds.Count; i++){
            for(int j = i+1; j < _blobIds.Count; j++){
                if(_blobIds[i] == _blobIds[j]){
                    //if same id, check size
                    if(_blobSizes[i] > _blobSizes[j]){
                        //delete j
                        _blobIds.RemoveAt(j);
                        _blobSizes.RemoveAt(j);
                        _blobStarts.RemoveAt(j);
                        //decrement j
                        j--;
                    }
                    else{
                        //delete i
                        _blobIds.RemoveAt(i);
                        _blobSizes.RemoveAt(i);
                        _blobStarts.RemoveAt(i);
                        //decrement i
                        i--;
                        //break
                        break;
                    }
                }
            }
        }
        */

        //check _blobIds list, if there are duplicates, keep the biggest one
        for(int i = 0; i < _blobIds.Count; i++){
            for(int j = i+1; j < _blobIds.Count; j++){
                if(_blobIds[i] == _blobIds[j]){
                    //if same id, check size
                    if(_blobSizes[i] > _blobSizes[j]){
                        //delete j
                        _blobIds.RemoveAt(j);
                        _blobSizes.RemoveAt(j);
                        _blobStarts.RemoveAt(j);
                        //decrement j
                        j--;
                    }
                    else{
                        //delete i
                        _blobIds.RemoveAt(i);
                        _blobSizes.RemoveAt(i);
                        _blobStarts.RemoveAt(i);
                        //decrement i
                        i--;
                        //break
                        break;
                    }
                }
            }
        }

        //for each blob spawn corresponding cylinder at middle
        for(int i = 0; i < _blobStarts.Count; i++){
            Transform point;
            if(!_STATION_TO_CLOSEST){
                //MOVE TO MIDDLE
                //spawn cylinder at middle of maxIndex
                int middle = _blobStarts[i] + _blobSizes[i]/2;
                if(middle >= 360){
                    middle -= 360;
                }
                //if middle id is not valid try next one
                //drifts counter clockwise, try to fix by looking left and right
                int id = _blobs[middle];
                //while(id < 0){
                //    middle++;
                //    if(middle >= 360){
                //        middle -= 360;
                //    }
                //    id = _blobs[middle];
                //}
                bool lookRight = true;
                int middleRight = middle;
                int middleLeft = middle;
                while(id < 0){
                    if(lookRight){
                        middleRight++;
                        if(middleRight >= 360){
                            middleRight -= 360;
                        }
                        id = _blobs[middleRight];
                        lookRight = false;
                    }
                    else{
                        middleLeft--;
                        if(middleLeft < 0){
                            middleLeft += 360;
                        }
                        id = _blobs[middleLeft];
                        lookRight = true;
                    }
                }

                point = _points[middle].transform;
            }
            else{
                //MOVE TO CLOSEST PILLAR
                //spawn cylinder at middle of maxIndex
                //int middle = i + count/2;
                //middle = index of the pillar closest to world center
                //for each point in the blob, check distance from world center and keep the smallest
                int closestIndex = _blobStarts[i] + _blobSizes[i]/2;
                float minDistance = 1000f;
                for(int k = _blobStarts[i]; k < _blobStarts[i] + _blobSizes[i]; k++){
                    if(k >= 360){
                        k -= 360;
                    }
                    //get distance from world center
                    float distance = Vector3.Distance(_points[k].transform.position, new Vector3(0,0,0));
                    if(distance < minDistance){
                        minDistance = distance;
                        closestIndex = k;
                    }
                }
                if(closestIndex >= 360){
                    closestIndex -= 360;
                }
                point = _points[closestIndex].transform;
            }


            //lerp corresponding blobtracker at point
            //_blobTracker.transform.position = Vector3.Lerp(_blobTracker.transform.position, point.position, _lidarTrackingLerp);
            //_blobTrackers[_blobIds[i]].transform.position = Vector3.Lerp(_blobTrackers[_blobIds[i]].transform.position, point.position, _lidarTrackingLerp);
            //no lerp
            _blobTrackers[_blobIds[i]].transform.position = point.position;
            //enable mesh
            //_blobTracker.GetComponent<MeshRenderer>().enabled = true;
            ////_blobTrackers[_blobIds[i]].GetComponent<MeshRenderer>().enabled = true;
            //lerp corresponding station in _stationList to cylinder only if it is not further than _maxJumpDistance
            ////_stationList[_blobIds[i]].GetComponent<MeshRenderer>().enabled = true;
            ///
            //_stationList[_blobIds[i]].GetComponent<StationController>().Show();

            //LIDAR TRACKING MOVES INTERACTABLE STATION
            
            if(Vector3.Distance(_stationList[_blobIds[i]].transform.position, point.position) < _maxJumpDistance){
                ////_stationList[_blobIds[i]].GetComponent<MeshRenderer>().enabled = true;
                _stationList[_blobIds[i]].transform.position = Vector3.Lerp(_stationList[_blobIds[i]].transform.position, point.position, _lidarTrackingLerp);
            }
            else{
                //move without lerp
                _stationList[_blobIds[i]].transform.position = point.position;
            }
        }
        ////spawn cylinder at middle of maxIndex
        //int middle = _blobStarts[maxIndex] + max/2;
        //if(middle >= 360){
        //    middle -= 360;
        //}
        //Transform point = _points[middle].transform;
        ////lerp corresponding blobtracker at point
        ////_blobTracker.transform.position = Vector3.Lerp(_blobTracker.transform.position, point.position, _lidarTrackingLerp);
        //_blobTrackers[_blobIds[maxIndex]].transform.position = Vector3.Lerp(_blobTrackers[_blobIds[maxIndex]].transform.position, point.position, _lidarTrackingLerp);
        ////enable mesh
        ////_blobTracker.GetComponent<MeshRenderer>().enabled = true;
        //_blobTrackers[_blobIds[maxIndex]].GetComponent<MeshRenderer>().enabled = true;

    }

    public void LidarTrack(GameObject station){
        //move station to corresponding cylinder position using id
        //get id from station name
        int id = int.Parse(station.name.Substring(station.name.Length - 1));
        //get cylinder position
        Vector3 position = _blobTrackers[id].transform.position;
        //lerp station position to cylinder position
        //station.transform.position = Vector3.Lerp(station.transform.position, position, _lidarTrackingLerp);
        // no lerp
        station.transform.position = position;

    }
    
    public void AddStationInteraction(GameObject stationInteraction){
        //add a _stationInteractionPrefab to list
        //GameObject newStation = Instantiate(_stationInteractionPrefab);
        //rename to corresponding station id
        //newStation.name = station.name;
        //set y to -100
        //newStation.transform.position = new Vector3(0,-100,0);
        //newStation.GetComponent<StationController>().Init();
        //set stations _interactionGameObject to newStation
        //station.GetComponent<SingleStationManager>()._interactionGameObject = newStation;
        //set newstation's station to station
        //newStation.GetComponent<StationController>()._station = station;
        //set child of station
        //newStation.transform.parent = station.transform;
        //newStation.GetComponent<SingleStationManager>().enabled = false;
        //disable its Collider child
        //newStation.transform.GetChild(0).gameObject.SetActive(false);
        //add a new station to _stationList
        _stationList.Add(stationInteraction);
    }

    /*
    
        //find the first true value in blobs and count until the last consecutive, then take the middle value and spawn _blobTracker pillarTall at the _point position
        for(int i = 0; i < 360; i++){
            //if true
            if(_blobs[i]){
                //count until false
                count = 0;
                int j = i;
                bool seamPassed = false;
                while(_skippableBlobPoints>0){
                    if(!_blobs[j]){
                        _skippableBlobPoints--;
                    }
                    else{
                        _skippableBlobPoints = 2;
                    }
                    count++;
                    j++;
                    /*if(!seamPassed){
                        //if j is out of range
                        if(j >= 360){
                            j -= 360;
                            seamPassed = true;
                        }
                    }
                    else{
                        //if j is out of range
                        if(j >= 180){
                            _skippableBlobPoints = 1;
                        }
                    }*//*
                }
                //spawn _blobTracker at the middle of the count
                //get middle
                _middle = i + count/2;
                //use _points[] array
                if(_middle >= 360){
                    _middle -= 360;
                }
                Transform point = _points[_middle].transform;
                //lerp blobtracker at point
                _blobTracker.transform.position = Vector3.Lerp(_blobTracker.transform.position, point.position, 0.5f);
                //enable mesh
                _blobTracker.GetComponent<MeshRenderer>().enabled = true;
                break;
            }
        }
    */
    void WallsUsingDistances(){
        int skippedPoints = 0;
        int lastValid = 0;
        int newStart = 0;
        float _debugLineTime = 0.1f;
        
        //disable all elements in walls
        foreach(GameObject wall in _walls){
            wall.GetComponent<MeshRenderer>().enabled = false;
        }
        _wallCount = 0;

        for (int j = 0; j < 360; j++)
        {
            //check distance difference between current and next
            int next = j + 1;
            if (next == 360)
            {
                Debug.DrawLine(_points[newStart].transform.position, _points[j].transform.position, Color.red, _debugLineTime);
                //spawn cube along line stretching it
                SpawnWall(newStart, j);

                break;
            }
            float relativeDistance = (float)Math.Max(currentPositions[j], currentPositions[next]) / (float)Math.Min(currentPositions[j], currentPositions[next]);

            //if another obstacle is found, not infinity
            if (relativeDistance > _aggregatePointsTolerance + _aggregatePointsTolerance / 10 && relativeDistance < _aggregatePointsTolerance * 10){
                Debug.Log("New blob found, blob ended");
                //draw last valid point
                Debug.DrawLine(_points[newStart].transform.position, _points[lastValid].transform.position, Color.red, _debugLineTime);
                //spawn cube along line stretching it
                SpawnWall(newStart, j);
                //break;
                newStart = next;
                lastValid = next;
                skippedPoints = 0;
            }
            else if (relativeDistance > _aggregatePointsTolerance)
            {
                skippedPoints++;
                //Debug.Log("Distance difference between " + j + " and " + next + " is " + relativeDistance);
                //draw a debug line that goes from _point 0 to point j
            }
            else
            {
                if(j > 0 && skippedPoints == 0)
                    lastValid = j-1;
                skippedPoints = 0;
            }
            if (relativeDistance > _aggregatePointsTolerance * 10)
            {
                if(skippedPoints > _skippablePoints)
                {
                    Debug.Log("Too far, blob ended");
                    //draw last valid point
                    Debug.DrawLine(_points[newStart].transform.position, _points[lastValid].transform.position, Color.red, _debugLineTime);
                    //spawn cube along line stretching it
                    SpawnWall(newStart, j);
                    //break;
                    newStart = next;
                    lastValid = next;
                    skippedPoints = 0;
                }
            }
        }
        
        //////// GPT ALTERNATIVE, add epsilon to avoid zero check
        /*
        //init currentPositions outside
        foreach (int n in bytesAsInts)
            {
                //if max between current and new divided by min between current and new is greater than tolerance then update
                float relativeDistance = (float)Math.Max(currentPositions[i] +1, n +1) / (float)Math.Min(currentPositions[i] +1, n +1);
                if(relativeDistance > newTolerance)
                {
                    UpdatePosition(i, n);
                    currentPositions[i] = n;
                }

                i++;
            }
        */

        //UpdatePoseDistance(); //MAYBE IMPORTANT BUT NOT WORKING

        //sb.Append("}");
        //Debug.Log(sb.ToString());
    }

    private void WallsUsingDistancesSkippableGroups(){
        int skippedPoints = 0;
        int skippedAt = 0;
        int newStart = 0;
        bool requiredNewStart = true;
        int numberOfBlobs = 0;

        //disable all elements in walls
        foreach(GameObject wall in _walls){
            wall.SetActive(false);
        }
        foreach(GameObject cylinder in _cylinders){
            cylinder.SetActive(false);
        }
        _wallCount = 0; 

        for(int i = 0; i < 360; i++){
            if(currentPositions[i] >= 10000000){
                Debug.Log("currentPositions[i] "+ currentPositions[i]);
                skippedPoints++;
            
                if(skippedPoints > _skippablePoints){
                    Debug.Log("Skipped at "+ (skippedAt));
                    //spawn cube along line stretching it
                    SpawnWall(newStart , skippedAt);
                    requiredNewStart = true;
                }
                continue;
            }
            else{
                if(requiredNewStart){
                    skippedPoints = 0;
                    newStart = i;
                    skippedAt = i;
                    requiredNewStart = false;
                    numberOfBlobs++;
                }
            }
            //check distance difference between current and next
            int next = i+1;
            if(next == 360){
                SpawnWall(newStart , 359);
                Debug.Log("Ended");
                break;
            }
            float relativeDistance = 0;
            if(skippedPoints > 0)
                relativeDistance = (float)Math.Max(currentPositions[skippedAt], currentPositions[i]) / (float)Math.Min(currentPositions[skippedAt], currentPositions[i]);
            else
                relativeDistance = (float)Math.Max(currentPositions[i], currentPositions[next]) / (float)Math.Min(currentPositions[i], currentPositions[next]);

            //if relativeDistance is more than tolerance then skip
            if(relativeDistance > _aggregatePointsTolerance*10){
                skippedPoints++;
                Debug.Log("skippedPoints "+ (skippedPoints));
                Debug.Log("relativeDistance "+ (relativeDistance));
                //Debug.Log("Distance difference between " + i + " and " + next + " is " + relativeDistance);
                //draw a debug line that goes from _point 0 to point i
            }
            else{
                if(relativeDistance > _aggregatePointsTolerance + _aggregatePointsTolerance/10){
                    SpawnWall(newStart , skippedAt);
                    requiredNewStart = true;
                    continue;
                }

                skippedPoints = 0;
                skippedAt = i;
            }
            
            if(skippedPoints > _skippablePoints){
                Debug.Log("Skipped at "+ (skippedAt));
                //spawn cube along line stretching it
                SpawnWall(newStart , skippedAt);
                requiredNewStart = true;
            }
        }
        Debug.Log("Spawned "+ (numberOfBlobs));
    }

    private void WallsUsingDistancesSkippableGroupsBackup(){
        int skippedPoints = 0;
        int skippedAt = 0;
        int newStart = 0;
        bool requiredNewStart = true;
        int numberOfBlobs = 0;

        //disable all elements in walls
        foreach(GameObject wall in _walls){
            wall.GetComponent<MeshRenderer>().enabled = false;
        }
        _wallCount = 0; 

        for(int i = 0; i < 360; i++){
            if(currentPositions[i] >= 10000000){
                Debug.Log("currentPositions[i] "+ currentPositions[i]);
                skippedPoints++;
            
                if(skippedPoints > _skippablePoints){
                    Debug.Log("Skipped at "+ (skippedAt));
                    //spawn cube along line stretching it
                    SpawnWall(newStart , skippedAt);
                    requiredNewStart = true;
                }
                continue;
            }
            else{
                if(requiredNewStart){
                    skippedPoints = 0;
                    newStart = i;
                    skippedAt = i;
                    requiredNewStart = false;
                    numberOfBlobs++;
                }
            }
            //check distance difference between current and next
            int next = i+1;
            if(next == 360){
                //SpawnWall(newStart , next);
                Debug.Log("Ended");
                break;
            }
            float relativeDistance = 0;
            if(skippedPoints > 0)
                relativeDistance = (float)Math.Max(currentPositions[skippedAt], currentPositions[i]) / (float)Math.Min(currentPositions[skippedAt], currentPositions[i]);
            else
                relativeDistance = (float)Math.Max(currentPositions[i], currentPositions[next]) / (float)Math.Min(currentPositions[i], currentPositions[next]);

            //if relativeDistance is more than tolerance then skip
            if(relativeDistance > _aggregatePointsTolerance*10){
                skippedPoints++;
                Debug.Log("skippedPoints "+ (skippedPoints));
                Debug.Log("relativeDistance "+ (relativeDistance));
                //Debug.Log("Distance difference between " + i + " and " + next + " is " + relativeDistance);
                //draw a debug line that goes from _point 0 to point i
            }
            else{
                /*if(relativeDistance > _aggregatePointsTolerance + _aggregatePointsTolerance/10){
                    SpawnWall(newStart , skippedAt);
                    requiredNewStart = false;
                    newStart = i;
                }*/

                skippedPoints = 0;
                skippedAt = i;
            }
            
            if(skippedPoints > _skippablePoints){
                Debug.Log("Skipped at "+ (skippedAt));
                //spawn cube along line stretching it
                SpawnWall(newStart , skippedAt);
                requiredNewStart = true;
            }
        }
        Debug.Log("Spawned "+ (numberOfBlobs));
    }

    private void WallsUsingDistancesSkippableBACKUP(){
        int skippedPoints = 0;
        int skippedAt = 0;

        //disable all elements in walls
        foreach(GameObject wall in _walls){
            wall.GetComponent<MeshRenderer>().enabled = false;
        }
        _wallCount = 0; 

        for(int i = 0; i < 360; i++){
            //check distance difference between current and next
            int next = i+1;
            if(next == 360){
                Debug.Log("Ended");
                break;
            }
            float relativeDistance = 0;
            if(skippedPoints > 0)
                relativeDistance = (float)Math.Max(currentPositions[skippedAt], currentPositions[next]) / (float)Math.Min(currentPositions[skippedAt], currentPositions[next]);
            else
                relativeDistance = (float)Math.Max(currentPositions[i], currentPositions[next]) / (float)Math.Min(currentPositions[i], currentPositions[next]);

            //if relativeDistance is more than tolerance then skip
            if(relativeDistance > _aggregatePointsTolerance*10){
                skippedPoints++;
                Debug.Log("skippedPoints "+ (skippedPoints));
                Debug.Log("relativeDistance "+ (relativeDistance));
                //Debug.Log("Distance difference between " + i + " and " + next + " is " + relativeDistance);
                //draw a debug line that goes from _point 0 to point i
            }
            else{
                skippedPoints = 0;
                skippedAt = i;
            }
            
            if(skippedPoints > _skippablePoints){
                Debug.Log("Skipped at "+ (skippedAt));
                //spawn cube along line stretching it
                SpawnWall(0 , skippedAt);
                break;
            }
        }
    }

    private void SpawnWall(int start, int end){
        //enable
        _walls[_wallCount].SetActive(true);
        Vector3 startVec = _points[start].transform.position;
        Vector3 endVec = _points[end].transform.position;
        Vector3 middleVec = (startVec + endVec) / 2;
        float length = Vector3.Distance(startVec, endVec);
        Vector3 scaleVec = new Vector3(0.1f, _wallHeight, length + (middleVec.magnitude/5.0f/_wallSizeMultiplier) );
        Vector3 directionVec = endVec - startVec;
        Quaternion rotation = Quaternion.LookRotation(directionVec);
        _walls[_wallCount].transform.position = middleVec;
        _walls[_wallCount].transform.localScale = scaleVec;
        _walls[_wallCount].transform.rotation = rotation;
        if(scaleVec.z < _wallCylinderThreshold){
            _walls[_wallCount].SetActive(false);
            _cylinders[_wallCount].SetActive(true);
            _cylinders[_wallCount].transform.position = middleVec;
            //scale cylinders x and y by scalevec's z
            _cylinders[_wallCount].transform.localScale = new Vector3(scaleVec.z, _wallHeight/2, scaleVec.z);
        }
        _wallCount++;

    }

    private void SpawnWall2(int start, int end){
        //enable
        _walls[_wallCount].GetComponent<MeshRenderer>().enabled = true;
        //rotations
        List<Quaternion> rotations = new List<Quaternion>();
        Vector3 startVec = new Vector3(0,0,0);
        Vector3 endVec = new Vector3(0,0,0);
        int skippedRotations = 0;
        //spawn cube along line stretching it, consider all points in the middle too
        for(int q = start; q < end; q++){
            //if point is too far skip it
            float relativeDistance = (float)Math.Max(currentPositions[q], currentPositions[q+1]) / (float)Math.Min(currentPositions[q], currentPositions[q+1]);
            if(relativeDistance > _aggregatePointsTolerance * 10){
                skippedRotations++;
                continue;
            }
            //calculate direction of q and q+1
            startVec = _points[q].transform.position;
            endVec = _points[q+1].transform.position;
            Vector3 directionVec = endVec - startVec;
            Quaternion rotation = Quaternion.LookRotation(directionVec);
            rotations.Add(rotation);
        }
        //get average rotation quaternion
        Quaternion averageRotation = new Quaternion(0,0,0,0);
        foreach(Quaternion q in rotations){
            averageRotation = Quaternion.Lerp(averageRotation, q, 1.0f/(rotations.Count-skippedRotations));
        }


        //spawn cube
        startVec = _points[start].transform.position;
        endVec = _points[end].transform.position;
        Vector3 middleVec = (startVec + endVec) / 2;
        float length = Vector3.Distance(startVec, endVec);
        Vector3 scaleVec = new Vector3(0.1f, _wallHeight, length + (middleVec.magnitude/5.0f/_wallSizeMultiplier) );
        _walls[_wallCount].transform.position = middleVec;
        _walls[_wallCount].transform.localScale = scaleVec;
        _walls[_wallCount].transform.rotation = averageRotation;
        _wallCount++;


    }

    private void UpdatePoseDistance()
    {
        //No pose detected or invalid pose
        if (MinConvertedAngle.runtimeValue == -1 || MaxConvertedAngle.runtimeValue == -1)
        {
            //poseDistance.runtimeValue = defaultPoseDistance;
            //Debug.Log(poseDistance.runtimeValue);
        }
        else
        {
            int nOfPoseDegrees = MaxConvertedAngle.runtimeValue - MinConvertedAngle.runtimeValue + 1;
            Debug.Log(nOfPoseDegrees);
            int halfDegrees = (nOfPoseDegrees + 1) / 2;

            if (halfDegrees == 0)
                halfDegrees = 1;
            
            int[] arrayCopy = new int[nOfPoseDegrees];

            int j = 0;
            for (int i = MinConvertedAngle.runtimeValue; i <= MaxConvertedAngle.runtimeValue; i++)
            {
                arrayCopy[j] = trackedCameraDegrees[i];
                j++;
            }
            
            Array.Sort(arrayCopy);

            int sum = 0;
            
            for (int i = 0; i < halfDegrees; i++)
            {
                sum += arrayCopy[i];
            }

            int littleMean = sum / halfDegrees;

            int tolerance = 750; //CM

            sum = 0;

            j = 0;
            
            for (int i = MinConvertedAngle.runtimeValue; i <= MaxConvertedAngle.runtimeValue; i++)
            {
                int angle = trackedCameraDegrees[i];

                if (angle > (littleMean + tolerance) || angle < (littleMean - tolerance))
                {
                    //skip this angle
                    
                }
                else
                {
                    sum += angle;
                    j++;
                }
            }

            /* (int i = lastConvertedAngles[0]; i <= lastConvertedAngles[1]; i++)
            {
                sum += trackedCameraDegrees[i];
            }
            
            float mean = sum / (lastConvertedAngles[1] - lastConvertedAngles[0] + 1);*/

            if (j != 0)
            {
                if (sum == 0)
                {
                    //poseDistance.runtimeValue = defaultPoseDistance;
                    return;
                }
                float mean = sum / j;
            
                //Debug.Log(lastConvertedAngles[1] - lastConvertedAngles[0] + 1);
            
                poseDistance.runtimeValue = mean/100.0f;
                //Debug.Log(poseDistance.runtimeValue); 
            }
        }
    }

    private int[] CopyTrackedDegrees(int[] inputArray, int trackedDegrees)
    {
        int[] outputArray = new int[trackedDegrees];

        int isOdd = trackedDegrees % 2;

        int halfTrackedArray = trackedDegrees / 2;

        int start = inputArray.Length - (halfTrackedArray + isOdd);

        int j = 0;
        
        for (int i = start; i < inputArray.Length; i++)
        {
            outputArray[j] = inputArray[i];
            j++;
        }

        for (int i = 0; i < halfTrackedArray; i++)
        {
            outputArray[j] = inputArray[i];
            j++;
        }

        return outputArray;
    }

    private void SpawnPoints()
    {
        GameObject toSpawn;
        switch (lidarMode.runtimeValue)
        {
            case 1:
                toSpawn = lidarPoint;
                break;
            case 2:
                toSpawn = lidarPoint2;
                break;
            default:
                toSpawn = lidarPoint;
                break;
        }
        
        float radius = 10f;

        for (int i = 0; i < arraySize; ++i)
        {
            float circleposition = (float)i / (float)arraySize;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;
            GameObject obj = Instantiate(toSpawn, new Vector3(x, 0.0f, z), Quaternion.LookRotation(new Vector3(x,0.0f,z)));
            //GameObject obj = Instantiate(lidarPoint, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.LookRotation(new Vector3(x, 0.0f, z)));
            obj.transform.parent = transform;
            //obj.transform.position += obj.transform.forward*8.0f;
            _points[i] = obj;
            //_points[i] pillar blob checker SetLidarManager and SetPillarId
            _points[i].GetComponent<PillarManager>().SetPillarId(i);
        }
    }

    private void UpdatePosition(int pos, int value)
    {
        if (value == 0) //Default invalid value
        {
            StartCoroutine(FadeOutPoint(_points[pos]));
            //_points[pos].transform.position = defaultHidePosition;
            //_points[pos].transform.localScale = defaultScale;
        }
        else
        {
            //if blob checker id is >0
            if(_points[pos].GetComponent<PillarManager>().GetStationId() < 0){
                StartCoroutine(FadeInPoint(_points[pos]));
            }
            if ((ConvertAngleTo360(pos) >= MinConvertedAngle.runtimeValue && ConvertAngleTo360(pos) <= MaxConvertedAngle.runtimeValue) && (MinConvertedAngle.runtimeValue != -1)) //if one of the angles where Pose is
            {
                _points[pos].GetComponent<MeshRenderer>().enabled = true;
            }
            //else //HERE IS CONFLICT WITH BLOB
            //{
            //    if(!_mergeWalls)
            //        _points[pos].GetComponent<MeshRenderer>().enabled = true;
            //    else
            //        _points[pos].GetComponent<MeshRenderer>().enabled = false;
            //}
            
            //float convertedValue = ConvertRange(value);
            float convertedValue = (((float) value) / 100.0f)-0.5f;
            convertedValue = convertedValue * _lidarScale;
            float circleposition = (float)pos / (float)arraySize;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * convertedValue;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * convertedValue;
            Vector3 posit = _points[pos].transform.position;
            _points[pos].transform.position = new Vector3(x, posit.y, z); //TODO CHANGE HERE FOR PILLAR LERP

            _points[pos].transform.localScale = new Vector3(convertedValue / 10, _points[pos].transform.localScale.y,
                convertedValue / 10);
            //if point pos is first 15 or last 15 then scale y to 0.1
            //if (pos < 30 || pos > 330)
            //{
            //    _points[pos].transform.localScale = new Vector3(_points[pos].transform.localScale.x, 0.5f,
            //        _points[pos].transform.localScale.z);
            //}
        }
    }

    


    private IEnumerator FadeOutPoint(GameObject point)
    {
        Renderer renderer = point.GetComponent<Renderer>();
        Material material = renderer.material;

        Color originalColor = material.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade out to fully transparent

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration; // Normalized time value (0 to 1)
            //fadeout linear duration/elapsed
            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the material color is set to the target color at the end of the fade-out
        material.color = targetColor;
    }

    private IEnumerator FadeInPoint(GameObject point)
    {
        Renderer renderer = point.GetComponent<Renderer>();
        Material material = renderer.material;

        Color originalColor = material.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // Fade in to fully opaque

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration; // Normalized time value (0 to 1)
            //fadeout linear duration/elapsed
            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the material color is set to the target color at the end of the fade-in
        material.color = targetColor;
    }

    private float ConvertRange(int oldValue)
    {
        float newValue = ((((float) oldValue - (float) minMeasure) * (maxDistValue - minDistValue)) /
                    (maxMeasure - minMeasure)) + minDistValue;
        return newValue;
    }

    private int ConvertAngleTo360(int oldAngle)
    {
        int newAngle = oldAngle + (nOfLidarDegrees / 2);
        if (newAngle > 359)
        {
            newAngle = newAngle - 360;
        }

        return newAngle;
    }

    public float GetLidarScale()
    {
        return _lidarScale;
    }
    
}