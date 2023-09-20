//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//replace back all .localPosition to .position if there are problems

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
//this manager receives the 34 body landmarks detected by the depthai camera and positions a sphere at each landmark
public class PersonManager : MonoBehaviour
{
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
    private bool spawned = false;

    private GameObject[] spheres;

    //prev rcv time
    private float prevRcvTime = 0f;
    public void OnMsgRcv(byte[] msg)
    {
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
        prevRcvTime = Time.time;
    }
    void Start()
    {
        newObject = new GameObject("Transformer");
        newTransform = newObject.AddComponent<Transform>();
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
    //function to parse it into an array of arrays of 3 integers
    private int[][] ParseData(string data)
    {
        //split the data into lines
        string[] lines = data.Split('\n');
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

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void Update()
    {
        //if first receive
        if (data != null)
        {
            if(!spawned)
                //call function to spawn spheres
                SpawnSpheres();
            
            //move spheres to position
            MoveSpheres();
            data = null;
        }

    }

    //spawn spheres
    private void SpawnSpheres()
    {
        spheres = new GameObject[parsedData.Length];
        //spawn a sphere for each element in the array
        for (int i = 0; i < parsedData.Length; i++)
        {
            //spawn a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //name the sphere
            sphere.name = "Sphere (" + i + ")";
            //scale 10
            sphere.transform.localScale = new Vector3(80f/_scale, 80f/_scale, 80f/_scale);
            //from 0 to 10 make spheres red, from 11 to 21 make spheres yellow only on odd numbers, from 10 to 20 make spheres orange only on even numbers,
            //from 24 to 32 make spheres green only on even numbers, from 23 to 31 make spheres blue only on odd numbers
            if (i >= 0 && i <= 10)
            {
                sphere.GetComponent<Renderer>().material.color = Color.cyan;
            }
            if (i >= 11 && i <= 21)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
            if (i >= 10 && i <= 20)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            if (i >= 24 && i <= 32)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.green;
                }
            }
            if (i >= 23 && i <= 31)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.blue;
                }
            }
            //set the position of the sphere
            sphere.transform.localPosition = new Vector3(i, 0, 0);
            //add the sphere to the spheres array
            spheres[i] = sphere;
        }
        //set spawned to true
        spawned = true;
        //set sphere34 if sphere name is sphere 34
        sphere34 = spheres[33];
        //set all spheres as children of this gameobject
        foreach (GameObject sphere in spheres)
        {
            sphere.transform.parent = this.transform;
        }
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
                GameObject sphere = spheres[i];
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
                Vector3 rotationCenter = new Vector3(224.1144f/_scale, -26f/_scale, -359.3866f/_scale); // You can adjust the center of rotation as desired
                // Rotate the sphere around the center of rotation
                newTransform.RotateAround(rotationCenter, rotationAxis, rotationAngle);
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
            Debug.Log(spheres.Length);
        }
    }

    private void MoveSpheres()
    {
        try{
            //for each sphere
            for (int i = 0; i < parsedData.Length; i++)
            {
                //get the sphere
                GameObject sphere = spheres[i];
                //set positions
                //sphere.transform.localPosition = new Vector3(parsedData[i][1], parsedData[i][0], parsedData[i][2]);
                //should rotate z by 45 degrees. if a point has y = 0 z is unchanged, if a point has y = 100, z is brought closer
                sphere.transform.localPosition = new Vector3(parsedData[i][1]/_scale, parsedData[i][0]/_scale, parsedData[i][2]/_scale);
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
                sphere.transform.localPosition = new Vector3(sphere.transform.localPosition.x + xOffset, sphere.transform.localPosition.y + yOffset, sphere.transform.localPosition.z + zOffset);// + 1/sphere34.transform.localPosition.y * zMultiplier);
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
            Debug.Log(spheres.Length);
        }
    }
}

