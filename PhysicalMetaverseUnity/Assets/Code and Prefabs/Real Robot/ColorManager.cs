//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
//this manager receives x, y of the biggest blob of one chosen color in image frame and positions a sphere at such coordinates
public class ColorManager : MonoBehaviour
{
    //listen for udp messages on port 5004
    static int port = 5004;
    //udpclient object
    private UdpClient client;
    //udp packet storage
    private byte[] data;
    private int[] parsedData;

    //spawned
    private bool spawned = false;

    private GameObject[] spheres;

    public int _minColorSize = 300;
    private bool _colorTracked = false;
    public void OnMsgRcv(byte[] msg)
    {
        data = msg;
        char[] bytesAsChars = new char[msg.Length];
        for (int i = 0; i < msg.Length; i++)
        {
            bytesAsChars[i] = (char)msg[i];
        }
        string message = new string(bytesAsChars);
        Debug.Log("Color Manager received message: " + message);
        parsedData = ParseData(message);
        //if parsed 3 is smaller than 100 print UNTRACKED
        if (parsedData[2] < _minColorSize)
        {
            Debug.Log("COLOR UNTRACKED");
            if (_colorTracked){
                //save angle of sun
                _untrackedAngle = (int)_sun.transform.eulerAngles.y;
                _untrackedLocation = _sphere.transform;
            }
            _colorTracked = false;
        }
        else
            _colorTracked = true;
    }   
    void Start()
    {
    }

    //a receive looks like
/*
[460, 590]
*/
    //function to parse it into an array of arrays of 3 integers
    private int[] ParseData(string data)
    {
        //mind the comma and the space
        string[] splitData = data.Split(new string[] { ", " }, StringSplitOptions.None);
        int[] parsedData = new int[splitData.Length];
        //remove [ and ] from elements of the array
        for (int i = 0; i < splitData.Length; i++)
        {
            splitData[i] = splitData[i].Replace("[", "");
            splitData[i] = splitData[i].Replace("]", "");
            parsedData[i] = int.Parse(splitData[i]);
        }
        //print elements of parsedData
        for (int i = 0; i < parsedData.Length; i++)
        {
            Debug.Log(parsedData[i]);
        }
        return parsedData;
    }

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void FixedUpdate()
    {
        //if first receive
        if (parsedData != null)
        {
            if(!spawned)
                //call function to spawn spheres
                SpawnSphere();
            
            //move spheres to position
            MoveSphere();
        }

    }

    [Range(1f, 100f)]
    public float _scale = 2f;
    [Range(1f, 100f)]
    public float _imageFrameScale = 100;
    [Range(-100f, 100f)]
    public float zOffset = 0f;

    [Range(-100f, 100f)]
    public float yOffset = 0f;
    [Range(-100f, 100f)]
    public float xOffset = 0f;

    private GameObject _sphere;
    //spawn spheres
    private void SpawnSphere()
    {
        try{
            //create a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawned = true;
            //name the sphere
            sphere.name = "Sphere";
            //set the sphere's position
            sphere.transform.position = new Vector3(parsedData[1]/_scale, parsedData[0]/_scale, 0);
            //set the sphere's scale
            sphere.transform.localScale = new Vector3(_scale, _scale, _scale);
            //set the sphere's color
            sphere.GetComponent<Renderer>().material.color = Color.blue;
            _sphere = sphere;

        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    [Range(0.01f, 2f)]
    public float _speed = 0.1f;
    public int _untrackedAngle = 0;
    public Transform _sun;
    private Transform _untrackedLocation;
    //move spheres
    private void MoveSphere()
    {
        try{
            //set the sphere's position
            //_sphere.transform.position = new Vector3(parsedData[1]/_imageFrameScale + xOffset, parsedData[0]/_imageFrameScale + yOffset, zOffset);
            //linear movement to new position
            if (_colorTracked)
                _sphere.transform.position = Vector3.Lerp(_sphere.transform.position, new Vector3(parsedData[1]/_imageFrameScale + xOffset, yOffset, zOffset - parsedData[0]/_imageFrameScale), _speed);
            else{
                //rotate sphere around center of ther world around y axis _untrackedAngle, lerp use RotateAround to angle
                _sphere.transform.RotateAround(Vector3.zero, Vector3.up, -(_untrackedAngle-(int)_sun.transform.eulerAngles.y));
                _untrackedAngle = (int)_sun.transform.eulerAngles.y;
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}


