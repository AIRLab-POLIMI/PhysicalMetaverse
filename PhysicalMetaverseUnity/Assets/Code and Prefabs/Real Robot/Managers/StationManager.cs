//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class StationManager : Monosingleton<StationManager>
{
    [Header("ACTIONS")]
    [Space]
    [SerializeField] private bool _RESET_STATIONS = true;
    [SerializeField] private bool _ENABLE_LOG = false;
    [SerializeField] private bool _UPDATE_STATIONS_BEHAVIOUR = false;
    [SerializeField] private float _consumeMultiplier = 3f;
    [SerializeField] private float _activationPermanenceTime = 1f;


    [Space]
    [Space]
    [Header("ACTIONS")]
    [Space]
    [SerializeField] private GameObject _stationPrefab;

    private Transform _cameraStartRotationAngle;
    [SerializeField] private Transform _cameraRotationAngle;
    [SerializeField] private OdometryManager _odometryManager;
    //transforms list _untrackedStations
    private List<Transform> _untrackedStations = new List<Transform>();
    //float list untrackedangles
    [SerializeField] private List<float> _untrackedAngles = new List<float>();
    //[SerializeField] private station ips strings list
    [SerializeField] private List<string> _stationIps = new List<string>();
    //udp packet storage
    private byte[] data;
    [SerializeField] private int _totalStations = 7;
    [SerializeField] private Transform _orientationTransform;
    private float[] _lastPingTimes;
    //stations list
    [SerializeField] private List<int[]> _stationsData = new List<int[]>();
    //gameobject station list
    [SerializeField] private List<GameObject> _stations = new List<GameObject>();
    

    //spawned
    private bool spawned = false;
    [Space]
    [Space]
    [Header("CAMERA EXTRINSIC CALIBRATION")]
    [Space]
    //[Range(-5f, 5f)]    
    //[SerializeField] private float _perspectiveRotationCorrection = 1f;
    [Range(1f, 100f)]
    [SerializeField] private float _scale = 2f;
    [Range(1f, 500f)]
    [SerializeField] private float _imageFrameScale = 480f;
    //[SerializeField] private float _imageRatio = 4f/3f;
    
    [Range(1f, 100f)]
    [SerializeField] private float _perspectiveCorrection = 1f;
    
    [Range(10f, 100f)]
    [SerializeField] private float _zScale = 1f;

    [Range(-15f, 15)]
    [SerializeField] private float zOffset = 0f;

    //[Range(-15f, 15)]
    //[SerializeField] private float yOffset = 0f;
    [Range(-15f, 15)]
    [SerializeField] private float xOffset = 0f;
    private float _currentZ = 0f;
    [SerializeField] private float _yPosition = -0.8f;
    
    //[Range(0.01f, 2f)]
    //[SerializeField] private float _cameraSidesCorrection = 1f;
    
    [Range(0.1f, 3f)]
    [SerializeField] private float _zTrackedTolerance = 1.0f;
    [SerializeField] private bool _TOLERANCE_CHECK = false;
    
    [Space]
    [Space]
    [Header("LERP")]
    [Space]
    [Range(0.01f, 2f)]
    [SerializeField] private float _speed = 0.1f;
    //[SerializeField] private STATIONS_DECAY_TIME
    [SerializeField] private float STATIONS_DECAY_TIME = 0.3f; //TODO REPLACE WITH DRIFTED TOO MUCH AWAY WITHOUT SEEING AGAIN
    //[SerializeField] private TRACKING_DECAY_TIME
    [SerializeField] private float TRACKING_DECAY_TIME = 0.1f;
    [SerializeField] private bool _lerp = false;
    [SerializeField] private string _rightStationMessage = "R:1";
    [SerializeField] private string _wrongStationMessage = "W:10";
    [SerializeField] private int _completedStations = 0;
    private GameObject _sphere;
    private float _lidarScale = 1f;
    public void OnMsgRcv(byte[] msg)
    {
        //disable Debug.Log for this object
        Debug.unityLogger.logEnabled = _ENABLE_LOG;
        data = msg;
        char[] bytesAsChars = new char[msg.Length];
        for (int i = 0; i < msg.Length; i++)
        {
            bytesAsChars[i] = (char)msg[i];
        }
        string message = new string(bytesAsChars);
        Debug.Log("Station Manager received message: " + message);
        ParseData(message);
        Debug.unityLogger.logEnabled = true;
    }   
    void Start()
    {
        _totalStations = _stationIps.Count;
        //create an untracked gameobject for each station add it to _untrackedStations, parent is this object
        for (int i = 0; i < _totalStations; i++)
        {
            GameObject untrackedStation = new GameObject();
            untrackedStation.transform.parent = this.transform;
            untrackedStation.name = "Untracked Station " + i;
            _untrackedStations.Add(untrackedStation.transform);
            _untrackedAngles.Add(0f);
        }
        //copy y angle of _cameraRotationAngle to _cameraStartRotationAngle
        _cameraStartRotationAngle = new GameObject().transform;
        _cameraStartRotationAngle.eulerAngles = new Vector3(0, _cameraRotationAngle.eulerAngles.y, 0);
        _lastPingTimes = new float[_totalStations];

        //get lidar scale
        _lidarScale = LidarManager.Instance.GetLidarScale();
    }

    //a receive looks like
/*
[460, 590]
*/
    //function to parse it into an array of arrays of 3 integers
    private List<int[]> ParseData(string data)
    {
        //split data with key byte 195 as separator
        string[] messages = data.Split((char)195);
        //clear stations
        _stationsData.Clear();
        foreach (string message in messages)
        {
            //mind the comma and the space
            string[] singleMessage = message.Split(new string[] { ", " }, StringSplitOptions.None);
            int[] parsedData = new int[singleMessage.Length];
            //remove [ and ] from elements of the array
            for (int i = 0; i < singleMessage.Length; i++)
            {
                singleMessage[i] = singleMessage[i].Replace("[", "");
                singleMessage[i] = singleMessage[i].Replace("]", "");
                parsedData[i] = int.Parse(singleMessage[i]);
            }
            _stationsData.Add(parsedData);
            _lastPingTimes[parsedData[0]] = Time.time;
            float currentZ = parsedData[3] / _zScale + zOffset;
            //if place 3 of array is more than ztrackedtolerance don't set to true
            if (_TOLERANCE_CHECK){
                if (currentZ < _zTrackedTolerance)
                    _stations[parsedData[0]].GetComponent<SingleStationManager>().SetTracked(false);
                else
                    _stations[parsedData[0]].GetComponent<SingleStationManager>().SetTracked(true);
            }
            else
                _stations[parsedData[0]].GetComponent<SingleStationManager>().SetTracked(true);
        }
        //log stations
        foreach (int[] station in _stationsData)
        {
            string s = "Station: ";
            foreach (int i in station)
            {
                s += i + " ";
            }
            Debug.Log(s);
        }
        Debug.Log("Total stations: " + messages.Length);
        return _stationsData;
    }

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void FixedUpdate()
    {
        if (spawned)
        {
            
            MoveStations();
        }
        else
        {
            SpawnStations();
        }
        //rotate this gameobject like delta y angle of _cameraStartRotationAngle
        //this.transform.eulerAngles = new Vector3(0, -(_cameraRotationAngle.eulerAngles.y - _cameraStartRotationAngle.eulerAngles.y)*(_currentZ/_perspectiveRotationCorrection), 0);
        if(_RESET_STATIONS){
            _RESET_STATIONS = false;
            ResetStations();
        }

        if(_UPDATE_STATIONS_BEHAVIOUR){
            _UPDATE_STATIONS_BEHAVIOUR = false;
            UpdateStationsBehaviour();
        }
    }

    private void UpdateStationsBehaviour(){
        //for each station
        foreach (GameObject station in _stations)
        {
            station.GetComponent<SingleStationManager>().UpdateBehaviour(_consumeMultiplier, _fadeSpeed, _activationPermanenceTime);
        }
    }

    [SerializeField] private float _fadeSpeed = 0.1f;
    private void ResetStations(){
        //for each ip send RESET using networking manager
        foreach (string ip in _stationIps)
        {
            NetworkingManager.Instance.SendString("RESET", ip);
        }
    }

    private void ExpireStations()
    {
        //clear stations data
        _stationsData.Clear();
    }


    //spawn spheres
    private void SpawnStations()
    {
        for (int i = 0; i < _totalStations; i++)
        {
            GameObject station = Instantiate(_stationPrefab);
            _stations.Add(station);
            //set station's untrackedParent to untrackedStation
            station.GetComponent<SingleStationManager>().SetUntrackedParent(_untrackedStations[i]);
            station.transform.localScale = new Vector3(_scale, _scale, _scale);
            //random color
            station.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0, 1f));
            station.transform.position = new Vector3(0, 0, 50);
            //disable
            station.SetActive(false);
            station.name = "Station " + i;
            //set father to this gameobject
            station.transform.parent = this.transform;
            //set ip
            station.GetComponent<SingleStationManager>().SetIp(_stationIps[i]);
            //set orientation transform
            station.GetComponent<SingleStationManager>().SetOrientationTransform(_orientationTransform);
            LidarManager.Instance.AddStationInteraction(station.GetComponent<SingleStationManager>().GetStationInteraction());
        }
        spawned = true;
        //call LidarManager method SpawnLidarBlobs
        LidarManager.Instance.SpawnLidarBlobs();
    }
    
    private void MoveStations()
    {
        try
        {
            //for each message in station data enable corresponding station
            for (int i = 0; i < _stationsData.Count; i++)
            {
                //log station data
                //Debug.Log("Station " + i + " data: " + ((int[])_stationsData[i])[0] + " " + ((int[])_stationsData[i])[1]);
                int stationCode = ((int[])_stationsData[i])[0];
                GameObject station = (GameObject)_stations[stationCode];
                if (stationCode >= 0 && stationCode < _totalStations)
                {
                    //eanble station
                    station.SetActive(true);
                    ////station.GetComponent<SingleStationManager>().GetStationInteraction().gameObject.SetActive(true);
                    //set station position, data is formatted as [station code, x, y, size(diagonal)]
                    //((GameObject)_stations[i]).transform.localPosition = new Vector3(((int[])_stationsData[i])[2] / _imageFrameScale + xOffset, (((int[])_stationsData[i])[1] / _imageFrameScale) + yOffset, ((int[])_stationsData[i])[3]/10.0f + zOffset);
                    //if _stations[i].GetComponent<SingleStationManager>()._tracked
                    if (station.GetComponent<SingleStationManager>().GetTracked())
                    {
                        //lerp
                        //station.transform.localPosition = Vector3.Lerp(station.transform.localPosition, new Vector3(((int[])_stationsData[i])[2] / _imageFrameScale + xOffset, (((int[])_stationsData[i])[1] / _imageFrameScale) + yOffset, ((int[])_stationsData[i])[3] / 10.0f + zOffset), _speed);
                        //switch x and y
                        //station.transform.localPosition = Vector3.Lerp(station.transform.localPosition, new Vector3((((int[])_stationsData[i])[1] / _imageFrameScale) + xOffset, (((int[])_stationsData[i])[2] / _imageFrameScale * _imageRatio ) + yOffset, ((int[])_stationsData[i])[3] / _zScale + zOffset), _speed);
                        _currentZ = ((int[])_stationsData[i])[3] / _zScale * _lidarScale + zOffset;
                        float currentX = ((((int[])_stationsData[i])[1] / _imageFrameScale) + xOffset)*(_currentZ/_perspectiveCorrection);
                        //block Y to -0.8
                        if(_lerp)
                            station.transform.localPosition = Vector3.Lerp(station.transform.localPosition, new Vector3(currentX, _yPosition, _currentZ), _speed);
                        else
                            station.transform.localPosition = new Vector3(currentX, _yPosition, _currentZ);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        //for each lastpingtimes disable object if decay time passed
        for (int i = 0; i < _lastPingTimes.Length; i++)
        {
            float _elapsedTime = Time.time - _lastPingTimes[i];
            if (_elapsedTime > TRACKING_DECAY_TIME)
            {
                _stations[i].GetComponent<SingleStationManager>().SetTracked(false);
                if (_elapsedTime > STATIONS_DECAY_TIME){
                    ////_stations[i].GetComponent<SingleStationManager>().GetStationInteraction().gameObject.SetActive(false);
                    _stations[i].SetActive(false);
                }
            }
        }
    }

    public void CompleteRightStation(){
        NetworkingManager.Instance.SendString(_rightStationMessage, NetworkingManager.Instance.GetPythonGamemanagerIp());
        _completedStations++;
        GameManager.Instance.UpdateScore(_completedStations);
    }
    
    public void CompleteWrongStation(){
        NetworkingManager.Instance.SendString(_wrongStationMessage, NetworkingManager.Instance.GetPythonGamemanagerIp());
        GameManager.Instance.SubtractTime(ParseTime(_wrongStationMessage));
    }
    
    //parse time from _wrongStationMessage
    private int ParseTime(string message){
        string[] splitted = message.Split(':');
        return int.Parse(splitted[1]);
    }

    public List<GameObject> GetStations(){
        return _stations;
    }
}


