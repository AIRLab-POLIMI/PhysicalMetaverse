//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
//this manager receives x, y of the biggest blob of one chosen color in image frame and positions a sphere at such coordinates
public class StationManager : MonoBehaviour
{
    public GameObject _stationPrefab;
    //listen for udp messages on port 5004
    static int port = 5004;
    //udpclient object
    private UdpClient client;
    //udp packet storage
    private byte[] data;
    public static int _totalStations = 2;
    private float[] _lastPingTimes = new float[_totalStations];
    //stations list
    public List<int[]> _stationsData = new List<int[]>();
    //gameobject station list
    public List<GameObject> _stations = new List<GameObject>();

    //spawned
    private bool spawned = false;

    private GameObject[] spheres;

    public int _minColorSize = 300;
    private bool _colorTracked = false;
    public bool ENABLE_LOG = false;
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
        Debug.Log("Station Manager received message: " + message);
        ParseData(message);
        Debug.unityLogger.logEnabled = true;
    }   
    void Start()
    {
    }

    //a receive looks like
/*
[460, 590]
*/
    //function to parse it into an array of arrays of 3 integers
    private List<int[]> ParseData(string data)
    {
        _lastPingTime = Time.time;
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

    }

    private void ExpireStations()
    {
        //clear stations data
        _stationsData.Clear();
    }

    [Range(1f, 100f)]
    public float _scale = 2f;
    [Range(1f, 500f)]
    public float _imageFrameScale = 500;
    [Range(-100f, 100f)]
    public float zOffset = 0f;

    [Range(-100f, 100f)]
    public float yOffset = 0f;
    [Range(-100f, 100f)]
    public float xOffset = 0f;

    private GameObject _sphere;
    //spawn spheres
    private void SpawnStations()
    {
        for (int i = 0; i < _totalStations; i++)
        {
            GameObject station = Instantiate(_stationPrefab);
            _stations.Add(station);
            station.transform.localScale = new Vector3(_scale, _scale, _scale);
            //random color
            station.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0, 1f));
            station.transform.position = new Vector3(0, 0, 0);
            //disable
            station.SetActive(false);
            station.name = "Station " + i;
            //set father to this gameobject
            station.transform.parent = this.transform;
        }
        spawned = true;
    }
    [Range(0.01f, 2f)]
    public float _speed = 0.1f;
    public int _untrackedAngle = 0;
    public Transform _sun;
    private Transform _untrackedLocation;
    //_lastPingTime
    private float _lastPingTime = 0f;
    //public STATIONS_DECAY_TIME
    public float STATIONS_DECAY_TIME = 0.3f; //TODO REPLACE WITH DRIFTED TOO MUCH AWAY WITHOUT SEEING AGAIN
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
                if (stationCode >= 0 && stationCode < _totalStations)
                {
                    //eanble station
                    ((GameObject)_stations[stationCode]).SetActive(true);
                    //set station position, data is formatted as [station code, x, y, size(diagonal)]
                    //((GameObject)_stations[i]).transform.localPosition = new Vector3(((int[])_stationsData[i])[2] / _imageFrameScale + xOffset, (((int[])_stationsData[i])[1] / _imageFrameScale) + yOffset, ((int[])_stationsData[i])[3]/10.0f + zOffset);
                    //lerp
                    ((GameObject)_stations[stationCode]).transform.localPosition = Vector3.Lerp(((GameObject)_stations[stationCode]).transform.localPosition, new Vector3(((int[])_stationsData[i])[2] / _imageFrameScale + xOffset, (((int[])_stationsData[i])[1] / _imageFrameScale) + yOffset, ((int[])_stationsData[i])[3] / 10.0f + zOffset), _speed);
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
            if (Time.time - _lastPingTimes[i] > STATIONS_DECAY_TIME)
            {
                _stations[i].SetActive(false);
            }
        }
    }

    //move spheres
    //private void MoveSphere()
    //{
    //    try{
    //        //set the sphere's position
    //        //_sphere.transform.position = new Vector3(parsedData[1]/_imageFrameScale + xOffset, parsedData[0]/_imageFrameScale + yOffset, zOffset);
    //        //linear movement to new position using _stations
    //        if (_colorTracked)
    //            //_sphere.transform.position = Vector3.Lerp(_sphere.transform.position, new Vector3(parsedData[1]/_imageFrameScale + xOffset, yOffset, zOffset - parsedData[0]/_imageFrameScale), _speed);
    //            _sphere.transform.position = Vector3.Lerp(_sphere.transform.position, _untrackedLocation.position, _speed);
    //        else{
    //            //rotate sphere around center of ther world around y axis _untrackedAngle, lerp use RotateAround to angle
    //            _sphere.transform.RotateAround(Vector3.zero, Vector3.up, -(_untrackedAngle-(int)_sun.transform.eulerAngles.y));
    //            _untrackedAngle = (int)_sun.transform.eulerAngles.y;
    //        }
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.Log(e);
    //    }
    //}
}


